using GameSpace.Areas.social_hub.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Filters
{
	/// <summary>
	/// 只有 customer_service=true 的管理員才能進入。
	/// 用法：套在 Controller 或 Action 上。
	/// </summary>
	public class CustomerServiceOnlyAttribute : TypeFilterAttribute
	{
		public CustomerServiceOnlyAttribute() : base(typeof(CustomerServiceOnlyFilter)) { }

		private class CustomerServiceOnlyFilter : IAsyncActionFilter
		{
			private readonly IManagerPermissionService _perm;

			public CustomerServiceOnlyFilter(IManagerPermissionService perm)
			{
				_perm = perm;
			}

			public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
			{
				var gate = await _perm.GetCustomerServiceContextAsync(context.HttpContext);
				if (!gate.CanEnter)
				{
					// 直接 403；你也可以改成 RedirectToAction(...)
					context.Result = new ForbidResult();
					return;
				}

				// 提供給後續 Action / View 使用
				context.HttpContext.Items["CustomerServiceManagerId"] = gate.ManagerId;

				await next();
			}
		}
	}
}
