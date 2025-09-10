using System.Threading.Tasks;
using GameSpace.Areas.social_hub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace GameSpace.Areas.social_hub.Filters
{
	/// <summary>只有具備 MessagePermissionManagement 的管理員，才能通過。</summary>
	public sealed class RequireMuteManagePermissionAttribute : Attribute, IAsyncAuthorizationFilter
	{
		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			var svc = context.HttpContext.RequestServices.GetRequiredService<IManagerPermissionService>();
			var (_, _, canManage) = await svc.GetMuteManagementContextAsync(context.HttpContext);
			if (!canManage)
			{
				// 你也可以改成 RedirectToAction 到未授權頁
				context.Result = new ForbidResult();
			}
		}
	}
}
