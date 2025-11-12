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

		#region Helpers for Fake Popularity
		private static uint HashSeed(string s)
		{
			uint h = 2166136261;
			for (int i = 0; i < s.Length; i++)
			{
				h ^= s[i];
				h *= 16777619;
			}
			return h;
		}

		private static int GetStableXorshift32Random(uint seed, int min, int max)
		{
			uint x = seed;
			x ^= x << 13;
			x ^= x >> 17;
			x ^= x << 5;
			double u = (double)x / 0xFFFFFFFF;
			return (int)Math.Floor(min + u * (max - min + 1));
		}
		#endregion

		public async Task<IActionResult> Index()
		{
			var allForums = await _forumsService.GetForumsAsync();

			// Calculate popularity and sort the entire list using unified xorshift32 logic
			var sortedForums = allForums.Select(forum => {
				var seed = HashSeed(forum.ForumId.ToString());
				var fakeViews = GetStableXorshift32Random(seed, 3000, 500000);
				var fakeThreads = GetStableXorshift32Random(seed * 7, 30, 800);
				var popularityScore = (fakeViews * 0.8) + (fakeThreads * 100 * 0.2);
				return new { Forum = forum, Popularity = popularityScore };
			})
			.OrderByDescending(x => x.Popularity)
			.Select(x => x.Forum);

			var featuredForums = sortedForums.Take(12);

			var pageSize = 4;
			var totalForums = featuredForums.Count();
			var totalPages = (int)Math.Ceiling(totalForums / (double)pageSize);
			var paginatedForums = featuredForums.Take(pageSize).ToList();

			ViewData["FeaturedForums"] = paginatedForums;
			ViewData["TotalPages"] = totalPages;
			ViewData["CurrentPage"] = 1;

			return View();
		}

		public async Task<IActionResult> _FeaturedForumsPartial(int page = 1)
		{
			var allForums = await _forumsService.GetForumsAsync();

			// Calculate popularity and sort the entire list using unified xorshift32 logic
			var sortedForums = allForums.Select(forum => {
				var seed = HashSeed(forum.ForumId.ToString());
				var fakeViews = GetStableXorshift32Random(seed, 3000, 500000);
				var fakeThreads = GetStableXorshift32Random(seed * 7, 30, 800);
				var popularityScore = (fakeViews * 0.8) + (fakeThreads * 100 * 0.2);
				return new { Forum = forum, Popularity = popularityScore };
			})
			.OrderByDescending(x => x.Popularity)
			.Select(x => x.Forum);

			var featuredForums = sortedForums.Take(12);

			var pageSize = 4;
			var totalForums = featuredForums.Count();
			var totalPages = (int)Math.Ceiling(totalForums / (double)pageSize);
			var paginatedForums = featuredForums.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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