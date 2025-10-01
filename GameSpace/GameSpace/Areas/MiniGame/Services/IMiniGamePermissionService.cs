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
    }
}
