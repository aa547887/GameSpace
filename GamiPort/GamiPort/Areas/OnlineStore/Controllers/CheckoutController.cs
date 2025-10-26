using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;                  // ★ for Session GetString/SetString
using GamiPort.Areas.OnlineStore.Services;
using GamiPort.Areas.OnlineStore.ViewModels;
using GamiPort.Areas.OnlineStore.DTO;
using GamiPort.Areas.OnlineStore.Utils;           // ★ 取用 AnonCookie

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	public class CheckoutController : Controller
	{
		private readonly ICartService _cartSvc;
		private readonly ILookupService _lookup;

		// 台灣郵遞區號（3 或 5 碼）
		private static readonly Regex ZipRegex = new(@"^\d{3}(\d{2})?$", RegexOptions.Compiled);

		public CheckoutController(ICartService cartSvc, ILookupService lookup)
		{
			_cartSvc = cartSvc;
			_lookup = lookup;
		}

		// ===== Step1 (GET) =====
		// 新增：把清單塞進 ViewBag 的共用方法
		private async Task LoadShipMethodsAsync()
		{
			ViewBag.ShipMethods = await _lookup.GetShipMethodsAsync();
		}

		private async Task LoadPayMethodsAsync()
		{
			ViewBag.PayMethods = await _lookup.GetPayMethodsAsync();
		}

		// Step1(GET)
		[HttpGet]
		public async Task<IActionResult> Step1()
		{
			var (cartId, initShipId, initZip) = await GetDefaultsAsync();
			var summary = await _cartSvc.GetSummaryAsync(cartId, initShipId, initZip, null);
			ViewBag.Full = new { Summary = summary };

			await LoadShipMethodsAsync();   // ★★★ 這一行很重要

			return View(new CheckoutStep1Vm
			{
				ShipMethodId = initShipId,
				DestZip = initZip
			});
		}

		// Step1(POST) 回畫面時也要補
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Step1(CheckoutStep1Vm vm)
		{
			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.DestZip, null);
				await LoadShipMethodsAsync();   // ★
				return View(vm);
			}

			if (!await _lookup.ShipMethodExistsAsync(vm.ShipMethodId))
				ModelState.AddModelError(nameof(vm.ShipMethodId), "配送方式不合法");

			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(vm.ShipMethodId, vm.DestZip, null);
				await LoadShipMethodsAsync();   // ★
				return View(vm);
			}

			TempData["Step1"] = System.Text.Json.JsonSerializer.Serialize(vm);
			return RedirectToAction(nameof(Step2));
		}

		// Step2(GET)
		[HttpGet]
		public async Task<IActionResult> Step2()
		{
			var (shipMethodId, destZip) = GetStep1ValuesOrDefault();
			await RefreshSummaryToViewBagAsync(shipMethodId, destZip, null);

			await LoadPayMethodsAsync();    // ★★★ 也很重要

			return View(new CheckoutStep2Vm());
		}

		// Step2(POST) 回畫面時也要補
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Step2(CheckoutStep2Vm vm)
		{
			var (shipMethodId, destZip) = GetStep1ValuesOrDefault();

			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(shipMethodId, destZip, vm.CouponCode);
				await LoadPayMethodsAsync(); // ★
				return View(vm);
			}

			if (!await _lookup.PayMethodExistsAsync(vm.PayMethodId))
				ModelState.AddModelError(nameof(vm.PayMethodId), "付款方式不合法");

			if (!ModelState.IsValid)
			{
				await RefreshSummaryToViewBagAsync(shipMethodId, destZip, vm.CouponCode);
				await LoadPayMethodsAsync(); // ★
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

			return View(step2); // 你的頁面已能顯示 TempData Demo 訊息
		}

		// ===== PlaceOrder (POST) — M11 會改成真的建單 =====
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult PlaceOrder()
		{
			TempData["Msg"] = "（Demo）已收到下單請求；M11 會改為正式建單流程。";
			return RedirectToAction(nameof(Review));
		}

		// ===== 右側摘要 Partial（友善容錯）=====
		[HttpGet]
		public async Task<IActionResult> SummaryBox(int shipMethodId, string destZip, string? coupon)
		{
			var (cartId, _, _) = await GetDefaultsAsync();

			// 1) 容錯：非法參數不丟例外，改安全預設 + 警示訊息
			string? warn = null;

			if (!await _lookup.ShipMethodExistsAsync(shipMethodId))
			{
				warn = Append(warn, "配送方式無效，已自動套用預設");
				shipMethodId = 1; // 你的預設（超商/宅配擇一）
			}

			if (string.IsNullOrWhiteSpace(destZip) || !ZipRegex.IsMatch(destZip))
			{
				warn = Append(warn, "郵遞區號格式無效，請重新輸入");
				destZip = "100"; // 合法安全預設
			}

			// 2) 取摘要
			var summary = await _cartSvc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);

			// 3) 警示訊息塞到 CouponMessage（不破壞未來真正的 coupon 訊息）
			if (!string.IsNullOrEmpty(warn))
			{
				summary.CouponMessage = string.IsNullOrEmpty(summary.CouponMessage)
					? warn
					: $"{summary.CouponMessage}；{warn}";
			}

			return PartialView("_CheckoutSummary", summary);
		}

		// ===== Private Helpers =====

		private async Task RefreshSummaryToViewBagAsync(int shipMethodId, string destZip, string? coupon)
		{
			var (cartId, _, _) = await GetDefaultsAsync();

			// 與 SummaryBox 一致的容錯（伺端回頁面也不會炸）
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
			catch { /* 忽略還原錯誤 */ }
			return (1, "100"); // 預設值
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
			// ★★★ 關鍵：用「加入購物車」同一顆匿名 Token 來取得 cart_id
			var anonToken = AnonCookie.GetOrSet(HttpContext);

			const string CartKey = "CartId";

			// 先嘗試用 Session 快取（同一瀏覽器流程更省查詢）
			if (System.Guid.TryParse(HttpContext.Session.GetString(CartKey), out var cached) && cached != System.Guid.Empty)
				return (cached, 1, "100");

			// 未快取就用匿名 Token 向 DB 取回或建立購物車，再寫回 Session
			var cartId = await _cartSvc.EnsureCartIdAsync(null, anonToken);
			HttpContext.Session.SetString(CartKey, cartId.ToString());

			return (cartId, 1, "100");
		}
	}
}
