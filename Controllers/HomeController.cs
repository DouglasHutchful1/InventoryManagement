using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using InventoryManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using InventoryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Controllers;

public class HomeController(ILogger<HomeController> logger,InventoryDbContext dbcon) : Controller
{
    

    public IActionResult Index()
    {
        return View();
    }

    //login  method
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        try
        {
            var user = await dbcon.User.FirstOrDefaultAsync(u =>
                u.Username == username || u.Email == username);

            if (user == null)
                return Unauthorized(new { success = false, message = "User not found" });

            if (!PasswordHelper.VerifyPassword(password, user.Password))
                return Unauthorized(new { success = false, message = "Wrong password" });

            if (!user.Active)
                return Unauthorized(new { success = false, message = "User is inactive" });

            // store session values 
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetInt32("UserType", user.UserType); 

            string redirectUrl = user.UserType == 1 
                ? Url.Action("Index", "Admin")      
                : Url.Action("Index", "Dashboard");
            if (redirectUrl == null)
            {
                Url.Action("Index", "Home");
            }

            return Ok(new 
            {
                success = true,
                message = "Login Successful",
                username = user.Username,
                redirect = redirectUrl
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login error");
            return StatusCode(500, new { success = false, message = "Unexpected error occurred" });
        }
    }
    
    //method for handling registration
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] User requestDto)
    {
        try
        {

            if (await dbcon.User.AnyAsync(u => u.Username == requestDto.Username))
                return BadRequest(new { message = "Username already exists" });

            if (await dbcon.User.AnyAsync(u => u.Email == requestDto.Email))
                return BadRequest(new { message = "Email already exists" });
            if (requestDto.Password != requestDto.ConfirmPassword)
                return BadRequest(new { success = false, message = "Passwords do not match" });

            var user = new User
            {

                Firstname = requestDto.Firstname,
                Surname = requestDto.Surname,
                Email = requestDto.Email,
                Username = requestDto.Username,
                Password = PasswordHelper.HashPassword(requestDto.Password),
                Active = true,
                UserType = 0, //default 0 as user
                CreationDate = DateTime.UtcNow
            };
            dbcon.User.Add(user);
            await dbcon.SaveChangesAsync();

            return Ok(new { success = true, message = "Registration was successful", user.Id });
        }
        catch (Exception e)
        {
            logger.LogError(e,"Error occured");
            return StatusCode(500, new { message = "something happened,please try again later" });
        }
        
    }
    
    private class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public static bool VerifyPassword(string inputPass, string storedHash)
        {
            return HashPassword(inputPass) == storedHash;
        }
    }
    // Get current user info
    [HttpGet]
    public IActionResult GetCurrentUser()
    {
        var username = HttpContext.Session.GetString("Username");
        var email = HttpContext.Session.GetString("Email");

        if (!string.IsNullOrEmpty(username))
        {
            return Json(new { success = true, isLoggedIn = true, username ,email});
        }
        return Json(new { success = true, isLoggedIn = false });
    }
    
    // logout 
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged Out" });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}