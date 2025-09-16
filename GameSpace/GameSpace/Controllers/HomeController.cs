using System.Diagnostics;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Controllers
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
			if (User.Identity?.IsAuthenticated == true && User.HasClaim(c => c.Type == "ManagerId"))
			{
				return RedirectToAction("Index", "AdminDashboard"); // 你的後台首頁
			}
			return View();
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
		public IActionResult Dashboard()
		{
			return View(); // 會自動找 Views/Home/Dashboard.cshtml
		}


	}
}
