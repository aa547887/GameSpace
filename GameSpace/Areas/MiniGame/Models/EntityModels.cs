using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    // Main entity models based on database schema
    
    [Table("Users")]
    public class Users
    {
        [Key]
        public int User_ID { get; set; }
        
        [Required]
        [StringLength(30)]
        public string User_name { get; set; }
        
        [Required]
        [StringLength(30)]
        public string User_Account { get; set; }
        
        [Required]
        [StringLength(30)]
        public string User_Password { get; set; }
        
        public bool User_EmailConfirmed { get; set; }
        public bool User_PhoneNumberConfirmed { get; set; }
        public bool User_TwoFactorEnabled { get; set; }
        public int User_AccessFailedCount { get; set; }
        public bool User_LockoutEnabled { get; set; }
        public DateTime? User_LockoutEnd { get; set; }
        
        // Navigation properties
        public virtual User_Wallet User_Wallet { get; set; }
        public virtual User_Rights User_Rights { get; set; }
        public virtual User_Introduce User_Introduce { get; set; }
        public virtual ICollection<Pet> Pets { get; set; }
        public virtual ICollection<UserSignInStats> SignInStats { get; set; }
        public virtual ICollection<MiniGame> MiniGames { get; set; }
        public virtual ICollection<WalletHistory> WalletHistory { get; set; }
        
        // Additional property for status check
        public bool User_Status => User_Rights?.User_Status ?? true;
    }

    [Table("User_Wallet")]
    public class User_Wallet
    {
        [Key]
        public int User_Id { get; set; }
        
        public int User_Point { get; set; }
        
        // Navigation property
        [ForeignKey("User_Id")]
        public virtual Users User { get; set; }
    }

    [Table("User_Rights")]
    public class User_Rights
    {
        [Key]
        public int User_Id { get; set; }
        
        public bool? User_Status { get; set; }
        public bool? ShoppingPermission { get; set; }
        public bool? MessagePermission { get; set; }
        public bool? SalesAuthority { get; set; }
        
        // Navigation property
        [ForeignKey("User_Id")]
        public virtual Users User { get; set; }
    }

    [Table("User_Introduce")]
    public class User_Introduce
    {
        [Key]
        public int User_ID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string User_NickName { get; set; }
        
        [Required]
        [StringLength(1)]
        public string Gender { get; set; }
        
        [Required]
        [StringLength(30)]
        public string IdNumber { get; set; }
        
        [Required]
        [StringLength(30)]
        public string Cellphone { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Email { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Address { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        public DateTime Create_Account { get; set; }
        public byte[] User_Picture { get; set; }
        
        [StringLength(200)]
        public string User_Introduce { get; set; }
        
        // Navigation property
        [ForeignKey("User_ID")]
        public virtual Users User { get; set; }
    }

    [Table("Pet")]
    public class Pet
    {
        [Key]
        public int PetID { get; set; }
        
        public int UserID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PetName { get; set; }
        
        public int Level { get; set; }
        public DateTime LevelUpTime { get; set; }
        public int Experience { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Stamina { get; set; }
        public int Cleanliness { get; set; }
        public int Health { get; set; }
        
        [StringLength(10)]
        public string SkinColor { get; set; }
        
        public DateTime SkinColorChangedTime { get; set; }
        
        [StringLength(20)]
        public string BackgroundColor { get; set; }
        
        public DateTime BackgroundColorChangedTime { get; set; }
        public int PointsChanged_SkinColor { get; set; }
        public int PointsChanged_BackgroundColor { get; set; }
        public int PointsGained_LevelUp { get; set; }
        public DateTime PointsGainedTime_LevelUp { get; set; }
        
        // Navigation properties
        [ForeignKey("UserID")]
        public virtual Users User { get; set; }
        
        public virtual ICollection<MiniGame> MiniGames { get; set; }
    }

    [Table("MiniGame")]
    public class MiniGame
    {
        [Key]
        public int PlayID { get; set; }
        
        public int UserID { get; set; }
        public int PetID { get; set; }
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public decimal SpeedMultiplier { get; set; }
        
        [StringLength(20)]
        public string Result { get; set; }
        
        public int ExpGained { get; set; }
        public DateTime ExpGainedTime { get; set; }
        public int PointsGained { get; set; }
        public DateTime PointsGainedTime { get; set; }
        
        [StringLength(50)]
        public string CouponGained { get; set; }
        
        public DateTime CouponGainedTime { get; set; }
        public int HungerDelta { get; set; }
        public int MoodDelta { get; set; }
        public int StaminaDelta { get; set; }
        public int CleanlinessDelta { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Aborted { get; set; }
        
        // Navigation properties
        [ForeignKey("UserID")]
        public virtual Users User { get; set; }
        
        [ForeignKey("PetID")]
        public virtual Pet Pet { get; set; }
    }

    [Table("UserSignInStats")]
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
        
        [StringLength(50)]
        public string CouponGained { get; set; }
        
        public DateTime CouponGainedTime { get; set; }
        
        // Navigation property
        [ForeignKey("UserID")]
        public virtual Users User { get; set; }
    }

    [Table("WalletHistory")]
    public class WalletHistory
    {
        [Key]
        public int LogID { get; set; }
        
        public int UserID { get; set; }
        
        [StringLength(20)]
        public string ChangeType { get; set; }
        
        public int PointsChanged { get; set; }
        
        [StringLength(50)]
        public string ItemCode { get; set; }
        
        [StringLength(200)]
        public string Description { get; set; }
        
        public DateTime ChangeTime { get; set; }
        
        // Navigation property
        [ForeignKey("UserID")]
        public virtual Users User { get; set; }
    }

    [Table("Coupon")]
    public class Coupon
    {
        [Key]
        public int CouponID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; }
        
        public int CouponTypeID { get; set; }
        public int UserID { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        public int? UsedInOrderID { get; set; }
        
        // Navigation properties
        [ForeignKey("UserID")]
        public virtual Users User { get; set; }
        
        [ForeignKey("CouponTypeID")]
        public virtual CouponType CouponType { get; set; }
    }

    [Table("CouponType")]
    public class CouponType
    {
        [Key]
        public int CouponTypeID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        [StringLength(20)]
        public string DiscountType { get; set; }
        
        public decimal DiscountValue { get; set; }
        public decimal MinSpend { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int PointsCost { get; set; }
        
        [StringLength(600)]
        public string Description { get; set; }
        
        // Navigation property
        public virtual ICollection<Coupon> Coupons { get; set; }
    }

    [Table("EVoucher")]
    public class EVoucher
    {
        [Key]
        public int EVoucherID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string EVoucherCode { get; set; }
        
        public int EVoucherTypeID { get; set; }
        public int UserID { get; set; }
        public bool IsUsed { get; set; }
        public DateTime AcquiredTime { get; set; }
        public DateTime? UsedTime { get; set; }
        
        // Navigation properties
        [ForeignKey("UserID")]
        public virtual Users User { get; set; }
        
        [ForeignKey("EVoucherTypeID")]
        public virtual EVoucherType EVoucherType { get; set; }
    }

    [Table("EVoucherType")]
    public class EVoucherType
    {
        [Key]
        public int EVoucherTypeID { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        public decimal ValueAmount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int PointsCost { get; set; }
        public int TotalAvailable { get; set; }
        
        [StringLength(600)]
        public string Description { get; set; }
        
        // Navigation property
        public virtual ICollection<EVoucher> EVouchers { get; set; }
    }
}
