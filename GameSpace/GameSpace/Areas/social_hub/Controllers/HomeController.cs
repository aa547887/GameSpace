// Areas/social_hub/Controllers/HomeController.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;      // SignInAsync / SignOutAsync
using Microsoft.AspNetCore.Identity;           // IdentityConstants
using Microsoft.AspNetCore.Authorization;      // AllowAnonymous
using System.Security.Claims;                  // Claims
using GameSpace.Infrastructure.Login;          // ILoginIdentity（顯示 WhoAmI 用）
using GameSpace.Areas.social_hub.Filters;      // SocialHubAuthAttribute（把 Claims → Items["gs_*"]）

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	// 不強制登入，但每次請求仍會把外部身分映射到 Items["gs_id"]/["gs_kind"]
	[SocialHubAuth(RequireAuthenticated = false)]
	public class HomeController : Controller
	{
		private readonly ILoginIdentity _login;         // 統一讀身分（WhoAmI 顯示）
		private const string AdminCookieScheme = "AdminCookie"; // 與 Program.cs 保持一致

		public HomeController(ILoginIdentity login) => _login = login;

		[HttpGet, AllowAnonymous]
		public IActionResult Index() => RedirectToAction(nameof(WhoAmI));

		// SocialHubAuth 預設導回此路徑（未登入時）
		// 現在不提供傳統登入頁，直接導到 WhoAmI（頁面上有快速登入按鈕）
		[HttpGet, AllowAnonymous]
		public IActionResult Login(string? returnUrl = null) => RedirectToAction(nameof(WhoAmI));

		// ======================================================
		// 共用：簽入 AdminCookie（唯一真實來源）
		// 只寫兩個關鍵 Claim：
		//   1) NameIdentifier = id
		//   2) IsManager = "true"（僅管理員時）
		// SocialHubAuth 會用這些值映射成 Items["gs_id"]/["gs_kind"]。
		// ======================================================
		private async Task SignInAsAsync(string kind, int id, bool persistent)
		{
			// 切換身分前先登出舊的
			await HttpContext.SignOutAsync(AdminCookieScheme);

			var claims = new System.Collections.Generic.List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, id.ToString())
			};
			if (string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase))
				claims.Add(new Claim("IsManager", "true"));

			// （可選）各加一個自定義 id，方便除錯/相容
			claims.Add(new Claim(
				string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase) ? "mgr:id" : "usr:id",
				id.ToString()
			));

			var identity = new ClaimsIdentity(
				claims,
				AdminCookieScheme,
				ClaimTypes.Name,   // NameClaimType
				ClaimTypes.Role    // RoleClaimType（目前未用）
			);

			var principal = new ClaimsPrincipal(identity);

			var props = new AuthenticationProperties
			{
				IsPersistent = persistent,
				ExpiresUtc = persistent ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(4)
			};

			await HttpContext.SignInAsync(AdminCookieScheme, principal, props);
		}

		// ====================== 登出 ======================
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> Logout()
		{
			// 登出「唯一真實來源」Cookie（AdminCookie），以及 Identity（若有）
			await HttpContext.SignOutAsync(AdminCookieScheme);
			await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

			// 保險：清掉舊測試用 gs_*（即便目前未再使用）
			var baseOpt = new CookieOptions
			{
				Path = "/",
				SameSite = SameSiteMode.Lax,
				Secure = HttpContext?.Request?.IsHttps ?? false,
				HttpOnly = true,
				IsEssential = true
			};
			Response.Cookies.Delete("gs_id", baseOpt);
			Response.Cookies.Delete("gs_kind", baseOpt);

			TempData["msg"] = "已登出";
			return RedirectToAction(nameof(WhoAmI));
		}

		// ====================== 我是誰（顯示用） ======================
		// 主要顯示以 ILoginIdentity 讀取（外部身分優先）。
		// 若本動作未先被 SocialHubAuth 寫入 Items，這裡會「回填」一次 Items["gs_id"]/["gs_kind"]，
		// 讓你的檢視頁「中間變數」偵錯區塊也能看到。
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> WhoAmI()
		{
			var me = await _login.GetAsync();

			// 回填 Items（保險用；通常由 SocialHubAuth 先寫好）
			if (me.IsAuthenticated)
			{
				if (HttpContext.Items["gs_id"] is null)
					HttpContext.Items["gs_id"] = me.EffectiveId;

				if (HttpContext.Items["gs_kind"] is null)
					HttpContext.Items["gs_kind"] =
						string.Equals(me.Kind, "manager", StringComparison.OrdinalIgnoreCase) ? "manager" :
						(string.Equals(me.Kind, "user", StringComparison.OrdinalIgnoreCase) ? "user" : "");
			}

			ViewData["msg"] = TempData["msg"] as string ?? "";
			ViewData["kind"] = me.Kind ?? "(未設定)";
			ViewData["id"] = me.IsAuthenticated ? me.EffectiveId.ToString() : "（未登入）";
			return View();
		}

		// ====================== 快速登入（測試） ======================
		// 依你的方針：快速登入 = 直接「重簽 AdminCookie」，不再寫 gs_* Cookie。
		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> LoginUser(int uid)
		{
			if (uid <= 0) return BadRequest("uid 必須為正整數。");
			await SignInAsAsync("user", uid, persistent: false);
			TempData["msg"] = $"Login OK：User #{uid}";
			return RedirectToAction(nameof(WhoAmI));
		}

		[HttpGet, AllowAnonymous]
		public async Task<IActionResult> LoginManager(int mid)
		{
			if (mid <= 0) return BadRequest("mid 必須為正整數。");
			await SignInAsAsync("manager", mid, persistent: false);
			TempData["msg"] = $"Login OK：Manager #{mid}";
			return RedirectToAction(nameof(WhoAmI));
		}
	}
}
