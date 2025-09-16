using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.OnlineStore.Controllers
{
    [Area("OnlineStore")]
    public class OrderInfoesController : Controller
    {
        private readonly GameSpacedatabaseContext _dbContext;

        public OrderInfoesController(GameSpacedatabaseContext dbContext)
        {
			_dbContext = dbContext;
		}

		// GET: OnlineStore/OrderInfoes
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}

		// DataTables 伺服器端資料
		[HttpGet]
		public async Task<IActionResult> List()
		{
			// 1) DataTables 參數
			var draw = Request.Query["draw"].FirstOrDefault();
			int start = int.TryParse(Request.Query["start"], out var s) ? s : 0;
			int length = int.TryParse(Request.Query["length"], out var l) ? l : 10;
			string searchValue = Request.Query["search[value]"].FirstOrDefault() ?? string.Empty;

			int sortColIndex = int.TryParse(Request.Query["order[0][column]"], out var c) ? c : 0;
			string sortDir = Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";
			string sortColKey = Request.Query[$"columns[{sortColIndex}][data]"].FirstOrDefault() ?? "orderDate";

			var columnMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				["orderCode"] = nameof(OrderInfo.OrderCode),
				["userId"] = nameof(OrderInfo.UserId),
				["orderDate"] = nameof(OrderInfo.OrderDate),
				["orderStatus"] = nameof(OrderInfo.OrderStatus),
				["paymentStatus"] = nameof(OrderInfo.PaymentStatus),
				["orderTotal"] = nameof(OrderInfo.OrderTotal),
				["paymentAt"] = nameof(OrderInfo.PaymentAt),
				["shippedAt"] = nameof(OrderInfo.ShippedAt),
				["completedAt"] = nameof(OrderInfo.CompletedAt),
			};

			columnMap.TryGetValue(sortColKey, out var efSortCol);
			efSortCol ??= nameof(OrderInfo.OrderDate);

			// 2) 基礎查詢
			var query = _dbContext.OrderInfos.AsNoTracking();

			int recordsTotal = await query.CountAsync();

			// 3) 搜尋（注意可能為 null 的字串欄位）
			if (!string.IsNullOrWhiteSpace(searchValue))
			{
				query = query.Where(o =>
					o.OrderCode.ToString().Contains(searchValue) ||
					o.UserId.ToString().Contains(searchValue) ||
					(o.OrderStatus ?? "").Contains(searchValue) ||
					(o.PaymentStatus ?? "").Contains(searchValue)
				);
			}

			int recordsFiltered = await query.CountAsync();

			// 4) 排序
			query = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase)
				? query.OrderByDescending(e => EF.Property<object>(e, efSortCol))
				: query.OrderBy(e => EF.Property<object>(e, efSortCol));

			// 5) 分頁 + 投影
			var data = await query
				.Skip(start)
				.Take(length)
				.Select(o => new
				{
					o.OrderId,
					o.OrderCode,
					o.UserId,
					o.OrderDate,
					o.OrderStatus,
					o.PaymentStatus,
					o.OrderTotal,
					o.PaymentAt,
					o.ShippedAt,
					o.CompletedAt
				})
				.ToListAsync();

			// 6) 回傳 DataTables 格式
			return Json(new
			{
				draw,
				recordsTotal,
				recordsFiltered,
				data
			});
		}

		[HttpGet]
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();
			var orderInfo = await _dbContext.OrderInfos.FirstOrDefaultAsync(m => m.OrderId == id);
			if (orderInfo == null) return NotFound();
			return View(orderInfo);
		}


		// GET: OnlineStore/OrderInfoes/Create
		public IActionResult Create()
        {
            return View();
        }

        // POST: OnlineStore/OrderInfoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,OrderCode,UserId,OrderDate,OrderStatus,PaymentStatus,OrderTotal,PaymentAt,ShippedAt,CompletedAt")] OrderInfo orderInfo)
        {
            if (ModelState.IsValid)
            {
				_dbContext.Add(orderInfo);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(orderInfo);
        }

        // GET: OnlineStore/OrderInfoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderInfo = await _dbContext.OrderInfos.FindAsync(id);
            if (orderInfo == null)
            {
                return NotFound();
            }
            return View(orderInfo);
        }

        // POST: OnlineStore/OrderInfoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,OrderCode,UserId,OrderDate,OrderStatus,PaymentStatus,OrderTotal,PaymentAt,ShippedAt,CompletedAt")] OrderInfo orderInfo)
        {
            if (id != orderInfo.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					_dbContext.Update(orderInfo);
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderInfoExists(orderInfo.OrderId))
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
            return View(orderInfo);
        }

        // GET: OnlineStore/OrderInfoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderInfo = await _dbContext.OrderInfos
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (orderInfo == null)
            {
                return NotFound();
            }

            return View(orderInfo);
        }

        // POST: OnlineStore/OrderInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderInfo = await _dbContext.OrderInfos.FindAsync(id);
            if (orderInfo != null)
            {
				_dbContext.OrderInfos.Remove(orderInfo);
            }

            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderInfoExists(int id)
        {
            return _dbContext.OrderInfos.Any(e => e.OrderId == id);
        }
    }
}
