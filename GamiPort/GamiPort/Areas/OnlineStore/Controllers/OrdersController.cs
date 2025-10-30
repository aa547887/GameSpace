// Areas/OnlineStore/Controllers/OrdersController.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;                 // Database.GetConnectionString()
using Microsoft.Extensions.Configuration;           // IConfiguration
using GamiPort.Models;                              // GameSpacedatabaseContext
using GamiPort.Infrastructure.Security;             // IAppCurrentUser
using GamiPort.Areas.OnlineStore.ViewModels;        // OrdersListVm / OrderDetailVm

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	[Route("OnlineStore/[controller]")]
	public sealed class OrdersController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IAppCurrentUser _me;
		private readonly IConfiguration _cfg;
		private readonly string _connString;

		public OrdersController(GameSpacedatabaseContext db, IAppCurrentUser me, IConfiguration cfg)
		{
			_db = db;
			_me = me;
			_cfg = cfg;

			_connString = _db.Database.GetConnectionString()
				?? _cfg.GetConnectionString("GameSpace")
				?? throw new InvalidOperationException("找不到資料庫連線字串，請確認 Program.cs 或 appsettings.json 已設定。");
		}

		// GET /OnlineStore/Orders
		[HttpGet("")]
		public async Task<IActionResult> Index(string? status, string? keyword, DateTime? dateFrom, DateTime? dateTo, int page = 1, int pageSize = 10)
		{
			var userId = _me.UserId;
			if (userId <= 0) userId = _cfg.GetValue<int?>("Demo:UserId") ?? 0;
			if (userId <= 0)
			{
				TempData["Toast"] = "Demo 模式未設定 Demo:UserId。";
				return RedirectToAction("Index", "Home", new { area = "OnlineStore" });
			}

			var vm = new OrdersListVm
			{
				Status = status,
				Keyword = keyword,
				PageIndex = page,
				PageSize = pageSize
			};

			await using var conn = new SqlConnection(_connString);
			await conn.OpenAsync();

			// ② 列表（分頁）
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "dbo.usp_MyOrders_ListByUser";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@UserId", userId);
				cmd.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@Keyword", (object?)keyword ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@DateFrom", (object?)dateFrom ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@DateTo", (object?)dateTo ?? DBNull.Value);
				cmd.Parameters.AddWithValue("@PageIndex", page);
				cmd.Parameters.AddWithValue("@PageSize", pageSize);

				await using var rd = await cmd.ExecuteReaderAsync();
				while (await rd.ReadAsync())
				{
					vm.Items.Add(new OrdersListItemVm
					{
						OrderId = rd.GetInt32(rd.GetOrdinal("order_id")),
						OrderCode = rd.GetString(rd.GetOrdinal("order_code")),
						CreatedAt = rd.GetDateTime(rd.GetOrdinal("created_at")),
						GrandTotal = rd.GetDecimal(rd.GetOrdinal("grand_total")),
						// ★ 重點：你的 SP 輸出的是 status_text / status_key
						StatusText = TryGet<string>(rd, "status_text") ?? "",
						StatusKey = TryGet<string>(rd, "status_key") ?? "",
						PayMethod = TryGet<string>(rd, "pay_method") ?? "",
						PayStatus = TryGet<string>(rd, "pay_status") ?? ""
					});
				}
				if (await rd.NextResultAsync() && await rd.ReadAsync())
				{
					vm.TotalCount = rd.GetInt32(rd.GetOrdinal("total_count"));
				}
			}

			// ③ Dashboard（各狀態數量）
			await using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = "dbo.usp_MyOrders_Dashboard";
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@UserId", userId);

				await using var rd = await cmd.ExecuteReaderAsync();
				if (await rd.ReadAsync())
				{
					vm.UnpaidCount = TryGet<int>(rd, "UnpaidCount");
					vm.PaidCount = TryGet<int>(rd, "PaidCount");
					vm.ShippedCount = TryGet<int>(rd, "ShippedCount");
					vm.CompletedCount = TryGet<int>(rd, "CompletedCount");
					vm.CanceledCount = TryGet<int>(rd, "CanceledCount");
				}
			}

			return View(vm);
		}

		// GET /OnlineStore/Orders/{orderCode}
		[HttpGet("{orderCode}")]
		public async Task<IActionResult> Detail(string orderCode)
		{
			var userId = _me.UserId;
			if (userId <= 0) userId = _cfg.GetValue<int?>("Demo:UserId") ?? 0;
			if (userId <= 0)
			{
				TempData["Toast"] = "Demo 模式未設定 Demo:UserId。";
				return RedirectToAction("Index", "Home", new { area = "OnlineStore" });
			}

			var vm = new OrderDetailVm();

			await using var conn = new SqlConnection(_connString);
			await conn.OpenAsync();

			await using var cmd = conn.CreateCommand();
			cmd.CommandText = "dbo.usp_MyOrders_GetDetail";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.AddWithValue("@UserId", userId);
			cmd.Parameters.AddWithValue("@OrderCode", orderCode);

			await using var rd = await cmd.ExecuteReaderAsync();

			// A) Head
			if (await rd.ReadAsync())
			{
				vm.Head = new OrderHeadVm
				{
					OrderId = TryGet<int>(rd, "order_id"),
					OrderCode = TryGet<string>(rd, "order_code") ?? "",
					StatusText = TryGet<string>(rd, "status")
								 ?? TryGet<string>(rd, "order_status") ?? "",
					CreatedAt = TryGet<DateTime?>(rd, "created_at")
								 ?? TryGet<DateTime?>(rd, "order_date")
								 ?? DateTime.MinValue,
					GrandTotal = TryGet<decimal?>(rd, "grand_total") ?? 0m,
					Recipient = TryGet<string>(rd, "recipient"),
					Phone = TryGet<string>(rd, "phone"),
					Address = BuildAddress(rd)
				};
			}
			else
			{
				return NotFound();
			}

			// B) Items
			if (await rd.NextResultAsync())
			{
				while (await rd.ReadAsync())
				{
					vm.Items.Add(new OrderItemVm
					{
						LineNo = TryGet<int>(rd, "line_no"),
						ProductName = TryGet<string>(rd, "product_name") ?? "",
						UnitPrice = TryGet<decimal?>(rd, "unit_price") ?? 0m,
						Quantity = TryGet<int>(rd, "quantity"),
						LineTotal = TryGet<decimal?>(rd, "line_total")
									   ?? TryGet<decimal?>(rd, "subtotal") ?? 0m,
						ProductId = TryGet<int?>(rd, "product_id") ?? 0
					});
				}
			}

			// C) Payments
			if (await rd.NextResultAsync())
			{
				while (await rd.ReadAsync())
				{
					vm.Payments.Add(new PaymentVm
					{
						Provider = TryGet<string>(rd, "provider"),
						ProviderTxn = TryGet<string>(rd, "provider_txn"),
						Amount = TryGet<decimal?>(rd, "amount") ?? 0m,
						CreatedAt = TryGet<DateTime?>(rd, "created_at") ?? DateTime.MinValue,
						StatusText = TryGet<string>(rd, "status")
					});
				}
			}

			// D) Shipments
			if (await rd.NextResultAsync())
			{
				while (await rd.ReadAsync())
				{
					vm.Shipments.Add(new ShipmentVm
					{
						ShipmentCode = TryGet<string>(rd, "shipment_code"),
						Provider = TryGet<string>(rd, "provider"),
						TrackingNo = TryGet<string>(rd, "tracking_no"),
						TrackTime = TryGet<DateTime?>(rd, "track_time"),
						Message = TryGet<string>(rd, "message")
					});
				}
			}

			return View(vm);
		}

		// POST /OnlineStore/Orders/{orderCode}/cancel
		[HttpPost("{orderCode}/cancel")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Cancel(string orderCode, string? reason)
		{
			var userId = _me.UserId;
			if (userId <= 0) userId = _cfg.GetValue<int?>("Demo:UserId") ?? 0;

			await using var conn = new SqlConnection(_connString);
			await conn.OpenAsync();

			await using var cmd = conn.CreateCommand();
			cmd.CommandText = "dbo.usp_Order_CancelIfUnpaid";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.AddWithValue("@UserId", userId);
			cmd.Parameters.AddWithValue("@OrderCode", orderCode);
			cmd.Parameters.AddWithValue("@Reason", (object?)reason ?? DBNull.Value);

			var affected = await cmd.ExecuteNonQueryAsync();

			TempData["Toast"] = (affected >= 0) ? "已送出取消（僅未付款有效）。" : "取消失敗或狀態不允許。";
			return RedirectToAction(nameof(Detail), new { orderCode });
		}

		// GET /OnlineStore/Orders/{orderCode}/retry-pay
		[HttpGet("{orderCode}/retry-pay")]
		public IActionResult RetryPay(string orderCode)
			=> RedirectToAction("OrderResult", "Ecpay", new { area = "OnlineStore", orderCode });

		// 回傳狀態鍵（前端輪詢，驅動進度軸）
		[HttpGet("status/{orderCode}")]
		public async Task<IActionResult> Status(string orderCode)
		{
			var userId = _me.UserId;
			if (userId <= 0) userId = _cfg.GetValue<int?>("Demo:UserId") ?? 0;

			await using var conn = new SqlConnection(_connString);
			await conn.OpenAsync();

			await using var cmd = conn.CreateCommand();
			cmd.CommandText =
				"SELECT TOP 1 CASE " +
				"WHEN oi.order_status IN (N'已取消',N'Canceled') THEN N'canceled' " +
				"WHEN oi.order_status IN (N'已完成',N'Completed') THEN N'completed' " +
				"WHEN oi.order_status IN (N'已出貨',N'Shipped')   THEN N'shipped' " +
				"WHEN oi.payment_status IN (N'已付款',N'Paid')     THEN N'paid' " +
				"ELSE N'unpaid' END AS status_key " +
				"FROM dbo.SO_OrderInfoes oi WHERE oi.user_id=@uid AND oi.order_code=@code";
			cmd.Parameters.AddWithValue("@uid", userId);
			cmd.Parameters.AddWithValue("@code", orderCode);

			var result = (string?)await cmd.ExecuteScalarAsync() ?? "unpaid";
			return Json(new { statusKey = result });
		}

		// 取得明細（Partial）
		[HttpGet("items")]
		public async Task<IActionResult> Items(string orderCode)
		{
			var userId = _me.UserId;
			if (userId <= 0) userId = _cfg.GetValue<int?>("Demo:UserId") ?? 0;

			var list = new List<OrderItemVm>();

			await using var conn = new SqlConnection(_connString);
			await conn.OpenAsync();
			await using var cmd = conn.CreateCommand();
			cmd.CommandText = "dbo.usp_MyOrders_GetDetail";
			cmd.CommandType = CommandType.StoredProcedure;
			cmd.Parameters.AddWithValue("@UserId", userId);
			cmd.Parameters.AddWithValue("@OrderCode", orderCode);

			await using var rd = await cmd.ExecuteReaderAsync();

			// 跳過 Head
			if (!await rd.ReadAsync())
				return Content("<div class='text-danger small'>訂單不存在</div>", "text/html");

			// B: Items
			await rd.NextResultAsync();
			while (await rd.ReadAsync())
			{
				list.Add(new OrderItemVm
				{
					LineNo = TryGet<int>(rd, "line_no"),
					ProductName = TryGet<string>(rd, "product_name") ?? "",
					UnitPrice = TryGet<decimal?>(rd, "unit_price") ?? 0m,
					Quantity = TryGet<int>(rd, "quantity"),
					LineTotal = TryGet<decimal?>(rd, "line_total") ?? 0m,
					ProductId = TryGet<int?>(rd, "product_id") ?? 0
				});
			}

			return PartialView("~/Areas/OnlineStore/Views/Orders/_OrderItemsMini.cshtml", list);
		}

		// ===== Helpers =====

		// 安全取值（欄位不存在或為 DBNull 時回傳 default）
		// 安全版：支援 Nullable<T>、DBNull、缺欄位
		private static T? TryGet<T>(IDataRecord r, string name, T? fallback = default)
		{
			var ordinal = -1;
			try { ordinal = r.GetOrdinal(name); }
			catch { return fallback; }

			if (ordinal < 0) return fallback;
			if (r.IsDBNull(ordinal)) return fallback;

			object value = r.GetValue(ordinal);

			// 若 T 是 Nullable<>, 取其 underlying type
			var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

			try
			{
				if (value.GetType() == targetType || targetType.IsAssignableFrom(value.GetType()))
					return (T)value;

				var converted = Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
				return (T)converted!;
			}
			catch
			{
				return fallback;
			}
		}


		private static string? BuildAddress(IDataRecord r)
		{
			var a1 = TryGet<string>(r, "address1");
			var a2 = TryGet<string>(r, "address2");
			var zip = TryGet<string>(r, "dest_zip");
			var parts = new List<string?>(3) { zip, a1, a2 };
			parts.RemoveAll(s => string.IsNullOrWhiteSpace(s));
			return parts.Count == 0 ? null : string.Join(" ", parts);
		}
	}
}
