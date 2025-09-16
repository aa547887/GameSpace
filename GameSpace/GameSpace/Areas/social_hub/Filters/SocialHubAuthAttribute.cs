// Areas/social_hub/Filters/SocialHubAuthAttribute.cs
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameSpace.Areas.social_hub.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class SocialHubAuthAttribute : Attribute, IAsyncActionFilter
	{
		/// <summary>是否一定要登入（預設 true；建議在僅做展示/偵錯頁設為 false）。</summary>
		public bool RequireAuthenticated { get; set; } = true;

		/// <summary>若要求特定身分，設定其一。設定後會隱含需要登入。</summary>
		public bool RequireManager { get; set; } = false;
		public bool RequireUser { get; set; } = false;

		/// <summary>未登入時導向的登入頁。</summary>
		public string LoginPath { get; set; } = "/social_hub/Home/Login";

		/// <summary>無權限時導向的拒絕頁（一般頁）。</summary>
		public string AccessDeniedPath { get; set; } = "/Login/Denied";

		/// <summary>AJAX/JSON 請求時改用 401/403，而不是 Redirect。</summary>
		public bool AjaxStatusCodeInsteadOfRedirect { get; set; } = true;

		public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
		{
			var http = ctx.HttpContext;
			var req = http.Request;

			// ---- 0) 避免自我循環：若本來就在 LoginPath，就直接放行，不做任何導向 ----
			if (!string.IsNullOrWhiteSpace(LoginPath) &&
				req.Path.HasValue &&
				req.Path.Value!.StartsWith(LoginPath, StringComparison.OrdinalIgnoreCase))
			{
				await next();
				return;
			}

			// 這次請求「是否真的需要登入」：有任何要求（登入/特定身分）就算需要
			var mustBeAuthenticated = RequireAuthenticated || RequireManager || RequireUser;

			// ---- 1) 嘗試從 Claims 讀身分（外部登入 AdminCookie）----
			var isAuth = http.User?.Identity?.IsAuthenticated ?? false;

			int? eid = null;
			string? kind = null;

			if (isAuth)
			{
				var idStr =
					http.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
					http.User.FindFirst("mgr:id")?.Value ??
					http.User.FindFirst("usr:id")?.Value;

				if (int.TryParse(idStr, out var parsedId) && parsedId > 0)
					eid = parsedId;

				var isManager = string.Equals(http.User.FindFirst("IsManager")?.Value, "true", StringComparison.OrdinalIgnoreCase);
				// 如果外部只標了 IsManager=true，其餘視為 user
				kind = isManager ? "manager" : "user";
			}

			// ---- 2) 若不需要登入：絕不導向；有拿到就寫 Items，拿不到就空著 ----
			if (!mustBeAuthenticated)
			{
				if (isAuth && eid is int v && v > 0)
				{
					http.Items["gs_id"] = v;
					http.Items["gs_kind"] = string.IsNullOrWhiteSpace(kind) ? "user" : kind;

					// 兼容 SH_*（可選）
					http.Items["SH_EffectiveId"] = v;
					http.Items["SH_Kind"] = http.Items["gs_kind"];
					http.Items["SH_ManagerId"] = string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase) ? v : null;
					http.Items["SH_UserId"] = string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase) ? null : v;
				}
				await next();
				return;
			}

			// ---- 3) 需要登入：沒登入或沒有有效 ID → 未登入處理 ----
			if (!isAuth || eid is null || eid <= 0)
			{
				HandleUnauthenticated(ctx, LoginPath);
				return;
			}

			// ---- 4) 需要特定身分：檢查 kind ----
			var isMgr = string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase);
			if (RequireManager && !isMgr)
			{
				HandleForbidden(ctx, AccessDeniedPath);
				return;
			}
			if (RequireUser && isMgr)
			{
				HandleForbidden(ctx, AccessDeniedPath);
				return;
			}

			// ---- 5) 通過：寫入中間變數，讓內部程式用舊名取值 ----
			http.Items["gs_id"] = eid.Value;
			http.Items["gs_kind"] = isMgr ? "manager" : "user";

			// 兼容 SH_*（可選）
			http.Items["SH_EffectiveId"] = eid.Value;
			http.Items["SH_Kind"] = http.Items["gs_kind"];
			http.Items["SH_ManagerId"] = isMgr ? eid.Value : null;
			http.Items["SH_UserId"] = isMgr ? null : eid.Value;

			await next();
		}

		// ========== helpers ==========
		private void HandleUnauthenticated(ActionExecutingContext ctx, string loginPath)
		{
			if (AjaxStatusCodeInsteadOfRedirect && IsAjaxOrJson(ctx.HttpContext.Request))
			{
				ctx.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
				return;
			}

			var req = ctx.HttpContext.Request;
			var returnUrl = Uri.EscapeDataString(req.Path + req.QueryString);

			// 若 loginPath 自己已帶 returnUrl，就不要再塞以免重複
			var target = string.IsNullOrWhiteSpace(loginPath) ? "/" : loginPath;
			if (!target.Contains("returnUrl=", StringComparison.OrdinalIgnoreCase))
				target += (target.Contains('?') ? "&" : "?") + "returnUrl=" + returnUrl;

			ctx.Result = new RedirectResult(target);
		}

		private void HandleForbidden(ActionExecutingContext ctx, string deniedPath)
		{
			if (AjaxStatusCodeInsteadOfRedirect && IsAjaxOrJson(ctx.HttpContext.Request))
			{
				ctx.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
				return;
			}
			ctx.Result = new RedirectResult(string.IsNullOrWhiteSpace(deniedPath) ? "/" : deniedPath);
		}

		private static bool IsAjaxOrJson(HttpRequest req)
		{
			if (string.Equals(req.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
				return true;
			var accept = req.Headers["Accept"];
			return accept.Any(v => v?.IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0);
		}
	}
}
