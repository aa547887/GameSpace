using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    // 會員點數相關 ViewModel
    public class AdminWalletIndexViewModel
    {
        public List<UserWallet> UserPoints { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public CouponQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GrantPointsViewModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "請輸入點數")]
        [Range(1, 999999, ErrorMessage = "點數必須在1-999999之間")]
        public int Points { get; set; }
        
        [Required(ErrorMessage = "請輸入原因")]
        [StringLength(500, ErrorMessage = "原因不能超過500字")]
        public string Reason { get; set; } = string.Empty;
        
        public List<User> Users { get; set; } = new();
    }

    // 商城優惠券相關 ViewModel
    public class AdminCouponIndexViewModel
    {
        public List<UserCouponReadModel> Coupons { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public CouponQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GrantCouponsViewModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "請選擇優惠券類型")]
        public int CouponTypeId { get; set; }
        
        [Required(ErrorMessage = "請輸入數量")]
        [Range(1, 100, ErrorMessage = "數量必須在1-100之間")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "請輸入原因")]
        [StringLength(500, ErrorMessage = "原因不能超過500字")]
        public string Reason { get; set; } = string.Empty;
        
        public List<User> Users { get; set; } = new();
        public List<CouponType> CouponTypes { get; set; } = new();
    }

    // 電子禮券相關 ViewModel
    public class AdminEVoucherIndexViewModel
    {
        public List<Evoucher> EVouchers { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public EVoucherQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class AdjustEVouchersViewModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "請選擇電子禮券類型")]
        public int EVoucherTypeId { get; set; }
        
        [Required(ErrorMessage = "請輸入數量")]
        [Range(1, 100, ErrorMessage = "數量必須在1-100之間")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "請輸入原因")]
        [StringLength(500, ErrorMessage = "原因不能超過500字")]
        public string Reason { get; set; } = string.Empty;
        
        public List<User> Users { get; set; } = new();
        public List<EvoucherType> EVoucherTypes { get; set; } = new();
    }

    // 錢包歷史相關 ViewModel
    public class AdminWalletHistoryViewModel
    {
        public List<WalletTransaction> WalletHistories { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public WalletHistoryQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    // 簽到相關 ViewModel
    public class AdminSignInIndexViewModel
    {
        public List<UserSignInStat> SignInStats { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class AdminSignInRulesViewModel
    {
        public SignInRuleReadModel SignInRule { get; set; } = new();
    }

    public class AdminSignInUserHistoryViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<SignInRecordReadModel> SignInHistory { get; set; } = new();
    }

    // 寵物相關 ViewModel
    public class PetSystemRulesViewModel
    {
        public int MaxLevel { get; set; }
        public int ExperiencePerLevel { get; set; }
        public int MaxHunger { get; set; }
        public int MaxHappiness { get; set; }
        public int MaxHealth { get; set; }
        public int MaxEnergy { get; set; }
        public int MaxCleanliness { get; set; }
        public int DailyDecayRate { get; set; }
        public int InteractionBonus { get; set; }
    }

    public class IndividualPetSettingsViewModel
    {
        public int SelectedUserId { get; set; }
        public Pet? Pet { get; set; }
        public List<User> Users { get; set; } = new();
    }

    public class AdminPetListViewModel
    {
        public List<Pet> Pets { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public PetQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class PetColorChangeHistoryViewModel
    {
        public List<PetSkinColorChangeLog> ColorChangeHistory { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public PetColorChangeQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    // 小遊戲相關 ViewModel
    public class MiniGameRulesViewModel
    {
        public int MaxPlayTime { get; set; }
        public int PointsPerWin { get; set; }
        public int PointsPerLose { get; set; }
        public int ExperiencePerWin { get; set; }
        public int ExperiencePerLose { get; set; }
        public int MaxDailyPlays { get; set; }
    }

    public class AdminMiniGameRecordsViewModel
    {
        public List<MiniGame> GameRecords { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public MiniGameRecordQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GameStatisticsViewModel
    {
        public int TotalGamePlays { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int ActivePlayers { get; set; }
        public int AverageGameDuration { get; set; }
        public int HighestScore { get; set; }
        public int AverageScore { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int DrawCount { get; set; }
        public Dictionary<string, int> DailyGameCounts { get; set; } = new();
    }

    // 查詢模型
    public class CouponQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public int? MinPoints { get; set; }
        public int? MaxPoints { get; set; }
        public DateTime? DateRange { get; set; }
    }

    public class EVoucherQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public int? EVoucherTypeId { get; set; }
        public string? Status { get; set; }
    }

    public class WalletHistoryQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? UserId { get; set; }
        public string? TransactionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class PetQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? UserId { get; set; }
        public string? PetName { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
    }

    public class PetColorChangeQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? UserId { get; set; }
        public string? ChangeType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class MiniGameRecordQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? UserId { get; set; }
        public string? GameResult { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GameStatisticsQueryModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UserId { get; set; }
    }

    // 其他必要的模型
    public class UserCouponReadModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int CouponTypeId { get; set; }
        public string CouponTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class SignInRecordReadModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime SignInDate { get; set; }
        public int ConsecutiveDays { get; set; }
        public int PointsEarned { get; set; }
    }

    public class WalletTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Amount { get; set; }
        public int Balance { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class PetSkinColorChangeLog
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string OldColor { get; set; } = string.Empty;
        public string NewColor { get; set; } = string.Empty;
        public DateTime ChangedDate { get; set; }
    }

    public class PetBackgroundColorChangeLog
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string OldBackground { get; set; } = string.Empty;
        public string NewBackground { get; set; } = string.Empty;
        public DateTime ChangedDate { get; set; }
    }
}
