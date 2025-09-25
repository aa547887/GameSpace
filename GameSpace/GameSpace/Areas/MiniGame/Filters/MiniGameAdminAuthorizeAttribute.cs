using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameSpace.Areas.MiniGame.Filters
{
    public class MiniGameAdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            
            // 檢查是否已登入
            if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Home", new { area = "MiniGame" });
                return;
            }

            // 檢查是否為管理員
            var managerIdClaim = httpContext.User.FindFirst("ManagerId");
            if (managerIdClaim == null)
            {
                context.Result = new RedirectToActionResult("Login", "Home", new { area = "MiniGame" });
                return;
            }

            // 這裡可以添加更詳細的權限檢查
            // 例如檢查特定的管理員權限
        }
    }
}
