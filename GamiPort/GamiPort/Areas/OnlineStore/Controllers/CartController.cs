using System;
using System.Data;
using System.Linq;
using GamiPort.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GamiPort.Areas.OnlineStore.Services;
using GamiPort.Areas.OnlineStore.Utils;
using Microsoft.AspNetCore.Http;
using GamiPort.Infrastructure.Security;
using GamiPort.Areas.OnlineStore.Infrastructure; // CartCookie
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;



namespace GamiPort.Areas.OnlineStore.Controllers
{
	[Area("OnlineStore")]
	[Route("OnlineStore/[controller]")]
	public sealed class CartController : Controller
	{
		private readonly ICartService _cart;
		private readonly IAppCurrentUser _me;
		private readonly GameSpacedatabaseContext _db;

		public CartController(ICartService cart, IAppCurrentUser me, GameSpacedatabaseContext db)
		{
			_cart = cart;
			_me = me;
			_db = db;
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

		// ★ 放在 CartController 類別內（私有方法）
		// 任何需要 cartId 的 Action 都呼叫這個，確保會先做合併
		private async Task<Guid> EnsureCartAsync()
		{
			// 1) 取得匿名 cookie 與目前使用者
			var anon = AnonCookie.GetOrSet(HttpContext);      // 仍保留你原本的工具
			var userId = await _me.GetUserIdAsync();          // 你的 IAppCurrentUser

			// 2) 交給服務決定是否需要合併並回傳「最終 cartId」
			//    規則：userId>0 就把 anon 購物車合併進會員車，否則維持匿名車
			var cartId = await _cart.EnsureCartIdAsync(userId > 0 ? userId : (int?)null, anon);

			// 3) 把「最終 cartId」回寫到 Session（修掉登入後仍拿舊車的問題）
			HttpContext.Session.SetString("CartId", cartId.ToString());

			//// 4) 若已登入，清掉匿名 cookie（避免下一次又被判定成需要合併）
			//try { AnonCookie.Clear(); } catch { /* 若無此方法可略過 */ }

			return cartId;
		}
		// GET: /OnlineStore/Cart/CountJson  → Navbar 會打這支來更新徽章
		[HttpGet("CountJson")]
		public async Task<IActionResult> CountJson()
		{
			var cartId = await EnsureCartAsync();
			var count = await _cart.GetItemCountAsync(cartId);
			return Json(new { count });
		}

		// GET: /OnlineStore/Cart/Index
		// 只回 Razor 骨架；初始參數用 ViewData 給前端
		[HttpGet("")]
		public async Task<IActionResult> Index(int shipMethodId = 1, string destZip = "320", string? couponCode = null)
		{
			// 1) 匿名也要有 cart_id（cookie: cart_id）
			var cartId = CartCookie.GetOrCreate(HttpContext);

			// 2) 如果已登入：回車時自動合併「匿名購物車 → 會員購物車」
			var userId = await _me.GetUserIdAsync();
			if (userId > 0 && cartId != Guid.Empty)
			{
				await using var conn = _db.Database.GetDbConnection();
				if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
				await using var cmd = conn.CreateCommand();
				cmd.CommandText = "dbo.usp_Cart_AttachToUser";             // ★ 對應下面提供的 SP 名稱
				cmd.CommandType = System.Data.CommandType.StoredProcedure;
				cmd.Parameters.Add(new SqlParameter("@CartId", cartId));    // UNIQUEIDENTIFIER
				cmd.Parameters.Add(new SqlParameter("@UserId", userId));    // INT
				await cmd.ExecuteNonQueryAsync();
				// 合併後 cookie 可留著（或你想 Drop 也行）
			}

			ViewData["shipMethodId"] = shipMethodId;
			ViewData["destZip"] = destZip;
			ViewData["couponCode"] = couponCode ?? "";
			return View();
		}


		// ＝＝＝ Full API：加相容三參數 overload（舊呼叫都能編譯） ＝＝＝
		[HttpGet("Full")]
		public Task<IActionResult> Full(int shipMethodId, string destZip, string? couponCode)
			=> Full(shipMethodId, destZip, couponCode, null);

		// ＝＝＝ 主要版本（支援 selected；你先前已有，這裡給可覆蓋版） ＝＝＝
		[HttpGet("Full")]
		public async Task<IActionResult> Full(int shipMethodId, string destZip, string? couponCode, string? selected)
		{
			var cartId = await EnsureCartAsync(); // ★ 關鍵：每次查資料前都會觸發合併

			var vm = await _cart.GetFullAsync(cartId, shipMethodId, destZip ?? "", couponCode);

			// 以下「selected 過濾」邏輯維持你既有的（如果你已經加過就沿用）
			HashSet<int>? allow = null;
			if (!string.IsNullOrWhiteSpace(selected))
			{
				allow = selected.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
								.Select(s => int.TryParse(s, out var id) ? id : (int?)null)
								.Where(id => id.HasValue).Select(id => id!.Value)
								.ToHashSet();

				var only = vm.Lines.Where(x => allow.Contains(x.Product_Id)).ToList();

				vm.Summary.Subtotal = only.Sum(x => x.Line_Subtotal);
				vm.Summary.Subtotal_Physical = only.Where(x => x.Is_Physical).Sum(x => x.Line_Subtotal);
				vm.Summary.Item_Count_Total = only.Sum(x => x.Quantity);
				vm.Summary.Item_Count_Physical = only.Where(x => x.Is_Physical).Sum(x => x.Quantity);
				if (vm.Summary.Item_Count_Physical == 0) vm.Summary.Shipping_Fee = 0;
				vm.Summary.Grand_Total = vm.Summary.Subtotal + vm.Summary.Shipping_Fee - (vm.Summary.CouponDiscount ?? 0);

				vm = new GamiPort.Areas.OnlineStore.DTO.CartVm { Lines = only, Summary = vm.Summary };
			}

			var items = vm.Lines.Select(x => new {
				productId = x.Product_Id,
				productName = x.Product_Name,
				imageThumb = x.Image_Thumb,
				unitPrice = x.Unit_Price,
				quantity = x.Quantity,
				lineSubtotal = x.Line_Subtotal
			}).ToList();

			return Json(new
			{
				ok = true,
				items,
				summary = new
				{
					totalQty = vm.Summary.Item_Count_Total,
					subtotal = vm.Summary.Subtotal,
					discount = vm.Summary.Discount,
					shipping = vm.Summary.Shipping_Fee,
					grandTotal = vm.Summary.Grand_Total,
					shipMethodId = shipMethodId,
					destZip = destZip,
					couponCode = couponCode
				}
			});
		}

		// POST: /OnlineStore/Cart/UpdateQty
		[HttpPost("UpdateQty")]
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
		[HttpPost("Remove")]
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
		[HttpPost("Clear")]
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
		// ★ 共用：挑一個可用商品（若傳入的 productId 不存在，就撈一個庫內的商品）
		private async Task<(int pid, string name, decimal price, string? img)> PickProductAsync(int productId)
		{
			// 先找指定的
			var p = await _db.SProductInfos
				.AsNoTracking()
				.Where(x => x.ProductId == productId && (x.IsDeleted == null || x.IsDeleted == false))
				.Select(x => new { x.ProductId, x.ProductName, x.Price })
				.FirstOrDefaultAsync();

			// 指定的沒有就挑一個可用商品
			if (p is null)
			{
				p = await _db.SProductInfos
					.AsNoTracking()
					.Where(x => (x.IsDeleted == null || x.IsDeleted == false))
					.OrderBy(x => x.ProductId)
					.Select(x => new { x.ProductId, x.ProductName, x.Price })
					.FirstOrDefaultAsync();
			}

			if (p is null) return (0, "", 0m, null);

			// price 為非 Null decimal，不可用 ??
			return (p.ProductId, p.ProductName ?? $"PID-{p.ProductId}", p.Price, null);
		}



		// ★ 共用：把商品寫進 SO_CartItems（存在就加數量，不存在就 INSERT）
		private async Task AddItemToCartAsync(
			Guid cartId, int productId, int qty,
			string productName, decimal unitPrice, string? imageThumb)
		{
			var exist = await _db.SoCartItems
				.FirstOrDefaultAsync(x => x.CartId == cartId
									   && x.ProductId == productId
									   && (x.IsDeleted == null || x.IsDeleted == false)
									   && (x.VariantSku == null || x.VariantSku == "")); // 無規格就這樣比

			var now = DateTime.UtcNow;

			if (exist != null)
			{
				// Qty 是非 Null int
				exist.Qty += Math.Max(1, qty);
				exist.UnitPrice = unitPrice;
				exist.UpdatedAt = now;
				await _db.SaveChangesAsync();
				return;
			}

			// 新增一筆（依你的表欄位對齊）
			_db.SoCartItems.Add(new SoCartItem
			{
				CartId = cartId,
				ProductId = productId,
				ProductName = productName,
				UnitPrice = unitPrice,
				Qty = Math.Max(1, qty),   // ← 這裡用 Math.Max（不是 Math.max）
				ImageThumb = imageThumb,
				IsSelected = true,
				//ItemStatus = "Active",
				IsDeleted = false,
				CreatedAt = now,
				UpdatedAt = now
			});
			await _db.SaveChangesAsync();
		}

#if DEBUG
		// GET: /OnlineStore/Cart/DevAdd?productId=101&qty=2...
		[HttpGet("DevAdd")]
		public async Task<IActionResult> DevAdd(int productId = 101, int qty = 2,
			int shipMethodId = 1, string destZip = "320", string? couponCode = null)
		{
			try
			{
				var cartId = await EnsureCartAsync();
				var picked = await PickProductAsync(productId);
				if (picked.pid == 0)
					return StatusCode(500, new { ok = false, code = "NO_PRODUCT", message = "無可用商品可加入（請先建立商品資料）" });

				await AddItemToCartAsync(cartId, picked.pid, Math.Max(1, qty), picked.name, picked.price, picked.img);
				// 回傳整包資料給前端渲染
				return await Full(shipMethodId, destZip, couponCode);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { ok = false, code = "DEVADD_FAILED", message = ex.Message, detail = ex.InnerException?.Message });
			}
		}
		//// GET: /OnlineStore/Cart/DevSeed10?shipMethodId=1&destZip=320&couponCode=
		//// 目的：一次插入 product_id 121~130 各 1 件，用來快速測試未登入/登入購物流程
		//[HttpGet("DevSeed10")]
		//public async Task<IActionResult> DevSeed10(int shipMethodId = 1, string destZip = "320", string? couponCode = null)
		//{
		//	try
		//	{
		//		var cartId = await EnsureCartAsync();

		//		// 依序嘗試 121..130；有缺的 id 會自動略過，不會中斷測試
		//		int added = 0;
		//		for (int pid = 121; pid <= 130; pid++)
		//		{
		//			var picked = await PickProductAsync(pid);
		//			if (picked.pid == 0) continue; // 該商品不存在或被刪除 → 略過

		//			await AddItemToCartAsync(
		//				cartId: cartId,
		//				productId: picked.pid,
		//				qty: 1,
		//				productName: picked.name,
		//				unitPrice: picked.price,
		//				imageThumb: picked.img
		//			);
		//			added++;
		//		}

		//		if (added == 0)
		//		{
		//			// 都找不到 → 回傳清楚訊息給前端
		//			return StatusCode(500, new { ok = false, code = "NO_PRODUCT_121_130", message = "121~130 都沒有可加入的商品（請先建立商品資料）" });
		//		}

		//		// 回傳最新購物車（跟原本 DevAdd 一樣走 Full()，前端可直接重繪）
		//		return await Full(shipMethodId, destZip, couponCode);
		//	}
		//	catch (Exception ex)
		//	{
		//		return StatusCode(500, new { ok = false, code = "DEVSEED10_FAIL", message = ex.Message, detail = ex.InnerException?.Message });
		//	}
		//}
#endif
		// 使用者在購物車按「前往結帳」
		[HttpPost("GoCheckout")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> GoCheckout()
		{
			var userId = await _me.GetUserIdAsync();
			if (userId <= 0)
			{
				TempData["Toast"] = "請先登入會員再結帳"; // 你前端可接這個顯示提示
				var back = Url.Action("Index", "Cart", new { area = "OnlineStore" }) ?? "/OnlineStore/Cart";
				// 這裡的登入 URL 視你們 Services 專案路由調整（常見：/Login 或 /Account/Login）
				var loginUrl = $"/Login?returnUrl={Uri.EscapeDataString(back)}";
				return Redirect(loginUrl);
			}

			// 已登入 → 進入結帳流程
			return RedirectToAction("Step1", "Checkout", new { area = "OnlineStore" });
		}

		// POST: /OnlineStore/Cart/InsertTestItem
		[HttpPost("InsertTestItem")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> InsertTestItem()
		{
			try
			{
				var cartId = await EnsureCartAsync();
				var picked = await PickProductAsync(101);
				if (picked.pid == 0)
				{
					TempData["Toast"] = "無可用商品可加入，請先建立商品。";
					return RedirectToAction("Index");
				}

				await AddItemToCartAsync(cartId, picked.pid, 1, picked.name, picked.price, picked.img);
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["Toast"] = "插入失敗：" + ex.Message;
				return RedirectToAction("Index");
			}
		}
		[HttpGet("DevSeed10")]
		public async Task<IActionResult> DevSeed10(int shipMethodId = 1, string destZip = "320", string? couponCode = null)
		{
			try
			{
				var cartId = await EnsureCartAsync();

				int added = 0;
				for (int pid = 121; pid <= 130; pid++)
				{
					var picked = await PickProductAsync(pid);   // 你現有的挑商品方法
					if (picked.pid == 0) continue;

					await AddItemToCartAsync(
						cartId: cartId,
						productId: picked.pid,
						qty: 1,
						productName: picked.name,
						unitPrice: picked.price,
						imageThumb: picked.img
					);
					added++;
				}

				if (added == 0)
					return StatusCode(500, new { ok = false, code = "NO_PRODUCT_121_130", message = "121~130 都沒有可加入的商品" });

				// 回傳 Full() 讓前端重繪（沿用你原本流程）
				return await Full(shipMethodId, destZip, couponCode);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { ok = false, code = "DEVSEED10_FAIL", message = ex.Message, detail = ex.InnerException?.Message });
			}
		}
	}
}
