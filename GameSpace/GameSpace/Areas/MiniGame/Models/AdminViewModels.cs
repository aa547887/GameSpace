using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    // 分頁模型
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // 用戶管理相關模型
    public class AdminUserIndexViewModel
    {
        public PagedResult<Users> Users { get; set; } = new PagedResult<Users>();
        public List<Users> UserList { get; set; } = new List<Users>();
    }

    public class AdminUserCreateViewModel
    {
        [Required(ErrorMessage = "用戶名稱是必填的")]
        [StringLength(50, ErrorMessage = "用戶名稱不能超過50個字符")]
        public string User_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "用戶帳號是必填的")]
        [StringLength(50, ErrorMessage = "用戶帳號不能超過50個字符")]
        public string User_account { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼是必填的")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在6-100個字符之間")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "確認密碼是必填的")]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不匹配")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子郵件是必填的")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        public string User_email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "請輸入有效的電話號碼")]
        public string? User_phone { get; set; }

        public DateTime? User_birthday { get; set; }
        public string? User_gender { get; set; }
        public string? User_address { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AdminUserEditViewModel
    {
        public int User_Id { get; set; }

        [Required(ErrorMessage = "用戶名稱是必填的")]
        [StringLength(50, ErrorMessage = "用戶名稱不能超過50個字符")]
        public string User_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "用戶帳號是必填的")]
        [StringLength(50, ErrorMessage = "用戶帳號不能超過50個字符")]
        public string User_account { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子郵件是必填的")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        public string User_email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "請輸入有效的電話號碼")]
        public string? User_phone { get; set; }

        public DateTime? User_birthday { get; set; }
        public string? User_gender { get; set; }
        public string? User_address { get; set; }
        public bool IsActive { get; set; } = true;
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }

    // 點數管理相關模型
    public class AdminWalletIndexViewModel
    {
        public PagedResult<Wallet> Wallets { get; set; } = new PagedResult<Wallet>();
        public List<Wallet> WalletList { get; set; } = new List<Wallet>();
    }

    public class AdminWalletTransactionViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int CurrentPoints { get; set; }
        public int TransactionAmount { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "add", "subtract"
        public string Description { get; set; } = string.Empty;
    }

    // 簽到管理相關模型
    public class AdminSignInIndexViewModel
    {
        public PagedResult<SignIn> SignIns { get; set; } = new PagedResult<SignIn>();
        public List<SignIn> SignInList { get; set; } = new List<SignIn>();
    }

    public class AdminSignInCreateViewModel
    {
        [Required(ErrorMessage = "簽到名稱是必填的")]
        [StringLength(100, ErrorMessage = "簽到名稱不能超過100個字符")]
        public string SignInName { get; set; } = string.Empty;

        [Required(ErrorMessage = "簽到描述是必填的")]
        [StringLength(500, ErrorMessage = "簽到描述不能超過500個字符")]
        public string SignInDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "獎勵點數是必填的")]
        [Range(1, 10000, ErrorMessage = "獎勵點數必須在1-10000之間")]
        public int RewardPoints { get; set; }

        [Required(ErrorMessage = "開始日期是必填的")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "結束日期是必填的")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(30);

        public bool IsActive { get; set; } = true;
    }

    // 寵物管理相關模型
    public class AdminPetIndexViewModel
    {
        public PagedResult<Pet> Pets { get; set; } = new PagedResult<Pet>();
        public List<Pet> PetList { get; set; } = new List<Pet>();
    }

    public class AdminPetCreateViewModel
    {
        [Required(ErrorMessage = "寵物名稱是必填的")]
        [StringLength(50, ErrorMessage = "寵物名稱不能超過50個字符")]
        public string PetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "寵物描述是必填的")]
        [StringLength(500, ErrorMessage = "寵物描述不能超過500個字符")]
        public string PetDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "寵物類型是必填的")]
        [StringLength(50, ErrorMessage = "寵物類型不能超過50個字符")]
        public string PetType { get; set; } = string.Empty;

        [Required(ErrorMessage = "稀有度是必填的")]
        [StringLength(20, ErrorMessage = "稀有度不能超過20個字符")]
        public string Rarity { get; set; } = string.Empty;

        [Required(ErrorMessage = "獲得機率是必填的")]
        [Range(0.01, 100, ErrorMessage = "獲得機率必須在0.01-100之間")]
        public decimal DropRate { get; set; }

        public string? PetImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // 遊戲管理相關模型
    public class AdminMiniGameIndexViewModel
    {
        public PagedResult<MiniGame> MiniGames { get; set; } = new PagedResult<MiniGame>();
        public List<MiniGame> MiniGameList { get; set; } = new List<MiniGame>();
    }

    public class AdminMiniGameCreateViewModel
    {
        [Required(ErrorMessage = "遊戲名稱是必填的")]
        [StringLength(100, ErrorMessage = "遊戲名稱不能超過100個字符")]
        public string GameName { get; set; } = string.Empty;

        [Required(ErrorMessage = "遊戲描述是必填的")]
        [StringLength(1000, ErrorMessage = "遊戲描述不能超過1000個字符")]
        public string GameDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "遊戲類型是必填的")]
        [StringLength(50, ErrorMessage = "遊戲類型不能超過50個字符")]
        public string GameType { get; set; } = string.Empty;

        [Required(ErrorMessage = "消耗點數是必填的")]
        [Range(0, 10000, ErrorMessage = "消耗點數必須在0-10000之間")]
        public int CostPoints { get; set; }

        [Required(ErrorMessage = "獎勵點數是必填的")]
        [Range(0, 10000, ErrorMessage = "獎勵點數必須在0-10000之間")]
        public int RewardPoints { get; set; }

        [Required(ErrorMessage = "最大遊玩次數是必填的")]
        [Range(1, 1000, ErrorMessage = "最大遊玩次數必須在1-1000之間")]
        public int MaxPlayCount { get; set; }

        public string? GameImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // 優惠券管理相關模型
    public class AdminCouponIndexViewModel
    {
        public PagedResult<Coupon> Coupons { get; set; } = new PagedResult<Coupon>();
        public List<Coupon> CouponList { get; set; } = new List<Coupon>();
    }

    public class AdminCouponCreateViewModel
    {
        [Required(ErrorMessage = "優惠券名稱是必填的")]
        [StringLength(100, ErrorMessage = "優惠券名稱不能超過100個字符")]
        public string CouponName { get; set; } = string.Empty;

        [Required(ErrorMessage = "優惠券代碼是必填的")]
        [StringLength(50, ErrorMessage = "優惠券代碼不能超過50個字符")]
        public string CouponCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "折扣類型是必填的")]
        public string DiscountType { get; set; } = string.Empty; // "percentage", "fixed"

        [Required(ErrorMessage = "折扣值是必填的")]
        [Range(0.01, 100, ErrorMessage = "折扣值必須在0.01-100之間")]
        public decimal DiscountValue { get; set; }

        [Required(ErrorMessage = "最小消費金額是必填的")]
        [Range(0, 100000, ErrorMessage = "最小消費金額必須在0-100000之間")]
        public decimal MinOrderAmount { get; set; }

        [Required(ErrorMessage = "最大折扣金額是必填的")]
        [Range(0, 100000, ErrorMessage = "最大折扣金額必須在0-100000之間")]
        public decimal MaxDiscountAmount { get; set; }

        [Required(ErrorMessage = "開始日期是必填的")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "結束日期是必填的")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(30);

        [Required(ErrorMessage = "使用次數限制是必填的")]
        [Range(1, 100000, ErrorMessage = "使用次數限制必須在1-100000之間")]
        public int UsageLimit { get; set; }

        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // 電子禮券管理相關模型
    public class AdminEVoucherIndexViewModel
    {
        public PagedResult<EVoucher> EVouchers { get; set; } = new PagedResult<EVoucher>();
        public List<EVoucher> EVoucherList { get; set; } = new List<EVoucher>();
    }

    public class AdminEVoucherCreateViewModel
    {
        [Required(ErrorMessage = "禮券類型是必填的")]
        public int EVoucherTypeID { get; set; }

        [Required(ErrorMessage = "用戶是必填的")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "禮券代碼是必填的")]
        [StringLength(50, ErrorMessage = "禮券代碼不能超過50個字符")]
        public string EVoucherCode { get; set; } = string.Empty;

        public DateTime? AcquiredTime { get; set; } = DateTime.Now;
        public DateTime? UsedTime { get; set; }
        public bool IsUsed { get; set; } = false;
    }

    public class AdminEVoucherTypeCreateViewModel
    {
        [Required(ErrorMessage = "禮券類型名稱是必填的")]
        [StringLength(100, ErrorMessage = "禮券類型名稱不能超過100個字符")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "禮券描述是必填的")]
        [StringLength(500, ErrorMessage = "禮券描述不能超過500個字符")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "面額是必填的")]
        [Range(1, 100000, ErrorMessage = "面額必須在1-100000之間")]
        public decimal ValueAmount { get; set; }

        [Required(ErrorMessage = "有效期限是必填的")]
        [Range(1, 365, ErrorMessage = "有效期限必須在1-365天之間")]
        public int ValidDays { get; set; }

        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // 管理者管理相關模型
    public class AdminManagerIndexViewModel
    {
        public PagedResult<Manager> Managers { get; set; } = new PagedResult<Manager>();
        public List<Manager> ManagerList { get; set; } = new List<Manager>();
    }

    public class AdminManagerCreateViewModel
    {
        [Required(ErrorMessage = "管理者名稱是必填的")]
        [StringLength(50, ErrorMessage = "管理者名稱不能超過50個字符")]
        public string Manager_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "管理者帳號是必填的")]
        [StringLength(50, ErrorMessage = "管理者帳號不能超過50個字符")]
        public string Manager_Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼是必填的")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在6-100個字符之間")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "確認密碼是必填的")]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不匹配")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子郵件是必填的")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        public string Manager_Email { get; set; } = string.Empty;

        public List<int> RoleIds { get; set; } = new List<int>();
        public bool IsActive { get; set; } = true;
    }

    public class AdminManagerEditViewModel
    {
        public int Manager_Id { get; set; }

        [Required(ErrorMessage = "管理者名稱是必填的")]
        [StringLength(50, ErrorMessage = "管理者名稱不能超過50個字符")]
        public string Manager_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "管理者帳號是必填的")]
        [StringLength(50, ErrorMessage = "管理者帳號不能超過50個字符")]
        public string Manager_Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "電子郵件是必填的")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
        public string Manager_Email { get; set; } = string.Empty;

        public List<int> RoleIds { get; set; } = new List<int>();
        public bool IsActive { get; set; } = true;
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }

    public class AdminManagerRoleCreateViewModel
    {
        [Required(ErrorMessage = "角色名稱是必填的")]
        [StringLength(50, ErrorMessage = "角色名稱不能超過50個字符")]
        public string role_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "角色描述是必填的")]
        [StringLength(200, ErrorMessage = "角色描述不能超過200個字符")]
        public string role_description { get; set; } = string.Empty;

        public List<int> PermissionIds { get; set; } = new List<int>();
        public bool IsActive { get; set; } = true;
    }

    // 儀表板相關模型
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalMiniGames { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TodaySignIns { get; set; }
        public int TodayGamePlays { get; set; }
        public decimal TotalPointsEarned { get; set; }
        public decimal TotalPointsSpent { get; set; }
        public List<ChartData> UserGrowthData { get; set; } = new List<ChartData>();
        public List<ChartData> GamePlayData { get; set; } = new List<ChartData>();
        public List<ChartData> PointsData { get; set; } = new List<ChartData>();
    }

    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public int Count { get; set; }
    }
}
