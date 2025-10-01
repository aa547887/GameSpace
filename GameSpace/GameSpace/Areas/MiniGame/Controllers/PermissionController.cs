using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 權限管理控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class PermissionController : MiniGameBaseController
    {
        private readonly IMiniGamePermissionService _permissionService;

        public PermissionController(GameSpacedatabaseContext context, IMiniGamePermissionService permissionService) 
            : base(context, null, permissionService)
        {
            _permissionService = permissionService;
        }

        // ========== 管理員權限管理 ==========

        /// <summary>
        /// 管理員權限首頁
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var managerId = GetCurrentManagerId();
            if (!managerId.HasValue) return RedirectToAction("Login", "Home");

            var roleInfo = await _permissionService.GetManagerRoleInfoAsync(managerId.Value);
            var statistics = await _permissionService.GetPermissionStatisticsAsync();

            var viewModel = new ViewModels.AdminPermissionIndexViewModel
            {
                ManagerRoleInfo = roleInfo,
                Statistics = statistics
            };

            return View(viewModel);
        }

        /// <summary>
        /// 獲取管理員權限信息
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetManagerPermissions(int managerId)
        {
            try
            {
                var permissions = await _permissionService.GetManagerPermissionsAsync(managerId);
                return Json(new { success = true, data = permissions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新管理員權限
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateManagerPermissions([FromBody] ViewModels.UpdateManagerPermissionsRequest request)
        {
            try
            {
                var result = await _permissionService.UpdateManagerPermissionsAsync(
                    request.ManagerId, 
                    request.Permissions
                );

                if (result)
                {
                    await LogOperationAsync("UpdateManagerPermissions", 
                        $"Updated permissions for manager {request.ManagerId}");
                    return Json(new { success = true, message = "權限更新成功" });
                }
                else
                {
                    return Json(new { success = false, message = "權限更新失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== 用戶權限管理 ==========

        /// <summary>
        /// 用戶權限管理首頁
        /// </summary>
        public async Task<IActionResult> UserRights(string userId = "", string rightName = "", 
            string rightType = "", bool? isActive = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = new ViewModels.UserRightsQuery
                {
                    UserId = userId,
                    RightName = rightName,
                    RightType = rightType,
                    IsActive = isActive,
                    PageNumber = page,
                    PageSize = pageSize
                };

                var userRights = await _permissionService.GetUserRightsAsync(query);
                var users = await _permissionService.GetAllUsersAsync();
                var rightTypes = await _permissionService.GetRightTypesAsync();

                var viewModel = new ViewModels.UserRightsIndexViewModel
                {
                    UserRights = userRights,
                    Users = users,
                    RightTypes = rightTypes,
                    Query = query
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入用戶權限失敗: {ex.Message}";
                return View(new ViewModels.UserRightsIndexViewModel());
            }
        }

        /// <summary>
        /// 添加用戶權限
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddUserRight([FromBody] ViewModels.AddUserRightRequest request)
        {
            try
            {
                // 驗證管理員權限
                var hasPermission = await HasPermissionAsync("User.Edit");
                if (!hasPermission)
                {
                    return Json(new { success = false, message = "您沒有權限執行此操作" });
                }

                var result = await _permissionService.AddUserRightAsync(
                    request.UserId,
                    request.RightName,
                    request.RightType,
                    request.RightLevel,
                    request.Description,
                    request.ExpiresAt
                );

                if (result)
                {
                    await LogOperationAsync("AddUserRight", 
                        $"Added right '{request.RightName}' for user {request.UserId}", 
                        request.UserId);
                    return Json(new { success = true, message = "權限添加成功" });
                }
                else
                {
                    return Json(new { success = false, message = "權限添加失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 移除用戶權限
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RemoveUserRight([FromBody] ViewModels.RemoveUserRightRequest request)
        {
            try
            {
                // 驗證管理員權限
                var hasPermission = await HasPermissionAsync("User.Edit");
                if (!hasPermission)
                {
                    return Json(new { success = false, message = "您沒有權限執行此操作" });
                }

                var result = await _permissionService.RemoveUserRightAsync(
                    request.UserId,
                    request.RightName
                );

                if (result)
                {
                    await LogOperationAsync("RemoveUserRight", 
                        $"Removed right '{request.RightName}' for user {request.UserId}", 
                        request.UserId);
                    return Json(new { success = true, message = "權限移除成功" });
                }
                else
                {
                    return Json(new { success = false, message = "權限移除失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新用戶權限
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateUserRight([FromBody] ViewModels.UpdateUserRightRequest request)
        {
            try
            {
                // 驗證管理員權限
                var hasPermission = await HasPermissionAsync("User.Edit");
                if (!hasPermission)
                {
                    return Json(new { success = false, message = "您沒有權限執行此操作" });
                }

                var result = await _permissionService.UpdateUserRightAsync(
                    request.UserRightId,
                    request.RightLevel,
                    request.Description,
                    request.ExpiresAt,
                    request.IsActive
                );

                if (result)
                {
                    await LogOperationAsync("UpdateUserRight", 
                        $"Updated right {request.UserRightId} for user", 
                        request.UserId);
                    return Json(new { success = true, message = "權限更新成功" });
                }
                else
                {
                    return Json(new { success = false, message = "權限更新失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 獲取用戶權限詳情
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserRightDetails(int userRightId)
        {
            try
            {
                var right = await _permissionService.GetUserRightByIdAsync(userRightId);
                if (right == null)
                {
                    return Json(new { success = false, message = "權限不存在" });
                }

                return Json(new { success = true, data = right });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 獲取用戶權限列表
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUserRights(int userId)
        {
            try
            {
                var rights = await _permissionService.GetUserRightsByUserIdAsync(userId);
                return Json(new { success = true, data = rights });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== 權限類型管理 ==========

        /// <summary>
        /// 權限類型管理
        /// </summary>
        public async Task<IActionResult> RightTypes()
        {
            try
            {
                var rightTypes = await _permissionService.GetRightTypesAsync();
                return View(rightTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入權限類型失敗: {ex.Message}";
                return View(new List<ViewModels.RightTypeViewModel>());
            }
        }

        /// <summary>
        /// 添加權限類型
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddRightType([FromBody] ViewModels.AddRightTypeRequest request)
        {
            try
            {
                // 驗證管理員權限
                var hasPermission = await HasPermissionAsync("Admin.Edit");
                if (!hasPermission)
                {
                    return Json(new { success = false, message = "您沒有權限執行此操作" });
                }

                var result = await _permissionService.AddRightTypeAsync(
                    request.TypeName,
                    request.DisplayName,
                    request.Description
                );

                if (result)
                {
                    await LogOperationAsync("AddRightType", 
                        $"Added right type '{request.TypeName}'");
                    return Json(new { success = true, message = "權限類型添加成功" });
                }
                else
                {
                    return Json(new { success = false, message = "權限類型添加失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新權限類型
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateRightType([FromBody] ViewModels.UpdateRightTypeRequest request)
        {
            try
            {
                // 驗證管理員權限
                var hasPermission = await HasPermissionAsync("Admin.Edit");
                if (!hasPermission)
                {
                    return Json(new { success = false, message = "您沒有權限執行此操作" });
                }

                var result = await _permissionService.UpdateRightTypeAsync(
                    request.TypeId,
                    request.DisplayName,
                    request.Description
                );

                if (result)
                {
                    await LogOperationAsync("UpdateRightType", 
                        $"Updated right type {request.TypeId}");
                    return Json(new { success = true, message = "權限類型更新成功" });
                }
                else
                {
                    return Json(new { success = false, message = "權限類型更新失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 刪除權限類型
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteRightType([FromBody] ViewModels.DeleteRightTypeRequest request)
        {
            try
            {
                // 驗證管理員權限
                var hasPermission = await HasPermissionAsync("Admin.Edit");
                if (!hasPermission)
                {
                    return Json(new { success = false, message = "您沒有權限執行此操作" });
                }

                var result = await _permissionService.DeleteRightTypeAsync(request.TypeId);

                if (result)
                {
                    await LogOperationAsync("DeleteRightType", 
                        $"Deleted right type {request.TypeId}");
                    return Json(new { success = true, message = "權限類型刪除成功" });
                }
                else
                {
                    return Json(new { success = false, message = "權限類型刪除失敗" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== 操作日誌 ==========

        /// <summary>
        /// 操作日誌
        /// </summary>
        public async Task<IActionResult> OperationLogs(int page = 1, int pageSize = 20)
        {
            try
            {
                var logs = await _permissionService.GetOperationLogsAsync(page, pageSize);
                return View(logs);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入操作日誌失敗: {ex.Message}";
                return View(new PagedResult<ViewModels.OperationLogViewModel>());
            }
        }

        /// <summary>
        /// 獲取操作日誌詳情
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOperationLogDetails(int logId)
        {
            try
            {
                var log = await _permissionService.GetOperationLogByIdAsync(logId);
                if (log == null)
                {
                    return Json(new { success = false, message = "日誌不存在" });
                }

                return Json(new { success = true, data = log });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== 權限統計 ==========

        /// <summary>
        /// 權限統計
        /// </summary>
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _permissionService.GetPermissionStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入統計資料失敗: {ex.Message}";
                return View(new ViewModels.PermissionStatisticsViewModel());
            }
        }

        /// <summary>
        /// 獲取權限統計數據
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatisticsData()
        {
            try
            {
                var statistics = await _permissionService.GetPermissionStatisticsAsync();
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
