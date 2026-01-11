using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers;

public class DashboardController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}