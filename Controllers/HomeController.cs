using Microsoft.AspNetCore.Mvc;

namespace SmartGearOnline.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Home page view
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
