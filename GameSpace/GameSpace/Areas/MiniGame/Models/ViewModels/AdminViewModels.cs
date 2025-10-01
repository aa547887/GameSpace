using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
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
        [Required]
        [StringLength(50)]
        public string User_name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string User_Account { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string User_Password { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string User_Email { get; set; } = string.Empty;
    }

    public class AdminUserEditViewModel
    {
        public int User_Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string User_name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string User_Account { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? User_Password { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string User_Email { get; set; } = string.Empty;
    }

    // 錢包相關 ViewModels
    public class AdminWalletIndexViewModel
    {
        public List<UserWallet> UserWallets { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }

    public class AdminWalletTransactionViewModel
    {
        public int UserID { get; set; }
        public decimal Amount { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AdjustEVouchersModel
    {
        public int UserID { get; set; }
        public int EVoucherTypeID { get; set; }
        public int Quantity { get; set; }
    }

    public class GrantCouponsModel
    {
        public int UserID { get; set; }
        public int CouponTypeID { get; set; }
        public int Quantity { get; set; }
    }

    public class GrantEVouchersModel
    {
        public int UserID { get; set; }
        public int EVoucherTypeID { get; set; }
        public int Quantity { get; set; }
    }

    public class WalletHistoryModel
    {
        public List<WalletHistory> Histories { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }

    // 優惠券相關 ViewModels
    public class AdminCouponCreateViewModel
    {
        [Required]
        public int UserID { get; set; }
        
        [Required]
        public int CouponTypeID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;
    }

    // 電子券相關 ViewModels
    public class AdminEVoucherCreateViewModel
    {
        [Required]
        public int UserID { get; set; }
        
        [Required]
        public int EVoucherTypeID { get; set; }
        
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

    public class EVoucherCreateModel
    {
        [Required]
        public int UserID { get; set; }
        
        [Required]
        public int EVoucherTypeID { get; set; }
    }

    public class EVoucherEditModel
    {
        public int EVoucherID { get; set; }
        public int UserID { get; set; }
        public int EVoucherTypeID { get; set; }
    }

    // 簽到相關 ViewModels
    public class AdminSignInCreateViewModel
    {
        [Required]
        public int UserID { get; set; }
        
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
        public int UserID { get; set; }
        
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
        public int UserID { get; set; }
        
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
        public List<MiniGame> GameRecords { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
        public int CurrentPage { get; set; } = 1;
    }
}
