using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Warehouse_23DH111747.Models;

namespace Warehouse_23DH111747.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Products");
        }

        public IActionResult Privacy()
        {
            return RedirectToAction("Statistics", "Products");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
