using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GameSpace.Filters
{
	/// <summary>
	/// 僅允許指定 managerrole_id 的使用者進入（預設 1,2,8）。
	/// 從 cookie "sh_uid" 取得目前使用者的 ManagerId，透過 ManagerDatum.ManagerRoles 檢查角色。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class AdminOnlyAttribute : Attribute, IAsyncActionFilter
	{
		public int[] AllowedRoles { get; set; } = new[] { 1, 2, 8 };

		public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
		{
			var http = ctx.HttpContext;

			// 取目前登入者 id（社群區使用 cookie "sh_uid"）
			if (!http.Request.Cookies.TryGetValue("sh_uid", out var s) || !int.TryParse(s, out var uid) || uid <= 0)
			{
				ctx.Result = new ForbidResult(); // 未登入或無效
				return;
			}

			// 解析 DbContext
			var db = http.RequestServices.GetService(typeof(GameSpacedatabaseContext)) as GameSpacedatabaseContext;
			if (db == null)
			{
				ctx.Result = new StatusCodeResult(500);
				return;
			}

			// 經由多對多關聯（ManagerDatum -> ManagerRoles）檢查角色是否在 {1,2,8}
			var isAdmin = await db.ManagerData
				.AsNoTracking()
				.Where(m => m.ManagerId == uid)
				.SelectMany(m => m.ManagerRoles)               // ManagerRolePermission 導覽集合
				.AnyAsync(rp => AllowedRoles.Contains(rp.ManagerRoleId));

			if (!isAdmin)
			{
				ctx.Result = new ForbidResult();
				return;
			}

			await next();
		}
	}
}
