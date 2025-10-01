using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    // 管理者資料表
    public class ManagerData
    {
        [Key]
        public int Manager_Id { get; set; }
        
        [StringLength(30)]
        public string? Manager_Name { get; set; }
        
        [StringLength(30)]
        public string? Manager_Account { get; set; }
        
        [StringLength(200)]
        public string? Manager_Password { get; set; }
        
        public DateTime? Administrator_registration_date { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Manager_Email { get; set; } = string.Empty;
        
        public bool Manager_EmailConfirmed { get; set; } = false;
        
        public int Manager_AccessFailedCount { get; set; } = 0;
        
        public bool Manager_LockoutEnabled { get; set; } = true;
        
        public DateTime? Manager_LockoutEnd { get; set; }
    }

    // 管理者角色權限表
    public class ManagerRolePermission
    {
        [Key]
        public int ManagerRole_Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string role_name { get; set; } = string.Empty;
        
        public bool AdministratorPrivilegesManagement { get; set; } = false;
        
        public bool UserStatusManagement { get; set; } = false;
        
        public bool ShoppingPermissionManagement { get; set; } = false;
        
        public bool MessagePermissionManagement { get; set; } = false;
        
        public bool Pet_Rights_Management { get; set; } = false;
        
        public bool customer_service { get; set; } = false;
    }

    // 管理者角色分配表
    public class ManagerRole
    {
        [Key]
        public int Manager_Id { get; set; }
        
        [Key]
        public int ManagerRole_Id { get; set; }
        
        [ForeignKey("Manager_Id")]
        public virtual ManagerData ManagerData { get; set; } = null!;
        
        [ForeignKey("ManagerRole_Id")]
        public virtual ManagerRolePermission ManagerRolePermission { get; set; } = null!;
    }

    // 使用者資料表
    public class Users
    {
        [Key]
        public int User_ID { get; set; }
        
        [Required]
        [StringLength(30)]
        public string User_name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(30)]
        public string User_Account { get; set; } = string.Empty;
        
        [Required]
        [StringLength(30)]
        public string User_Password { get; set; } = string.Empty;
        
        public bool User_EmailConfirmed { get; set; } = false;
        
        public bool User_PhoneNumberConfirmed { get; set; } = false;
        
        public bool User_TwoFactorEnabled { get; set; } = false;
        
        public int User_AccessFailedCount { get; set; } = 0;
        
        public bool User_LockoutEnabled { get; set; } = true;
        
        public DateTime? User_LockoutEnd { get; set; }
    }

    // 使用者錢包表
    public class User_Wallet
    {
        [Key]
        public int User_Id { get; set; }
        
        public int User_Point { get; set; } = 0;
        
        [ForeignKey("User_Id")]
        public virtual Users Users { get; set; } = null!;
    }

    // 錢包歷史記錄表
    public class WalletHistory
    {
        [Key]
        public int HistoryID { get; set; }
        
        public int UserID { get; set; }
        
        public int ChangeAmount { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ChangeType { get; set; } = string.Empty;
        
        public DateTime ChangeTime { get; set; } = DateTime.Now;
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        public int? RelatedID { get; set; }
        
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }

    // 優惠券類型表
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
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MinSpend { get; set; }
        
        public DateTime ValidFrom { get; set; }
        
        public DateTime ValidTo { get; set; }
        
        public int PointsCost { get; set; } = 0;
        
        [StringLength(500)]
        public string? Description { get; set; }
    }

    // 優惠券表
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
        
        public DateTime AcquiredTime { get; set; } = DateTime.Now;
        
        public DateTime? UsedTime { get; set; }
        
        public int? UsedInOrderID { get; set; }
        
        [ForeignKey("CouponTypeID")]
        public virtual CouponType CouponType { get; set; } = null!;
        
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }

    // 電子禮券類型表
    public class EVoucherType
    {
        [Key]
        public int EVoucherTypeID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValueAmount { get; set; }
        
        public DateTime ValidFrom { get; set; }
        
        public DateTime ValidTo { get; set; }
        
        public int PointsCost { get; set; } = 0;
        
        public int TotalAvailable { get; set; } = 0;
        
        [StringLength(500)]
        public string? Description { get; set; }
    }

    // 電子禮券表
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
        
        public DateTime AcquiredTime { get; set; } = DateTime.Now;
        
        public DateTime? UsedTime { get; set; }
        
        [ForeignKey("EVoucherTypeID")]
        public virtual EVoucherType EVoucherType { get; set; } = null!;
        
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }

    // 電子禮券代幣表
    public class EVoucherToken
    {
        [Key]
        public int TokenID { get; set; }
        
        public int EVoucherID { get; set; }
        
        [Required]
        [StringLength(64)]
        public string Token { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
        
        public bool IsRevoked { get; set; } = false;
        
        [ForeignKey("EVoucherID")]
        public virtual EVoucher EVoucher { get; set; } = null!;
    }

    // 電子禮券兌換記錄表
    public class EVoucherRedeemLog
    {
        [Key]
        public int LogID { get; set; }
        
        public int EVoucherID { get; set; }
        
        public int TokenID { get; set; }
        
        public int UserID { get; set; }
        
        public DateTime ScannedAt { get; set; } = DateTime.Now;
        
        [StringLength(200)]
        public string? StoreLocation { get; set; }
        
        [StringLength(50)]
        public string? StaffID { get; set; }
        
        [ForeignKey("EVoucherID")]
        public virtual EVoucher EVoucher { get; set; } = null!;
        
        [ForeignKey("TokenID")]
        public virtual EVoucherToken EVoucherToken { get; set; } = null!;
        
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }

    // 使用者簽到統計表
    public class UserSignInStats
    {
        [Key]
        public int StatsID { get; set; }
        
        public int UserID { get; set; }
        
        public DateTime SignTime { get; set; } = DateTime.Now;
        
        public int PointsEarned { get; set; } = 0;
        
        public int PetExpEarned { get; set; } = 0;
        
        public int? CouponEarned { get; set; }
        
        public int ConsecutiveDays { get; set; } = 1;
        
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }

    // 寵物表
    public class Pet
    {
        [Key]
        public int PetID { get; set; }
        
        public int UserID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PetName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(30)]
        public string PetType { get; set; } = string.Empty;
        
        public int PetLevel { get; set; } = 1;
        
        public int PetExp { get; set; } = 0;
        
        [StringLength(30)]
        public string PetSkin { get; set; } = "default";
        
        [StringLength(30)]
        public string PetBackground { get; set; } = "default";
        
        public int Hunger { get; set; } = 100;
        
        public int Happiness { get; set; } = 100;
        
        public int Health { get; set; } = 100;
        
        public int Energy { get; set; } = 100;
        
        public int Cleanliness { get; set; } = 100;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? LastFed { get; set; }
        
        public DateTime? LastPlayed { get; set; }
        
        public DateTime? LastBathed { get; set; }
        
        public DateTime? LastSlept { get; set; }
        
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }

    // 寵物外觀變更記錄表
    public class PetAppearanceChangeLog
    {
        [Key]
        public int LogID { get; set; }
        
        public int PetID { get; set; }
        
        [Required]
        [StringLength(20)]
        public string ChangeType { get; set; } = string.Empty;
        
        [StringLength(30)]
        public string? OldValue { get; set; }
        
        [Required]
        [StringLength(30)]
        public string NewValue { get; set; } = string.Empty;
        
        public int PointsCost { get; set; } = 0;
        
        public DateTime ChangedAt { get; set; } = DateTime.Now;
        
        [ForeignKey("PetID")]
        public virtual Pet Pet { get; set; } = null!;
    }

    // 小遊戲記錄表
    public class MiniGame
    {
        [Key]
        public int GameID { get; set; }
        
        public int UserID { get; set; }
        
        public int PetID { get; set; }
        
        [Required]
        [StringLength(30)]
        public string GameType { get; set; } = string.Empty;
        
        public DateTime StartTime { get; set; } = DateTime.Now;
        
        public DateTime? EndTime { get; set; }
        
        [StringLength(10)]
        public string? GameResult { get; set; }
        
        public int PointsEarned { get; set; } = 0;
        
        public int PetExpEarned { get; set; } = 0;
        
        public int? CouponEarned { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SessionID { get; set; } = string.Empty;
        
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
        
        [ForeignKey("PetID")]
        public virtual Pet Pet { get; set; } = null!;
    }

    // 排行榜快照表
    public class leaderboard_snapshots
    {
        [Key]
        public int snapshot_id { get; set; }
        
        public int game_id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string period { get; set; } = string.Empty;
        
        public DateTime ts { get; set; } = DateTime.Now;
        
        public int rank { get; set; }
        
        public int user_id { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal score { get; set; }
        
        [StringLength(500)]
        public string? metadata { get; set; }
        
        [ForeignKey("user_id")]
        public virtual Users Users { get; set; } = null!;
    }

    // 使用者代幣表
    public class UserTokens
    {
        [Key]
        public int TokenID { get; set; }
        
        public int User_ID { get; set; }
        
        [Required]
        [StringLength(20)]
        public string TokenType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string TokenValue { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsUsed { get; set; } = false;
        
        [ForeignKey("User_ID")]
        public virtual Users Users { get; set; } = null!;
    }
}
