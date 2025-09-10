using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.OnlineStore.Controllers
{
	public class ProductInfosController : Controller
	{

		public GameSpacedatabaseContext _context;

		public ProductInfosController(GameSpacedatabaseContext context)
		{
			_context = context;
		}
		public IActionResult Index()
		{
			return View();
		}
		[HttpGet]
		public IActionResult Index(string? search, int page = 1, int pageSize = 10)
		{
			var query = _context.ProductInfos.AsQueryable();

			// 搜尋（依商品名稱或類型）
			if (!string.IsNullOrEmpty(search))
			{
				query = query.Where(p => p.ProductName.Contains(search) ||
										 p.ProductType.Contains(search));
			}

			// 分頁
			var totalCount = query.Count();
			var productInfos = query
							.OrderBy(p => p.ProductId)
							.Skip((page - 1) * pageSize)
							.Take(pageSize)
							.ToList();

			ViewBag.TotalCount = totalCount;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;
			ViewBag.Search = search;

			return View(productInfos);
		}




	}
}
