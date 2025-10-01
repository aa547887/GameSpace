using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// MiniGame Area 基礎控制器
    /// 提供統一的DbContext和權限驗證
    /// </summary>
    [Area(""MiniGame"")]
    [Authorize(AuthenticationSchemes = ""AdminCookie"", Policy = ""AdminOnly"")]
    public abstract class MiniGameBaseController : Controller
    {
        protected readonly MiniGameDbContext _context;
        protected readonly IMiniGameAdminService _adminService;
        protected readonly IMiniGamePermissionService _permissionService;
        protected readonly ILogger<MiniGameBaseController> _logger;

        protected MiniGameBaseController(
            MiniGameDbContext context, 
            ILogger<MiniGameBaseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        protected MiniGameBaseController(
            MiniGameDbContext context, 
            IMiniGameAdminService adminService,
            ILogger<MiniGameBaseController> logger) : this(context, logger)
        {
            _adminService = adminService;
        }

        protected MiniGameBaseController(
            MiniGameDbContext context, 
            IMiniGameAdminService adminService, 
            IMiniGamePermissionService permissionService,
            ILogger<MiniGameBaseController> logger) : this(context, adminService, logger)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// 獲取當前管理員ID
        /// </summary>
        protected int? GetCurrentManagerId()
        {
            try
            {
                var managerIdClaim = User.FindFirst(""ManagerId"");
                if (managerIdClaim != null && int.TryParse(managerIdClaim.Value, out int managerId))
                {
                    return managerId;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""獲取當前管理員ID時發生錯誤"");
                return null;
            }
        }

        /// <summary>
        /// 獲取當前管理員信息
        /// </summary>
        protected async Task<ManagerData?> GetCurrentManagerAsync()
        {
            try
            {
                var managerId = GetCurrentManagerId();
                if (managerId.HasValue)
                {
                    return await _context.ManagerData
                        .Include(m => m.ManagerRoles)
                        .ThenInclude(mr => mr.ManagerRolePermission)
                        .FirstOrDefaultAsync(m => m.Manager_Id == managerId.Value);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""獲取當前管理員信息時發生錯誤"");
                return null;
            }
        }

        /// <summary>
        /// 檢查管理員權限
        /// </summary>
        protected async Task<bool> CheckPermissionAsync(string permission)
        {
            try
            {
                if (_permissionService == null) return false;
                
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue) return false;

                return await _permissionService.HasPermissionAsync(managerId.Value, permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""檢查管理員權限時發生錯誤: {Permission}"", permission);
                return false;
            }
        }

        /// <summary>
        /// 記錄操作日誌
        /// </summary>
        protected void LogOperation(string operation, string details = "")
        {
            try
            {
                var managerId = GetCurrentManagerId();
                _logger.LogInformation(""管理員 {ManagerId} 執行操作: {Operation} - {Details}"", 
                    managerId, operation, details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""記錄操作日誌時發生錯誤"");
            }
        }

        /// <summary>
        /// 統一錯誤處理
        /// </summary>
        protected IActionResult HandleError(Exception ex, string operation)
        {
            _logger.LogError(ex, ""執行操作 {Operation} 時發生錯誤"", operation);
            
            TempData[""ErrorMessage""] = $""執行 {operation} 時發生錯誤，請稍後再試"";
            
            return RedirectToAction(""Index"", ""Admin"");
        }
    }
}
