using System.ComponentModel.DataAnnotations;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    // 管理員相關 ViewModels
    public class AdminManagerCreateViewModel
    {
        [Required]
        [StringLength(50)]
        public string Manager_Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Manager_Account { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Manager_Password { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Manager_Email { get; set; } = string.Empty;
    }

    public class AdminManagerEditViewModel
    {
        public int Manager_Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Manager_Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Manager_Account { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Manager_Password { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Manager_Email { get; set; } = string.Empty;
    }

    public class AdminManagerRoleCreateViewModel
    {
        [Required]
        public int Manager_Id { get; set; }
        
        [Required]
        public int ManagerRole_Id { get; set; }
    }

    // 用戶相關 ViewModels
    public class AdminUserCreateViewModel
    {
        [Required(ErrorMessage = "用戶名稱為必填")]
        [StringLength(50, ErrorMessage = "用戶名稱長度不可超過 50 字元")]
        public string User_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "用戶帳號為必填")]
        [StringLength(50, ErrorMessage = "用戶帳號長度不可超過 50 字元")]
        public string User_account { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼為必填")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在 6-100 字元之間")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "確認密碼為必填")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "密碼與確認密碼不符")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子郵件為必填")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        [StringLength(100, ErrorMessage = "電子郵件長度不可超過 100 字元")]
        public string User_email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "電話號碼格式不正確")]
        [StringLength(20, ErrorMessage = "電話號碼長度不可超過 20 字元")]
        public string? User_phone { get; set; }

        [DataType(DataType.Date)]
        public DateTime? User_birthday { get; set; }

        [StringLength(10, ErrorMessage = "性別長度不可超過 10 字元")]
        public string? User_gender { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "地址長度不可超過 500 字元")]
        public string? User_address { get; set; }
    }

    public class AdminUserEditViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "用戶名稱為必填")]
        [StringLength(50, ErrorMessage = "用戶名稱長度不可超過 50 字元")]
        public string User_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "用戶帳號為必填")]
        [StringLength(50, ErrorMessage = "用戶帳號長度不可超過 50 字元")]
        public string User_account { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子郵件為必填")]
        [EmailAddress(ErrorMessage = "電子郵件格式不正確")]
        [StringLength(100, ErrorMessage = "電子郵件長度不可超過 100 字元")]
        public string User_email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "電話號碼格式不正確")]
        [StringLength(20, ErrorMessage = "電話號碼長度不可超過 20 字元")]
        public string? User_phone { get; set; }

        [DataType(DataType.Date)]
        public DateTime? User_birthday { get; set; }

        [StringLength(10, ErrorMessage = "性別長度不可超過 10 字元")]
        public string? User_gender { get; set; }

        public bool IsActive { get; set; }

        [StringLength(500, ErrorMessage = "地址長度不可超過 500 字元")]
        public string? User_address { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "新密碼長度必須在 6-100 字元之間")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "新密碼與確認新密碼不符")]
        public string? ConfirmNewPassword { get; set; }
    }

    // 錢包相關 ViewModels
    public class UserWalletViewModel
    {
        public int WalletId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // Point, Coupon, EVoucher
        public int Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class AdminWalletIndexViewModel
    {
        public List<UserWalletViewModel> UserWallets { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }

    public class AdminWalletTransactionViewModel
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // EVoucher Index page model
    public class AdminEVoucherIndexViewModel
    {
        public PagedResult<GameSpace.Models.Evoucher> Evouchers { get; set; } = new();
    }

    public class AdjustEVouchersModel
    {
        public int UserId { get; set; }
        public int EvoucherTypeId { get; set; }
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public decimal? CustomValue { get; set; }
        public string? Description { get; set; }
    }

    public class GrantCouponsModel
    {
        [Required(ErrorMessage = "使用者 ID 為必填")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "優惠券類型 ID 為必填")]
        public int CouponTypeId { get; set; }

        [Required(ErrorMessage = "數量為必填")]
        [Range(1, 1000, ErrorMessage = "數量必須介於 1 到 1000 之間")]
        public int Quantity { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ExpiryDate { get; set; }

        [Required(ErrorMessage = "原因為必填")]
        [StringLength(200, ErrorMessage = "原因不得超過 200 個字元")]
        public string Reason { get; set; } = string.Empty;

        [Range(0, 999999.99, ErrorMessage = "自訂金額必須介於 0 到 999999.99 之間")]
        public decimal? CustomValue { get; set; }

        [StringLength(500, ErrorMessage = "描述不得超過 500 個字元")]
        public string Description { get; set; } = string.Empty;

        // 保留舊屬性名稱以支持向後相容
        [Obsolete("請使用 UserId 屬性")]
        public int UserID
        {
            get => UserId;
            set => UserId = value;
        }

        [Obsolete("請使用 CouponTypeId 屬性")]
        public int CouponTypeID
        {
            get => CouponTypeId;
            set => CouponTypeId = value;
        }
    }

    public class GrantEVouchersModel
    {
        public int UserId { get; set; }
        public int EvoucherTypeId { get; set; }
        public int Quantity { get; set; }
    }

    public class WalletHistoryModel
    {
        public List<WalletHistory> Histories { get; set; } = new();
        public List<WalletHistory> Transactions { get; set; } = new(); // Alias for Histories
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }

    // 優惠券相關 ViewModels
    public class AdminCouponCreateViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CouponTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        // 保留舊屬性名稱以支持向後相容
        [Obsolete("請使用 UserId 屬性")]
        public int UserID
        {
            get => UserId;
            set => UserId = value;
        }

        [Obsolete("請使用 CouponTypeId 屬性")]
        public int CouponTypeID
        {
            get => CouponTypeId;
            set => CouponTypeId = value;
        }
    }

    // 電子券相關 ViewModels
    public class AdminEVoucherCreateViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int EvoucherTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string EVoucherCode { get; set; } = string.Empty;
    }

    public class AdminEVoucherTypeCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public decimal ValueAmount { get; set; }
        
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        
        [Required]
        public int PointsCost { get; set; }
        
        [Required]
        public int TotalAvailable { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class EVoucherCreateVM
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int EvoucherTypeId { get; set; }
    }

    public class EVoucherEditVM
    {
        public int EvoucherId { get; set; }
        public int UserId { get; set; }
        public int EvoucherTypeId { get; set; }
    }

    // 簽到相關 ViewModels
    public class AdminSignInCreateViewModel
    {
        [Required]
        public int UserId { get; set; }
        
        public DateTime SignTime { get; set; } = DateTime.Now;
        
        public int PointsGained { get; set; } = 0;
        
        public int ExpGained { get; set; } = 0;
        
        [StringLength(50)]
        public string? CouponGained { get; set; }
    }

    public class AdminSignInRulesViewModel
    {
        public SignInRuleReadModel? SignInRule { get; set; }
        public List<SignInRecordReadModel> RecentRecords { get; set; } = new();
    }

    public class SignInRecordModel
    {
        public List<UserSignInStats> Records { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }

    // 寵物相關 ViewModels
    public class AdminPetCreateViewModel
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PetName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string SkinColor { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string BackgroundColor { get; set; } = string.Empty;
    }

    public class PetColorChangeQueryModel
    {
        public List<PetSkinColorChangeLog> ColorChanges { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }

    public class PetIndividualSettingsModel
    {
        public Pet? Pet { get; set; }
        public List<PetSkinColorChangeLog> SkinChanges { get; set; } = new();
        public List<PetBackgroundColorChangeLog> BackgroundChanges { get; set; } = new();
    }

    public class PetListQueryModel
    {
        public List<Pet> Pets { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }

    public class AdminPetRulesViewModel
    {
        public PetRuleReadModel? PetRule { get; set; }
        public List<PetSummary> PetSummaries { get; set; } = new();
    }

    // 小遊戲相關 ViewModels
    public class AdminMiniGameCreateViewModel
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int PetID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string GameType { get; set; } = string.Empty;
        
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        
        [StringLength(20)]
        public string? Result { get; set; }
        
        public int PointsEarned { get; set; } = 0;
        public int ExpEarned { get; set; } = 0;
        public int CouponEarned { get; set; } = 0;
        
        [StringLength(100)]
        public string? SessionID { get; set; }
    }

    public class GameRecordModel
    {
        public List<GameSpace.Models.MiniGame> GameRecords { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }

    // 系統統計模型
    public class SystemStatsModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TodaySignIns { get; set; }
        public int ThisMonthSignIns { get; set; }
    }

    // 圖表數據模型
    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    // 最近錢包交易模型
    public class RecentWalletTransactionModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public int PointsChanged { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ChangeTime { get; set; }
    }

    // 最近簽到模型
    public class RecentSignInModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SignTime { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public string? CouponGained { get; set; }
    }

    // 最近遊戲記錄模型
    public class RecentGameRecordModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Level { get; set; }
        public string? Result { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? GameName { get; set; }
    }

    // 系統狀態模型
    public class SystemStatusModel
    {
        public bool DatabaseConnection { get; set; }
        public string MemoryUsage { get; set; } = string.Empty;
        public DateTime? LastBackup { get; set; }
        public int ErrorCount { get; set; }
        public string CpuUsage { get; set; } = string.Empty;
        public string DiskSpace { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; } = DateTime.Now;
    }

}


