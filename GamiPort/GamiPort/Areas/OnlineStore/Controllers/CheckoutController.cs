// Areas/OnlineStore/Controllers/CheckoutController.cs
// ======================================================================
// ▶▶▶ 一鍵切換（只改這裡）：
//    取消註解 = 啟用「開發期：跳過金流」
//    註解/刪除 = 走「真實金流（綠界）」 
//#define DEV_SKIP_PAYMENT
// ======================================================================

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;                  // Session
using GamiPort.Areas.OnlineStore.Services;
using GamiPort.Areas.OnlineStore.ViewModels;
using GamiPort.Areas.OnlineStore.DTO;
using GamiPort.Areas.OnlineStore.Utils;           // AnonCookie
using Microsoft.EntityFrameworkCore;              // EF Core
using GamiPort.Models;                            // GameSpacedatabaseContext
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;                   // for usp_Order_CreateFromCart
using System.Data;
using GamiPort.Areas.OnlineStore.Payments;        // EcpayPaymentService（真實金流用）

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	public class CheckoutController : Controller
	{
		private readonly ICartService _cartSvc;
		private readonly ILookupService _lookup;
		private readonly GameSpacedatabaseContext _db;

		private static readonly Regex ZipRegex = new(@"^\d{3}(\d{2})?$", RegexOptions.Compiled);

		public CheckoutController(ICartService cartSvc, ILookupService lookup, GameSpacedatabaseContext db)
		{
			_cartSvc = cartSvc;
			_lookup = lookup;
			_db = db;
		}

		private async Task LoadShipMethodsAsync() => ViewBag.ShipMethods = await _lookup.GetShipMethodsAsync();
		private async Task LoadPayMethodsAsync() => ViewBag.PayMethods = await _lookup.GetPayMethodsAsync();

		[HttpGet]
		public async Task<IActionResult> Step1()
		{
			var (cartId, initShipId, initZip) = await GetDefaultsAsync();
			var summary = await _cartSvc.GetSummaryAsync(cartId, initShipId, initZip, null);
			ViewBag.Full = new { Summary = summary };

			await LoadShipMethodsAsync();
			return View(new CheckoutStep1Vm { ShipMethodId = initShipId, DestZip = initZip });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Step1(CheckoutStep1Vm vm)
		{
			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.DestZip, null);
				await LoadShipMethodsAsync();
				return View(vm);
			}

			if (!await _lookup.ShipMethodExistsAsync(vm.ShipMethodId))
				ModelState.AddModelError(nameof(vm.ShipMethodId), "配送方式不合法");

			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.DestZip, null);
				await LoadShipMethodsAsync();
				return View(vm);
			}

			TempData["Step1"] = System.Text.Json.JsonSerializer.Serialize(vm);
			return RedirectToAction(nameof(Step2));
		}

		[HttpGet]
		public async Task<IActionResult> Step2()
		{
			var (shipMethodId, destZip) = GetStep1ValuesOrDefault();
			await RefreshSummaryToViewBagAsync(shipMethodId, destZip, null);
			await LoadPayMethodsAsync();
			return View(new CheckoutStep2Vm());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Step2(CheckoutStep2Vm vm)
		{
			var (shipMethodId, destZip) = GetStep1ValuesOrDefault();

			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(shipMethodId, destZip, vm.CouponCode);
				await LoadPayMethodsAsync();
				return View(vm);
			}

			if (!await _lookup.PayMethodExistsAsync(vm.PayMethodId))
				ModelState.AddModelError(nameof(vm.PayMethodId), "付款方式不合法");

			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(shipMethodId, destZip, vm.CouponCode);
				await LoadPayMethodsAsync();
				return View(vm);
			}

			TempData["Step2"] = System.Text.Json.JsonSerializer.Serialize(vm);
			return RedirectToAction(nameof(Review));
		}

		[HttpGet]
		public async Task<IActionResult> Review()
		{
			var (shipMethodId, destZip) = GetStep1ValuesOrDefault();
			var step2 = GetStep2OrDefault();
			await RefreshSummaryToViewBagAsync(shipMethodId, destZip, step2.CouponCode);
#if DEV_SKIP_PAYMENT
			ViewBag.DevSkip = true;   // 可在 View 顯示提示（非必要）
#else
			ViewBag.DevSkip = false;
#endif
			return View(step2);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PlaceOrder()
		{
			// 1) 取 Step1/Step2
			var step1 = default(CheckoutStep1Vm);
			var step2 = default(CheckoutStep2Vm);
			try
			{
				step1 = System.Text.Json.JsonSerializer.Deserialize<CheckoutStep1Vm>((string)TempData.Peek("Step1"));
				step2 = System.Text.Json.JsonSerializer.Deserialize<CheckoutStep2Vm>((string)TempData.Peek("Step2"));
			}
			catch { }

			if (step1 == null || step2 == null)
			{
				TempData["Msg"] = "結帳流程逾時或資料遺失，請重新操作。";
				return RedirectToAction(nameof(Step1));
			}

			// 2) 金額重算
			var (shipMethodId, destZip) = (step1.ShipMethodId, step1.DestZip);
			var (cartId, _, _) = await GetDefaultsAsync();
			var summary = await _cartSvc.GetSummaryAsync(cartId, shipMethodId, destZip, step2.CouponCode);

			if (!summary.Can_Checkout)
			{
				TempData["Msg"] = summary.Block_Reason ?? "目前無法下單，請稍後再試。";
				return RedirectToAction(nameof(Review));
			}

			// 2.5) 取得 userId
			int? cartUserId = await _db.SoCarts.Where(c => c.CartId == cartId).Select(c => (int?)c.UserId).FirstOrDefaultAsync();
			int userId = (cartUserId.HasValue && await _db.Users.AnyAsync(u => u.UserId == cartUserId.Value))
				? cartUserId.Value
				: await _db.Users.OrderBy(u => u.UserId).Select(u => u.UserId).FirstOrDefaultAsync();

			if (userId == 0)
			{
				TempData["Msg"] = "找不到有效的使用者資料，無法建立訂單。";
				return RedirectToAction(nameof(Review));
			}

			// 3) 建單 SP
			var orderIdParam = new SqlParameter
			{
				ParameterName = "@OrderId",
				SqlDbType = SqlDbType.Int,
				Direction = ParameterDirection.Output
			};

			var couponDiscount = summary.CouponDiscount ?? 0m;
			var couponMsg = summary.CouponMessage ?? (string?)null;

			var sql = @"
EXEC dbo.usp_Order_CreateFromCart
    @CartId=@p0, @UserId=@p1, @ShipMethodId=@p2, @PayMethodId=@p3,
    @Recipient=@p4, @Phone=@p5, @DestZip=@p6, @Address1=@p7, @Address2=@p8,
    @CouponCode=@p9, @CouponDiscount=@p10, @CouponMessage=@p11,
    @OrderId=@OrderId OUTPUT;";

			await _db.Database.ExecuteSqlRawAsync(sql, new object[] {
				new SqlParameter("@p0", cartId),
				new SqlParameter("@p1", userId),
				new SqlParameter("@p2", step1.ShipMethodId),
				new SqlParameter("@p3", step2.PayMethodId),
				new SqlParameter("@p4", step1.Recipient),
				new SqlParameter("@p5", step1.Phone),
				new SqlParameter("@p6", step1.DestZip),
				new SqlParameter("@p7", step1.Address1),
				new SqlParameter("@p8", (object?)step1.Address2 ?? DBNull.Value),
				new SqlParameter("@p9", (object?)step2.CouponCode ?? DBNull.Value),
				new SqlParameter("@p10", couponDiscount),
				new SqlParameter("@p11", (object?)couponMsg ?? DBNull.Value),
				orderIdParam
			});

			int newOrderId = (int)(orderIdParam.Value ?? 0);
			if (newOrderId <= 0)
			{
				TempData["Msg"] = "下單失敗，請稍後再試。";
				return RedirectToAction(nameof(Review));
			}

			// 4) 清暫存
			TempData.Remove("Step1");
			TempData.Remove("Step2");

#if DEV_SKIP_PAYMENT
			// ───────────────────────────────────────────────
			// ★ 開發期：跳過金流 → 只更新主檔，不寫交易/稽核/歷史（避免約束衝突）
			// ───────────────────────────────────────────────
			var order = await _db.SoOrderInfoes.FirstOrDefaultAsync(o => o.OrderId == newOrderId);
			if (order != null)
			{
				order.PaymentStatus = "已付款";
				order.PaymentAt = DateTime.UtcNow;
				await _db.SaveChangesAsync();
			}
			TempData["Msg"] = "（開發）已跳過金流並標記已付款。";
			return RedirectToAction(nameof(Success), new { id = newOrderId });
#else
			// ───────────────────────────────────────────────
			// ★ 正式：轉去金流（由 StartPayment → 綠界）
			// ───────────────────────────────────────────────
			return RedirectToAction(nameof(StartPayment), new { id = newOrderId });
#endif
		}

		[HttpGet]
		public async Task<IActionResult> StartPayment(int id)
		{
#if DEV_SKIP_PAYMENT
			// 防止手動打 URL 時又被送去綠界
			TempData["Msg"] = "（開發）目前為跳過金流模式。";
			return RedirectToAction(nameof(Success), new { id });
#else
			var order = await _db.SoOrderInfoes
				.Where(o => o.OrderId == id)
				.Select(o => new { o.OrderId, o.OrderCode, o.GrandTotal })
				.FirstOrDefaultAsync();

			if (order == null) return RedirectToAction(nameof(Step1));

			var svc = HttpContext.RequestServices.GetRequiredService<EcpayPaymentService>();
			var (action, fields) = svc.BuildCreditRequest(
				orderCode: order.OrderCode,
				amount: order.GrandTotal ?? 0m,
				itemName: "GamiPort 商品",
				returnPath: $"/Ecpay/Return?oid={id}",
				orderResultPath: $"/Ecpay/OrderResult?oid={id}",
				clientBackPath: "/OnlineStore/Checkout/Review"
			);

			var sb = new System.Text.StringBuilder();
			sb.AppendLine($"<form id='f' method='post' action='{action}'>");
			foreach (var kv in fields)
				sb.AppendLine($"<input type='hidden' name='{kv.Key}' value='{System.Net.WebUtility.HtmlEncode(kv.Value)}'/>");
			sb.AppendLine("</form><script>document.getElementById('f').submit();</script>");
			return Content(sb.ToString(), "text/html; charset=utf-8");
#endif
		}

		[HttpGet]
		public async Task<IActionResult> Success(int id, string? orderCode)
		{
			if (id <= 0 && !string.IsNullOrWhiteSpace(orderCode))
				id = await _db.SoOrderInfoes.Where(o => o.OrderCode == orderCode).Select(o => o.OrderId).FirstOrDefaultAsync();

			if (id <= 0) return RedirectToAction(nameof(Step1));

			var order = await _db.SoOrderInfoes
				.Where(o => o.OrderId == id)
				.Select(o => new {
					o.OrderId,
					o.OrderCode,
					o.OrderDate,
					o.OrderStatus,
					o.PaymentStatus,
					o.OrderTotal,
					o.Subtotal,
					o.DiscountTotal,
					o.ShippingFee,
					o.GrandTotal,
					o.PayMethodId,
					o.Recipient,
					o.Phone,
					o.DestZip,
					o.Address1,
					o.Address2
				}).FirstOrDefaultAsync();

			if (order == null) return RedirectToAction(nameof(Step1));

			var itemCount = await _db.SoOrderItems.Where(i => i.OrderId == id).SumAsync(i => (int?)i.Quantity) ?? 0;

			string? payMethodName = null;
			if (order.PayMethodId > 0)
				payMethodName = await _db.SoPayMethods.Where(p => p.PayMethodId == order.PayMethodId).Select(p => p.MethodName).FirstOrDefaultAsync();

			ViewBag.ItemCount = itemCount;
			ViewBag.PayMethodName = payMethodName;
			return View(order);
		}

		[HttpGet]
		public async Task<IActionResult> SummaryBox(int shipMethodId, string destZip, string? coupon)
		{
			var (cartId, _, _) = await GetDefaultsAsync();
			string? warn = null;

			if (!await _lookup.ShipMethodExistsAsync(shipMethodId))
			{
				warn = Append(warn, "配送方式無效，已自動套用預設");
				shipMethodId = 1;
			}
			if (string.IsNullOrWhiteSpace(destZip) || !ZipRegex.IsMatch(destZip))
			{
				warn = Append(warn, "郵遞區號格式無效，請重新輸入");
				destZip = "100";
			}

			var summary = await _cartSvc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);

			if (!string.IsNullOrEmpty(warn))
				summary.CouponMessage = string.IsNullOrEmpty(summary.CouponMessage) ? warn : $"{summary.CouponMessage}；{warn}";

			return PartialView("_CheckoutSummary", summary);
		}

		// Helpers
		private async Task RefreshSummaryToViewBagAsync(int shipMethodId, string destZip, string? coupon)
		{
			var (cartId, _, _) = await GetDefaultsAsync();
			if (!await _lookup.ShipMethodExistsAsync(shipMethodId)) shipMethodId = 1;
			if (string.IsNullOrWhiteSpace(destZip) || !ZipRegex.IsMatch(destZip)) destZip = "100";
			var summary = await _cartSvc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);
			ViewBag.Full = new { Summary = summary };
		}

		private static string Append(string? msg, string add) => string.IsNullOrEmpty(msg) ? add : $"{msg}、{add}";

		private (int shipMethodId, string destZip) GetStep1ValuesOrDefault()
		{
			try
			{
				if (TempData.Peek("Step1") is string json)
				{
					var vm = System.Text.Json.JsonSerializer.Deserialize<CheckoutStep1Vm>(json);
					if (vm != null) return (vm.ShipMethodId, vm.DestZip);
				}
			}
			catch { }
			return (1, "100");
		}

		private CheckoutStep2Vm GetStep2OrDefault()
		{
			try
			{
				if (TempData.Peek("Step2") is string json)
				{
					var vm = System.Text.Json.JsonSerializer.Deserialize<CheckoutStep2Vm>(json);
					if (vm != null) return vm;
				}
			}
			catch { }
			return new CheckoutStep2Vm();
		}

		private async Task<(Guid cartId, int initShipId, string initZip)> GetDefaultsAsync()
		{
			var anonToken = AnonCookie.GetOrSet(HttpContext);
			const string CartKey = "CartId";

			if (Guid.TryParse(HttpContext.Session.GetString(CartKey), out var cached) && cached != Guid.Empty)
				return (cached, 1, "100");

			var cartId = await _cartSvc.EnsureCartIdAsync(null, anonToken);
			HttpContext.Session.SetString(CartKey, cartId.ToString());
			return (cartId, 1, "100");
		}
	}
}
