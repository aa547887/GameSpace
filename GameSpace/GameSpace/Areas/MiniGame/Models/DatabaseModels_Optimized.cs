using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理員資料表 - 對應 SSMS 中的 ManagerData 表
    /// </summary>
    [Table(""ManagerData"")]
    public class ManagerData
    {
        /// <summary>
        /// 管理員唯一識別碼
        /// </summary>
        [Key]
        public int Manager_Id { get; set; }
        
        /// <summary>
        /// 管理員姓名
        /// </summary>
        [Required(ErrorMessage = ""管理員姓名不能為空"")]
        [StringLength(50, ErrorMessage = ""管理員姓名長度不能超過50個字符"")]
        [Display(Name = ""管理員姓名"")]
        public string Manager_Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 管理員帳號
        /// </summary>
        [Required(ErrorMessage = ""管理員帳號不能為空"")]
        [StringLength(50, ErrorMessage = ""管理員帳號長度不能超過50個字符"")]
        [Display(Name = ""管理員帳號"")]
        public string Manager_Account { get; set; } = string.Empty;
        
        /// <summary>
        /// 管理員密碼
        /// </summary>
        [Required(ErrorMessage = ""管理員密碼不能為空"")]
        [StringLength(100, ErrorMessage = ""管理員密碼長度不能超過100個字符"")]
        [Display(Name = ""管理員密碼"")]
        public string Manager_Password { get; set; } = string.Empty;
        
        /// <summary>
        /// 管理員電子郵件
        /// </summary>
        [Required(ErrorMessage = ""管理員電子郵件不能為空"")]
        [StringLength(100, ErrorMessage = ""管理員電子郵件長度不能超過100個字符"")]
        [EmailAddress(ErrorMessage = ""管理員電子郵件格式不正確"")]
        [Display(Name = ""管理員電子郵件"")]
        public string Manager_Email { get; set; } = string.Empty;
        
        /// <summary>
        /// 電子郵件確認狀態
        /// </summary>
        [Display(Name = ""電子郵件確認狀態"")]
        public bool Manager_EmailConfirmed { get; set; } = false;
        
        /// <summary>
        /// 登入失敗次數
        /// </summary>
        [Display(Name = ""登入失敗次數"")]
        public int Manager_AccessFailedCount { get; set; } = 0;
        
        /// <summary>
        /// 帳號鎖定啟用狀態
        /// </summary>
        [Display(Name = ""帳號鎖定啟用狀態"")]
        public bool Manager_LockoutEnabled { get; set; } = true;
        
        /// <summary>
        /// 帳號鎖定結束時間
        /// </summary>
        [Display(Name = ""帳號鎖定結束時間"")]
        public DateTime? Manager_LockoutEnd { get; set; }
        
        /// <summary>
        /// 管理員註冊日期
        /// </summary>
        [Display(Name = ""管理員註冊日期"")]
        public DateTime Administrator_registration_date { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 管理員角色關聯
        /// </summary>
        public virtual ICollection<ManagerRole> ManagerRoles { get; set; } = new List<ManagerRole>();
    }

    /// <summary>
    /// 管理員角色權限表 - 對應 SSMS 中的 ManagerRolePermission 表
    /// </summary>
    [Table(""ManagerRolePermission"")]
    public class ManagerRolePermission
    {
        /// <summary>
        /// 角色唯一識別碼
        /// </summary>
        [Key]
        public int ManagerRole_Id { get; set; }
        
        /// <summary>
        /// 角色名稱
        /// </summary>
        [Required(ErrorMessage = ""角色名稱不能為空"")]
        [StringLength(100, ErrorMessage = ""角色名稱長度不能超過100個字符"")]
        [Display(Name = ""角色名稱"")]
        public string role_name { get; set; } = string.Empty;
        
        /// <summary>
        /// 管理者平台管理權限
        /// </summary>
        [Display(Name = ""管理者平台管理權限"")]
        public bool AdministratorPrivilegesManagement { get; set; } = false;
        
        /// <summary>
        /// 使用者狀態管理權限
        /// </summary>
        [Display(Name = ""使用者狀態管理權限"")]
        public bool UserStatusManagement { get; set; } = false;
        
        /// <summary>
        /// 購物權限管理
        /// </summary>
        [Display(Name = ""購物權限管理"")]
        public bool ShoppingPermissionManagement { get; set; } = false;
        
        /// <summary>
        /// 訊息權限管理
        /// </summary>
        [Display(Name = ""訊息權限管理"")]
        public bool MessagePermissionManagement { get; set; } = false;
        
        /// <summary>
        /// 寵物權限管理
        /// </summary>
        [Display(Name = ""寵物權限管理"")]
        public bool Pet_Rights_Management { get; set; } = false;
        
        /// <summary>
        /// 客服權限
        /// </summary>
        [Display(Name = ""客服權限"")]
        public bool customer_service { get; set; } = false;
        
        /// <summary>
        /// 管理員角色關聯
        /// </summary>
        public virtual ICollection<ManagerRole> ManagerRoles { get; set; } = new List<ManagerRole>();
    }

    /// <summary>
    /// 管理員角色分配表 - 對應 SSMS 中的 ManagerRole 表
    /// </summary>
    [Table(""ManagerRole"")]
    public class ManagerRole
    {
        /// <summary>
        /// 角色分配唯一識別碼
        /// </summary>
        [Key]
        public int Id { get; set; }
        
        /// <summary>
        /// 管理員ID
        /// </summary>
        [Required]
        [Display(Name = ""管理員ID"")]
        public int Manager_Id { get; set; }
        
        /// <summary>
        /// 角色ID
        /// </summary>
        [Required]
        [Display(Name = ""角色ID"")]
        public int ManagerRole_Id { get; set; }
        
        /// <summary>
        /// 管理員資料關聯
        /// </summary>
        [ForeignKey(""Manager_Id"")]
        public virtual ManagerData Manager { get; set; } = null!;
        
        /// <summary>
        /// 角色權限關聯
        /// </summary>
        [ForeignKey(""ManagerRole_Id"")]
        public virtual ManagerRolePermission ManagerRolePermission { get; set; } = null!;
    }

    /// <summary>
    /// 用戶錢包表 - 對應 SSMS 中的 User_Wallet 表
    /// </summary>
    [Table(""User_Wallet"")]
    public class UserWallet
    {
        /// <summary>
        /// 用戶唯一識別碼
        /// </summary>
        [Key]
        public int User_Id { get; set; }
        
        /// <summary>
        /// 用戶點數餘額
        /// </summary>
        [Display(Name = ""用戶點數餘額"")]
        public int User_Point { get; set; } = 0;
        
        /// <summary>
        /// 用戶資料關聯
        /// </summary>
        [ForeignKey(""User_Id"")]
        public virtual User User { get; set; } = null!;
        
        /// <summary>
        /// 錢包歷史記錄關聯
        /// </summary>
        public virtual ICollection<WalletHistory> WalletHistories { get; set; } = new List<WalletHistory>();
    }

    /// <summary>
    /// 錢包歷史記錄表 - 對應 SSMS 中的 WalletHistory 表
    /// </summary>
    [Table(""WalletHistory"")]
    public class WalletHistory
    {
        /// <summary>
        /// 記錄唯一識別碼
        /// </summary>
        [Key]
        public int LogID { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required]
        [Display(Name = ""用戶ID"")]
        public int UserID { get; set; }
        
        /// <summary>
        /// 異動類型
        /// </summary>
        [Required(ErrorMessage = ""異動類型不能為空"")]
        [StringLength(20, ErrorMessage = ""異動類型長度不能超過20個字符"")]
        [Display(Name = ""異動類型"")]
        public string ChangeType { get; set; } = string.Empty;
        
        /// <summary>
        /// 點數變動量
        /// </summary>
        [Display(Name = ""點數變動量"")]
        public int PointsChanged { get; set; }
        
        /// <summary>
        /// 相關物品代碼
        /// </summary>
        [StringLength(50, ErrorMessage = ""相關物品代碼長度不能超過50個字符"")]
        [Display(Name = ""相關物品代碼"")]
        public string? ItemCode { get; set; }
        
        /// <summary>
        /// 異動描述
        /// </summary>
        [StringLength(200, ErrorMessage = ""異動描述長度不能超過200個字符"")]
        [Display(Name = ""異動描述"")]
        public string? Description { get; set; }
        
        /// <summary>
        /// 異動時間
        /// </summary>
        [Display(Name = ""異動時間"")]
        public DateTime ChangeTime { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 用戶資料關聯
        /// </summary>
        [ForeignKey(""UserID"")]
        public virtual User User { get; set; } = null!;
    }

    /// <summary>
    /// 用戶表 - 對應 SSMS 中的 Users 表
    /// </summary>
    [Table(""Users"")]
    public class User
    {
        /// <summary>
        /// 用戶唯一識別碼
        /// </summary>
        [Key]
        public int User_Id { get; set; }
        
        /// <summary>
        /// 用戶名稱
        /// </summary>
        [Required(ErrorMessage = ""用戶名稱不能為空"")]
        [StringLength(50, ErrorMessage = ""用戶名稱長度不能超過50個字符"")]
        [Display(Name = ""用戶名稱"")]
        public string User_name { get; set; } = string.Empty;
        
        /// <summary>
        /// 用戶帳號
        /// </summary>
        [Required(ErrorMessage = ""用戶帳號不能為空"")]
        [StringLength(50, ErrorMessage = ""用戶帳號長度不能超過50個字符"")]
        [Display(Name = ""用戶帳號"")]
        public string User_Account { get; set; } = string.Empty;
        
        /// <summary>
        /// 用戶密碼
        /// </summary>
        [Required(ErrorMessage = ""用戶密碼不能為空"")]
        [StringLength(100, ErrorMessage = ""用戶密碼長度不能超過100個字符"")]
        [Display(Name = ""用戶密碼"")]
        public string User_Password { get; set; } = string.Empty;
        
        /// <summary>
        /// 用戶電子郵件
        /// </summary>
        [Required(ErrorMessage = ""用戶電子郵件不能為空"")]
        [StringLength(100, ErrorMessage = ""用戶電子郵件長度不能超過100個字符"")]
        [EmailAddress(ErrorMessage = ""用戶電子郵件格式不正確"")]
        [Display(Name = ""用戶電子郵件"")]
        public string User_Email { get; set; } = string.Empty;
        
        /// <summary>
        /// 電子郵件確認狀態
        /// </summary>
        [Display(Name = ""電子郵件確認狀態"")]
        public bool User_EmailConfirmed { get; set; } = false;
        
        /// <summary>
        /// 登入失敗次數
        /// </summary>
        [Display(Name = ""登入失敗次數"")]
        public int User_AccessFailedCount { get; set; } = 0;
        
        /// <summary>
        /// 帳號鎖定啟用狀態
        /// </summary>
        [Display(Name = ""帳號鎖定啟用狀態"")]
        public bool User_LockoutEnabled { get; set; } = true;
        
        /// <summary>
        /// 帳號鎖定結束時間
        /// </summary>
        [Display(Name = ""帳號鎖定結束時間"")]
        public DateTime? User_LockoutEnd { get; set; }
        
        /// <summary>
        /// 用戶註冊日期
        /// </summary>
        [Display(Name = ""用戶註冊日期"")]
        public DateTime User_registration_date { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 用戶錢包關聯
        /// </summary>
        public virtual UserWallet? UserWallet { get; set; }
        
        /// <summary>
        /// 錢包歷史記錄關聯
        /// </summary>
        public virtual ICollection<WalletHistory> WalletHistories { get; set; } = new List<WalletHistory>();
        
        /// <summary>
        /// 寵物關聯
        /// </summary>
        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
        
        /// <summary>
        /// 小遊戲記錄關聯
        /// </summary>
        public virtual ICollection<MiniGame> MiniGames { get; set; } = new List<MiniGame>();
        
        /// <summary>
        /// 簽到統計關聯
        /// </summary>
        public virtual ICollection<UserSignInStats> UserSignInStats { get; set; } = new List<UserSignInStats>();
        
        /// <summary>
        /// 優惠券關聯
        /// </summary>
        public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
        
        /// <summary>
        /// 電子禮券關聯
        /// </summary>
        public virtual ICollection<EVoucher> EVouchers { get; set; } = new List<EVoucher>();
    }
}
