using _301222912_abraham_mehta_Lab3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace _301222912_abraham_mehta_Lab3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        /*public IActionResult Privacy()
        {
            return View();
        }*/

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult LogOut()
        {
            Response.Cookies.Delete("userId");
            Response.Cookies.Delete("MovieId");
            Response.Cookies.Delete("Firstname");
            Response.Cookies.Delete("commentid");
            return RedirectToAction("Index", "Home");
        }
    }
}