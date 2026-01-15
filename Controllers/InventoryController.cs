using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class InventoryController(InventoryDbContext _db,ILogger<InventoryController> _logger) : Controller
    {
        

        // GET: /Inventory
        public async Task<IActionResult> Index()
        {
            var items = await _db.Inventories.ToListAsync();
            return View(items);
        }

        // GET: /Inventory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryCreateViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(vm);

                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                    return RedirectToAction("Index", "Home");

                var item = new Inventory
                {
                    Name = vm.Name,
                    SKU = vm.SKU,
                    Category = vm.Category,
                    Quantity = vm.Quantity,
                    ReorderLevel = vm.ReorderLevel,
                    Price = vm.Price,
                    CreatedBy = userId.Value,
                    CreatedAt = DateTime.Now
                };

                _db.Inventories.Add(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory item");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the inventory item.");
                return View(vm);
            }
        }

        // GET: /Inventory/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Inventories.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        // POST: /Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inventory item)
        {
            if (id != item.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                item.UpdatedAt = DateTime.Now;
                _db.Inventories.Update(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        // POST: /Inventory/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Inventories.FindAsync(id);
            if (item == null) return NotFound();

            _db.Inventories.Remove(item);
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
