using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class OrderController : Controller
    {
        private readonly InventoryDbContext _db;
        private readonly ILogger<OrderController> _logger;

        public OrderController(InventoryDbContext db, ILogger<OrderController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // LIST
        public async Task<IActionResult> Index()
        {
            try
            {
                /*var orders = await _db.Orders
                    /*
                    .Include(o => o.CustomerId)
                    #1#
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.InventoryId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();*/
                
                var orders = await _db.Orders.ToListAsync();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return View(new List<Order>());
            }
        }

        // CREATE (GET)
        // CREATE (GET)
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new Order
                {
                    Id = 0,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    OrderItems = new List<OrderItem>()
                };

                ViewBag.Customers = await _db.Customers.Where(c => c.CreatedBy != 0).ToListAsync() ?? new List<Customer>();
                ViewBag.Inventories = await _db.Inventories.Where(i => i.Quantity > 0).ToListAsync() ?? new List<Inventory>();
                
                return View(model);  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing order creation");
                return RedirectToAction(nameof(Index));
            }
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Index", "Home");

                order.CreatedBy = userId.Value;
                order.OrderDate = DateTime.Now;

                if (order.OrderItems != null && order.OrderItems.Any())
                {
                    foreach (var item in order.OrderItems)
                    {
                        var inventory = await _db.Inventories.FindAsync(item.InventoryId);
                        if (inventory == null) continue;

                        item.UnitPrice = inventory.Price ?? 0;
                        item.TotalPrice = item.UnitPrice * item.Quantity;

                        inventory.Quantity -= item.Quantity;
                    }

                    order.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Customers = await _db.Customers.Where(c => c.CreatedBy != 0).ToListAsync() ?? new List<Customer>();
                    ViewBag.Inventories = await _db.Inventories.Where(i => i.Quantity > 0).ToListAsync() ?? new List<Inventory>();
                    return View(order);
                }

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ViewBag.Customers = await _db.Customers.Where(c => c.CreatedBy != 0).ToListAsync() ?? new List<Customer>();
                ViewBag.Inventories = await _db.Inventories.Where(i => i.Quantity > 0).ToListAsync() ?? new List<Inventory>();
                return View(order);
            }
        }

        // DELETE (POST)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var order = await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
                if (order == null) return NotFound();

                foreach (var item in order.OrderItems)
                {
                    var inventory = await _db.Inventories.FindAsync(item.InventoryId);
                    if (inventory != null)
                        inventory.Quantity += item.Quantity;
                }

                _db.Orders.Remove(order);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order");
                return RedirectToAction(nameof(Index));
            }
        }

        // EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var order = await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
                if (order == null) return NotFound();

                ViewBag.Customers = await _db.Customers.Where(c => c.CreatedBy != 0).ToListAsync() ?? new List<Customer>();
                ViewBag.Inventories = await _db.Inventories.ToListAsync() ?? new List<Inventory>();

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order for edit");
                return RedirectToAction(nameof(Index));
            }
        }

        // EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Order order)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Index", "Home");

                var existingOrder = await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == order.Id);
                if (existingOrder == null) return NotFound();

                // Restore old inventory
                foreach (var item in existingOrder.OrderItems)
                {
                    var inventory = await _db.Inventories.FindAsync(item.InventoryId);
                    if (inventory != null)
                        inventory.Quantity += item.Quantity;
                }

                _db.OrderItems.RemoveRange(existingOrder.OrderItems);

                // Add new items
                if (order.OrderItems != null && order.OrderItems.Any())
                {
                    foreach (var item in order.OrderItems)
                    {
                        var inventory = await _db.Inventories.FindAsync(item.InventoryId);
                        if (inventory == null) continue;

                        item.UnitPrice = inventory.Price ?? 0;
                        item.TotalPrice = item.UnitPrice * item.Quantity;

                        inventory.Quantity -= item.Quantity;
                        item.OrderId = order.Id;
                    }

                    existingOrder.OrderItems = order.OrderItems;
                    existingOrder.TotalAmount = order.OrderItems.Sum(oi => oi.TotalPrice);
                }

                existingOrder.CustomerId = order.CustomerId;
                existingOrder.Status = order.Status;

                _db.Update(existingOrder);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order");

                ViewBag.Customers = await _db.Customers.Where(c => c.CreatedBy != 0).ToListAsync() ?? new List<Customer>();
                ViewBag.Inventories = await _db.Inventories.ToListAsync() ?? new List<Inventory>();
                return View(order);
            }
        }
    }
}
