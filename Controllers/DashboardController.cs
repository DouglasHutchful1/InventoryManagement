using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class DashboardController(InventoryDbContext _db) : Controller
    {
        

        public async Task<IActionResult> Index()
        {
            // Summary cards
            var totalInventory = await _db.Inventories.SumAsync(i => i.Quantity);
            var pendingOrders = await _db.Orders.CountAsync(o => o.Status == "Pending");
            var salesToday = await _db.Sales
                .Where(s => s.CreatedAt.Date == DateTime.Today)
                .SumAsync(s => s.SaleAmount);
            var usersCount = await _db.Suppliers.CountAsync();

            // Chart data  last 7 days
            var last7Days = Enumerable.Range(0, 7)
                                      .Select(i => DateTime.Today.AddDays(-6 + i))
                                      .ToList();

            var inventoryTrends = new List<int>();
            var salesTrends = new List<decimal>();

            foreach (var day in last7Days)
            {
                var dayInventory = await _db.Inventories
                    .Where(i => i.CreatedAt <= day)
                    .SumAsync(i => i.Quantity);
                inventoryTrends.Add(dayInventory);

                var daySales = await _db.Sales
                    .Where(s => s.CreatedAt.Date == day)
                    .SumAsync(s => s.SaleAmount);
                salesTrends.Add(daySales);
            }

            ViewBag.TotalInventory = totalInventory;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.SalesToday = salesToday;
            ViewBag.UsersCount = usersCount;

            ViewBag.InventoryLabels = last7Days.Select(d => d.ToString("ddd")).ToArray();
            ViewBag.InventoryData = inventoryTrends.ToArray();
            ViewBag.SalesData = salesTrends.ToArray();

            return View();
        }
    }
}
