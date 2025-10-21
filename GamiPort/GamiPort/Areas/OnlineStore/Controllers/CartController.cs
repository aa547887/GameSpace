using GamiPort.Areas.OnlineStore.DTO;          // 你放 DTO 的命名空間
using GamiPort.Areas.OnlineStore.Services;     // ICartService 介面
using GamiPort.Areas.OnlineStore.Utils;        // AnonCookie
using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	[Route("OnlineStore/[controller]/[action]")]
	public class CartController : Controller
	{
		private readonly ICartService _svc;
		public CartController(ICartService svc) => _svc = svc;

		//========================
		// View（同步版）
		//========================

		[HttpGet]
		public async Task<IActionResult> Index(int shipMethodId = 1, string destZip = "320", string? coupon = null)
		{
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _svc.EnsureCartIdAsync(null, anon);

			var vm = await _svc.GetFullAsync(cartId, shipMethodId, destZip, coupon);
			return View(vm); // @model CartVm
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Add(int productId, int qty = 1)
		{
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _svc.EnsureCartIdAsync(null, anon);

			await _svc.AddAsync(cartId, productId, qty);
			TempData["CartMessage"] = "已加入購物車";   // [ADDED] 給同步轉跳用的訊息
			return RedirectToAction(nameof(Index));
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateQty(int productId, int qty)
		{
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _svc.EnsureCartIdAsync(null, anon);

			await _svc.UpdateQtyAsync(cartId, productId, qty);
			TempData["CartMessage"] = "已更新數量";     // [ADDED]
			return RedirectToAction(nameof(Index));
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Remove(int productId)
		{
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _svc.EnsureCartIdAsync(null, anon);

			await _svc.RemoveAsync(cartId, productId);
			TempData["CartMessage"] = "已刪除商品";     // [ADDED]
			return RedirectToAction(nameof(Index));
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Clear()
		{
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _svc.EnsureCartIdAsync(null, anon);

			await _svc.ClearAsync(cartId);
			TempData["CartMessage"] = "已清空購物車";   // [ADDED]
			return RedirectToAction(nameof(Index));
		}

		//========================
		// API（AJAX 友善版）
		// 前端可用 fetch/$.post 呼叫，避免整頁重整
		//========================

		[HttpPost]
		[ValidateAntiForgeryToken] // 建議前端 AJAX 一併送 __RequestVerificationToken
		public async Task<IActionResult> AddApi(int productId, int qty = 1, int shipMethodId = 1, string destZip = "320", string? coupon = null)
		{
			try
			{
				var (cartId, anon) = await GetCartIdAsync();
				await _svc.AddAsync(cartId, productId, qty);

				var summary = await _svc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);
				return Ok(new { ok = true, message = "已加入購物車", count = summary.TotalQty, summary });
			}
			catch (Exception ex)
			{
				return BadRequest(new { ok = false, message = ex.Message });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateQtyApi(int productId, int qty, int shipMethodId = 1, string destZip = "320", string? coupon = null)
		{
			try
			{
				var (cartId, anon) = await GetCartIdAsync();
				await _svc.UpdateQtyAsync(cartId, productId, qty);

				var summary = await _svc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);
				return Ok(new { ok = true, message = "已更新數量", count = summary.TotalQty, summary });
			}
			catch (Exception ex)
			{
				return BadRequest(new { ok = false, message = ex.Message });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveApi(int productId, int shipMethodId = 1, string destZip = "320", string? coupon = null)
		{
			try
			{
				var (cartId, anon) = await GetCartIdAsync();
				await _svc.RemoveAsync(cartId, productId);

				var summary = await _svc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);
				return Ok(new { ok = true, message = "已刪除商品", count = summary.TotalQty, summary });
			}
			catch (Exception ex)
			{
				return BadRequest(new { ok = false, message = ex.Message });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ClearApi(int shipMethodId = 1, string destZip = "320", string? coupon = null)
		{
			try
			{
				var (cartId, anon) = await GetCartIdAsync();
				await _svc.ClearAsync(cartId);

				var summary = await _svc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);
				return Ok(new { ok = true, message = "已清空購物車", count = summary.TotalQty, summary });
			}
			catch (Exception ex)
			{
				return BadRequest(new { ok = false, message = ex.Message });
			}
		}

		[HttpGet]
		public async Task<IActionResult> SummaryApi(int shipMethodId = 1, string destZip = "320", string? coupon = null)
		{
			// [ADDED] 讀取目前摘要（例如頁面載入時更新徽章）
			try
			{
				var (cartId, anon) = await GetCartIdAsync();
				var summary = await _svc.GetSummaryAsync(cartId, shipMethodId, destZip, coupon);
				return Ok(new { ok = true, count = summary.TotalQty, summary });
			}
			catch (Exception ex)
			{
				return BadRequest(new { ok = false, message = ex.Message });
			}
		}

		//========================
		// Private Helper
		//========================
		private async Task<(Guid cartId, Guid anon)> GetCartIdAsync()
		{
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _svc.EnsureCartIdAsync(null, anon);
			return (cartId, anon);
		}
	}
}
