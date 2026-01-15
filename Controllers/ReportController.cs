using System.Globalization;
using InventoryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InventoryManagementSystem.Controllers
{
    public class ReportController(InventoryDbContext _db,ILogger<ReportController> _logger ) : Controller
    {
        
        // GET: /Report
        public IActionResult Index()
        {
            return View();
        }

        //  /Report/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(string reportType, DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                DateTime? from = fromDate?.Date;
                DateTime? to = toDate.HasValue
                    ? toDate.Value.Date.AddDays(1).AddTicks(-1)
                    : null;

                byte[] pdf = reportType switch
                {
                    "OrderSummary" => await BuildOrderSummaryPdf(from, to),
                    "InventoryStock" => await BuildInventoryStockPdf(),
                    "SalesRegister" => await BuildSalesRegisterPdf(from, to),
                    _ => null
                };

                if (pdf == null)
                    return BadRequest("Invalid report type.");

                var filename = $"{reportType}_{DateTime.Now:yyyyMMddHHmm}.pdf";
                return File(pdf, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                TempData["Error"] = "An error occurred while generating the report.";
                return RedirectToAction(nameof(Index));
            }
        }

        // -----------------------------
        // DTOs used for reports 
        // -----------------------------
        private sealed class OrderSummaryRow
        {
            public int OrderId { get; set; }
            public string CustomerName { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; }
            public decimal Total { get; set; }
        }

        private sealed class InventoryStockRow
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string SKU { get; set; }
            public string Category { get; set; }
            public int Quantity { get; set; }
            public int ReorderLevel { get; set; }
            public decimal Price { get; set; }
        }

        private sealed class SalesRegisterRow
        {
            public int OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public string CustomerName { get; set; }
            public int ItemsCount { get; set; }
            public decimal Total { get; set; }
        }

        // -----------------------------
        // Reports
        // -----------------------------
        private async Task<byte[]> BuildOrderSummaryPdf(DateTime? from, DateTime? to)
        {
            var baseQ =
                from o in _db.Orders.AsNoTracking()
                join c in _db.Customers.AsNoTracking()
                    on o.CustomerId equals c.Id
                select new { o, c };

            if (from.HasValue)
                baseQ = baseQ.Where(x => x.o.OrderDate >= from.Value);
            if (to.HasValue)
                baseQ = baseQ.Where(x => x.o.OrderDate <= to.Value);

            var orderIds = await baseQ.Select(x => x.o.Id).ToListAsync();

            // Load order items for these orders in one query
            var items = await _db.OrderItems.AsNoTracking()
                .Where(oi => orderIds.Contains(oi.OrderId))
                .ToListAsync();

            // Build rows
            var rows = await baseQ
                .OrderByDescending(x => x.o.OrderDate)
                .Select(x => new OrderSummaryRow
                {
                    OrderId = x.o.Id,
                    CustomerName = x.c.Name,
                    OrderDate = x.o.OrderDate,
                    Status = x.o.Status,
                    Total = 0m // compute below
                })
                .ToListAsync();

            // compute totals from stored TotalAmount if present, else from items
            var totalsByOrder = items
                .GroupBy(i => i.OrderId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.UnitPrice * i.Quantity));

            var totalsAmount = await _db.Orders.AsNoTracking()
                .Where(o => orderIds.Contains(o.Id))
                .Select(o => new { o.Id, o.TotalAmount })
                .ToListAsync();

            var totalAmountByOrder = totalsAmount.ToDictionary(x => x.Id, x => x.TotalAmount);

            foreach (var r in rows)
            {
                if (totalAmountByOrder.TryGetValue(r.OrderId, out var stored) && stored.HasValue)
                    r.Total = stored.Value;
                else
                    r.Total = totalsByOrder.TryGetValue(r.OrderId, out var t) ? t : 0m;
            }

            var culture = CultureInfo.GetCultureInfo("en-US");
            var totalOrders = rows.Count;
            var grandTotal = rows.Sum(r => r.Total);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(e => BuildHeader(e, "Order Summary", from, to));

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Total Orders: {totalOrders}").SemiBold();
                            r.RelativeItem().AlignRight().Text($"Grand Total: {grandTotal.ToString("C", culture)}").SemiBold();
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);   // ID
                                columns.RelativeColumn(3);    // Customer
                                columns.RelativeColumn(2);    // Date
                                columns.RelativeColumn(2);    // Status
                                columns.RelativeColumn(2);    // Total
                            });

                            table.Header(h =>
                            {
                                HeaderCell(h.Cell()).Text("ID");
                                HeaderCell(h.Cell()).Text("Customer");
                                HeaderCell(h.Cell()).Text("Date");
                                HeaderCell(h.Cell()).Text("Status");
                                HeaderCell(h.Cell()).AlignRight().Text("Total");
                            });

                            foreach (var o in rows)
                            {
                                BodyCell(table.Cell()).Text($"#{o.OrderId}");
                                BodyCell(table.Cell()).Text(o.CustomerName ?? "-");
                                BodyCell(table.Cell()).Text(o.OrderDate.ToString("dd MMM yyyy"));
                                BodyCell(table.Cell()).Text(o.Status ?? "-");
                                BodyCell(table.Cell()).AlignRight().Text(o.Total.ToString("C", culture));
                            }
                        });

                        col.Item().Text(" ").FontSize(2);
                        col.Item().Text("Notes: Totals use Orders.TotalAmount when available, otherwise computed from OrderItems.")
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Generated ");
                        txt.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).SemiBold();
                        txt.Span("  •  Page ");
                        txt.CurrentPageNumber();
                        txt.Span(" / ");
                        txt.TotalPages();
                    });
                });
            });

            return doc.GeneratePdf();
        }

        private async Task<byte[]> BuildInventoryStockPdf()
        {
            var rows = await _db.Inventories.AsNoTracking()
                .OrderBy(i => i.Name)
                .Select(i => new InventoryStockRow
                {
                    Id = i.Id,
                    Name = i.Name,
                    SKU = i.SKU,
                    Category = i.Category,
                    Quantity = i.Quantity,
                    ReorderLevel = i.ReorderLevel,
                    Price = i.Price ?? 0m
                })
                .ToListAsync();

            var lowStock = rows.Count(i => i.Quantity <= i.ReorderLevel);
            var culture = CultureInfo.GetCultureInfo("en-US");

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(e => BuildHeader(e, "Inventory Stock", null, null));

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Items: {rows.Count}").SemiBold();
                            r.RelativeItem().AlignRight().Text($"Low Stock: {lowStock}").SemiBold();
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(55); // ID
                                columns.RelativeColumn(3);  // Name
                                columns.RelativeColumn(2);  // SKU
                                columns.RelativeColumn(2);  // Category
                                columns.RelativeColumn(1);  // Qty
                                columns.RelativeColumn(1);  // Reorder
                                columns.RelativeColumn(1);  // Price
                            });

                            table.Header(h =>
                            {
                                HeaderCell(h.Cell()).Text("ID");
                                HeaderCell(h.Cell()).Text("Item");
                                HeaderCell(h.Cell()).Text("SKU");
                                HeaderCell(h.Cell()).Text("Category");
                                HeaderCell(h.Cell()).AlignRight().Text("Qty");
                                HeaderCell(h.Cell()).AlignRight().Text("Reorder");
                                HeaderCell(h.Cell()).AlignRight().Text("Price");
                            });

                            foreach (var i in rows)
                            {
                                var isLow = i.Quantity <= i.ReorderLevel;

                                BodyCell(table.Cell()).Text($"#{i.Id}");
                                BodyCell(table.Cell()).Text(i.Name ?? "-");
                                BodyCell(table.Cell()).Text(i.SKU ?? "-");
                                BodyCell(table.Cell()).Text(i.Category ?? "-");

                                var qtyCell = BodyCell(table.Cell()).AlignRight();
                                if (isLow)
                                    qtyCell.Text(i.Quantity.ToString()).FontColor(Colors.Red.Darken2).SemiBold();
                                else
                                    qtyCell.Text(i.Quantity.ToString());

                                BodyCell(table.Cell()).AlignRight().Text(i.ReorderLevel.ToString());
                                BodyCell(table.Cell()).AlignRight().Text(i.Price.ToString("C", culture));
                            }
                        });

                        col.Item().Text("Low stock items are highlighted in red.").FontColor(Colors.Grey.Darken2);
                    });

                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Generated ");
                        txt.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).SemiBold();
                        txt.Span("  •  Page ");
                        txt.CurrentPageNumber();
                        txt.Span(" / ");
                        txt.TotalPages();
                    });
                });
            });

            return doc.GeneratePdf();
        }

        private async Task<byte[]> BuildSalesRegisterPdf(DateTime? from, DateTime? to)
        {
            var baseQ =
                from o in _db.Orders.AsNoTracking()
                join c in _db.Customers.AsNoTracking()
                    on o.CustomerId equals c.Id
                where o.Status == "Completed"
                select new { o, c };

            if (from.HasValue)
                baseQ = baseQ.Where(x => x.o.OrderDate >= from.Value);
            if (to.HasValue)
                baseQ = baseQ.Where(x => x.o.OrderDate <= to.Value);

            var orderIds = await baseQ.Select(x => x.o.Id).ToListAsync();

            var items = await _db.OrderItems.AsNoTracking()
                .Where(oi => orderIds.Contains(oi.OrderId))
                .ToListAsync();

            var itemsCountByOrder = items
                .GroupBy(i => i.OrderId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            var totalsByOrder = items
                .GroupBy(i => i.OrderId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.UnitPrice * i.Quantity));

            var totalsAmount = await _db.Orders.AsNoTracking()
                .Where(o => orderIds.Contains(o.Id))
                .Select(o => new { o.Id, o.TotalAmount })
                .ToListAsync();

            var totalAmountByOrder = totalsAmount.ToDictionary(x => x.Id, x => x.TotalAmount);

            var rows = await baseQ
                .OrderByDescending(x => x.o.OrderDate)
                .Select(x => new SalesRegisterRow
                {
                    OrderId = x.o.Id,
                    OrderDate = x.o.OrderDate,
                    CustomerName = x.c.Name,
                    ItemsCount = 0,
                    Total = 0m
                })
                .ToListAsync();

            foreach (var r in rows)
            {
                r.ItemsCount = itemsCountByOrder.TryGetValue(r.OrderId, out var cnt) ? cnt : 0;

                if (totalAmountByOrder.TryGetValue(r.OrderId, out var stored) && stored.HasValue)
                    r.Total = stored.Value;
                else
                    r.Total = totalsByOrder.TryGetValue(r.OrderId, out var t) ? t : 0m;
            }

            var culture = CultureInfo.GetCultureInfo("en-US");
            var salesTotal = rows.Sum(r => r.Total);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(e => BuildHeader(e, "Sales Register (Completed Orders)", from, to));

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Completed Orders: {rows.Count}").SemiBold();
                            r.RelativeItem().AlignRight().Text($"Total Sales: {salesTotal.ToString("C", culture)}").SemiBold();
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(55); // Order ID
                                columns.RelativeColumn(2);  // Date
                                columns.RelativeColumn(3);  // Customer
                                columns.RelativeColumn(1);  // Items
                                columns.RelativeColumn(2);  // Total
                            });

                            table.Header(h =>
                            {
                                HeaderCell(h.Cell()).Text("Order");
                                HeaderCell(h.Cell()).Text("Date");
                                HeaderCell(h.Cell()).Text("Customer");
                                HeaderCell(h.Cell()).AlignRight().Text("Items");
                                HeaderCell(h.Cell()).AlignRight().Text("Total");
                            });

                            foreach (var o in rows)
                            {
                                BodyCell(table.Cell()).Text($"#{o.OrderId}");
                                BodyCell(table.Cell()).Text(o.OrderDate.ToString("dd MMM yyyy"));
                                BodyCell(table.Cell()).Text(o.CustomerName ?? "-");
                                BodyCell(table.Cell()).AlignRight().Text(o.ItemsCount.ToString());
                                BodyCell(table.Cell()).AlignRight().Text(o.Total.ToString("C", culture));
                            }
                        });

                        col.Item().Text("Note: This register includes only orders with Status = Completed.")
                            .FontColor(Colors.Grey.Darken2);
                    });

                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Generated ");
                        txt.Span(DateTime.Now.ToString("dd MMM yyyy HH:mm")).SemiBold();
                        txt.Span("  •  Page ");
                        txt.CurrentPageNumber();
                        txt.Span(" / ");
                        txt.TotalPages();
                    });
                });
            });

            return doc.GeneratePdf();
        }

        // -----------------------------
        // PDF helper
        // -----------------------------
        private static void BuildHeader(IContainer container, string title, DateTime? from, DateTime? to)
        {
            container.Column(col =>
            {
                // Top row
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text("Inventory Management System").FontSize(14).SemiBold();
                        left.Item().Text(title).FontSize(18).Bold();

                        if (from.HasValue || to.HasValue)
                        {
                            var fromText = from?.ToString("dd MMM yyyy") ?? "Any";
                            var toText = to?.ToString("dd MMM yyyy") ?? "Any";
                            left.Item().Text($"Period: {fromText} - {toText}")
                                .FontColor(Colors.Grey.Darken2);
                        }
                    });

                    row.ConstantItem(140).AlignRight().Column(right =>
                    {
                        right.Item().Text(DateTime.Now.ToString("dd MMM yyyy")).AlignRight();
                        right.Item().Text(DateTime.Now.ToString("HH:mm"))
                            .AlignRight()
                            .FontColor(Colors.Grey.Darken2);
                    });
                });

                // Divider line
                col.Item()
                    .PaddingTop(10)
                    .LineHorizontal(1)
                    .LineColor(Colors.Grey.Lighten2);
            });
        }


        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                .Background(Colors.Blue.Darken2)
                .PaddingVertical(6)
                .PaddingHorizontal(8);
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .PaddingVertical(6)
                .PaddingHorizontal(8);
        }
    }
}
