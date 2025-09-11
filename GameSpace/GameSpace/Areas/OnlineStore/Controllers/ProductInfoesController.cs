using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.OnlineStore.ViewModels;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class ProductInfoesController : Controller
    {
        private readonly GameSpacedatabaseContext _context;

        public ProductInfoesController(GameSpacedatabaseContext context)
        {
            _context = context; //Entity Framework Core (DbContext)存取資料表
        }

		//Index → 看全部    //Details → 看一筆	  //Create → 新增		//Edit → 修改		//Delete → 下架
		//複雜版
		//[HttpGet]//OnlineStore/ProductInfos?search=&type=&status=active|inactive|all&sort=created_desc&pagesize=10&page=1
		//public async Task<IActionResult> Index(
		//	string? search,
		//	string? type,
		//	string status = "active",
		//	string sort = "created_desc",
		//	int page = 1,
		//	int pageSize = 10)
		//{
		//	// 1) 基礎查詢（唯讀更快）
		//	var q = _context.ProductInfos
		//					.AsNoTracking()
		//					.AsQueryable();

		//	// 2) 搜尋 / 篩選
		//	if (!string.IsNullOrWhiteSpace(search))
		//		q = q.Where(p => p.ProductName.Contains(search) ||
		//						 p.ProductType.Contains(search));

		//	if (!string.IsNullOrWhiteSpace(type))
		//		q = q.Where(p => p.ProductType == type);

		//	if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
		//	{
		//		bool isActive = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
		//		q = q.Where(p => p.IsActive == isActive);
		//	}

		//	// 3) 排序
		//	q = sort switch
		//	{
		//		"name_asc" => q.OrderBy(p => p.ProductName),
		//		"name_desc" => q.OrderByDescending(p => p.ProductName),
		//		"price_asc" => q.OrderBy(p => p.Price),
		//		"price_desc" => q.OrderByDescending(p => p.Price),
		//		"updated_asc" => q.OrderBy(p => p.ProductUpdatedAt),
		//		"updated_desc" => q.OrderByDescending(p => p.ProductUpdatedAt),
		//		"created_asc" => q.OrderBy(p => p.ProductCreatedAt),
		//		_ => q.OrderByDescending(p => p.ProductCreatedAt) // created_desc
		//	};

		//	// 4) 先算總數（分頁用）
		//	var totalCount = await q.CountAsync();

		//	// 5) 投影到清單列 ViewModel（含「最新異動紀錄」子查詢）
		//	var rows = await q
		//		.Skip((page - 1) * pageSize)
		//		.Take(pageSize)
		//		.Select(p => new ProductIndexRowVM
		//		{
		//			ProductId = p.ProductId,
		//			ProductName = p.ProductName,
		//			ProductType = p.ProductType,
		//			Price = p.Price,
		//			CurrencyCode = p.CurrencyCode,
		//			ShipmentQuantity = p.ShipmentQuantity,
		//			IsActive = p.IsActive,
		//			ProductCreatedAt = p.ProductCreatedAt,
		//			ProductUpdatedAt = p.ProductUpdatedAt,
		//			// 如果你的 Scaffold 有這兩個導航屬性：ProductCreatedByNavigation / ProductUpdatedByNavigation
		//			// 下面可改成 ManagerName（若你有 manager_name 欄位）或先顯示 ManagerId
		//			CreatedByManagerId = p.ProductCreatedByNavigation != null ? p.ProductCreatedByNavigation.ManagerId : (int?)null,
		//			UpdatedByManagerId = p.ProductUpdatedByNavigation != null ? p.ProductUpdatedByNavigation.ManagerId : (int?)null,

		//			LastLog = _context.ProductInfoAuditLogs
		//				.Where(a => a.ProductId == p.ProductId)
		//				.OrderByDescending(a => a.ChangedAt)
		//				.Select(a => new LastLogDto
		//				{
		//					LogId = a.LogId,
		//					ManagerId = a.ManagerId,
		//					ChangedAt = a.ChangedAt
		//				})
		//				.FirstOrDefault()
		//		})
		//		.ToListAsync();

		//	// 6) 提供下拉用的類型清單
		//	var types = await _context.ProductInfos
		//							  .AsNoTracking()
		//							  .Select(p => p.ProductType)
		//							  .Distinct()
		//							  .OrderBy(s => s)
		//							  .ToListAsync();

		//	ViewBag.TotalCount = totalCount;
		//	ViewBag.Page = page;
		//	ViewBag.PageSize = pageSize;
		//	ViewBag.Search = search;
		//	ViewBag.Sort = sort;
		//	ViewBag.Status = status;
		//	ViewBag.Type = type;
		//	ViewBag.TypeList = types;

		//	return View(rows);
		//}
		//基本版
		[HttpGet]
		public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 10)
		{
			// 1) 基本查詢
			var query = _context.ProductInfos.AsQueryable();

			// 2) 搜尋（依名稱或類型）
			if (!string.IsNullOrEmpty(search))
			{
				query = query.Where(p =>
					p.ProductName.Contains(search) ||
					p.ProductType.Contains(search));
			}

			// 3) 總筆數
			var totalCount = await query.CountAsync();

			// 4) 撈資料（分頁 + 投影成 ViewModel）
			var products = await query
				.OrderByDescending(p => p.ProductCreatedAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Select(p => new ProductIndexRowVM
				{
					ProductId = p.ProductId,
					ProductName = p.ProductName,
					ProductType = p.ProductType,
					Price = p.Price,
					ShipmentQuantity = p.ShipmentQuantity,
					IsActive = p.IsActive,
					ProductCreatedAt = p.ProductCreatedAt,
					ProductUpdatedAt = p.ProductUpdatedAt,

					LastLog = _context.ProductInfoAuditLogs
						.Where(a => a.ProductId == p.ProductId)
						.OrderByDescending(a => a.ChangedAt)
						.Select(a => new LastLogDto
						{
							LogId = a.LogId,
							ManagerId = a.ManagerId,
							ChangedAt = a.ChangedAt
						})
						.FirstOrDefault()
				})
				.ToListAsync();

			// 5) 分頁資訊
			ViewBag.TotalCount = totalCount;
			ViewBag.Page = page;
			ViewBag.PageSize = pageSize;
			ViewBag.Search = search;

			return View(products);
		}




		//// GET: OnlineStore/ProductInfoes  //自動生成版
		//public async Task<IActionResult> Index()
		//{
		//    var gameSpacedatabaseContext = _context.ProductInfos
		//        .Include(p => p.ProductCreatedByNavigation)   //載入同時紀錄誰建立商品
		//        .Include(p => p.ProductUpdatedByNavigation);  //載入同時紀錄誰最後更新商品
		//    return View(await gameSpacedatabaseContext.ToListAsync()); //非同步轉換成 List執行查詢並取得所有資料

		//}


		// GET: OnlineStore/ProductInfoes/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productInfo = await _context.ProductInfos
                .Include(p => p.ProductCreatedByNavigation)
                .Include(p => p.ProductUpdatedByNavigation)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (productInfo == null)
            {
                return NotFound();
            }

            return View(productInfo);
        }

        // GET: OnlineStore/ProductInfoes/Create
        public IActionResult Create()
        {
            ViewData["ProductCreatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId");
            ViewData["ProductUpdatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId");
            return View();
        }

        // POST: OnlineStore/ProductInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,ProductType,Price,CurrencyCode,ShipmentQuantity,ProductCreatedBy,ProductCreatedAt,ProductUpdatedBy,ProductUpdatedAt,IsActive")] ProductInfo productInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productInfo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductCreatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductCreatedBy);
            ViewData["ProductUpdatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductUpdatedBy);
            return View(productInfo);
        }

        // GET: OnlineStore/ProductInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productInfo = await _context.ProductInfos.FindAsync(id);
            if (productInfo == null)
            {
                return NotFound();
            }
            ViewData["ProductCreatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductCreatedBy);
            ViewData["ProductUpdatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductUpdatedBy);
            return View(productInfo);
        }

        // POST: OnlineStore/ProductInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,ProductType,Price,CurrencyCode,ShipmentQuantity,ProductCreatedBy,ProductCreatedAt,ProductUpdatedBy,ProductUpdatedAt,IsActive")] ProductInfo productInfo)
        {
            if (id != productInfo.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productInfo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductInfoExists(productInfo.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductCreatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductCreatedBy);
            ViewData["ProductUpdatedBy"] = new SelectList(_context.ManagerData, "ManagerId", "ManagerId", productInfo.ProductUpdatedBy);
            return View(productInfo);
        }

        // GET: OnlineStore/ProductInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productInfo = await _context.ProductInfos
                .Include(p => p.ProductCreatedByNavigation)
                .Include(p => p.ProductUpdatedByNavigation)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (productInfo == null)
            {
                return NotFound();
            }

            return View(productInfo);
        }

        // POST: OnlineStore/ProductInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productInfo = await _context.ProductInfos.FindAsync(id);
            if (productInfo != null)
            {
                _context.ProductInfos.Remove(productInfo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductInfoExists(int id)
        {
            return _context.ProductInfos.Any(e => e.ProductId == id);
        }
    }
}
