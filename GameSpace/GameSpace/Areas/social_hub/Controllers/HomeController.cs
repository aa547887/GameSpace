// Areas/social_hub/Controllers/HomeController.cs
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using GameSpace.Infrastructure.Login;          // ILoginIdentity
using GameSpace.Areas.social_hub.Filters;     // SocialHubAuthAttribute

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	// 不強制登入；每次請求仍會由 SocialHubAuth 把 Claims → Items["gs_*"]
	[SocialHubAuth(RequireAuthenticated = false)]
	public class HomeController : Controller
	{
		private readonly ILoginIdentity _login;
		private const string AuthScheme = "AdminCookie"; // 與 Program.cs 一致

		public HomeController(ILoginIdentity login) => _login = login;

		[HttpGet, AllowAnonymous]
		public IActionResult Index() => RedirectToAction(nameof(WhoAmI));

		[HttpGet, AllowAnonymous]
		public IActionResult Login(string? returnUrl = null) => RedirectToAction(nameof(WhoAmI));

		// ========== 共用：簽入（user / manager） ==========
		private async Task SignInAsAsync(string kind, int id, bool persistent = false)
		{
			// 先登出舊的
			try { await HttpContext.SignOutAsync(AuthScheme); } catch { /* ignore */ }
			try { await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); } catch { /* ignore */ }

			var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, id.ToString()) };
			if (string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase))
				claims.Add(new Claim("IsManager", "true"));          // SocialHubAuth 用這個判斷 manager
			claims.Add(new Claim($"sh:{kind}:id", id.ToString()));   // 方便除錯

			var identity = new ClaimsIdentity(claims, AuthScheme);
			var principal = new ClaimsPrincipal(identity);
			var props = new AuthenticationProperties
			{
				IsPersistent = persistent,
				ExpiresUtc = persistent ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(4)
			};

			await HttpContext.SignInAsync(AuthScheme, principal, props);
		}

		// ========== 快速登入（唯一入口） ==========
		// 例：/social_hub/Home/QuickLogin?kind=user&id=10000012
		//     /social_hub/Home/QuickLogin?kind=manager&id=30000001
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> QuickLogin(string kind, int id, bool keep = false)
		{
			if (!string.Equals(kind, "user", StringComparison.OrdinalIgnoreCase) &&
				!string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase))
				return BadRequest("kind 只能是 user 或 manager");
			if (id <= 0) return BadRequest("id 必須為正整數。");

			await SignInAsAsync(kind.ToLowerInvariant(), id, persistent: keep);
			TempData["msg"] = $"Login OK：{kind} #{id}";
			return RedirectToAction(nameof(WhoAmI));
		}

		// ========== 登出 ==========
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> Logout()
		{
			try { await HttpContext.SignOutAsync(AuthScheme); } catch { /* ignore */ }
			try { await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme); } catch { /* ignore */ }
			TempData["msg"] = "已登出。";
			return RedirectToAction(nameof(WhoAmI));
		}

		// ========== 我是誰（顯示用） ==========
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> WhoAmI()
		{
			var me = await _login.GetAsync();

			// 回填 Items（保險；通常 SocialHubAuth 已先寫好）
			if (me.IsAuthenticated)
			{
				if (HttpContext.Items["gs_id"] is null) HttpContext.Items["gs_id"] = me.EffectiveId;
				if (HttpContext.Items["gs_kind"] is null) HttpContext.Items["gs_kind"] = me.Kind ?? "";
			}

			ViewData["msg"] = TempData["msg"] as string ?? "";
			ViewData["kind"] = me.Kind ?? "(未設定)";
			ViewData["id"] = me.IsAuthenticated ? me.EffectiveId.ToString() : "（未登入）";
			return View(); // 對應你現有的 WhoAmI.cshtml
		}
	}
}
