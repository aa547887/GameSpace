// Areas/OnlineStore/Controllers/OrdersController.cs
using GamiPort.Areas.OnlineStore.ViewModels;
using GamiPort.Infrastructure.Security;
using GamiPort.Models;                              // GameSpacedatabaseContext
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;                 // Database.GetDbConnection
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	[Route("OnlineStore/[controller]")]
	public sealed class OrdersController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IConfiguration _cfg;
		private readonly IAppCurrentUser _me; // ★ 新增
		public OrdersController(GameSpacedatabaseContext db, IConfiguration cfg, IAppCurrentUser me)
		{
			_db = db;
			_cfg = cfg;
			_me = me;
		}

		// GET + POST（AJAX 分頁/查詢會用 POST 並加 X-Requested-With）
		[HttpGet("")]
		[HttpPost("")]
		public async Task<IActionResult> Index(
			string? status,
			string? keyword,
			DateTime? dateFrom,
			DateTime? dateTo,
			int page = 1,
			int pageSize = 10)
		{
			if (page <= 0) page = 1;
			if (pageSize <= 0) pageSize = 10;

			var vm = new OrdersListVm
			{
				Status = status ?? "",
				Keyword = keyword ?? "",
				DateFrom = dateFrom,
				DateTo = dateTo,
				Page = page,
				PageSize = pageSize
			};

			// ── 安全模式 SQL：不依賴 DB 中可能不存在的欄位
			var sql = @"
IF OBJECT_ID('tempdb..#filtered') IS NOT NULL DROP TABLE #filtered;

-- 只投影一定存在的欄位，避免 Invalid column name
SELECT
    oi.order_id,
    oi.order_code,
    oi.user_id,
    oi.order_status,
    oi.payment_status,
    oi.grand_total
INTO #filtered
FROM dbo.SO_OrderInfoes AS oi
WHERE
	oi.user_id = @userId AND                                      -- ★ 關鍵守門：只能看自己的
    (@status  IS NULL OR @status = N'' OR oi.order_status = @status) AND
    (
        @keyword IS NULL OR @keyword = N'' OR
        oi.order_code LIKE N'%' + @keyword + N'%' OR
        CAST(oi.order_id AS nvarchar(20)) = @keyword
    );
    -- 暫不做 created_at 的日期篩選（你的 DB 目前沒有該欄位）

-- 結果集 #1：分頁清單（把缺欄位以 NULL 別名補上，前端/Reader 能正常取欄位）
SELECT
    f.order_id,
    f.order_code,
    CAST(NULL AS datetime)      AS created_at,        -- 你的 DB 暫無此欄位
    f.order_status,
    f.payment_status,
    CAST(NULL AS nvarchar(50))  AS shipment_status,   -- 暫無：已出貨狀態欄
    f.grand_total,
    CAST(NULL AS nvarchar(100)) AS pay_method_name    -- 暫無：付款方式名稱
FROM #filtered AS f
ORDER BY f.order_id DESC
OFFSET (@page - 1) * @pageSize ROWS FETCH NEXT @pageSize ROWS ONLY;

-- 結果集 #2：總筆數
SELECT COUNT(*) AS total_count FROM #filtered;

-- 結果集 #3：五種狀態數量（出貨數先為 0，其餘用現有欄位）
SELECT
    SUM(CASE WHEN f.order_status   = N'未付款' THEN 1 ELSE 0 END) AS cnt_unpaid,
    SUM(CASE WHEN f.payment_status = N'已付款' THEN 1 ELSE 0 END) AS cnt_paid,
    CAST(0 AS int) AS cnt_shipped,
    SUM(CASE WHEN f.order_status   = N'已完成' THEN 1 ELSE 0 END) AS cnt_completed,
    SUM(CASE WHEN f.order_status   = N'已取消' THEN 1 ELSE 0 END) AS cnt_canceled
FROM #filtered AS f;
";

			await using var conn = _db.Database.GetDbConnection();
			if (conn.State != ConnectionState.Open)
				await conn.OpenAsync();

			await using var cmd = conn.CreateCommand();
			cmd.CommandText = sql;

			var userId = await _me.GetUserIdAsync();
			if (userId <= 0) return Challenge();
			cmd.Parameters.AddRange(new[]
			{
				new SqlParameter("@userId",   System.Data.SqlDbType.Int) { Value = userId }, // ★ 新增
				new SqlParameter("@status",   (object?)status   ?? DBNull.Value),
				new SqlParameter("@keyword",  (object?)keyword  ?? DBNull.Value),
				new SqlParameter("@dateFrom", (object?)dateFrom ?? DBNull.Value),
				new SqlParameter("@dateTo",   (object?)dateTo   ?? DBNull.Value),
				new SqlParameter("@page",     page),
				new SqlParameter("@pageSize", pageSize),
			});

			var items = new List<OrdersListItemVm>();
			await using var rd = await cmd.ExecuteReaderAsync();

			// #1 清單
			while (await rd.ReadAsync())
			{
				items.Add(new OrdersListItemVm
				{
					OrderId = rd.GetInt32(rd.GetOrdinal("order_id")),
					OrderCode = rd.GetString(rd.GetOrdinal("order_code")),
					CreatedAt = rd.IsDBNull(rd.GetOrdinal("created_at")) ? (DateTime?)null : rd.GetDateTime(rd.GetOrdinal("created_at")),
					OrderStatus = rd.IsDBNull(rd.GetOrdinal("order_status")) ? null : rd.GetString(rd.GetOrdinal("order_status")),
					PaymentStatus = rd.IsDBNull(rd.GetOrdinal("payment_status")) ? null : rd.GetString(rd.GetOrdinal("payment_status")),
					ShipmentStatus = rd.IsDBNull(rd.GetOrdinal("shipment_status")) ? null : rd.GetString(rd.GetOrdinal("shipment_status")),
					PayStatus = rd.IsDBNull(rd.GetOrdinal("payment_status")) ? null : rd.GetString(rd.GetOrdinal("payment_status")), // 舊命名相容
					PayMethod = rd.IsDBNull(rd.GetOrdinal("pay_method_name")) ? null : rd.GetString(rd.GetOrdinal("pay_method_name")),
					GrandTotal = rd.IsDBNull(rd.GetOrdinal("grand_total")) ? 0m : rd.GetDecimal(rd.GetOrdinal("grand_total"))
				});
			}

			// #2 總筆數
			await rd.NextResultAsync();
			if (await rd.ReadAsync())
				vm.TotalCount = rd.GetInt32(0);

			// #3 狀態數
			await rd.NextResultAsync();
			if (await rd.ReadAsync())
			{
				// 依照結構：unpaid / paid / shipped(固定0) / completed / canceled
				vm.CntUnpaid = rd.IsDBNull(0) ? 0 : rd.GetInt32(0);
				vm.CntPaid = rd.IsDBNull(1) ? 0 : rd.GetInt32(1);
				vm.CntShipped = rd.IsDBNull(2) ? 0 : rd.GetInt32(2);
				vm.CntCompleted = rd.IsDBNull(3) ? 0 : rd.GetInt32(3);
				vm.CntCanceled = rd.IsDBNull(4) ? 0 : rd.GetInt32(4);

				// 若你的 Index.cshtml 用的是 UnpaidCount/PaidCount... 也一併對應
				vm.UnpaidCount = vm.CntUnpaid;
				vm.PaidCount = vm.CntPaid;
				vm.ShippedCount = vm.CntShipped;
				vm.CompletedCount = vm.CntCompleted;
				vm.CanceledCount = vm.CntCanceled;
			}

			vm.Items.AddRange(items);

			// AJAX：只回列表部分
			if (string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
			{
				return PartialView("_OrdersListPartial", vm);
			}

			return View("Index", vm);
		}

		// 給前端 stepper 查狀態（移除 shipment_status 依賴）
		[HttpGet("status/{orderCode}")]
		public async Task<IActionResult> Status(string orderCode)
		{
			const string sql = @"
SELECT TOP(1)
    order_status,
    payment_status
FROM dbo.SO_OrderInfoes
WHERE order_code = @orderCode AND user_id = @userId  -- ★ 只允許看自己的單
ORDER BY order_id DESC;";

			await using var conn = _db.Database.GetDbConnection();
			if (conn.State != ConnectionState.Open)
				await conn.OpenAsync();

			await using var cmd = conn.CreateCommand();
			cmd.CommandText = sql;

			var userId = await _me.GetUserIdAsync();
			if (userId <= 0) return Unauthorized();
			cmd.Parameters.Add(new SqlParameter("@orderCode", orderCode));
			cmd.Parameters.Add(new SqlParameter("@userId", userId)); // ★ 新增

			await using var rd = await cmd.ExecuteReaderAsync();
			if (await rd.ReadAsync())
			{
				var orderStatus = rd.IsDBNull(0) ? null : rd.GetString(0);
				var paymentStatus = rd.IsDBNull(1) ? null : rd.GetString(1);

				// 無 shipment_status；以 order/payment 推斷
				string key = "unpaid";
				if (string.Equals(orderStatus, "已取消")) key = "canceled";
				else if (string.Equals(orderStatus, "已完成")) key = "completed";
				else if (string.Equals(paymentStatus, "已付款")) key = "paid";
				// 沒有已出貨判斷 → 不會回 shipped

				return Json(new { statusKey = key });
			}

			return Json(new { statusKey = "unpaid" });
		}
		// 伸縮框：載入指定訂單的商品明細
		[HttpGet("Items")]
		public async Task<IActionResult> Items(string orderCode)
		{
			// === 0) 參數檢查 ===
			if (string.IsNullOrWhiteSpace(orderCode))
				return BadRequest("orderCode is required.");

			// === 1) 取得目前登入者 ===
			// 說明：
			// - 假設本 Controller 已經注入 IAppCurrentUser _me（你前面動作已完成）
			// - 若尚未注入，可改為自行從 Claims 解析（略）
			var userId = await _me.GetUserIdAsync();
			if (userId <= 0) return Unauthorized(); // 未登入

			// === 2) 先確認這張訂單屬於目前使用者，並取得 orderId ===
			var ownedOrder = await _db.SoOrderInfoes
				.AsNoTracking()
				.Where(o => o.OrderCode == orderCode && o.UserId == userId) // ★ 關鍵：只能看自己的訂單
				.Select(o => new { o.OrderId })
				.FirstOrDefaultAsync();

			if (ownedOrder is null)
			{
				// 找不到該單或不是自己的單
				return NotFound("Order not found or access denied.");
			}

			var orderId = ownedOrder.OrderId;

			// === 3) 查詢明細（僅針對這張屬於自己的訂單） ===
			// oi → it → p → (left join) pc 取 product_code
			var q =
				from it in _db.SoOrderItems.AsNoTracking()
				where it.OrderId == orderId
				join p in _db.SProductInfos.AsNoTracking() on it.ProductId equals p.ProductId
				join pc0 in _db.SProductCodes.AsNoTracking() on p.ProductId equals pc0.ProductId into pcJoin
				from pc in pcJoin.DefaultIfEmpty() // left join，避免沒有代碼時擋住
				orderby it.LineNo
				select new GamiPort.Areas.OnlineStore.ViewModels.OrderItemRowVm
				{
					ProductCode = pc != null ? pc.ProductCode : null, // 若沒代碼可改成 $"PID-{p.ProductId}"
					ProductName = p.ProductName,
					UnitPrice = it.UnitPrice,
					Quantity = it.Quantity,
					Subtotal = it.UnitPrice * it.Quantity
				};

			var list = await q.ToListAsync();
			return PartialView("_OrderItems", list);
		}
	}
}
