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
            var manager = dbContext.ManagerData
                .Include(m => m.ManagerRoles)
                .FirstOrDefault(m => m.ManagerId == managerId);

            if (manager == null || manager.ManagerLockoutEnabled)
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

        private bool CheckModulePermission(ManagerDatum manager, string requiredPermission)
        {
            if (manager?.ManagerRoles == null || !manager.ManagerRoles.Any())
                return false;

            // 檢查是否有任何角色具有管理員權限（最高權限）
            var hasAdminPrivilege = manager.ManagerRoles
                .Any(r => r.AdministratorPrivilegesManagement == true);

            if (hasAdminPrivilege) return true;

            // 根據權限類型檢查特定權限
            return requiredPermission switch
            {
                "MiniGame.View" or "MiniGame.Edit" or "MiniGame.Delete" => hasAdminPrivilege,
                "User.View" or "User.Edit" => manager.ManagerRoles.Any(r => r.UserStatusManagement == true),
                "Wallet.View" or "Wallet.Edit" => manager.ManagerRoles.Any(r => r.ShoppingPermissionManagement == true),
                "Pet.View" or "Pet.Edit" => manager.ManagerRoles.Any(r => r.PetRightsManagement == true),
                "Coupon.View" or "Coupon.Edit" or "EVoucher.View" or "EVoucher.Edit" => manager.ManagerRoles.Any(r => r.ShoppingPermissionManagement == true),
                "Message.View" or "Message.Edit" => manager.ManagerRoles.Any(r => r.MessagePermissionManagement == true),
                "CustomerService" => manager.ManagerRoles.Any(r => r.CustomerService == true),
                _ => false
            };
        }
    }
}
