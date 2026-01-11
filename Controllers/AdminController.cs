using Microsoft.AspNetCore.Mvc;

namespace InventoryManagementSystem.Controllers;

public class AdminController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}