using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROGP2.Data;
using PROGP2.Models;
using System.Diagnostics;

namespace PROGP2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Landing()
        {
            return View();
        }
        public IActionResult ClaimSuccess()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("UserName,Password")] User user)
        {
            var dbUser = await _context.Users
                .Where(u => u.UserName == user.UserName && u.Password == user.Password)
                .FirstOrDefaultAsync();

            if (dbUser == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            // Store user info in session
            HttpContext.Session.SetString("UserName", dbUser.UserName);
            HttpContext.Session.SetString("Role", dbUser.Role);
            HttpContext.Session.SetString("UserID", dbUser.UserID.ToString());

            // Redirect to landing page
            return RedirectToAction("Landing");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
