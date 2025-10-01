using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Data;
using GameSpace.Infrastructure.Login;

namespace GameSpace.Areas.MiniGame.Filters
{
    /// <summary>
    /// MiniGame Area 管理員權限驗證屬性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class MiniGameAdminAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _requiredPermission;

        public MiniGameAdminAuthorizeAttribute(string requiredPermission)
        {
            _requiredPermission = requiredPermission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var loginIdentity = httpContext.RequestServices.GetRequiredService<ILoginIdentity>();
            var loginResult = await loginIdentity.GetAsync(httpContext.RequestAborted);

            // 檢查是否已登入且為管理員
            if (!loginResult.IsAuthenticated || !loginResult.IsManager)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", new { area = "" });
                return;
            }

            var managerId = loginResult.ManagerId;
            if (managerId == null)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", new { area = "" });
                return;
            }

            // 檢查權限
            var dbContext = httpContext.RequestServices.GetRequiredService<MiniGameDbContext>();
            var hasPermission = await CheckManagerPermission(dbContext, managerId.Value, _requiredPermission);

            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", new { area = "" });
                return;
            }
        }

        private async Task<bool> CheckManagerPermission(MiniGameDbContext dbContext, int managerId, string requiredPermission)
        {
            var managerRoles = await dbContext.ManagerRole
                .Where(mr => mr.Manager_Id == managerId)
                .Include(mr => mr.ManagerRolePermission)
                .Select(mr => mr.ManagerRolePermission)
                .ToListAsync();

            return requiredPermission switch
            {
                "UserStatusManagement" => managerRoles.Any(r => r.UserStatusManagement),
                "Pet_Rights_Management" => managerRoles.Any(r => r.Pet_Rights_Management),
                "ShoppingPermissionManagement" => managerRoles.Any(r => r.ShoppingPermissionManagement),
                "MessagePermissionManagement" => managerRoles.Any(r => r.MessagePermissionManagement),
                "AdministratorPrivilegesManagement" => managerRoles.Any(r => r.AdministratorPrivilegesManagement),
                "customer_service" => managerRoles.Any(r => r.customer_service),
                _ => false
            };
        }
    }
}