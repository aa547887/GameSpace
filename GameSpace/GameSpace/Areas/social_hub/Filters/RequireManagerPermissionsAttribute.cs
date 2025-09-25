// Areas/social_hub/Filters/RequireManagerPermissionsAttribute.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using GameSpace.Infrastructure.Login;       // ILoginIdentity
using GameSpace.Models;                    // GameSpacedatabaseContext + EF 實體
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Filters
{
	/// <summary>
	/// 合併版授權屬性：同時負責「外部登入/身分檢查」與「細部權限」。
	/// 用法：
	///   [RequireManagerPermissions(Admin = true)]                     // 需要總管（AdministratorPrivilegesManagement）
	///   [RequireManagerPermissions(CustomerService = true)]           // 需要客服權限（customer_service）
	///   [RequireManagerPermissions(MuteManage = true)]                // 需要穢語/靜音管理權限
	///   [RequireManagerPermissions(MessageManage = true)]             // 需要訊息管理權限
	///   [RequireManagerPermissions(RequireManager = true, CustomerService = true)] // 僅限管理員 + 客服
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireManagerPermissionsAttribute : Attribute, IAsyncActionFilter
	{
		// ===== 身分要求 =====
		/// <summary>是否限定「管理員」身分（預設 true）。</summary>
		public bool RequireManager { get; set; } = true;

		/// <summary>是否接受「一般使用者」也能通過（預設 false）。若為 true，僅檢查已登入，不做管理員/權限檢查。</summary>
		public bool AllowUser { get; set; } = false;

		/// <summary>AJAX/JSON 請求時，是否直接回 401/403（不做 Redirect）。</summary>
		public bool AjaxStatusCodeInsteadOfRedirect { get; set; } = true;

		// ===== 細部權限（任一角色擁有為 true 即通過）=====
		public bool Admin { get; set; } = false;             // AdministratorPrivilegesManagement
		public bool CustomerService { get; set; } = false;   // customer_service
		public bool MuteManage { get; set; } = false;        // 穢語/靜音管理（對應你的 rp 欄位）
		public bool MessageManage { get; set; } = false;     // 訊息管理
		public bool ShoppingManage { get; set; } = false;    // 購物管理
		public bool PetManage { get; set; } = false;         // 寵物管理
		public bool UserStatusManage { get; set; } = false;  // 使用者狀態管理

		public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
		{
			var http = ctx.HttpContext;

			// 1) 取得統一身分（外部 AdminCookie/Claims 優先；未登入就會 IsAuthenticated=false）
			var login = http.RequestServices.GetService(typeof(ILoginIdentity)) as ILoginIdentity;
			if (login == null)
			{
				FailUnauthorized(ctx);
				return;
			}
			var me = await login.GetAsync();

			// 2) 必須已登入
			if (!me.IsAuthenticated)
			{
				FailUnauthorized(ctx);
				return;
			}

			// 3) 如果允許使用者，且當前是 user，直接放行（不做管理員權限檢查）
			if (AllowUser && string.Equals(me.Kind, "user", StringComparison.OrdinalIgnoreCase))
			{
				MapToItemsForView(http, me.Kind!, me.EffectiveId);
				await next();
				return;
			}

			// 4) 預設：需要管理員身分
			if (RequireManager)
			{
				if (!string.Equals(me.Kind, "manager", StringComparison.OrdinalIgnoreCase) || me.ManagerId is null)
				{
					FailForbidden(ctx);
					return;
				}
			}

			// 若沒有指定任何細部權限旗標，只要「是管理員且已登入」就放行
			bool needDetailPerm =
				Admin || CustomerService || MuteManage || MessageManage ||
				ShoppingManage || PetManage || UserStatusManage;

			if (!needDetailPerm)
			{
				MapToItemsForView(http, "manager", me.ManagerId ?? me.EffectiveId);
				await next();
				return;
			}

			// 5) 細部權限檢查（以 ManagerDatum -> ManagerRoles 多對多的布林欄位為準）
			var db = http.RequestServices.GetService(typeof(GameSpacedatabaseContext)) as GameSpacedatabaseContext;
			if (db == null)
			{
				FailForbidden(ctx);
				return;
			}

			var mid = me.ManagerId ?? 0;
			var rolePerms = await db.ManagerData
				.AsNoTracking()
				.Where(m => m.ManagerId == mid)
				.SelectMany(m => m.ManagerRoles)
				.Select(rp => new
				{
					rp.AdministratorPrivilegesManagement,
					rp.CustomerService,
					rp.MessagePermissionManagement,
					rp.ShoppingPermissionManagement,
					rp.PetRightsManagement,
					rp.UserStatusManagement
					// 🔧 如需「靜音管理」等自訂欄位，請在這裡加上 rp.YourColumn
				})
				.ToListAsync();

			bool ok =
				(!Admin || rolePerms.Any(p => p.AdministratorPrivilegesManagement == true)) &&
				(!CustomerService || rolePerms.Any(p => p.CustomerService == true)) &&
				(!MessageManage || rolePerms.Any(p => p.MessagePermissionManagement == true)) &&
				(!ShoppingManage || rolePerms.Any(p => p.ShoppingPermissionManagement == true)) &&
				(!PetManage || rolePerms.Any(p => p.PetRightsManagement == true)) &&
				(!UserStatusManage || rolePerms.Any(p => p.UserStatusManagement == true));
			// MuteManage：若你有對應欄位，請比照上面加一條 AND 條件

			if (!ok)
			{
				FailForbidden(ctx);
				return;
			}

			// 6) 映射到 Items（供 View/其他程式碼取用，不再依賴 gs_* cookie）
			MapToItemsForView(http, "manager", mid);

			await next();
		}

		private void MapToItemsForView(HttpContext http, string kind, int id)
		{
			http.Items["gs_kind"] = kind; // "manager" / "user"
			http.Items["gs_id"] = id;     // 目前有效 ID
		}

		private void FailUnauthorized(ActionExecutingContext ctx)
		{
			if (AjaxStatusCodeInsteadOfRedirect && IsAjaxOrJson(ctx.HttpContext.Request))
			{
				ctx.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
			}
			else
			{
				ctx.Result = new UnauthorizedResult();
			}
		}

		private void FailForbidden(ActionExecutingContext ctx)
		{
			if (AjaxStatusCodeInsteadOfRedirect && IsAjaxOrJson(ctx.HttpContext.Request))
			{
				ctx.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
			}
			else
			{
				ctx.Result = new ForbidResult();
			}
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
