using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    // 管理後台儀表板ViewModel
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalGames { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public decimal TotalPoints { get; set; }
        public int TodaySignIns { get; set; }
        public int TodayGames { get; set; }
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
    }

    // 最近活動ViewModel
    public class RecentActivityViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ActivityTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // 用戶管理ViewModel
    public class AdminUserViewModel
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "用戶名稱不能為空")]
        [StringLength(50, ErrorMessage = "用戶名稱長度不能超過50個字符")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "電子郵件不能為空")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        public string Email { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public int TotalPoints { get; set; }
        public int PetCount { get; set; }
        public int GameCount { get; set; }
    }

    // 錢包管理ViewModel
    public class AdminWalletViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int CurrentPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public List<WalletHistoryViewModel> RecentTransactions { get; set; } = new();
    }

    // 錢包歷史記錄ViewModel
    public class WalletHistoryViewModel
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public int PointsChanged { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ChangeTime { get; set; }
    }

    // 寵物管理ViewModel
    public class AdminPetViewModel
    {
        public int PetId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Energy { get; set; }
        public int Cleanliness { get; set; }
        public int Health { get; set; }
        public string SkinColor { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public DateTime LastInteraction { get; set; }
    }

    // 小遊戲管理ViewModel
    public class AdminMiniGameViewModel
    {
        public int GameId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Result { get; set; } = string.Empty;
        public int PointsEarned { get; set; }
        public int ExperienceEarned { get; set; }
        public string CouponEarned { get; set; } = string.Empty;
    }

    // 簽到管理ViewModel
    public class AdminSignInViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SignInDate { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public string CouponGained { get; set; } = string.Empty;
        public int ConsecutiveDays { get; set; }
        public bool IsHoliday { get; set; }
    }

    // 優惠券管理ViewModel
    public class AdminCouponViewModel
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public int CouponTypeId { get; set; }
        public string CouponTypeName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public string? UsedInOrderId { get; set; }
    }

    // 電子禮券管理ViewModel
    public class AdminEVoucherViewModel
    {
        public int EVoucherId { get; set; }
        public string EVoucherCode { get; set; } = string.Empty;
        public int EVoucherTypeId { get; set; }
        public string EVoucherTypeName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public string? UsedInOrderId { get; set; }
    }

    // 管理者管理ViewModel
    public class AdminManagerViewModel
    {
        public int ManagerId { get; set; }
        [Required(ErrorMessage = "管理員姓名不能為空")]
        public string ManagerName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "管理員帳號不能為空")]
        public string ManagerAccount { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "管理員密碼不能為空")]
        public string ManagerPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "管理員信箱不能為空")]
        [EmailAddress(ErrorMessage = "管理員信箱格式不正確")]
        public string ManagerEmail { get; set; } = string.Empty;
        
        public bool ManagerEmailConfirmed { get; set; }
        public int ManagerAccessFailedCount { get; set; }
        public bool ManagerLockoutEnabled { get; set; }
        public DateTime? ManagerLockoutEnd { get; set; }
        public DateTime AdministratorRegistrationDate { get; set; }
        public int ManagerRoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    // 權限管理ViewModel
    public class PermissionViewModel
    {
        public int ManagerRoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool AdministratorPrivilegesManagement { get; set; }
        public bool UserStatusManagement { get; set; }
        public bool ShoppingPermissionManagement { get; set; }
        public bool MessagePermissionManagement { get; set; }
        public bool PetRightsManagement { get; set; }
        public bool CustomerService { get; set; }
    }

    // 分頁ViewModel
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // 搜尋ViewModel
    public class SearchViewModel
    {
        public string? Keyword { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
    }
}
