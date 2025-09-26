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
        public int User_ID { get; set; }
        public string User_name { get; set; }
        public string User_Account { get; set; }
        public string User_Password { get; set; }
        public bool User_EmailConfirmed { get; set; }
        public bool User_PhoneNumberConfirmed { get; set; }
        public bool User_TwoFactorEnabled { get; set; }
        public int User_AccessFailedCount { get; set; }
        public bool User_LockoutEnabled { get; set; }
        public DateTime? User_LockoutEnd { get; set; }

        // 導航屬性
        public virtual UserIntroduce UserIntroduce { get; set; }
        public virtual UserRights UserRights { get; set; }
        public virtual UserWallet UserWallet { get; set; }
        public virtual UserSignInStats UserSignInStats { get; set; }
        public virtual Pet Pet { get; set; }
        public virtual ICollection<MiniGame> MiniGames { get; set; }
        public virtual ICollection<WalletHistory> WalletHistories { get; set; }
        public virtual ICollection<Coupon> Coupons { get; set; }
        public virtual ICollection<EVoucher> EVouchers { get; set; }
    }

    public class UserIntroduce
    {
        [Key]
        [ForeignKey("User")]
        public int User_ID { get; set; }
        public string User_NickName { get; set; }
        public string Gender { get; set; }
        public string IdNumber { get; set; }
        public string Cellphone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime Create_Account { get; set; }
        public byte[] User_Picture { get; set; }
        public string User_Introduce { get; set; }

        public virtual User User { get; set; }
    }

    public class UserRights
    {
        [Key]
        [ForeignKey("User")]
        public int User_Id { get; set; }
        public bool? User_Status { get; set; }
        public bool? ShoppingPermission { get; set; }
        public bool? MessagePermission { get; set; }
        public bool? SalesAuthority { get; set; }

        public virtual User User { get; set; }
    }

    // 錢包相關模型
    public class UserWallet
    {
        [Key]
        [ForeignKey("User")]
        public int User_Id { get; set; }
        public int User_Point { get; set; }

        public virtual User User { get; set; }
    }

    public class WalletHistory
    {
        [Key]
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string ChangeType { get; set; }
        public int PointsChanged { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public DateTime ChangeTime { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }

    // 簽到相關模型
    public class UserSignInStats
    {
        [Key]
        public int LogID { get; set; }
        public DateTime SignTime { get; set; }
        public int UserID { get; set; }
        public int PointsGained { get; set; }
        public DateTime PointsGainedTime { get; set; }
        public int ExpGained { get; set; }
        public DateTime ExpGainedTime { get; set; }
        public string CouponGained { get; set; }
        public DateTime CouponGainedTime { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }

    // 寵物相關模型
    public class Pet
    {
        [Key]
        public int PetID { get; set; }
        public int UserID { get; set; }
        public string PetName { get; set; }
        public int Level { get; set; }
        public DateTime LevelUpTime { get; set; }
        public int Experience { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Stamina { get; set; }
        public int Cleanliness { get; set; }
        public int Health { get; set; }
        public string SkinColor { get; set; }
        public DateTime SkinColorChangedTime { get; set; }
        public string BackgroundColor { get; set; }
        public DateTime BackgroundColorChangedTime { get; set; }
        public int PointsChanged_SkinColor { get; set; }
        public int PointsChanged_BackgroundColor { get; set; }
        public int PointsGained_LevelUp { get; set; }
        public DateTime PointsGainedTime_LevelUp { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        public virtual ICollection<MiniGame> MiniGames { get; set; }
    }

    // 小遊戲相關模型
    public class MiniGame
    {
        [Key]
        public int PlayID { get; set; }
        public int UserID { get; set; }
        public int PetID { get; set; }
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public string Result { get; set; }
        public int ExpGained { get; set; }
        public DateTime ExpGainedTime { get; set; }
        public int PointsGained { get; set; }
        public DateTime PointsGainedTime { get; set; }
        public string CouponGained { get; set; }
        public DateTime CouponGainedTime { get; set; }
        public int HungerDelta { get; set; }
        public int MoodDelta { get; set; }
        public int StaminaDelta { get; set; }
        public int CleanlinessDelta { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Aborted { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        [ForeignKey("PetID")]
        public virtual Pet Pet { get; set; }
    }

    // 優惠券相關模型
    public class CouponType
    {
        [Key]
        public int CouponTypeID { get; set; }
        public string Name { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinSpend { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int PointsCost { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Coupon> Coupons { get; set; }
    }

    public class Coupon
    {
        [Key]
        public int CouponID { get; set; }
        public string CouponCode { get; set; }
        public int CouponTypeID { get; set; }
        public int UserID { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public int? UsedInOrderID { get; set; }

        [ForeignKey("CouponTypeID")]
        public virtual CouponType CouponType { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }

    // 電子禮券相關模型
    public class EVoucherType
    {
        [Key]
        public int EVoucherTypeID { get; set; }
        public string Name { get; set; }
        public decimal ValueAmount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int PointsCost { get; set; }
        public int TotalAvailable { get; set; }
        public string Description { get; set; }

        public virtual ICollection<EVoucher> EVouchers { get; set; }
    }

    public class EVoucher
    {
        [Key]
        public int EVoucherID { get; set; }
        public string EVoucherCode { get; set; }
        public int EVoucherTypeID { get; set; }
        public int UserID { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }

        [ForeignKey("EVoucherTypeID")]
        public virtual EVoucherType EVoucherType { get; set; }
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }

    // 管理員相關模型
    public class ManagerData
    {
        [Key]
        public int Manager_Id { get; set; }
        public string Manager_Name { get; set; }
        public string Manager_Account { get; set; }
        public string Manager_Password { get; set; }
        public DateTime? Administrator_registration_date { get; set; }
        public string Manager_Email { get; set; }
        public bool Manager_EmailConfirmed { get; set; }
        public int Manager_AccessFailedCount { get; set; }
        public bool Manager_LockoutEnabled { get; set; }
        public DateTime? Manager_LockoutEnd { get; set; }

        public virtual ICollection<ManagerRole> ManagerRoles { get; set; }
    }

    public class ManagerRolePermission
    {
        [Key]
        public int ManagerRole_Id { get; set; }
        public string role_name { get; set; }
        public bool? AdministratorPrivilegesManagement { get; set; }
        public bool? UserStatusManagement { get; set; }
        public bool? ShoppingPermissionManagement { get; set; }
        public bool? MessagePermissionManagement { get; set; }
        public bool? Pet_Rights_Management { get; set; }
        public bool? customer_service { get; set; }

        public virtual ICollection<ManagerRole> ManagerRoles { get; set; }
    }

    public class ManagerRole
    {
        [Key]
        [Column(Order = 0)]
        public int Manager_Id { get; set; }
        [Key]
        [Column(Order = 1)]
        public int ManagerRole_Id { get; set; }

        [ForeignKey("Manager_Id")]
        public virtual ManagerData ManagerData { get; set; }
        [ForeignKey("ManagerRole_Id")]
        public virtual ManagerRolePermission ManagerRolePermission { get; set; }
    }
}
