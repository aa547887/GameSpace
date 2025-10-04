using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using System.Security.Claims;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// MiniGame 權限服務
    /// </summary>
    public interface IMiniGamePermissionService
    {
        /// <summary>
        /// 檢查管理員是否有使用者狀態管理權限
        /// </summary>
        Task<bool> HasUserStatusManagementPermissionAsync(int managerId);

        /// <summary>
        /// 檢查管理員是否有寵物管理權限
        /// </summary>
        Task<bool> HasPetManagementPermissionAsync(int managerId);

        /// <summary>
        /// 檢查管理員是否有購物管理權限
        /// </summary>
        Task<bool> HasShoppingManagementPermissionAsync(int managerId);

        /// <summary>
        /// 檢查管理員是否有訊息管理權限
        /// </summary>
        Task<bool> HasMessageManagementPermissionAsync(int managerId);

        /// <summary>
        /// 檢查管理員是否有客服權限
        /// </summary>
        Task<bool> HasCustomerServicePermissionAsync(int managerId);

        /// <summary>
        /// 檢查管理員是否有管理員權限
        /// </summary>
        Task<bool> HasAdministratorPermissionAsync(int managerId);

        /// <summary>
        /// 從 HttpContext 獲取管理員 ID
        /// </summary>
        int? GetManagerIdFromContext(HttpContext context);

        /// <summary>
        /// 檢查當前管理員是否有指定權限
        /// </summary>
        Task<bool> HasPermissionAsync(HttpContext context, string permissionType);

        // ========== 管理員權限管理方法 ==========

        /// <summary>
        /// 獲取管理員角色信息
        /// </summary>
        Task<Models.ViewModels.ManagerRoleInfoViewModel?> GetManagerRoleInfoAsync(int managerId);

        /// <summary>
        /// 獲取權限統計資訊
        /// </summary>
        Task<Models.ViewModels.PermissionStatisticsViewModel> GetPermissionStatisticsAsync();

        /// <summary>
        /// 獲取管理員權限列表
        /// </summary>
        Task<List<string>> GetManagerPermissionsAsync(int managerId);

        /// <summary>
        /// 更新管理員權限
        /// </summary>
        Task<bool> UpdateManagerPermissionsAsync(int managerId, List<string> permissions);

        // ========== 用戶權限管理方法 ==========

        /// <summary>
        /// 根據查詢條件獲取用戶權限列表
        /// </summary>
        Task<Models.ViewModels.PagedResult<Models.ViewModels.UserRightViewModel>> GetUserRightsAsync(Models.ViewModels.UserRightsQuery query);

        /// <summary>
        /// 獲取所有用戶列表
        /// </summary>
        Task<List<Models.ViewModels.UserViewModel>> GetAllUsersAsync();

        /// <summary>
        /// 獲取所有權限類型
        /// </summary>
        Task<List<Models.ViewModels.RightTypeViewModel>> GetRightTypesAsync();

        /// <summary>
        /// 添加用戶權限
        /// </summary>
        Task<bool> AddUserRightAsync(int userId, string rightName, string rightType, int rightLevel, string description, DateTime? expiresAt);

        /// <summary>
        /// 移除用戶權限
        /// </summary>
        Task<bool> RemoveUserRightAsync(int userId, string rightName);

        /// <summary>
        /// 更新用戶權限
        /// </summary>
        Task<bool> UpdateUserRightAsync(int userRightId, int rightLevel, string description, DateTime? expiresAt, bool isActive);

        /// <summary>
        /// 根據ID獲取用戶權限
        /// </summary>
        Task<Models.ViewModels.UserRightViewModel?> GetUserRightByIdAsync(int userRightId);

        /// <summary>
        /// 根據用戶ID獲取用戶權限列表
        /// </summary>
        Task<List<Models.ViewModels.UserRightViewModel>> GetUserRightsByUserIdAsync(int userId);

        // ========== 權限類型管理方法 ==========

        /// <summary>
        /// 添加權限類型
        /// </summary>
        Task<bool> AddRightTypeAsync(string typeName, string displayName, string description);

        /// <summary>
        /// 更新權限類型
        /// </summary>
        Task<bool> UpdateRightTypeAsync(int typeId, string displayName, string description);

        /// <summary>
        /// 刪除權限類型
        /// </summary>
        Task<bool> DeleteRightTypeAsync(int typeId);

        // ========== 操作日誌方法 ==========

        /// <summary>
        /// 獲取操作日誌列表
        /// </summary>
        Task<Models.ViewModels.PagedResult<Models.ViewModels.OperationLogViewModel>> GetOperationLogsAsync(int page, int pageSize);

        /// <summary>
        /// 根據ID獲取操作日誌
        /// </summary>
        Task<Models.ViewModels.OperationLogViewModel?> GetOperationLogByIdAsync(int logId);

        // ========== 基礎權限檢查方法（MiniGameBaseController需要） ==========

        /// <summary>
        /// 檢查管理員是否有指定權限
        /// </summary>
        Task<bool> HasManagerPermissionAsync(int managerId, string permission);

        /// <summary>
        /// 檢查用戶是否有指定權限
        /// </summary>
        Task<bool> HasUserRightAsync(int userId, string rightName);

        /// <summary>
        /// 記錄權限操作日誌
        /// </summary>
        Task LogPermissionOperationAsync(int managerId, string operation, string details, int? targetUserId = null);
    }
}

