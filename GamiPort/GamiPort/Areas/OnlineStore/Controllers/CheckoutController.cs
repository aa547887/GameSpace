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
// ★ 新增：引用付款服務所在命名空間
using GamiPort.Areas.OnlineStore.Payments;

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	public class CheckoutController : Controller
	{
		private readonly ICartService _cartSvc;
		private readonly ILookupService _lookup;
		private readonly GameSpacedatabaseContext _db;

		// 台灣郵遞區號：3 或 5 碼
		private static readonly Regex ZipRegex = new(@"^\d{3}(\d{2})?$", RegexOptions.Compiled);

		public CheckoutController(ICartService cartSvc, ILookupService lookup, GameSpacedatabaseContext db)
		{
			_cartSvc = cartSvc;
			_lookup = lookup;
			_db = db;
		}

		// ===== 共用：載入清單 =====
		private async Task LoadShipMethodsAsync() => ViewBag.ShipMethods = await _lookup.GetShipMethodsAsync();
		private async Task LoadPayMethodsAsync() => ViewBag.PayMethods = await _lookup.GetPayMethodsAsync();

		// ===== Step1 (GET) =====
		[HttpGet]
		public async Task<IActionResult> Step1()
		{
			var (cartId, initShipId, initZip) = await GetDefaultsAsync();
			var summary = await _cartSvc.GetSummaryAsync(cartId, initShipId, initZip, null);
			ViewBag.Full = new { Summary = summary };

			await LoadShipMethodsAsync();
			return View(new CheckoutStep1Vm
			{
				ShipMethodId = initShipId,
				DestZip = initZip
			});
		}

		// ===== Step1 (POST) =====
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

		// ===== Step2 (GET) =====
		[HttpGet]
		public async Task<IActionResult> Step2()
		{
			var (shipMethodId, destZip) = GetStep1ValuesOrDefault();
			await RefreshSummaryToViewBagAsync(shipMethodId, destZip, null);
			await LoadPayMethodsAsync();
			return View(new CheckoutStep2Vm());
		}

		// ===== Step2 (POST) =====
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

		// ===== Review (GET) =====
		[HttpGet]
		public async Task<IActionResult> Review()
		{
			var (shipMethodId, destZip) = GetStep1ValuesOrDefault();
			var step2 = GetStep2OrDefault();
			await RefreshSummaryToViewBagAsync(shipMethodId, destZip, step2.CouponCode);
			return View(step2);
		}

		// ===== PlaceOrder (POST) — 呼叫交易性 SP 建單 =====
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> PlaceOrder()
		{
			// 1) 取 Step1/Step2（遺失就擋掉）
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

			// 2) 重新計算摘要（金額防竄改）
			var (shipMethodId, destZip) = (step1.ShipMethodId, step1.DestZip);
			var (cartId, _, _) = await GetDefaultsAsync();
			var summary = await _cartSvc.GetSummaryAsync(cartId, shipMethodId, destZip, step2.CouponCode);

			if (!summary.Can_Checkout)
			{
				TempData["Msg"] = summary.Block_Reason ?? "目前無法下單，請稍後再試。";
				return RedirectToAction(nameof(Review));
			}

			// 2.5) 取得有效 userId（cart.user_id 優先，否則抓 Users 任一存在 id）
			int? cartUserId = await _db.SoCarts
				.Where(c => c.CartId == cartId)
				.Select(c => (int?)c.UserId)
				.FirstOrDefaultAsync();

			int userId;
			if (cartUserId.HasValue && await _db.Users.AnyAsync(u => u.UserId == cartUserId.Value))
			{
				userId = cartUserId.Value;
			}
			else
			{
				userId = await _db.Users.OrderBy(u => u.UserId).Select(u => u.UserId).FirstOrDefaultAsync();
				if (userId == 0)
				{
					TempData["Msg"] = "找不到有效的使用者資料，無法建立訂單。";
					return RedirectToAction(nameof(Review));
				}
			}

			// 3) 呼叫 SP（OUTPUT 取回 @OrderId）
			var orderIdParam = new Microsoft.Data.SqlClient.SqlParameter
			{
				ParameterName = "@OrderId",
				SqlDbType = System.Data.SqlDbType.Int,
				Direction = System.Data.ParameterDirection.Output
			};

			// 取優惠折抵（負數 = 折抵）與訊息
			var couponDiscount = summary.CouponDiscount ?? 0m;
			var couponMsg = summary.CouponMessage ?? (string?)null;

			var sql = @"
EXEC dbo.usp_Order_CreateFromCart
    @CartId=@p0, @UserId=@p1, @ShipMethodId=@p2, @PayMethodId=@p3,
    @Recipient=@p4, @Phone=@p5, @DestZip=@p6, @Address1=@p7, @Address2=@p8,
    @CouponCode=@p9, @CouponDiscount=@p10, @CouponMessage=@p11,
    @OrderId=@OrderId OUTPUT;";

			await _db.Database.ExecuteSqlRawAsync(sql, new object[] {
				new Microsoft.Data.SqlClient.SqlParameter("@p0", cartId),
				new Microsoft.Data.SqlClient.SqlParameter("@p1", userId),
				new Microsoft.Data.SqlClient.SqlParameter("@p2", step1.ShipMethodId),
				new Microsoft.Data.SqlClient.SqlParameter("@p3", step2.PayMethodId),
				new Microsoft.Data.SqlClient.SqlParameter("@p4", step1.Recipient),
				new Microsoft.Data.SqlClient.SqlParameter("@p5", step1.Phone),
				new Microsoft.Data.SqlClient.SqlParameter("@p6", step1.DestZip),
				new Microsoft.Data.SqlClient.SqlParameter("@p7", step1.Address1),
				new Microsoft.Data.SqlClient.SqlParameter("@p8", (object?)step1.Address2 ?? System.DBNull.Value),
				new Microsoft.Data.SqlClient.SqlParameter("@p9", (object?)step2.CouponCode ?? System.DBNull.Value),
				new Microsoft.Data.SqlClient.SqlParameter("@p10", couponDiscount),
				new Microsoft.Data.SqlClient.SqlParameter("@p11", (object?)couponMsg ?? System.DBNull.Value),
				orderIdParam
			});

			int newOrderId = (int)(orderIdParam.Value ?? 0);
			if (newOrderId <= 0)
			{
				TempData["Msg"] = "下單失敗，請稍後再試。";
				return RedirectToAction(nameof(Review));
			}

			// 4) 成功：清除暫存並導向【啟動綠界】（這裡不直接進成功頁）
			TempData.Remove("Step1");
			TempData.Remove("Step2");

			// 導向本站 StartPayment（下一個 action 會自動 POST 到綠界）
			return RedirectToAction(nameof(StartPayment), new { id = newOrderId });

		}

		// ===== 啟動綠界（GET）— 自動 POST 表單到綠界 =====
				[HttpGet]
		public async Task<IActionResult> StartPayment(int id)
		{
			var order = await _db.SoOrderInfoes
				.Where(o => o.OrderId == id)
				.Select(o => new
				{
					o.OrderId,
					o.OrderCode,
					o.GrandTotal
				})
				.FirstOrDefaultAsync();

			if (order == null) return RedirectToAction(nameof(Step1));

			var svc = HttpContext.RequestServices.GetRequiredService<EcpayPaymentService>();
			var (action, fields) = svc.BuildCreditRequest(
				orderCode: order.OrderCode,
				amount: order.GrandTotal ?? 0m,
				itemName: "GamiPort 商品",
				returnPath: $"/Ecpay/Return?oid={id}",          // 前台回傳
				orderResultPath: $"/Ecpay/OrderResult?oid={id}",      // ← 改這個
				clientBackPath: "/OnlineStore/Checkout/Review"
			);

			var sb = new System.Text.StringBuilder();
			sb.AppendLine($"<form id='f' method='post' action='{action}'>");
			foreach (var kv in fields)
				sb.AppendLine($"<input type='hidden' name='{kv.Key}' value='{System.Net.WebUtility.HtmlEncode(kv.Value)}'/>");
			sb.AppendLine("</form><script>document.getElementById('f').submit();</script>");
			return Content(sb.ToString(), "text/html; charset=utf-8");
		}

		// ===== Success (GET) — 顯示訂單摘要 + 收件資訊 =====
		// ★ 新增：支援用 orderCode 導入（讓 Ecpay/Return 可帶單號字串回來）
		[HttpGet]
		public async Task<IActionResult> Success(int id, string? orderCode)
		{
			if (id <= 0 && !string.IsNullOrWhiteSpace(orderCode))
			{
				id = await _db.SoOrderInfoes
					.Where(o => o.OrderCode == orderCode)
					.Select(o => o.OrderId)
					.FirstOrDefaultAsync();
			}

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
					// 收件資訊快照
					o.Recipient,
					o.Phone,
					o.DestZip,
					o.Address1,
					o.Address2
				})
				.FirstOrDefaultAsync();

			if (order == null) return RedirectToAction(nameof(Step1));

			var itemCount = await _db.SoOrderItems
				.Where(i => i.OrderId == id)
				.SumAsync(i => (int?)i.Quantity) ?? 0;

			string? payMethodName = null;
			if (order.PayMethodId > 0)
			{
				payMethodName = await _db.SoPayMethods
					.Where(p => p.PayMethodId == order.PayMethodId)
					.Select(p => p.MethodName)
					.FirstOrDefaultAsync();
			}

			ViewBag.ItemCount = itemCount;
			ViewBag.PayMethodName = payMethodName;

			return View(order); // 對應 Success.cshtml 的 dynamic
		}

		// ===== 右側摘要（Partial）=====
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
			{
				summary.CouponMessage = string.IsNullOrEmpty(summary.CouponMessage)
					? warn : $"{summary.CouponMessage}；{warn}";
			}
			return PartialView("_CheckoutSummary", summary);
		}

		// ===== Private Helpers =====
		private async Task RefreshSummaryToViewBagAsync(int shipMethodId, string destZip, string? coupon)
		{
			var (cartId, _, _) = await GetDefaultsAsync();
			if (!await _lookup.ShipMethodExistsAsync(shipMethodId)) shipMethodId = 1;
			if (string.IsNullOrWhiteSpace(destZip) || !ZipRegex.IsMatch(destZip)) destZip = "100";

			var summary = await _cartSvc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);
			ViewBag.Full = new { Summary = summary };
		}

		private static string Append(string? msg, string add)
			=> string.IsNullOrEmpty(msg) ? add : $"{msg}、{add}";

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

		private async Task<(System.Guid cartId, int initShipId, string initZip)> GetDefaultsAsync()
		{
			var anonToken = AnonCookie.GetOrSet(HttpContext);
			const string CartKey = "CartId";

			if (System.Guid.TryParse(HttpContext.Session.GetString(CartKey), out var cached) && cached != System.Guid.Empty)
				return (cached, 1, "100");

			var cartId = await _cartSvc.EnsureCartIdAsync(null, anonToken);
			HttpContext.Session.SetString(CartKey, cartId.ToString());
			return (cartId, 1, "100");
		}
	}
}
