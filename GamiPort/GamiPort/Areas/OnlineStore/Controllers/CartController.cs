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

		private int? GetUserIdOrNull()
		{
			if (User?.Identity?.IsAuthenticated == true)
			{
				var val = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("UserId");
				if (int.TryParse(val, out var uid)) return uid;
			}
			return null;
		}

		private string GetConnString()
		{
			// 先從 DbContext 拿；若為 null，再從設定檔拿
			var cs = _db.Database.GetConnectionString();
			if (string.IsNullOrWhiteSpace(cs))
			{
				var cfg = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
				cs = cfg.GetConnectionString("GameSpacedatabase");
			}
			return cs ?? throw new InvalidOperationException("找不到資料庫連線字串。");
		}

		// ★ 最終版：匿名→(如已登入)合併→取回最終 cart_id；寫回 Session + cart_id Cookie
		private async Task<Guid> EnsureCartAsync()
		{
			// A) 匿名 token（gp_anonymous）
			var anonToken = GamiPort.Areas.OnlineStore.Utils.AnonCookie.GetOrSet(HttpContext);

			// B) 先以匿名 token 取「匿名車 cart_id」
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

			// C) 若已登入 → 合併匿名→會員，再取「會員最終 cart_id」
			var userId = await _me.GetUserIdAsync();
			Guid finalCartId = anonCartId;

			if (userId > 0)
			{
				using (var conn = new SqlConnection(GetConnString()))
				{
					await conn.OpenAsync();

					// AttachToUser：把匿名車掛給會員（由 SP 內部處理合併/歸戶）
					using (var cmd = new SqlCommand("dbo.usp_Cart_AttachToUser", conn) { CommandType = CommandType.StoredProcedure })
					{
						cmd.Parameters.Add(new SqlParameter("@CartId", SqlDbType.UniqueIdentifier) { Value = anonCartId });
						cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
						await cmd.ExecuteNonQueryAsync();
					}

					// 以會員身分再取一次「最終 cart_id」
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

			// D) 寫回 Session + 覆寫 cart_id Cookie（保持你現有行為）
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

			return finalCartId;
		}
		// GET: /OnlineStore/Cart/CountJson
		[HttpGet("CountJson")]
		public async Task<IActionResult> CountJson()
		{
			var cartId = await EnsureCartAsync();
			var count = await _cart.GetItemCountAsync(cartId);
			return Json(new { count });
		}

		// GET: /OnlineStore/Cart
		[HttpGet("")]
		public async Task<IActionResult> Index(int shipMethodId = 1, string destZip = "320", string? couponCode = null)
		{
			// 保留你原先的 cookie 取得 + 會員附著流程
			var cartId = CartCookie.GetOrCreate(HttpContext);

			var userId = await _me.GetUserIdAsync();
			if (userId > 0 && cartId != Guid.Empty)
			{
				await using var conn = _db.Database.GetDbConnection();
				if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
				await using var cmd = conn.CreateCommand();
				cmd.CommandText = "dbo.usp_Cart_AttachToUser";
				cmd.CommandType = System.Data.CommandType.StoredProcedure;
				cmd.Parameters.Add(new SqlParameter("@CartId", cartId));
				cmd.Parameters.Add(new SqlParameter("@UserId", userId));
				await cmd.ExecuteNonQueryAsync();
			}

			ViewData["shipMethodId"] = shipMethodId;
			ViewData["destZip"] = destZip;
			ViewData["couponCode"] = couponCode ?? "";
			return View();
		}

		// ───────────────────────────── 舊版轉接 ─────────────────────────────
		[NonAction]
		public Task<IActionResult> Full(int shipMethodId, string destZip, string? couponCode)
			=> Full(shipMethodId, destZip, couponCode, null);

		// 主要版本（支援 selected）
		[HttpGet("Full")]
		public async Task<IActionResult> Full(int shipMethodId, string destZip, string? couponCode, string? selected)
		{
			var cartId = await EnsureCartAsync(); // ← 會自動處理匿名→會員合併

			var vm = await _cart.GetFullAsync(cartId, shipMethodId, destZip ?? "", couponCode);

			// 保留原本 selected 邏輯（完全不動你的輸出格式）
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
				return Json(new { ok = false, code = "UPDATE_FAILED", message = ex.Message });
			}
		}

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

		private async Task<(int pid, string name, decimal price, string? img)> PickProductAsync(int productId)
		{
			var p = await _db.SProductInfos
				.AsNoTracking()
				.Where(x => x.ProductId == productId && (x.IsDeleted == null || x.IsDeleted == false))
				.Select(x => new { x.ProductId, x.ProductName, x.Price })
				.FirstOrDefaultAsync();

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
			return (p.ProductId, p.ProductName ?? $"PID-{p.ProductId}", p.Price, null);
		}

		private async Task AddItemToCartAsync(
			Guid cartId, int productId, int qty,
			string productName, decimal unitPrice, string? imageThumb)
		{
			var exist = await _db.SoCartItems
				.FirstOrDefaultAsync(x => x.CartId == cartId
									   && x.ProductId == productId
									   && (x.IsDeleted == null || x.IsDeleted == false)
									   && (x.VariantSku == null || x.VariantSku == ""));

			var now = DateTime.UtcNow;

			if (exist != null)
			{
				exist.Qty += Math.Max(1, qty);
				exist.UnitPrice = unitPrice;
				exist.UpdatedAt = now;
				await _db.SaveChangesAsync();
				return;
			}

			_db.SoCartItems.Add(new SoCartItem
			{
				CartId = cartId,
				ProductId = productId,
				ProductName = productName,
				UnitPrice = unitPrice,
				Qty = Math.Max(1, qty),
				ImageThumb = imageThumb,
				IsSelected = true,
				IsDeleted = false,
				CreatedAt = now,
				UpdatedAt = now
			});
			await _db.SaveChangesAsync();
		}

#if DEBUG
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
				return await Full(shipMethodId, destZip, couponCode);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { ok = false, code = "DEVADD_FAILED", message = ex.Message, detail = ex.InnerException?.Message });
			}
		}
#endif

		// 使用者在購物車按「前往結帳」
		[HttpPost("GoCheckout")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> GoCheckout([FromForm] string? selected)
		{
			var userId = await _me.GetUserIdAsync();

			var onePageUrl = Url.Action("OnePage", "Checkout", new { area = "OnlineStore", selected })
							 ?? "/OnlineStore/Checkout/OnePage" + (string.IsNullOrEmpty(selected) ? "" : $"?selected={Uri.EscapeDataString(selected)}");

			if (userId <= 0)
			{
				TempData["Toast"] = "請先登入會員再結帳";
				var loginUrl =
					Url.Action("Login", "Login", new { area = "Login", ReturnUrl = onePageUrl }) ??
					Url.Action("Index", "Login", new { area = "Login", ReturnUrl = onePageUrl }) ??
					"/Identity/Account/Login?ReturnUrl=" + Uri.EscapeDataString(onePageUrl);
				return Redirect(loginUrl);
			}

			return Redirect(onePageUrl);
		}

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
					var picked = await PickProductAsync(pid);
					if (picked.pid == 0) continue;

					await AddItemToCartAsync(cartId, picked.pid, 1, picked.name, picked.price, picked.img);
					added++;
				}

				if (added == 0)
					return StatusCode(500, new { ok = false, code = "NO_PRODUCT_121_130", message = "121~130 都沒有可加入的商品" });

				return await Full(shipMethodId, destZip, couponCode);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { ok = false, code = "DEVSEED10_FAIL", message = ex.Message, detail = ex.InnerException?.Message });
			}
		}
	}
}
