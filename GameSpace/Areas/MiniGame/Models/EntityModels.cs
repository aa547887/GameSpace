using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    // 使用者相關模型
    public class User
    {
        [Key]
        [Column("User_ID")]
        public int User_ID { get; set; }

        [Required]
        [StringLength(30)]
        [Column("User_name")]
        public string User_name { get; set; }

        [Required]
        [StringLength(30)]
        [Column("User_Account")]
        public string User_Account { get; set; }

        [Required]
        [StringLength(30)]
        [Column("User_Password")]
        public string User_Password { get; set; }

        [Column("User_EmailConfirmed")]
        public bool User_EmailConfirmed { get; set; }

        [Column("User_PhoneNumberConfirmed")]
        public bool User_PhoneNumberConfirmed { get; set; }

        [Column("User_TwoFactorEnabled")]
        public bool User_TwoFactorEnabled { get; set; }

        [Column("User_AccessFailedCount")]
        public int User_AccessFailedCount { get; set; }

        [Column("User_LockoutEnabled")]
        public bool User_LockoutEnabled { get; set; }

        [Column("User_LockoutEnd")]
        public DateTime? User_LockoutEnd { get; set; }

        // 導航屬性
        public virtual UserIntroduce UserIntroduce { get; set; }
        public virtual UserRights UserRights { get; set; }
        public virtual UserWallet UserWallet { get; set; }
        public virtual ICollection<UserSignInStats> UserSignInStats { get; set; }
        public virtual Pet Pet { get; set; }
        public virtual ICollection<MiniGame> MiniGames { get; set; }
        public virtual ICollection<WalletHistory> WalletHistories { get; set; }
        public virtual ICollection<Coupon> Coupons { get; set; }
        public virtual ICollection<EVoucher> EVouchers { get; set; }
    }

    // 使用者介紹
    public class UserIntroduce
    {
        [Key]
        [Column("User_ID")]
        public int User_ID { get; set; }

        [StringLength(500)]
        [Column("User_Introduce")]
        public string User_Introduce { get; set; }

        [ForeignKey("User_ID")]
        public virtual User User { get; set; }
    }

    // 使用者權限
    public class UserRights
    {
        [Key]
        [Column("User_ID")]
        public int User_ID { get; set; }

        [Column("User_Rights")]
        public bool User_Rights { get; set; }

        [ForeignKey("User_ID")]
        public virtual User User { get; set; }
    }

    // 使用者錢包
    public class UserWallet
    {
        [Key]
        [Column("User_Id")]
        public int User_Id { get; set; }

        [Column("User_Point")]
        public int User_Point { get; set; }

        [ForeignKey("User_Id")]
        public virtual User User { get; set; }
    }

    // 使用者簽到統計
    public class UserSignInStats
    {
        [Key]
        [Column("LogID")]
        public int LogID { get; set; }

        [Column("SignTime")]
        public DateTime SignTime { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("PointsGained")]
        public int PointsGained { get; set; }

        [Column("PointsGainedTime")]
        public DateTime PointsGainedTime { get; set; }

        [Column("ExpGained")]
        public int ExpGained { get; set; }

        [Column("ExpGainedTime")]
        public DateTime ExpGainedTime { get; set; }

        [StringLength(50)]
        [Column("CouponGained")]
        public string CouponGained { get; set; }

        [Column("CouponGainedTime")]
        public DateTime CouponGainedTime { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }

    // 寵物
    public class Pet
    {
        [Key]
        [Column("PetID")]
        public int PetID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("PetName")]
        public string PetName { get; set; }

        [Column("Level")]
        public int Level { get; set; }

        [Column("LevelUpTime")]
        public DateTime LevelUpTime { get; set; }

        [Column("Experience")]
        public int Experience { get; set; }

        [Column("Hunger")]
        public int Hunger { get; set; }

        [Column("Mood")]
        public int Mood { get; set; }

        [Column("Stamina")]
        public int Stamina { get; set; }

        [Column("Cleanliness")]
        public int Cleanliness { get; set; }

        [Column("Health")]
        public int Health { get; set; }

        [Required]
        [StringLength(10)]
        [Column("SkinColor")]
        public string SkinColor { get; set; }

        [Column("SkinColorChangedTime")]
        public DateTime SkinColorChangedTime { get; set; }

        [Required]
        [StringLength(20)]
        [Column("BackgroundColor")]
        public string BackgroundColor { get; set; }

        [Column("BackgroundColorChangedTime")]
        public DateTime BackgroundColorChangedTime { get; set; }

        [Column("PointsChanged_SkinColor")]
        public int PointsChanged_SkinColor { get; set; }

        [Column("PointsChanged_BackgroundColor")]
        public int PointsChanged_BackgroundColor { get; set; }

        [Column("PointsGained_LevelUp")]
        public int PointsGained_LevelUp { get; set; }

        [Column("PointsGainedTime_LevelUp")]
        public DateTime PointsGainedTime_LevelUp { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        public virtual ICollection<MiniGame> MiniGames { get; set; }
    }

    // 小遊戲
    public class MiniGame
    {
        [Key]
        [Column("PlayID")]
        public int PlayID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("PetID")]
        public int PetID { get; set; }

        [Column("Level")]
        public int Level { get; set; }

        [Column("MonsterCount")]
        public int MonsterCount { get; set; }

        [Column("SpeedMultiplier")]
        public decimal SpeedMultiplier { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Result")]
        public string Result { get; set; }

        [Column("ExpGained")]
        public int ExpGained { get; set; }

        [Column("ExpGainedTime")]
        public DateTime ExpGainedTime { get; set; }

        [Column("PointsGained")]
        public int PointsGained { get; set; }

        [Column("PointsGainedTime")]
        public DateTime PointsGainedTime { get; set; }

        [Required]
        [StringLength(50)]
        [Column("CouponGained")]
        public string CouponGained { get; set; }

        [Column("CouponGainedTime")]
        public DateTime CouponGainedTime { get; set; }

        [Column("HungerDelta")]
        public int HungerDelta { get; set; }

        [Column("MoodDelta")]
        public int MoodDelta { get; set; }

        [Column("StaminaDelta")]
        public int StaminaDelta { get; set; }

        [Column("CleanlinessDelta")]
        public int CleanlinessDelta { get; set; }

        [Column("StartTime")]
        public DateTime StartTime { get; set; }

        [Column("EndTime")]
        public DateTime? EndTime { get; set; }

        [Column("Aborted")]
        public bool Aborted { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("PetID")]
        public virtual Pet Pet { get; set; }
    }

    // 錢包歷史
    public class WalletHistory
    {
        [Key]
        [Column("LogID")]
        public int LogID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [StringLength(20)]
        [Column("ChangeType")]
        public string ChangeType { get; set; }

        [Column("PointsChanged")]
        public int PointsChanged { get; set; }

        [StringLength(50)]
        [Column("ItemCode")]
        public string ItemCode { get; set; }

        [StringLength(200)]
        [Column("Description")]
        public string Description { get; set; }

        [Column("ChangeTime")]
        public DateTime ChangeTime { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }

    // 優惠券
    public class Coupon
    {
        [Key]
        [Column("CouponID")]
        public int CouponID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("CouponCode")]
        public string CouponCode { get; set; }

        [Column("CouponTypeID")]
        public int CouponTypeID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("IsUsed")]
        public bool IsUsed { get; set; }

        [Column("AcquiredTime")]
        public DateTime AcquiredTime { get; set; }

        [Column("UsedTime")]
        public DateTime? UsedTime { get; set; }

        [Column("UsedInOrderID")]
        public int? UsedInOrderID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("CouponTypeID")]
        public virtual CouponType CouponType { get; set; }
    }

    // 優惠券類型
    public class CouponType
    {
        [Key]
        [Column("CouponTypeID")]
        public int CouponTypeID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        [Column("DiscountType")]
        public string DiscountType { get; set; }

        [Column("DiscountValue")]
        public decimal DiscountValue { get; set; }

        [Column("MinSpend")]
        public decimal MinSpend { get; set; }

        [Column("ValidFrom")]
        public DateTime ValidFrom { get; set; }

        [Column("ValidTo")]
        public DateTime ValidTo { get; set; }

        [Column("PointsCost")]
        public int PointsCost { get; set; }

        [StringLength(600)]
        [Column("Description")]
        public string Description { get; set; }

        public virtual ICollection<Coupon> Coupons { get; set; }
    }

    // 電子禮券
    public class EVoucher
    {
        [Key]
        [Column("EVoucherID")]
        public int EVoucherID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("EVoucherCode")]
        public string EVoucherCode { get; set; }

        [Column("EVoucherTypeID")]
        public int EVoucherTypeID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("IsUsed")]
        public bool IsUsed { get; set; }

        [Column("AcquiredTime")]
        public DateTime AcquiredTime { get; set; }

        [Column("UsedTime")]
        public DateTime? UsedTime { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("EVoucherTypeID")]
        public virtual EVoucherType EVoucherType { get; set; }

        public virtual ICollection<EVoucherToken> EVoucherTokens { get; set; }
        public virtual ICollection<EVoucherRedeemLog> EVoucherRedeemLogs { get; set; }
    }

    // 電子禮券類型
    public class EVoucherType
    {
        [Key]
        [Column("EVoucherTypeID")]
        public int EVoucherTypeID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Name")]
        public string Name { get; set; }

        [Column("ValueAmount")]
        public decimal ValueAmount { get; set; }

        [Column("ValidFrom")]
        public DateTime ValidFrom { get; set; }

        [Column("ValidTo")]
        public DateTime ValidTo { get; set; }

        [Column("PointsCost")]
        public int PointsCost { get; set; }

        [Column("TotalAvailable")]
        public int TotalAvailable { get; set; }

        [StringLength(600)]
        [Column("Description")]
        public string Description { get; set; }

        public virtual ICollection<EVoucher> EVouchers { get; set; }
    }

    // 電子禮券代幣
    public class EVoucherToken
    {
        [Key]
        [Column("TokenID")]
        public int TokenID { get; set; }

        [Column("EVoucherID")]
        public int EVoucherID { get; set; }

        [Required]
        [StringLength(64)]
        [Column("Token")]
        public string Token { get; set; }

        [Column("ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        [Column("IsRevoked")]
        public bool IsRevoked { get; set; }

        [ForeignKey("EVoucherID")]
        public virtual EVoucher EVoucher { get; set; }
        public virtual ICollection<EVoucherRedeemLog> EVoucherRedeemLogs { get; set; }
    }

    // 電子禮券兌換記錄
    public class EVoucherRedeemLog
    {
        [Key]
        [Column("RedeemID")]
        public int RedeemID { get; set; }

        [Column("EVoucherID")]
        public int EVoucherID { get; set; }

        [Column("TokenID")]
        public int? TokenID { get; set; }

        [Column("UserID")]
        public int UserID { get; set; }

        [Column("ScannedAt")]
        public DateTime ScannedAt { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Status")]
        public string Status { get; set; }

        [ForeignKey("EVoucherID")]
        public virtual EVoucher EVoucher { get; set; }

        [ForeignKey("TokenID")]
        public virtual EVoucherToken EVoucherToken { get; set; }
    }

    // 管理者資料
    public class ManagerData
    {
        [Key]
        [Column("Manager_Id")]
        public int Manager_Id { get; set; }

        [StringLength(30)]
        [Column("Manager_Name")]
        public string Manager_Name { get; set; }

        [StringLength(30)]
        [Column("Manager_Account")]
        public string Manager_Account { get; set; }

        [StringLength(200)]
        [Column("Manager_Password")]
        public string Manager_Password { get; set; }

        [Column("Administrator_registration_date")]
        public DateTime? Administrator_registration_date { get; set; }

        [Required]
        [StringLength(255)]
        [Column("Manager_Email")]
        public string Manager_Email { get; set; }

        [Column("Manager_EmailConfirmed")]
        public bool Manager_EmailConfirmed { get; set; }

        [Column("Manager_AccessFailedCount")]
        public int Manager_AccessFailedCount { get; set; }

        [Column("Manager_LockoutEnabled")]
        public bool Manager_LockoutEnabled { get; set; }

        [Column("Manager_LockoutEnd")]
        public DateTime? Manager_LockoutEnd { get; set; }

        public virtual ICollection<ManagerRole> ManagerRoles { get; set; }
    }

    // 管理者角色
    public class ManagerRole
    {
        [Key]
        [Column("Manager_Id")]
        public int Manager_Id { get; set; }

        [Key]
        [Column("ManagerRole_Id")]
        public int ManagerRole_Id { get; set; }

        [ForeignKey("Manager_Id")]
        public virtual ManagerData ManagerData { get; set; }

        [ForeignKey("ManagerRole_Id")]
        public virtual ManagerRolePermission ManagerRolePermission { get; set; }
    }

    // 管理者角色權限
    public class ManagerRolePermission
    {
        [Key]
        [Column("ManagerRole_Id")]
        public int ManagerRole_Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("role_name")]
        public string role_name { get; set; }

        [Column("AdministratorPrivilegesManagement")]
        public bool? AdministratorPrivilegesManagement { get; set; }

        [Column("UserStatusManagement")]
        public bool? UserStatusManagement { get; set; }

        [Column("ShoppingPermissionManagement")]
        public bool? ShoppingPermissionManagement { get; set; }

        [Column("MessagePermissionManagement")]
        public bool? MessagePermissionManagement { get; set; }

        [Column("Pet_Rights_Management")]
        public bool? Pet_Rights_Management { get; set; }

        [Column("customer_service")]
        public bool? customer_service { get; set; }

        public virtual ICollection<ManagerRole> ManagerRoles { get; set; }
    }

    // 簽到規則設定
    public class SignInRuleSettings
    {
        [Key]
        public int SettingID { get; set; }

        [Required]
        [StringLength(50)]
        public string SettingName { get; set; }

        [Required]
        [StringLength(200)]
        public string SettingValue { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedByManagerId { get; set; }
        public int? UpdatedByManagerId { get; set; }
    }

    // 寵物系統規則設定
    public class PetSystemRuleSettings
    {
        [Key]
        public int SettingID { get; set; }

        [Required]
        [StringLength(50)]
        public string SettingName { get; set; }

        [Required]
        [StringLength(200)]
        public string SettingValue { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedByManagerId { get; set; }
        public int? UpdatedByManagerId { get; set; }
    }

    // 小遊戲規則設定
    public class MiniGameRuleSettings
    {
        [Key]
        public int SettingID { get; set; }

        [Required]
        [StringLength(50)]
        public string SettingName { get; set; }

        [Required]
        [StringLength(200)]
        public string SettingValue { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedByManagerId { get; set; }
        public int? UpdatedByManagerId { get; set; }
    }

    // 寵物外觀變更記錄
    public class PetAppearanceChangeLog
    {
        [Key]
        public int LogID { get; set; }

        [Required]
        public int PetID { get; set; }

        [Required]
        [StringLength(20)]
        public string ChangeType { get; set; }

        [StringLength(50)]
        public string OldValue { get; set; }

        [StringLength(50)]
        public string NewValue { get; set; }

        [Required]
        public int PointsCost { get; set; }

        public DateTime ChangedAt { get; set; }

        [ForeignKey("PetID")]
        public virtual Pet Pet { get; set; }
    }
}
