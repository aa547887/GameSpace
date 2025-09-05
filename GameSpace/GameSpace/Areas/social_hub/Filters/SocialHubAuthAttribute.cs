using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameSpace.Areas.social_hub.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class SocialHubAuthAttribute : Attribute, IAsyncActionFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var http = context.HttpContext;

			// 檢查 cookie 是否有 sh_uid
			if (!http.Request.Cookies.TryGetValue("sh_uid", out var uid) || !int.TryParse(uid, out var userId))
			{
				var returnUrl = Uri.EscapeDataString(http.Request.Path + http.Request.QueryString);
				context.Result = new RedirectResult($"/social_hub/Home/Login?returnUrl={returnUrl}");
				return;
			}

			// 用 DbContext 驗證此使用者是否存在
			var db = http.RequestServices.GetService(typeof(GameSpacedatabaseContext)) as GameSpacedatabaseContext;
			var user = await db!.Users.FindAsync(userId);
			if (user == null)
			{
				context.Result = new RedirectResult("/social_hub/Home/Login");
				return;
			}

			// 放到 Items 方便後續使用
			http.Items["SH_User"] = user;
			await next();
		}
	}
}
