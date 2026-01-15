using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class SupplierController(InventoryDbContext _db,ILogger<SupplierController> _logger) : Controller
    {
        

        // LIST
        public async Task<IActionResult> Index()
        {
            try
            {
                var suppliers = await _db.Suppliers
                    .Where(s => s.Active)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                return View(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching suppliers");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching suppliers");
                return View();            }
        }

        // CREATE (GET)
        public IActionResult Create()
        {
            return View(new Supplier());
        }

        // CREATE (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    ModelState.AddModelError("", "You must be logged in to create a supplier.");
                    return RedirectToAction("Index", "Home");
                }

                supplier.CreatedBy = userId.Value;   
                supplier.CreatedAt = DateTime.Now;

                if (!ModelState.IsValid)
                    return View(supplier);

                _db.Suppliers.Add(supplier);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the supplier.");
                return View(supplier);
            }
        }

        // EDIT (GET)
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var supplier = await _db.Suppliers.FindAsync(id);
                if (supplier == null) return NotFound();

                return View(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching supplier for edit");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching the supplier");
                return RedirectToAction(nameof(Index));
            }
        }

        // EDIT 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier supplier)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(supplier);

                _db.Update(supplier);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the supplier");
                return View(supplier);
            }
        }

        // DELETE 
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var supplier = await _db.Suppliers.FindAsync(id);
                if (supplier == null) return NotFound();

                supplier.Active = false;
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the supplier");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
