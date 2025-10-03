using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    // ========== 管理員權限相關 ViewModels ==========

    /// <summary>
    /// 管理員權限首頁 ViewModel
    /// </summary>
    public class AdminPermissionIndexViewModel
    {
        public ManagerRoleInfoViewModel ManagerRoleInfo { get; set; } = new();
        public PermissionStatisticsViewModel Statistics { get; set; } = new();
    }

    /// <summary>
    /// 管理員角色信息 ViewModel
    /// </summary>
    public class ManagerRoleInfoViewModel
    {
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerAccount { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public DateTime LastLoginTime { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 更新管理員權限請求
    /// </summary>
    public class UpdateManagerPermissionsRequest
    {
        [Required]
        public int ManagerId { get; set; }
        
        [Required]
        public List<string> Permissions { get; set; } = new();
    }

    // ========== 用戶權限相關 ViewModels ==========

    /// <summary>
    /// 用戶權限查詢條件
    /// </summary>
    public class UserRightsQuery
    {
        public string UserId { get; set; } = string.Empty;
        public string RightName { get; set; } = string.Empty;
        public string RightType { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 用戶權限首頁 ViewModel
    /// </summary>
    public class UserRightsIndexViewModel
    {
        public PagedResult<UserRightViewModel> UserRights { get; set; } = new();
        public List<UserViewModel> Users { get; set; } = new();
        public List<RightTypeViewModel> RightTypes { get; set; } = new();
        public UserRightsQuery Query { get; set; } = new();
    }

    /// <summary>
    /// 用戶權限 ViewModel
    /// </summary>
    public class UserRightViewModel
    {
        public int UserRightId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAccount { get; set; } = string.Empty;
        public string RightName { get; set; } = string.Empty;
        public string RightType { get; set; } = string.Empty;
        public int RightLevel { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 添加用戶權限請求
    /// </summary>
    public class AddUserRightRequest
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string RightName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string RightType { get; set; } = string.Empty;
        
        [Range(1, 10)]
        public int RightLevel { get; set; } = 1;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// 移除用戶權限請求
    /// </summary>
    public class RemoveUserRightRequest
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string RightName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新用戶權限請求
    /// </summary>
    public class UpdateUserRightRequest
    {
        [Required]
        public int UserRightId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Range(1, 10)]
        public int RightLevel { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsActive { get; set; }
    }

    // ========== 權限類型相關 ViewModels ==========

    /// <summary>
    /// 權限類型 ViewModel
    /// </summary>
    public class RightTypeViewModel
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 添加權限類型請求
    /// </summary>
    public class AddRightTypeRequest
    {
        [Required]
        [StringLength(50)]
        public string TypeName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 更新權限類型請求
    /// </summary>
    public class UpdateRightTypeRequest
    {
        [Required]
        public int TypeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 刪除權限類型請求
    /// </summary>
    public class DeleteRightTypeRequest
    {
        [Required]
        public int TypeId { get; set; }
    }

    // ========== 操作日誌相關 ViewModels ==========

    /// <summary>
    /// 操作日誌 ViewModel
    /// </summary>
    public class OperationLogViewModel
    {
        public int LogId { get; set; }
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public int? TargetUserId { get; set; }
        public string TargetUserName { get; set; } = string.Empty;
        public DateTime OperationTime { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }

    // ========== 統計相關 ViewModels ==========

    /// <summary>
    /// 權限統計 ViewModel
    /// </summary>
    public class PermissionStatisticsViewModel
    {
        public int TotalManagers { get; set; }
        public int ActiveManagers { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalUserRights { get; set; }
        public int ActiveUserRights { get; set; }
        public int TotalRightTypes { get; set; }
        public int TotalOperationLogs { get; set; }
        public List<RightTypeStatisticsViewModel> RightTypeStatistics { get; set; } = new();
        public List<ManagerRoleStatisticsViewModel> ManagerRoleStatistics { get; set; } = new();
        public List<OperationLogStatisticsViewModel> OperationLogStatistics { get; set; } = new();
    }

    /// <summary>
    /// 權限類型統計 ViewModel
    /// </summary>
    public class RightTypeStatisticsViewModel
    {
        public string RightType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int ActiveCount { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// 管理員角色統計 ViewModel
    /// </summary>
    public class ManagerRoleStatisticsViewModel
    {
        public string RoleName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// 操作日誌統計 ViewModel
    /// </summary>
    public class OperationLogStatisticsViewModel
    {
        public string Operation { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTime LastOperationTime { get; set; }
    }

    // ========== 用戶相關 ViewModels ==========

    /// <summary>
    /// 用戶 ViewModel
    /// </summary>
    public class UserViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAccount { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }
}

