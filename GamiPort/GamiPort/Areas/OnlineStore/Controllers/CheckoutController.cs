// Areas/OnlineStore/Controllers/CheckoutController.cs
//#define DEV_SKIP_PAYMENT
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GamiPort.Areas.OnlineStore.Services;
using GamiPort.Areas.OnlineStore.ViewModels;
using GamiPort.Areas.OnlineStore.DTO;
using GamiPort.Areas.OnlineStore.Utils;           // AnonCookie
using Microsoft.EntityFrameworkCore;
using GamiPort.Models;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using System.Data;
using GamiPort.Areas.OnlineStore.Payments;        // EcpayPaymentService
using GamiPort.Infrastructure.Security;           // IAppCurrentUser
using Microsoft.AspNetCore.Authorization;         // ★ NEW: 要求登入才可進入結帳

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	public class CheckoutController : Controller
	{
		private readonly ICartService _cartSvc;
		private readonly ILookupService _lookup;
		private readonly GameSpacedatabaseContext _db;
		private readonly IAppCurrentUser _me;

		private static readonly Regex ZipRegex = new(@"^\d{3}(\d{2})?$", RegexOptions.Compiled);

		public CheckoutController(ICartService cartSvc, ILookupService lookup, GameSpacedatabaseContext db, IAppCurrentUser me)
		{
			_cartSvc = cartSvc;
			_lookup = lookup;
			_db = db;
			_me = me;
		}

		// ──────────────────── 舊版三步驟：停用路由（避免誤用） ────────────────────
		[NonAction] public Task<IActionResult> Step1() => Task.FromResult<IActionResult>(NotFound());
		[NonAction] public Task<IActionResult> Step1(CheckoutStep1Vm vm) => Task.FromResult<IActionResult>(NotFound());
		[NonAction] public Task<IActionResult> Step2() => Task.FromResult<IActionResult>(NotFound());
		[NonAction] public Task<IActionResult> Step2(CheckoutStep2Vm vm) => Task.FromResult<IActionResult>(NotFound());
		[NonAction] public Task<IActionResult> Review() => Task.FromResult<IActionResult>(NotFound());
		[NonAction] public Task<IActionResult> PlaceOrder() => Task.FromResult<IActionResult>(NotFound());

		// ───────────────────────────── 一頁式：GET ─────────────────────────────
		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> OnePage(string? selected)
		{
			// 未登入：導到登入後回跳到本頁（保留你原有邏輯）
			if (!(User?.Identity?.IsAuthenticated ?? false))
			{
				var returnUrl = Url.Action(nameof(OnePage), "Checkout", new { area = "OnlineStore", selected })
								?? "/OnlineStore/Checkout/OnePage";
				var loginUrl =
					Url.Action("Login", "Login", new { area = "Login", ReturnUrl = returnUrl }) ??
					Url.Action("Index", "Login", new { area = "Login", ReturnUrl = returnUrl }) ??
					"/Identity/Account/Login?ReturnUrl=" + Uri.EscapeDataString(returnUrl);
				return Redirect(loginUrl);
			}

			// 解析勾選清單（product_id）
			var selectedIds = (selected ?? "")
				.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(s => int.TryParse(s, out var id) ? id : (int?)null)
				.Where(id => id.HasValue).Select(id => id!.Value)
				.Distinct().ToArray();

			if (selectedIds.Length == 0)
			{
				// 沒選任何東西 → 回購物車
				return RedirectToAction("Index", "Cart", new { area = "OnlineStore" });
			}

			// 取 cartId / 預設配送與郵遞區號（保持你原方法）
			var (cartId, initShipId, initZip) = await GetDefaultsAsync();

			// 抓整車摘要與明細（沿用你現有服務）
			var full = await _cartSvc.GetFullAsync(cartId, initShipId, initZip, null);

			// 篩選出「勾選」的明細
			var selLines = full.Lines.Where(l => selectedIds.Contains(l.Product_Id)).ToList();
			if (selLines.Count == 0)
				return RedirectToAction("Index", "Cart", new { area = "OnlineStore" });

			// 用勾選覆蓋摘要（不改動你其他欄位，避免牽動 SQL/DTO）
			var summary = full.Summary;
			summary.Subtotal = selLines.Sum(x => x.Line_Subtotal);
			summary.Subtotal_Physical = selLines.Where(x => x.Is_Physical).Sum(x => x.Line_Subtotal);
			summary.Item_Count_Total = selLines.Sum(x => x.Quantity);
			summary.Item_Count_Physical = selLines.Where(x => x.Is_Physical).Sum(x => x.Quantity);

			// 若全為非實體 → 運費歸 0（常見站點邏輯；也不牽動 SQL）
			if (summary.Item_Count_Physical == 0)
			{
				summary.Shipping_Fee = 0;
			}

			// 以你現有欄位重算 Grand_Total（不使用不存在的 Discount_Shipping）
			summary.Grand_Total = summary.Subtotal
								+ summary.Shipping_Fee
								- summary.Discount
								+ (summary.CouponDiscount ?? 0);

			ViewBag.Full = summary;
			TempData["__SelectedIds"] = string.Join(",", selectedIds); // 後續 PlaceOrder 可用

			return View(new CheckoutOnePageVm
			{
				ShipMethodId = initShipId,
				PayMethodId = 1,
				Zipcode = initZip
			});
		}


		// ────────────────── 一頁式：POST（驗證→建單→轉金流/或跳過） ──────────────────
		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> OnePage(CheckoutOnePageVm vm)
		{
			// ★ 如果沒有輸入優惠碼，就把它視為可選，移除 Required 的驗證錯誤
			if (string.IsNullOrWhiteSpace(vm.CouponCode))
			{
				ModelState.Remove(nameof(vm.CouponCode));
				vm.CouponCode = null;
			}
			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, vm.CouponCode);
				return View(vm);
			}

			// ★ 1) 後端嚴格驗證 + 自動回退到可用的配送/付款方式（避免 DB 無此 Id 造成 SP=0）
			if (!await _db.SoShipMethods.AnyAsync(s => s.ShipMethodId == vm.ShipMethodId))
			{
				var fallbackShip = await _db.SoShipMethods.OrderBy(s => s.ShipMethodId).Select(s => (int?)s.ShipMethodId).FirstOrDefaultAsync();
				if (fallbackShip == null)
					ModelState.AddModelError(nameof(vm.ShipMethodId), "找不到可用的配送方式");
				else
					vm.ShipMethodId = fallbackShip.Value;
			}
			if (!await _db.SoPayMethods.AnyAsync(p => p.PayMethodId == vm.PayMethodId))
			{
				var fallbackPay = await _db.SoPayMethods.OrderBy(p => p.PayMethodId).Select(p => (int?)p.PayMethodId).FirstOrDefaultAsync();
				if (fallbackPay == null)
					ModelState.AddModelError(nameof(vm.PayMethodId), "找不到可用的付款方式");
				else
					vm.PayMethodId = fallbackPay.Value;
			}
			if (string.IsNullOrWhiteSpace(vm.Zipcode) || !ZipRegex.IsMatch(vm.Zipcode))
				ModelState.AddModelError(nameof(vm.Zipcode), "郵遞區號格式錯誤");

			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, vm.CouponCode);
				return View(vm);
			}

			// ★ 2) 取 cartId / userId（避免 0 導致 SP 失敗）
			var (cartId, _, _) = await GetDefaultsAsync();
			int? cartUserId = await _db.SoCarts.Where(c => c.CartId == cartId).Select(c => (int?)c.UserId).FirstOrDefaultAsync();
			int userId = cartUserId ?? await _me.GetUserIdAsync();
			if (userId <= 0)
				userId = await _db.Users.OrderBy(u => u.UserId).Select(u => u.UserId).FirstOrDefaultAsync();
			if (userId <= 0)
			{
				ModelState.AddModelError(string.Empty, "找不到有效的使用者資料，無法建立訂單。");
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, vm.CouponCode);
				return View(vm);
			}

			// ★ 3) 拉摘要，確認可結帳
			var summary = await _cartSvc.GetSummaryAsync(cartId, vm.ShipMethodId, vm.Zipcode, vm.CouponCode);
			if (!summary.Can_Checkout)
			{
				ModelState.AddModelError(string.Empty, summary.Block_Reason ?? "目前無法下單，請稍後再試。");
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, vm.CouponCode);
				return View(vm);
			}

			// ★ 4) 以「命名參數」呼叫 SP（避免參數順序不一致）
			var pCartId = new SqlParameter("@CartId", cartId);
			var pUserId = new SqlParameter("@UserId", userId);
			var pShipMethodId = new SqlParameter("@ShipMethodId", vm.ShipMethodId);
			var pPayMethodId = new SqlParameter("@PayMethodId", vm.PayMethodId);
			var pRecipient = new SqlParameter("@Recipient", (object)vm.Recipient ?? DBNull.Value);
			var pPhone = new SqlParameter("@Phone", (object)vm.Phone ?? DBNull.Value);
			var pDestZip = new SqlParameter("@DestZip", (object)vm.Zipcode ?? DBNull.Value);
			var pAddress1 = new SqlParameter("@Address1", (object)($"{vm.City}{vm.District}{vm.Address1}") ?? DBNull.Value);
			var pAddress2 = new SqlParameter("@Address2", DBNull.Value);
			var pCouponCode = new SqlParameter("@CouponCode", (object?)vm.CouponCode ?? DBNull.Value);
			var pCouponDiscount = new SqlParameter("@CouponDiscount", (object)(summary.CouponDiscount ?? 0m));
			var pCouponMessage = new SqlParameter("@CouponMessage", (object?)summary.CouponMessage ?? DBNull.Value);
			var pOrderId = new SqlParameter("@OrderId", SqlDbType.Int) { Direction = ParameterDirection.Output };

			int newOrderId = 0;
			try
			{
				await _db.Database.ExecuteSqlRawAsync(@"
EXEC dbo.usp_Order_CreateFromCart
      @CartId=@CartId,
      @UserId=@UserId,
      @ShipMethodId=@ShipMethodId,
      @PayMethodId=@PayMethodId,
      @Recipient=@Recipient,
      @Phone=@Phone,
      @DestZip=@DestZip,
      @Address1=@Address1,
      @Address2=@Address2,
      @CouponCode=@CouponCode,
      @CouponDiscount=@CouponDiscount,
      @CouponMessage=@CouponMessage,
      @OrderId=@OrderId OUTPUT;",
				pCartId, pUserId, pShipMethodId, pPayMethodId, pRecipient, pPhone, pDestZip,
				pAddress1, pAddress2, pCouponCode, pCouponDiscount, pCouponMessage, pOrderId);

				newOrderId = (int)(pOrderId.Value ?? 0);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"建立訂單時發生例外：{ex.Message}");
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, vm.CouponCode);
				return View(vm);
			}

			if (newOrderId <= 0)
			{
				ModelState.AddModelError(string.Empty,
					"下單失敗：可能是 (1) 購物車為空、(2) 配送/付款方式在資料庫不存在、(3) 參數順序不符或金額/欄位驗證未通過。");
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, vm.CouponCode);
				return View(vm);
			}

#if DEV_SKIP_PAYMENT
			var order = await _db.SoOrderInfoes.FirstOrDefaultAsync(o => o.OrderId == newOrderId);
			if (order != null) { order.PaymentStatus = "已付款"; order.PaymentAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
			TempData["Msg"] = "（開發）已跳過金流並標記已付款。";
			return RedirectToAction(nameof(Success), new { id = newOrderId });
#else
			return RedirectToAction(nameof(StartPayment), new { id = newOrderId });
#endif
		}

		// ─────────────────── 產生綠界表單並自動送出（含 noscript 備援） ───────────────────
		[Authorize] // ★ NEW
		[HttpGet]
		public async Task<IActionResult> StartPayment(int id)
		{
#if DEV_SKIP_PAYMENT
			TempData["Msg"] = "（開發）目前為跳過金流模式。";
			return RedirectToAction(nameof(Success), new { id });
#else
			var order = await _db.SoOrderInfoes
				.Where(o => o.OrderId == id)
				.Select(o => new { o.OrderId, o.OrderCode, o.GrandTotal, o.PaymentStatus })
				.FirstOrDefaultAsync();

			if (order == null) return RedirectToAction(nameof(OnePage));

			var svc = HttpContext.RequestServices.GetRequiredService<EcpayPaymentService>();
			var (action, fields) = svc.BuildCreditRequest(
				orderCode: order.OrderCode,
				amount: order.GrandTotal ?? 0m,
				itemName: "GamiPort 商品",
				returnPath: $"/Ecpay/Return?oid={id}",
				orderResultPath: $"/Ecpay/OrderResult?oid={id}",
				clientBackPath: "/OnlineStore/Checkout/OnePage"
			);

			var sb = new StringBuilder();
			sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'><title>前往付款</title></head><body>");
			sb.AppendLine($"<form id='f' method='post' action='{action}'>");
			foreach (var kv in fields)
			{
				var k = System.Net.WebUtility.HtmlEncode(kv.Key);
				var v = System.Net.WebUtility.HtmlEncode(kv.Value);
				sb.AppendLine($"  <input type='hidden' name='{k}' value='{v}' />");
			}
			sb.AppendLine("  <noscript>");
			sb.AppendLine("    <p>已產生付款資料，但您的瀏覽器停用 JavaScript。請點下方按鈕前往綠界付款。</p>");
			sb.AppendLine("    <button type='submit'>前往付款</button>");
			sb.AppendLine("  </noscript>");
			sb.AppendLine("</form>");
			sb.AppendLine("<script>try{document.getElementById('f').submit();}catch(e){}</script>");
			sb.AppendLine("</body></html>");

			return Content(sb.ToString(), "text/html; charset=utf-8");
#endif
		}

		[HttpGet]
		public async Task<IActionResult> Success(int id, string? orderCode)
		{
			// 允許以 orderCode 或 id 開啟
			if (id <= 0 && !string.IsNullOrWhiteSpace(orderCode))
				id = await _db.SoOrderInfoes.Where(o => o.OrderCode == orderCode)
											.Select(o => o.OrderId).FirstOrDefaultAsync();
			if (id <= 0) return RedirectToAction(nameof(OnePage));

			// 撈訂單頭資料（維持匿名物件，方便 view 用 @model dynamic）
			var order = await _db.SoOrderInfoes
				.Where(o => o.OrderId == id)
				.Select(o => new {
					o.OrderId,
					o.OrderCode,
					o.OrderDate,
					o.OrderStatus,
					o.PaymentStatus,
					o.PaymentAt,
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
				})
				.FirstOrDefaultAsync();
			if (order == null) return RedirectToAction(nameof(OnePage));

			// 取付款方式名稱（若有）
			string? payMethodName = null;
			if ((order.PayMethodId ?? 0) > 0)
				payMethodName = await _db.SoPayMethods
										 .Where(p => p.PayMethodId == order.PayMethodId)
										 .Select(p => p.MethodName)
										 .FirstOrDefaultAsync();

			// 取明細數量（可選）
			var itemCount = await _db.SoOrderItems
									 .Where(i => i.OrderId == id)
									 .SumAsync(i => (int?)i.Quantity) ?? 0;

			ViewBag.PayMethodName = payMethodName;
			ViewBag.ItemCount = itemCount;

			return View(order);
		}

		// Ajax：摘要盒
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
			ViewBag.Full = summary;
		}
		private static string Append(string? msg, string add) => string.IsNullOrEmpty(msg) ? add : $"{msg}、{add}";

		private async Task<(Guid cartId, int initShipId, string initZip)> GetDefaultsAsync()
		{
			const string CartKey = "CartId";
			if (Guid.TryParse(HttpContext.Session.GetString(CartKey), out var cached) && cached != Guid.Empty)
				return (cached, 1, "100");

			var anon = AnonCookie.GetOrSet(HttpContext);
			var userId = await _me.GetUserIdAsync();
			var cartId = await _cartSvc.EnsureCartIdAsync(userId > 0 ? userId : (int?)null, anon);

			HttpContext.Session.SetString(CartKey, cartId.ToString());
			return (cartId, 1, "100");
		}
		// ★ 新增：回傳目前會員三種券（未使用、未刪除）的可用張數
		[HttpGet]
		[Area("OnlineStore")]
		public async Task<IActionResult> CouponCounts()
		{
			// 取得目前登入者的 UserId（未登入或取不到就回 0）
			var userId = await _me.GetUserIdAsync();
			if (userId <= 0)
				return Json(new { free = 0, pct = 0, minus = 0 });

			// 用最保守的 ADO.NET 查詢，避免碰到你 EF 實體命名差異
			using var conn = _db.Database.GetDbConnection();
			await conn.OpenAsync();

			using var cmd = conn.CreateCommand();
			cmd.CommandText = @"
        SELECT CouponTypeID, COUNT(*) AS Cnt
        FROM dbo.Coupon WITH (NOLOCK)
        WHERE UserID = @uid
          AND ISNULL(IsDeleted, 0) = 0
          AND ISNULL(IsUsed, 0) = 0
          AND UsedInOrderID IS NULL
          AND CouponTypeID IN (1,2,3)
        GROUP BY CouponTypeID;";
			var p = cmd.CreateParameter();
			p.ParameterName = "@uid";
			p.Value = userId;
			cmd.Parameters.Add(p);

			int free = 0, pct = 0, minus = 0;
			using (var rd = await cmd.ExecuteReaderAsync())
			{
				while (await rd.ReadAsync())
				{
					var t = rd.GetInt32(0);
					var c = rd.GetInt32(1);
					if (t == 1) free = c;
					else if (t == 2) pct = c;
					else if (t == 3) minus = c;
				}
			}
			return Json(new { free, pct, minus });
		}
	}
}
