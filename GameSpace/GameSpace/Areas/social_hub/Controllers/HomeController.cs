using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

// 依你的 DbContext 實際命名空間擇一或都留著
using GameSpace.Models; // 若 DbContext 在 Models
using GameSpace.Data;   // 若 DbContext 在 Data

using GameSpace.Areas.social_hub.Models;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class HomeController : Controller
	{
		private readonly GameSpacedatabaseContext? _db; // 允許為 null：沒有 DB 也能測試（數字即 ID）
		public HomeController(GameSpacedatabaseContext? db = null)
		{
			_db = db;
		}

		[HttpGet]
		public IActionResult Index() => View();

		private const string GsId = "gs_id";
		private const string GsKind = "gs_kind"; // user / manager

		private CookieOptions BuildCookieOptions(bool rememberMe)
		{
			return new CookieOptions
			{
				HttpOnly = true,
				SameSite = SameSiteMode.Lax,
				Secure = HttpContext?.Request?.IsHttps ?? false,
				IsEssential = true,
				Path = "/",
				Expires = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
			};
		}

		private bool IsLocalUrlSafe(string? url) =>
			!string.IsNullOrWhiteSpace(url) && Url.IsLocalUrl(url);

		private IActionResult LoginSucceededRedirect(string kind, string? returnUrl)
		{
			if (IsLocalUrlSafe(returnUrl))
				return Redirect(returnUrl!);

			// 預設導向：統一到通知中心（使用者可看到自己的通知；管理員顯示建置中）
			return RedirectToAction("Index", "MessageCenter", new { area = "social_hub" });
		}

		// ======= 管理員登入 =======
		[HttpGet]
		public IActionResult AdminLogin(string? returnUrl = null)
		{
			return View(new LoginViewModel { ReturnUrl = returnUrl });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AdminLogin(LoginViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			int? managerId = null;

			// 1) 先嘗試 DB 驗證（若有 DbContext）
			if (_db != null)
			{
				// Managers 實際表名/欄位請對應你的模型（常見：ManagerData / ManagerDatum）
				// 假設：ManagerAccount, ManagerPassword, ManagerId
				var mgr = await _db.ManagerData
					.AsNoTracking()
					.FirstOrDefaultAsync(m =>
						m.ManagerAccount == model.Account &&
						m.ManagerPassword == model.Password);

				if (mgr != null)
					managerId = mgr.ManagerId;
			}

			// 2) 找不到時，若帳號是純數字，直接視為 ID（測試用）
			if (managerId is null && int.TryParse(model.Account, out var parsedMid) && parsedMid > 0)
				managerId = parsedMid;

			if (managerId is null)
			{
				ModelState.AddModelError(string.Empty, "帳號或密碼不正確（或請直接輸入數字 ID 測試）");
				return View(model);
			}

			// 設 cookie
			var opt = BuildCookieOptions(model.RememberMe);
			Response.Cookies.Append(GsId, managerId.Value.ToString(), opt);
			Response.Cookies.Append(GsKind, "manager", opt);

			TempData["msg"] = $"管理員登入成功：#{managerId.Value}";
			return LoginSucceededRedirect("manager", model.ReturnUrl);
		}

		// ======= 使用者登入 =======
		[HttpGet]
		public IActionResult UserLogin(string? returnUrl = null)
		{
			return View(new LoginViewModel { ReturnUrl = returnUrl });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UserLogin(LoginViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			int? userId = null;

			// 1) 先嘗試 DB 驗證（若有 DbContext）
			if (_db != null)
			{
				// Users 實際欄位請對應你的模型（常見：UserAccount, UserPassword, UserId）
				var user = await _db.Users
					.AsNoTracking()
					.FirstOrDefaultAsync(u =>
						u.UserAccount == model.Account &&
						u.UserPassword == model.Password);

				if (user != null)
					userId = user.UserId;
			}

			// 2) 找不到時，若帳號是純數字，直接視為 ID（測試用）
			if (userId is null && int.TryParse(model.Account, out var parsedUid) && parsedUid > 0)
				userId = parsedUid;

			if (userId is null)
			{
				ModelState.AddModelError(string.Empty, "帳號或密碼不正確（或請直接輸入數字 ID 測試）");
				return View(model);
			}

			// 設 cookie
			var opt = BuildCookieOptions(model.RememberMe);
			Response.Cookies.Append(GsId, userId.Value.ToString(), opt);
			Response.Cookies.Append(GsKind, "user", opt);

			TempData["msg"] = $"使用者登入成功：#{userId.Value}";
			return LoginSucceededRedirect("user", model.ReturnUrl);
		}

		// ======= 登出 =======
		[HttpGet]
		public IActionResult Logout()
		{
			var baseOpt = new CookieOptions
			{
				Path = "/",
				SameSite = SameSiteMode.Lax,
				Secure = HttpContext?.Request?.IsHttps ?? false,
				HttpOnly = true,
				IsEssential = true
			};
			Response.Cookies.Delete(GsId, baseOpt);
			Response.Cookies.Delete(GsKind, baseOpt);

			TempData["msg"] = "已登出";
			return RedirectToAction(nameof(WhoAmI));
		}

		// ======= 身分顯示 =======
		[HttpGet]
		public IActionResult WhoAmI()
		{
			var id = Request.Cookies[GsId];
			var kind = Request.Cookies[GsKind] ?? "(未設定)";
			ViewData["msg"] = TempData["msg"] as string ?? "";
			ViewData["id"] = string.IsNullOrWhiteSpace(id) ? "（未登入）" : id;
			ViewData["kind"] = kind;
			return View();
		}

		// ======= （保留）快速測試登入（QueryString）=======
		[HttpGet]
		public IActionResult LoginUser(int uid)
		{
			if (uid <= 0) return BadRequest("uid 必須為正整數。");
			var opt = BuildCookieOptions(rememberMe: false);
			Response.Cookies.Append(GsId, uid.ToString(), opt);
			Response.Cookies.Append(GsKind, "user", opt);
			TempData["msg"] = $"Login OK：User #{uid}";
			return RedirectToAction(nameof(WhoAmI));
		}

		[HttpGet]
		public IActionResult LoginManager(int mid)
		{
			if (mid <= 0) return BadRequest("mid 必須為正整數。");
			var opt = BuildCookieOptions(rememberMe: false);
			Response.Cookies.Append(GsId, mid.ToString(), opt);
			Response.Cookies.Append(GsKind, "manager", opt);
			TempData["msg"] = $"Login OK：Manager #{mid}";
			return RedirectToAction(nameof(WhoAmI));
		}
	}
}
