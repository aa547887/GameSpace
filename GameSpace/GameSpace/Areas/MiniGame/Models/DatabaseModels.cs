using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    // 管理員資料表 - 對應 SSMS 中的 ManagerData 表
    [Table("ManagerData")]
    public class ManagerData
    {
        [Key]
        public int Manager_Id { get; set; }
        
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
        [StringLength(100)]
        public string Manager_Email { get; set; } = string.Empty;
        
        public bool Manager_EmailConfirmed { get; set; }
        
        public int Manager_AccessFailedCount { get; set; }
        
        public bool Manager_LockoutEnabled { get; set; }
        
        public DateTime? Manager_LockoutEnd { get; set; }
        
        public DateTime Administrator_registration_date { get; set; }
        
        // 導航屬性
        public virtual ICollection<ManagerRole> ManagerRoles { get; set; } = new List<ManagerRole>();
    }

    // 管理員角色權限表 - 對應 SSMS 中的 ManagerRolePermission 表
    [Table("ManagerRolePermission")]
    public class ManagerRolePermission
    {
        [Key]
        public int ManagerRole_Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string role_name { get; set; } = string.Empty;
        
        public bool AdministratorPrivilegesManagement { get; set; }
        
        public bool UserStatusManagement { get; set; }
        
        public bool ShoppingPermissionManagement { get; set; }
        
        public bool MessagePermissionManagement { get; set; }
        
        public bool Pet_Rights_Management { get; set; }
        
        public bool customer_service { get; set; }
        
        // 導航屬性
        public virtual ICollection<ManagerRole> ManagerRoles { get; set; } = new List<ManagerRole>();
    }

    // 管理員角色關聯表 - 對應 SSMS 中的 ManagerRole 表
    [Table("ManagerRole")]
    public class ManagerRole
    {
        [Key]
        public int Id { get; set; }
        
        public int Manager_Id { get; set; }
        
        public int ManagerRole_Id { get; set; }
        
        // 導航屬性
        [ForeignKey("Manager_Id")]
        public virtual ManagerData Manager { get; set; } = null!;
        
        [ForeignKey("ManagerRole_Id")]
        public virtual ManagerRolePermission ManagerRolePermission { get; set; } = null!;
    }

    // 用戶錢包表 - 對應 SSMS 中的 User_Wallet 表
    [Table("User_Wallet")]
    public class UserWallet
    {
        [Key]
        public int User_Id { get; set; }
        
        public int User_Point { get; set; } = 0;
        
        // 導航屬性
        [ForeignKey("User_Id")]
        public virtual User User { get; set; } = null!;
        
        public virtual ICollection<WalletHistory> WalletHistories { get; set; } = new List<WalletHistory>();
    }

    // 錢包歷史記錄表 - 對應 SSMS 中的 WalletHistory 表
    [Table("WalletHistory")]
    public class WalletHistory
    {
        [Key]
        public int LogID { get; set; }
        
        public int UserID { get; set; }
        
        [Required]
        [StringLength(20)]
        public string ChangeType { get; set; } = string.Empty;
        
        public int PointsChanged { get; set; }
        
        [StringLength(50)]
        public string? ItemCode { get; set; }
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        public DateTime ChangeTime { get; set; } = DateTime.UtcNow;
        
        // 導航屬性
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }

    // 用戶表 - 對應 SSMS 中的 Users 表
    [Table("Users")]
    public class User
    {
        [Key]
        public int User_Id { get; set; }
        
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
        [StringLength(100)]
        public string User_Email { get; set; } = string.Empty;
        
        public bool User_EmailConfirmed { get; set; }
        
        public int User_AccessFailedCount { get; set; }
        
        public bool User_LockoutEnabled { get; set; }
        
        public DateTime? User_LockoutEnd { get; set; }
        
        public DateTime User_registration_date { get; set; } = DateTime.UtcNow;
        
        // 導航屬性
        public virtual UserWallet? UserWallet { get; set; }
        public virtual ICollection<WalletHistory> WalletHistories { get; set; } = new List<WalletHistory>();
        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
        public virtual ICollection<MiniGame> MiniGames { get; set; } = new List<MiniGame>();
        public virtual ICollection<UserSignInStats> UserSignInStats { get; set; } = new List<UserSignInStats>();
        public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
        public virtual ICollection<EVoucher> EVouchers { get; set; } = new List<EVoucher>();
    }

    // 寵物表 - 對應 SSMS 中的 Pet 表
    [Table("Pet")]
    public class Pet
    {
        [Key]
        public int PetID { get; set; }
        
        public int UserID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PetName { get; set; } = string.Empty;
        
        public int Level { get; set; } = 1;
        
        public DateTime LevelUpTime { get; set; } = DateTime.UtcNow;
        
        public int Experience { get; set; } = 0;
        
        public int Hunger { get; set; } = 100;
        
        public int Mood { get; set; } = 100;
        
        public int Stamina { get; set; } = 100;
        
        public int Cleanliness { get; set; } = 100;
        
        public int Health { get; set; } = 100;
        
        [Required]
        [StringLength(10)]
        public string SkinColor { get; set; } = "#000000";
        
        public DateTime SkinColorChangedTime { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(20)]
        public string BackgroundColor { get; set; } = "白色";
        
        public DateTime BackgroundColorChangedTime { get; set; } = DateTime.UtcNow;
        
        public int PointsChanged_SkinColor { get; set; } = 0;
        
        public int PointsChanged_BackgroundColor { get; set; } = 0;
        
        public int PointsGained_LevelUp { get; set; } = 0;
        
        public DateTime PointsGainedTime_LevelUp { get; set; } = DateTime.UtcNow;
        
        // 導航屬性
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }

    // 小遊戲記錄表 - 對應 SSMS 中的 MiniGame 表
    [Table("MiniGame")]
    public class MiniGame
    {
        [Key]
        public int GameID { get; set; }
        
        public int UserID { get; set; }
        
        public int PetID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string GameType { get; set; } = string.Empty;
        
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndTime { get; set; }
        
        [StringLength(20)]
        public string? Result { get; set; }
        
        public int PointsEarned { get; set; } = 0;
        
        public int ExpEarned { get; set; } = 0;
        
        public int CouponEarned { get; set; } = 0;
        
        [StringLength(100)]
        public string? SessionID { get; set; }
        
        // 導航屬性
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("PetID")]
        public virtual Pet Pet { get; set; } = null!;
    }

    // 用戶簽到統計表 - 對應 SSMS 中的 UserSignInStats 表
    [Table("UserSignInStats")]
    public class UserSignInStats
    {
        [Key]
        public int LogID { get; set; }
        
        public DateTime SignTime { get; set; } = DateTime.UtcNow;
        
        public int UserID { get; set; }
        
        public int PointsGained { get; set; } = 0;
        
        public DateTime PointsGainedTime { get; set; } = DateTime.UtcNow;
        
        public int ExpGained { get; set; } = 0;
        
        public DateTime ExpGainedTime { get; set; } = DateTime.UtcNow;
        
        [StringLength(50)]
        public string? CouponGained { get; set; }
        
        public DateTime CouponGainedTime { get; set; } = DateTime.UtcNow;
        
        // 導航屬性
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }

    // 優惠券類型表 - 對應 SSMS 中的 CouponType 表
    [Table("CouponType")]
    public class CouponType
    {
        [Key]
        public int CouponTypeID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } = string.Empty;
        
        public decimal DiscountValue { get; set; }
        
        public decimal MinSpend { get; set; }
        
        public DateTime ValidFrom { get; set; }
        
        public DateTime ValidTo { get; set; }
        
        public int PointsCost { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // 導航屬性
        public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
    }

    // 優惠券表 - 對應 SSMS 中的 Coupon 表
    [Table("Coupon")]
    public class Coupon
    {
        [Key]
        public int CouponID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;
        
        public int CouponTypeID { get; set; }
        
        public int UserID { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime AcquiredTime { get; set; } = DateTime.UtcNow;
        
        public DateTime? UsedTime { get; set; }
        
        public int? UsedInOrderID { get; set; }
        
        // 導航屬性
        [ForeignKey("CouponTypeID")]
        public virtual CouponType CouponType { get; set; } = null!;
        
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }

    // 電子券類型表 - 對應 SSMS 中的 EVoucherType 表
    [Table("EVoucherType")]
    public class EVoucherType
    {
        [Key]
        public int EVoucherTypeID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public decimal ValueAmount { get; set; }
        
        public DateTime ValidFrom { get; set; }
        
        public DateTime ValidTo { get; set; }
        
        public int PointsCost { get; set; }
        
        public int TotalAvailable { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // 導航屬性
        public virtual ICollection<EVoucher> EVouchers { get; set; } = new List<EVoucher>();
    }

    // 電子券表 - 對應 SSMS 中的 EVoucher 表
    [Table("EVoucher")]
    public class EVoucher
    {
        [Key]
        public int EVoucherID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string EVoucherCode { get; set; } = string.Empty;
        
        public int EVoucherTypeID { get; set; }
        
        public int UserID { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime AcquiredTime { get; set; } = DateTime.UtcNow;
        
        public DateTime? UsedTime { get; set; }
        
        // 導航屬性
        [ForeignKey("EVoucherTypeID")]
        public virtual EVoucherType EVoucherType { get; set; } = null!;
        
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
        
        public virtual ICollection<EVoucherToken> EVoucherTokens { get; set; } = new List<EVoucherToken>();
        public virtual ICollection<EVoucherRedeemLog> EVoucherRedeemLogs { get; set; } = new List<EVoucherRedeemLog>();
    }

    // 電子券令牌表 - 對應 SSMS 中的 EVoucherToken 表
    [Table("EVoucherToken")]
    public class EVoucherToken
    {
        [Key]
        public int TokenID { get; set; }
        
        public int EVoucherID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Token { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
        
        public bool IsRevoked { get; set; } = false;
        
        // 導航屬性
        [ForeignKey("EVoucherID")]
        public virtual EVoucher EVoucher { get; set; } = null!;
    }

    // 電子券兌換記錄表 - 對應 SSMS 中的 EVoucherRedeemLog 表
    [Table("EVoucherRedeemLog")]
    public class EVoucherRedeemLog
    {
        [Key]
        public int LogID { get; set; }
        
        public int EVoucherID { get; set; }
        
        public int TokenID { get; set; }
        
        public int UserID { get; set; }
        
        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(200)]
        public string? StoreLocation { get; set; }
        
        [StringLength(50)]
        public string? StaffID { get; set; }
        
        // 導航屬性
        [ForeignKey("EVoucherID")]
        public virtual EVoucher EVoucher { get; set; } = null!;
        
        [ForeignKey("TokenID")]
        public virtual EVoucherToken EVoucherToken { get; set; } = null!;
        
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }

    // 新增：寵物換色所需點數設定表
    [Table("PetColorChangeCosts")]
    public class PetColorChangeCost
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PointsRequired { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // 新增：寵物換背景所需點數設定表
    [Table("PetBackgroundChangeCosts")]
    public class PetBackgroundChangeCost
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int PointsRequired { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // 新增：寵物顏色選項表
    [Table("PetColorOptions")]
    public class PetColorOption
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ColorName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string ColorValue { get; set; } = string.Empty;
        
        [Required]
        public int DisplayOrder { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // 新增：寵物背景選項表
    [Table("PetBackgroundOptions")]
    public class PetBackgroundOption
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string BackgroundName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string BackgroundValue { get; set; } = string.Empty;
        
        [Required]
        public int DisplayOrder { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // 新增：寵物升級規則表
    [Table("PetLevelUpRules")]
    public class PetLevelUpRule
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int Level { get; set; }
        
        [Required]
        public int ExperienceRequired { get; set; }
        
        [Required]
        public int PointsReward { get; set; }
        
        [Required]
        public int ExpReward { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // 新增：寵物互動狀態增益規則表
    [Table("PetInteractionBonusRules")]
    public class PetInteractionBonusRule
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string InteractionType { get; set; } = string.Empty;
        
        [Required]
        public int PointsCost { get; set; }
        
        [Required]
        public int HappinessGain { get; set; }
        
        [Required]
        public int ExpGain { get; set; }
        
        [Required]
        public int CooldownMinutes { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
