using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace GameSpace.Areas.MiniGame.Filters
{
    public class MiniGameModulePermissionAttribute : ActionFilterAttribute
    {
        private readonly string _requiredPermission;

        public MiniGameModulePermissionAttribute(string requiredPermission)
        {
            _requiredPermission = requiredPermission;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }

            var managerIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (managerIdClaim == null || !int.TryParse(managerIdClaim.Value, out int managerId))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", new { area = "" });
                return;
            }

            // 這裡應該檢查權限，暫時允許所有已認證的用戶
            // 實際實現時應該查詢資料庫檢查用戶權限
        }
    }
}
