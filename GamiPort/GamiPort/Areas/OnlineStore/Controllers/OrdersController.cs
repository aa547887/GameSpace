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
using System.Text;
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

			// 只看自己的訂單（沿用你的權限邏輯）
			var userId = await _me.GetUserIdAsync();
			if (userId <= 0) return Challenge();

			// 關鍵字（模糊；LIKE 安全轉義）
			string kw = (keyword ?? "").Trim();
			bool hasKw = kw.Length > 0;
			static string EscapeLike(string s) =>
				s.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
			string kwLike = hasKw ? $"%{EscapeLike(kw)}%" : "";

			// ★ 日期上界：嚴格日區間（支援只填起日 = 查當天；起迄皆填 = [from, to+1)）
			DateTime? dateToNext = null;
			if (dateFrom.HasValue && !dateTo.HasValue)
				dateToNext = dateFrom.Value.Date.AddDays(1);
			else if (dateTo.HasValue)
				dateToNext = dateTo.Value.Date.AddDays(1);

			int rowStart = (page - 1) * pageSize + 1;
			int rowEnd = page * pageSize;

			await using var conn = (SqlConnection)_db.Database.GetDbConnection();
			if (conn.State != ConnectionState.Open) await conn.OpenAsync();

			var sql = @"
IF OBJECT_ID('tempdb..#filtered') IS NOT NULL DROP TABLE #filtered;

-- 以 order_date 輸出成 created_at（前端不需改欄位）
SELECT
    oi.order_id,
    oi.order_code,
    CAST(oi.order_date AS datetime) AS created_at,
    oi.user_id,
    oi.order_status,
    oi.payment_status,
    oi.grand_total
INTO #filtered
FROM dbo.SO_OrderInfoes AS oi
WHERE 1=1
  AND oi.user_id = @userId
  AND (@status IS NULL OR @status = N'' OR oi.order_status = @status)
  AND (
        @hasKw = 0
        OR oi.order_code LIKE @kwLike
        OR EXISTS (
            SELECT 1
            FROM dbo.SO_OrderItems i
            JOIN dbo.S_ProductInfo p ON p.product_id = i.product_id
            WHERE i.order_id = oi.order_id
              AND p.product_name LIKE @kwLike
        )
      )
  AND (@dateFrom  IS NULL OR oi.order_date >= @dateFrom)
  AND (@dateToNext IS NULL OR oi.order_date <  @dateToNext);

-- #1 清單（不動你的欄位/順序）
WITH R AS (
  SELECT f.*,
         ROW_NUMBER() OVER (ORDER BY f.order_id DESC) AS rn
  FROM #filtered f
)
SELECT
    r.order_id,
    r.order_code,
    r.created_at,
    r.order_status,
    r.payment_status,
    CAST(NULL AS nvarchar(50)) AS shipment_status,
    r.grand_total,
    N'信用卡' AS pay_method_name
FROM R r
WHERE r.rn BETWEEN @rowStart AND @rowEnd
ORDER BY r.rn;

-- #2 總筆數
SELECT COUNT(*) AS total_count FROM #filtered;

-- #3 狀態數
SELECT
    SUM(CASE WHEN f.order_status   = N'未付款' THEN 1 ELSE 0 END) AS cnt_unpaid,
    SUM(CASE WHEN f.payment_status = N'已付款' THEN 1 ELSE 0 END) AS cnt_paid,
    CAST(0 AS int) AS cnt_shipped,
    SUM(CASE WHEN f.order_status   = N'已完成' THEN 1 ELSE 0 END) AS cnt_completed,
    SUM(CASE WHEN f.order_status   = N'已取消' THEN 1 ELSE 0 END) AS cnt_canceled
FROM #filtered f;
";

			await using var cmd = new SqlCommand(sql, conn);
			cmd.Parameters.Add(new SqlParameter("@userId", SqlDbType.Int) { Value = userId });
			cmd.Parameters.Add(new SqlParameter("@status", SqlDbType.NVarChar, 30) { Value = (object?)vm.Status ?? DBNull.Value });
			cmd.Parameters.Add(new SqlParameter("@hasKw", SqlDbType.Bit) { Value = hasKw });
			cmd.Parameters.Add(new SqlParameter("@kwLike", SqlDbType.NVarChar, 200) { Value = hasKw ? (object)kwLike : DBNull.Value });
			cmd.Parameters.Add(new SqlParameter("@dateFrom", SqlDbType.DateTime2) { Value = (object?)vm.DateFrom?.Date ?? DBNull.Value });
			cmd.Parameters.Add(new SqlParameter("@dateToNext", SqlDbType.DateTime2) { Value = (object?)dateToNext ?? DBNull.Value });
			cmd.Parameters.Add(new SqlParameter("@rowStart", SqlDbType.Int) { Value = rowStart });
			cmd.Parameters.Add(new SqlParameter("@rowEnd", SqlDbType.Int) { Value = rowEnd });

			var items = new List<OrdersListItemVm>();
			await using var rd = await cmd.ExecuteReaderAsync();

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
					PayStatus = rd.IsDBNull(rd.GetOrdinal("payment_status")) ? null : rd.GetString(rd.GetOrdinal("payment_status")),
					PayMethod = rd.IsDBNull(rd.GetOrdinal("pay_method_name")) ? null : rd.GetString(rd.GetOrdinal("pay_method_name")),
					GrandTotal = rd.IsDBNull(rd.GetOrdinal("grand_total")) ? 0m : rd.GetDecimal(rd.GetOrdinal("grand_total"))
				});
			}

			await rd.NextResultAsync();
			if (await rd.ReadAsync())
				vm.TotalCount = rd.GetInt32(0);

			await rd.NextResultAsync();
			if (await rd.ReadAsync())
			{
				vm.CntUnpaid = rd.IsDBNull(0) ? 0 : rd.GetInt32(0);
				vm.CntPaid = rd.IsDBNull(1) ? 0 : rd.GetInt32(1);
				vm.CntShipped = rd.IsDBNull(2) ? 0 : rd.GetInt32(2);
				vm.CntCompleted = rd.IsDBNull(3) ? 0 : rd.GetInt32(3);
				vm.CntCanceled = rd.IsDBNull(4) ? 0 : rd.GetInt32(4);

				vm.UnpaidCount = vm.CntUnpaid;
				vm.PaidCount = vm.CntPaid;
				vm.ShippedCount = vm.CntShipped;
				vm.CompletedCount = vm.CntCompleted;
				vm.CanceledCount = vm.CntCanceled;
			}

			vm.Items.AddRange(items);

			// 這裡維持你的 AJAX/Partial 行為，不動
			if (string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
				return PartialView("_OrdersListPartial", vm);

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
