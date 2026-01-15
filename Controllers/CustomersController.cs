using InventoryManagementSystem.Data;
using InventoryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers
{
    public class CustomersController(InventoryDbContext _db,ILogger<CustomersController> _logger) : Controller
    {
       

        // LIST: GET /Customers
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _db.Customers
                    .Where(c => c.Active) 
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customers");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching customers.");
                return View(new List<Customer>());
            }
        }

        // CREATE: GET /Customers/Create
        public IActionResult Create()
        {
            return View(new Customer());
        }

        // CREATE: POST /Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return RedirectToAction("Index", "Home");

                customer.CreatedBy = userId.Value;
                customer.CreatedAt = DateTime.Now;

                if (!ModelState.IsValid)
                    return View(customer);

                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the customer.");
                return View(customer);
            }
        }

        // EDIT: GET /Customers/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var customer = await _db.Customers.FindAsync(id);
                if (customer == null) return NotFound();

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer for edit");
                ModelState.AddModelError(string.Empty, "An error occurred while fetching the customer.");
                return RedirectToAction(nameof(Index));
            }
        }

        // EDIT: POST /Customers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(customer);

                _db.Update(customer);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the customer.");
                return View(customer);
            }
        }

        // DELETE: POST /Customers/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customer = await _db.Customers.FindAsync(id);
                if (customer == null) return NotFound();

                _db.Customers.Remove(customer); // or set Active=false if you want soft delete
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the customer.");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
