// Areas/OnlineStore/Controllers/CheckoutController.cs
//#define DEV_SKIP_PAYMENT
using GamiPort.Areas.OnlineStore.DTO;
using GamiPort.Areas.OnlineStore.Infrastructure;
using GamiPort.Areas.OnlineStore.Payments;
using GamiPort.Areas.OnlineStore.Services;
using GamiPort.Areas.OnlineStore.Utils;
using GamiPort.Areas.OnlineStore.ViewModels;
using GamiPort.Infrastructure.Security;
using GamiPort.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization; 

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

		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> OnePage(string? selected)
		{
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

			var selectedIds = (selected ?? "")
				.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(s => int.TryParse(s, out var id) ? id : (int?)null)
				.Where(id => id.HasValue).Select(id => id!.Value)
				.Distinct().ToArray();
			if (selectedIds.Length == 0)
				return RedirectToAction("Index", "Cart", new { area = "OnlineStore" });

			var (cartId, initShipId, initZip) = await GetDefaultsAsync();
			var full = await _cartSvc.GetFullAsync(cartId, initShipId, initZip, null);

			var selLines = full.Lines.Where(l => selectedIds.Contains(l.Product_Id)).ToList();
			var summary = full.Summary;
			summary.Subtotal = selLines.Sum(x => x.Line_Subtotal);
			summary.Subtotal_Physical = selLines.Where(x => x.Is_Physical).Sum(x => x.Line_Subtotal);
			summary.Item_Count_Total = selLines.Sum(x => x.Quantity);
			summary.Item_Count_Physical = selLines.Where(x => x.Is_Physical).Sum(x => x.Quantity);
			if (summary.Item_Count_Physical == 0) summary.Shipping_Fee = 0;
			summary.Grand_Total = summary.Subtotal + summary.Shipping_Fee - (summary.CouponDiscount ?? 0);

			ViewBag.Full = summary;
			ViewBag.SelectedIdsStr = string.Join(",", selectedIds);
			TempData["__SelectedIds"] = ViewBag.SelectedIdsStr;

			return View(new CheckoutOnePageVm { ShipMethodId = initShipId, PayMethodId = 1, Zipcode = initZip });
		}

		[Authorize]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> OnePage(CheckoutOnePageVm vm)
		{
			// 1) 輕校驗（原樣保留）
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
			if (!await _db.SoShipMethods.AnyAsync(s => s.ShipMethodId == vm.ShipMethodId))
				ModelState.AddModelError(nameof(vm.ShipMethodId), "找不到可用的配送方式");
			if (!await _db.SoPayMethods.AnyAsync(p => p.PayMethodId == vm.PayMethodId))
				ModelState.AddModelError(nameof(vm.PayMethodId), "找不到可用的付款方式");
			if (string.IsNullOrWhiteSpace(vm.Zipcode) || !System.Text.RegularExpressions.Regex.IsMatch(vm.Zipcode, @"^\d{3}(\d{2})?$"))
				ModelState.AddModelError(nameof(vm.Zipcode), "郵遞區號格式錯誤");
			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, vm.CouponCode);
				return View(vm);
			}

			// 2) 取得 cart/user
			var (cartId, _, _) = await GetDefaultsAsync();
			int? cartUserId = await _db.SoCarts.Where(c => c.CartId == cartId).Select(c => (int?)c.UserId).FirstOrDefaultAsync();
			int userId = cartUserId ?? await _me.GetUserIdAsync();
			if (userId <= 0)
			{
				ModelState.AddModelError(string.Empty, "找不到有效的使用者資料，無法建立訂單。");
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, vm.CouponCode);
				return View(vm);
			}

			// 3) 從表單或 VM 取券碼，並在伺服器端重算摘要
			var coupon = string.IsNullOrWhiteSpace(vm.CouponCode)
				? (Request.Form["CouponCode"].ToString() ?? null)
				: vm.CouponCode;

			var summary = await _cartSvc.GetSummaryAsync(cartId, vm.ShipMethodId, vm.Zipcode, coupon);
			if (!summary.Can_Checkout)
			{
				ModelState.AddModelError(string.Empty, summary.Block_Reason ?? "目前無法下單，請稍後再試。");
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, coupon);
				return View(vm);
			}

			// 4) 金額（以伺服器重算為準）
			decimal sub = Convert.ToDecimal(summary.Subtotal);
			decimal ship = Convert.ToDecimal(summary.Shipping_Fee);
			decimal discPos = Convert.ToDecimal(summary.CouponDiscount); // 正值（50／15%差額）
			decimal discNeg = -discPos;                                  // SP 需要負數

			// 5) 呼叫 SP 建單（@CouponDiscount 傳「負數」）
			var pCartId = new SqlParameter("@CartId", cartId);
			var pUserId = new SqlParameter("@UserId", userId);
			var pShipMethodId = new SqlParameter("@ShipMethodId", vm.ShipMethodId);
			var pPayMethodId = new SqlParameter("@PayMethodId", vm.PayMethodId);
			var pRecipient = new SqlParameter("@Recipient", (object)vm.Recipient ?? DBNull.Value);
			var pPhone = new SqlParameter("@Phone", (object)vm.Phone ?? DBNull.Value);
			var pDestZip = new SqlParameter("@DestZip", (object)vm.Zipcode ?? DBNull.Value);
			var pAddress1 = new SqlParameter("@Address1", (object)($"{vm.City}{vm.District}{vm.Address1}") ?? DBNull.Value);
			var pAddress2 = new SqlParameter("@Address2", DBNull.Value);
			var pCouponCode = new SqlParameter("@CouponCode", (object?)coupon ?? DBNull.Value);
			var pCouponDiscount = new SqlParameter("@CouponDiscount", (object)discNeg); // ← 傳負數
			var pCouponMessage = new SqlParameter("@CouponMessage", (object?)summary.CouponMessage ?? DBNull.Value);
			var pOrderId = new SqlParameter("@OrderId", SqlDbType.Int) { Direction = ParameterDirection.Output };

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
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"建立訂單時發生例外：{ex.Message}");
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, coupon);
				return View(vm);
			}

			int newOrderId = (int)(pOrderId.Value ?? 0);
			if (newOrderId <= 0)
			{
				ModelState.AddModelError(string.Empty, "下單失敗，請稍後再試。");
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.Zipcode, coupon);
				return View(vm);
			}

			// 6) 保險覆寫（以摘要金額為準，確保 DB 與摘要一致）
			try
			{
				await _db.Database.ExecuteSqlRawAsync(@"
UPDATE dbo.SO_OrderInfoes
SET Subtotal      = {0},
    ShippingFee   = {1},
    DiscountTotal = {2},              -- 正值
    GrandTotal    = {0} + {1} - {2}   -- 小計 + 運費 - 折抵
WHERE OrderId     = {3};",
					sub, ship, discPos, newOrderId);
			}
			catch { /* 欄位名異動時可無視 */ }

			return RedirectToAction(nameof(StartPayment), new { id = newOrderId });
		}

		[Authorize]
		[HttpGet]
		public async Task<IActionResult> StartPayment(int id)
		{
			var order = await _db.SoOrderInfoes
				.Where(o => o.OrderId == id)
				.Select(o => new { o.OrderId, o.OrderCode, o.Subtotal, o.ShippingFee, o.DiscountTotal })
				.FirstOrDefaultAsync();
			if (order == null) return RedirectToAction(nameof(OnePage));

			// ★ 送到綠界的金額 = 小計 + 運費 - 折抵（正值）
			decimal grand = order.Subtotal + order.ShippingFee - order.DiscountTotal;
			int amtInt = (int)Math.Round(grand, MidpointRounding.AwayFromZero);

			var svc = HttpContext.RequestServices.GetRequiredService<EcpayPaymentService>();
			var (action, fields) = svc.BuildCreditRequest(
				orderCode: order.OrderCode,
				amount: amtInt,
				itemName: "GamiPort 商品",
				returnPath: $"/Ecpay/Return?oid={id}",
				orderResultPath: $"/Ecpay/OrderResult?oid={id}",
				clientBackPath: "/OnlineStore/Checkout/OnePage"
			);

			var sb = new System.Text.StringBuilder();
			sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'><title>前往付款</title></head><body>");
			sb.AppendLine($"<form id='f' method='post' action='{action}'>");
			foreach (var kv in fields)
				sb.AppendLine($"<input type='hidden' name='{System.Net.WebUtility.HtmlEncode(kv.Key)}' value='{System.Net.WebUtility.HtmlEncode(kv.Value)}' />");
			sb.AppendLine("</form><script>document.getElementById('f').submit();</script></body></html>");
			return Content(sb.ToString(), "text/html; charset=utf-8");
		}

		[HttpGet]
		public async Task<IActionResult> Success(int id, string? orderCode)
		{
			if (id <= 0 && !string.IsNullOrWhiteSpace(orderCode))
				id = await _db.SoOrderInfoes.Where(o => o.OrderCode == orderCode)
											.Select(o => o.OrderId).FirstOrDefaultAsync();
			if (id <= 0) return RedirectToAction(nameof(OnePage));

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
					o.Address2,
					o.CouponCode
				})
				.FirstOrDefaultAsync();
			if (order == null) return RedirectToAction(nameof(OnePage));

			string? payMethodName = null;
			if ((order.PayMethodId ?? 0) > 0)
				payMethodName = await _db.SoPayMethods
										 .Where(p => p.PayMethodId == order.PayMethodId)
										 .Select(p => p.MethodName)
										 .FirstOrDefaultAsync();

			var itemCount = await _db.SoOrderItems
									 .Where(i => i.OrderId == id)
									 .SumAsync(i => (int?)i.Quantity) ?? 0;

			ViewBag.PayMethodName = payMethodName;
			ViewBag.ItemCount = itemCount;
			ViewBag.CouponCode = order?.CouponCode;

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
			if (string.IsNullOrWhiteSpace(destZip) || !System.Text.RegularExpressions.Regex.IsMatch(destZip, @"^\d{3}(\d{2})?$"))
				destZip = "100";

			var summary = await _cartSvc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);

			if (!string.IsNullOrEmpty(warn))
				summary.CouponMessage = string.IsNullOrEmpty(summary.CouponMessage) ? warn : $"{summary.CouponMessage}；{warn}";

			// ★ 新增：把目前選到的券值帶給 Partial & 存備援
			ViewBag.CouponChosen = coupon ?? "";
			TempData["__Coupon"] = coupon ?? "";

			return PartialView("_CheckoutSummary", summary);
		}

		private async Task RefreshSummaryToViewBagAsync(int shipMethodId, string destZip, string? coupon)
		{
			var (cartId, _, _) = await GetDefaultsAsync();
			if (!await _lookup.ShipMethodExistsAsync(shipMethodId)) shipMethodId = 1;
			if (string.IsNullOrWhiteSpace(destZip) || !ZipRegex.IsMatch(destZip)) destZip = "100";
			var summary = await _cartSvc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);
			ViewBag.Full = summary;
		}
		private static string Append(string? msg, string add) => string.IsNullOrEmpty(msg) ? add : $"{msg}、{add}";

		private string GetConnString()
		{
			var cs = _db.Database.GetConnectionString();
			if (string.IsNullOrWhiteSpace(cs))
			{
				var cfg = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
				cs = cfg.GetConnectionString("GameSpacedatabase");
			}
			return cs ?? throw new InvalidOperationException("找不到資料庫連線字串。");
		}

		private async Task<(Guid cartId, int initShipId, string initZip)> GetDefaultsAsync()
		{
			var anonToken = GamiPort.Areas.OnlineStore.Utils.AnonCookie.GetOrSet(HttpContext);

			Guid anonCartId;
			using (var conn = new SqlConnection(GetConnString()))
			{
				await conn.OpenAsync();
				using var cmd = new SqlCommand("dbo.usp_Cart_Ensure", conn) { CommandType = CommandType.StoredProcedure };
				cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = DBNull.Value });
				cmd.Parameters.Add(new SqlParameter("@AnonymousToken", SqlDbType.UniqueIdentifier) { Value = anonToken });
				var pOut = new SqlParameter("@OutCartId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output };
				cmd.Parameters.Add(pOut);
				await cmd.ExecuteNonQueryAsync();
				anonCartId = (Guid)pOut.Value;
			}

			var userId = await _me.GetUserIdAsync();
			Guid finalCartId = anonCartId;

			if (userId > 0)
			{
				using (var conn = new SqlConnection(GetConnString()))
				{
					await conn.OpenAsync();

					using (var cmd = new SqlCommand("dbo.usp_Cart_AttachToUser", conn) { CommandType = CommandType.StoredProcedure })
					{
						cmd.Parameters.Add(new SqlParameter("@CartId", SqlDbType.UniqueIdentifier) { Value = anonCartId });
						cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
						await cmd.ExecuteNonQueryAsync();
					}

					using (var cmd2 = new SqlCommand("dbo.usp_Cart_Ensure", conn) { CommandType = CommandType.StoredProcedure })
					{
						cmd2.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
						cmd2.Parameters.Add(new SqlParameter("@AnonymousToken", SqlDbType.UniqueIdentifier) { Value = DBNull.Value });
						var pOut2 = new SqlParameter("@OutCartId", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output };
						cmd2.Parameters.Add(pOut2);
						await cmd2.ExecuteNonQueryAsync();
						finalCartId = (Guid)pOut2.Value;
					}
				}
			}

			HttpContext.Session.SetString("CartId", finalCartId.ToString());
			HttpContext.Response.Cookies.Append(
				"cart_id",
				finalCartId.ToString(),
				new CookieOptions
				{
					HttpOnly = true,
					Secure = HttpContext.Request.IsHttps,
					SameSite = SameSiteMode.Lax,
					Path = "/",
					Expires = DateTimeOffset.UtcNow.AddDays(30),
					IsEssential = true
				});
			return (finalCartId, 1, "100");
		}

		[HttpGet]
		[Area("OnlineStore")]
		public async Task<IActionResult> CouponCounts()
		{
			var userId = await _me.GetUserIdAsync();
			if (userId <= 0)
				return Json(new { free = 0, pct = 0, minus = 0 });

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
