using GameSpace.Areas.social_hub.Models;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class HomeController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		public HomeController(GameSpacedatabaseContext context) => _context = context;

		// 首頁
		public IActionResult Index() => View();

		// ======== 一般使用者登入 ========
		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			return View(new LoginViewModel { ReturnUrl = returnUrl });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			var user = await _context.Users
				.FirstOrDefaultAsync(u =>
					(u.UserAccount != null && u.UserAccount == model.Account)
				 || (u.UserName != null && u.UserName == model.Account));

			if (user == null || user.UserPassword != model.Password)
			{
				ModelState.AddModelError(string.Empty, "帳號或密碼不正確");
				return View(model);
			}

			var options = new CookieOptions
			{
				HttpOnly = true,
				SameSite = SameSiteMode.Lax,
				Expires = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8),
				Path = "/"
			};
			// 設定使用者 Cookie
			Response.Cookies.Append("sh_uid", user.UserId.ToString(), options);
			Response.Cookies.Append("sh_uname", user.UserName ?? user.UserAccount ?? "", options);
			// 避免身分混淆：清掉管理員 Cookie（可留可刪，建議清）
			Response.Cookies.Delete("sh_is_admin");
			Response.Cookies.Delete("sh_mid");
			Response.Cookies.Delete("sh_mname");

			if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
				return Redirect(model.ReturnUrl);

			return RedirectToAction(nameof(Index));
		}

		// ======== 管理員登入 ========
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

			// 假設你的管理員主檔是 ManagerData（EF 實體多半是 ManagerDatum，DbSet 叫 ManagerData）
			// 欄位：ManagerAccount / ManagerPassword / ManagerName / ManagerId
			var manager = await _context.ManagerData
				.FirstOrDefaultAsync(m => m.ManagerAccount == model.Account);

			if (manager == null || manager.ManagerPassword != model.Password)
			{
				ModelState.AddModelError(string.Empty, "帳號或密碼不正確（管理員）");
				return View(model);
			}

			

			

			var options = new CookieOptions
			{
				HttpOnly = true,
				SameSite = SameSiteMode.Lax,
				Expires = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8),
				Path = "/"
			};
			// 設定管理員 Cookie
			Response.Cookies.Append("sh_is_admin", "1", options);
			Response.Cookies.Append("sh_mid", manager.ManagerId.ToString(), options);
			Response.Cookies.Append("sh_mname", manager.ManagerName ?? manager.ManagerAccount, options);
			// 避免身分混淆：清掉使用者 Cookie（可留可刪，建議清）
			Response.Cookies.Delete("sh_uid");
			Response.Cookies.Delete("sh_uname");

			if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
				return Redirect(model.ReturnUrl);

			return RedirectToAction(nameof(Index));
		}

		// ======== 登出（同時清除兩種身分） ========
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Logout()
		{
			// 使用者
			Response.Cookies.Delete("sh_uid");
			Response.Cookies.Delete("sh_uname");
			// 管理員
			Response.Cookies.Delete("sh_is_admin");
			Response.Cookies.Delete("sh_mid");
			Response.Cookies.Delete("sh_mname");

			return RedirectToAction(nameof(Login));
		}
	}
}
