using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using GameSpace.Infrastructure.Login;

namespace GameSpace.Areas.social_hub.Filters
{
	/// <summary>
	/// social_hub 區域專用的身分過濾器：
	/// 1) 若尚未登入，嘗試用 AdminCookie 進行 Authenticate（不動全站 Program.cs）。
	/// 2) 若本次請求已能取得有效身分（ILoginIdentity），就：
	///    - 回寫 HttpContext.Items["gs_id"/"gs_kind"]（供舊程式在本次請求使用）
	///    - 確保 response 種下相容 cookies：gs_id / gs_kind（給前端 JS / 外部模組在後續請求讀）
	/// 3) RequireAuthenticated=true 時，仍未登入則 401（AJAX）或 Redirect 到 /Login。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class SocialHubAuthAttribute : Attribute, IAsyncActionFilter
	{
		/// <summary>是否強制需要登入；預設 true。</summary>
		public bool RequireAuthenticated { get; set; } = true;

		/// <summary>未登入時（非 AJAX）導向路徑。</summary>
		public string? DeniedPath { get; set; } = "/Login";

		private const string AdminCookieScheme = "AdminCookie";

		public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
		{
			var http = ctx.HttpContext;

			// 1) 若目前主體未登入 → 嘗試用 AdminCookie 取回 Principal（只在本 Area 做，不動全站）
			if (!(http.User?.Identity?.IsAuthenticated ?? false))
			{
				var adminAuth = await http.AuthenticateAsync(AdminCookieScheme);
				if (adminAuth.Succeeded && adminAuth.Principal != null)
				{
					http.User = adminAuth.Principal; // 只影響這次請求的 HttpContext.User
				}
			}

			// 2) 透過 ILoginIdentity 取得統一身分
			var login = http.RequestServices.GetRequiredService<ILoginIdentity>();
			var me = await login.GetAsync();

			// 3) 若需要登入但仍未登入 → 友善處理
			if (RequireAuthenticated && !me.IsAuthenticated)
			{
				if (IsAjaxOrJson(http.Request))
				{
					ctx.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
					return;
				}
				ctx.Result = new RedirectResult(string.IsNullOrWhiteSpace(DeniedPath) ? "/Login" : DeniedPath);
				return;
			}

			// 4) 已有身分 → 回填 Items（本次請求可用）+ 確保相容 cookies（後續請求/前端 JS 可讀）
			if (me.IsAuthenticated)
			{
				// (a) Items（只活在這次請求）
				http.Items["gs_id"] = me.EffectiveId;
				http.Items["gs_kind"] = me.Kind ?? "user";

				// (b) Cookies：若未設或不一致就補齊；確保前端 JS 可讀（HttpOnly=false）
				var cid = http.Request.Cookies["gs_id"];
				var ckind = http.Request.Cookies["gs_kind"];
				var needWrite =
					!int.TryParse(cid, out var rid) || rid != me.EffectiveId ||
					string.IsNullOrWhiteSpace(ckind) || !ckind.Equals(me.Kind, StringComparison.OrdinalIgnoreCase);

				if (needWrite)
				{
					WriteCompatCookies(http, me.Kind ?? "user", me.EffectiveId);
				}
			}

			// 放行
			await next();
		}

		private static bool IsAjaxOrJson(HttpRequest req)
		{
			if (string.Equals(req.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
				return true;
			var accept = req.Headers["Accept"];
			return accept.Any(v => v?.IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0);
		}

		/// <summary>寫入相容 cookies：gs_id / gs_kind（給前端 JS / 外部模組讀）</summary>
		private static void WriteCompatCookies(HttpContext http, string kind, int id)
		{
			// 與 AdminCookie 對齊的短效到期（你需要長效可自行調整）
			var expires = DateTimeOffset.UtcNow.AddHours(4);

			var opts = new CookieOptions
			{
				HttpOnly = false,                          // ★ 前端 JS 需要讀取 → false
				IsEssential = true,
				Path = "/",                                // ★ 全站可見（避免只在 /social_hub 底下）
				SameSite = SameSiteMode.Lax,               // 同站足夠；若跨站嵌入請用 None + Secure
				Secure = http.Request.IsHttps,             // dev http:false；prod https:true
				Expires = expires
			};

			http.Response.Cookies.Append("gs_id", id.ToString(), opts);
			http.Response.Cookies.Append("gs_kind", kind.ToLowerInvariant(), opts);
		}
	}
}
