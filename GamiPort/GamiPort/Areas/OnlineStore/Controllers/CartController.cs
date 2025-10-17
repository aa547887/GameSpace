using GamiPort.Areas.OnlineStore.DTO;   // 你放 DTO 的命名空間
using GamiPort.Areas.OnlineStore.Services; // 之後 2-3 服務
using GamiPort.Areas.OnlineStore.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	[Route("OnlineStore/[controller]/[action]")]
	public class CartController : Controller
	{
		private readonly ICartService _svc; // 2-3 會實作
		public CartController(ICartService svc) => _svc = svc;

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var anon = AnonCookie.GetOrSet(HttpContext);      // 2-1
			var cartId = await _svc.EnsureCartIdAsync(null, anon);
			var vm = await _svc.GetAsync(cartId);             // 回傳 CartSummaryDto
			return View(vm);                                   // 對應 Index.cshtml

		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Add(int productId, int qty = 1)
		{
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _svc.EnsureCartIdAsync(null, anon);
			await _svc.AddAsync(cartId, productId, qty);
			return RedirectToAction(nameof(Index));
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateQty(int productId, int qty)
		{
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _svc.EnsureCartIdAsync(null, anon);
			await _svc.UpdateQtyAsync(cartId, productId, qty);
			return RedirectToAction(nameof(Index));
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Remove(int productId)
		{
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _svc.EnsureCartIdAsync(null, anon);
			await _svc.RemoveAsync(cartId, productId);
			return RedirectToAction(nameof(Index));
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Clear()
		{
			var anon = AnonCookie.GetOrSet(HttpContext);
			var cartId = await _svc.EnsureCartIdAsync(null, anon);
			await _svc.ClearAsync(cartId);
			return RedirectToAction(nameof(Index));
		}
	}
}