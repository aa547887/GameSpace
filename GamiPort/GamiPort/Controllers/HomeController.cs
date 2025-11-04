using System.Diagnostics;
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using GamiPort.Areas.Forum.Services.Forums;
using GamiPort.Areas.Forum.Dtos.Forum;

namespace GamiPort.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IForumsService _forumsService;

        public HomeController(ILogger<HomeController> logger, IForumsService forumsService)
        {
            _logger = logger;
            _forumsService = forumsService;
        }

        public async Task<IActionResult> Index()
        {
            var forums = (await _forumsService.GetForumsAsync()).Take(12);
            var pageSize = 4;
            var totalForums = forums.Count();
            var totalPages = (int)Math.Ceiling(totalForums / (double)pageSize);
            var paginatedForums = forums.Take(pageSize).ToList();

            ViewData["FeaturedForums"] = paginatedForums;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = 1;

            return View();
        }

        public async Task<IActionResult> _FeaturedForumsPartial(int page = 1)
        {
            var forums = (await _forumsService.GetForumsAsync()).Take(12);
            var pageSize = 4;
            var totalForums = forums.Count();
            var totalPages = (int)Math.Ceiling(totalForums / (double)pageSize);
            var paginatedForums = forums.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewData["FeaturedForums"] = paginatedForums;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return PartialView("_FeaturedForumsPartial");
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
