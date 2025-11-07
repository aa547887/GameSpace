using GamiPort.Areas.Forum.Services.Forums;
using GamiPort.Areas.OnlineStore.DTO.Store;
using GamiPort.Areas.OnlineStore.Services.store.Abstractions;
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using GamiPort.Services.NewsApi;
using GamiPort.Models.NewsApi;

namespace GamiPort.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IForumsService _forumsService;
		private readonly IStoreService _storeService;
		private readonly NewsService _newsService;

		public HomeController(ILogger<HomeController> logger, IForumsService forumsService, IStoreService storeService, NewsService newsService)
		{
			_logger = logger;
			_forumsService = forumsService;
			_storeService = storeService;
			_newsService = newsService;
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

		public async Task<IActionResult> GetProductShowcase(string type = "latest")
		{
			const int take = 10;
			if (type == "clicks")
			{
				var items = await _storeService.GetRankings("click", "daily", take);
				return PartialView("_ProductShowcasePartial", items);
			}
			else
			{
				var query = new ProductQuery { sort = "newest", pageSize = take };
				var result = await _storeService.GetProducts(query, null, null);
				return PartialView("_ProductShowcasePartial", result.items);
			}
		}

		public async Task<IActionResult> GetLatestNews()
		{
			try
			{
				var articles = await _newsService.GetTopHeadlinesAsync(sources: "bbc-news", pageSize: 6);
				return PartialView("_LatestNewsPartial", articles);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching latest news from NewsAPI.");
				return PartialView("_LatestNewsPartial", new List<Article>());
			}
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