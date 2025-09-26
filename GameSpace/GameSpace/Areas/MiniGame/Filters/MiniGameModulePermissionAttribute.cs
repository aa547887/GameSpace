using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

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

            // 檢查管理員權限
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<GameSpacedatabaseContext>();
            var manager = dbContext.Managers.Find(managerId);
            if (manager == null || !manager.IsActive)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", new { area = "" });
                return;
            }

            // 檢查特定模組權限
            var hasPermission = CheckModulePermission(manager, _requiredPermission);
            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", new { area = "" });
                return;
            }
        }

        private bool CheckModulePermission(Manager manager, string requiredPermission)
        {
            // 根據管理員角色和權限檢查
            return manager.Role switch
            {
                "SuperAdmin" => true,
                "Admin" => requiredPermission switch
                {
                    "MiniGame.View" => true,
                    "MiniGame.Edit" => true,
                    "MiniGame.Delete" => true,
                    _ => false
                },
                "Manager" => requiredPermission switch
                {
                    "MiniGame.View" => true,
                    _ => false
                },
                _ => false
            };
        }
    }
}
