using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GamiPort.Areas.OnlineStore.Services;
using GamiPort.Areas.OnlineStore.Utils;
using Microsoft.AspNetCore.Http;

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	[Route("OnlineStore/[controller]/[action]")]
	public class CartController : Controller
	{
		private readonly ICartService _cart;

		public CartController(ICartService cart)
		{
			_cart = cart;
		}

		// 取得目前登入者的 userId（若你的 Claim 名稱不同，這裡改一下）
		private int? GetUserIdOrNull()
		{
			if (User?.Identity?.IsAuthenticated == true)
			{
				// 常見幾種寫法：NameIdentifier / "UserId" / 自訂 Claim
				var val = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
				if (int.TryParse(val, out var uid)) return uid;
			}
			return null;
		}

		// 內部共用：確保 cartId（若沒有匿名 cookie 就會寫入）
		private async Task<Guid> EnsureCartAsync()
		{
			var userId = GetUserIdOrNull();
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _cart.EnsureCartIdAsync(userId, anon);

			// ★ 關鍵：回寫 Session，統一各頁能讀到同一個 cart
			HttpContext.Session.SetString("CartId", cartId.ToString());
			return cartId;
		}
		// GET: /OnlineStore/Cart/CountJson  → Navbar 會打這支來更新徽章
		[HttpGet]
		public async Task<IActionResult> CountJson()
		{
			var cartId = await EnsureCartAsync();
			var count = await _cart.GetItemCountAsync(cartId);
			return Json(new { count });
		}

		// GET: /OnlineStore/Cart/Index
		// 只回 Razor 骨架；初始參數用 ViewData 給前端
		[HttpGet]
		public IActionResult Index(int shipMethodId = 1, string destZip = "320", string? couponCode = null)
		{
			ViewData["shipMethodId"] = shipMethodId;
			ViewData["destZip"] = destZip;
			ViewData["couponCode"] = couponCode ?? "";
			return View();
		}

		// GET: /OnlineStore/Cart/Full?shipMethodId=&destZip=&couponCode=
		[HttpGet]
		public async Task<IActionResult> Full(int shipMethodId, string destZip, string? couponCode)
		{
			var cartId = await EnsureCartAsync();
			var vm = await _cart.GetFullAsync(cartId, shipMethodId, destZip ?? "", couponCode);

			// 1) 明細：vm.Lines（不是 Items）
			var items = vm.Lines.Select(x => new {
				productId = x.Product_Id,
				productName = x.Product_Name,
				imageThumb = x.Image_Thumb,
				unitPrice = x.Unit_Price,
				quantity = x.Quantity,
				lineSubtotal = x.Line_Subtotal
			}).ToList();

			// 2) 總計映射：把你的底線命名 → 前端慣用駝峰命名
			var payload = new
			{
				ok = true,
				items,
				summary = new
				{
					totalQty = vm.Summary.Item_Count_Total, // ← 由你定義的總件數
					subtotal = vm.Summary.Subtotal,
					discount = vm.Summary.Discount,
					shipping = vm.Summary.Shipping_Fee,
					grandTotal = vm.Summary.Grand_Total,
					shipMethodId = shipMethodId,
					destZip = destZip,
					couponCode = couponCode,
					couponMessage = (string?)null // 目前你的 DTO 沒有此欄，先給 null；之後若有就改這裡
				}
			};
			return Json(payload);
		}


		// POST: /OnlineStore/Cart/UpdateQty
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateQty(int productId, int qty, int shipMethodId, string destZip, string? couponCode)
		{
			try
			{
				var cartId = await EnsureCartAsync();
				await _cart.UpdateQtyAsync(cartId, productId, qty);
				return await Full(shipMethodId, destZip, couponCode);
			}
			catch (Exception ex)
			{
				// 這裡簡化示範；實務上請分辨錯誤碼（例如 INVALID_QTY、PRODUCT_NOT_FOUND…）
				return Json(new { ok = false, code = "UPDATE_FAILED", message = ex.Message });
			}
		}

		// POST: /OnlineStore/Cart/Remove
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Remove(int productId, int shipMethodId, string destZip, string? couponCode)
		{
			try
			{
				var cartId = await EnsureCartAsync();
				await _cart.RemoveAsync(cartId, productId);
				return await Full(shipMethodId, destZip, couponCode);
			}
			catch (Exception ex)
			{
				return Json(new { ok = false, code = "REMOVE_FAILED", message = ex.Message });
			}
		}

		// POST: /OnlineStore/Cart/Clear
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Clear(int shipMethodId, string destZip, string? couponCode)
		{
			try
			{
				var cartId = await EnsureCartAsync();
				await _cart.ClearAsync(cartId);
				return await Full(shipMethodId, destZip, couponCode);
			}
			catch (Exception ex)
			{
				return Json(new { ok = false, code = "CLEAR_FAILED", message = ex.Message });
			}
		}
#if DEBUG
		[HttpGet]
		public async Task<IActionResult> DevAdd(int productId = 101, int qty = 2, int shipMethodId = 1, string destZip = "320", string? couponCode = null)
		{
			try
			{
				var cartId = await EnsureCartAsync();
				await _cart.AddAsync(cartId, productId, qty);
				return await Full(shipMethodId, destZip, couponCode);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { ok = false, code = "DEVADD_FAILED", message = ex.Message, detail = ex.InnerException?.Message });
			}
		}
#endif



	}
}
