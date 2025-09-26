using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 優化的 ViewModel 集合
    /// 確保所有 Admin 後台功能完整實作
    /// </summary>

    #region 會員錢包系統 ViewModels

    /// <summary>
    /// 會員點數查詢結果視圖模型
    /// </summary>
    public class AdminUserPointsViewModel
    {
        public List<UserWalletModel> UserWallets { get; set; } = new();
        public List<dynamic> Users { get; set; } = new();
        public UserPointsQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// 會員優惠券查詢結果視圖模型
    /// </summary>
    public class AdminUserCouponsViewModel
    {
        public List<UserCouponReadModel> UserCoupons { get; set; } = new();
        public List<dynamic> Users { get; set; } = new();
        public List<dynamic> CouponTypes { get; set; } = new();
        public CouponQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// 會員電子禮券查詢結果視圖模型
    /// </summary>
    public class AdminUserEVouchersViewModel
    {
        public List<UserEVoucherReadModel> UserEVouchers { get; set; } = new();
        public List<dynamic> Users { get; set; } = new();
        public List<dynamic> EVoucherTypes { get; set; } = new();
        public EVoucherQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// 錢包歷史查詢結果視圖模型
    /// </summary>
    public class AdminWalletHistoryViewModel
    {
        public List<WalletHistoryDetailModel> WalletHistory { get; set; } = new();
        public List<dynamic> Users { get; set; } = new();
        public WalletHistoryQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// 錢包歷史詳情視圖模型
    /// </summary>
    public class AdminWalletHistoryDetailViewModel
    {
        public dynamic User { get; set; } = new();
        public List<WalletHistoryDetailModel> WalletHistory { get; set; } = new();
        public string TransactionType { get; set; } = "all";
    }

    #endregion

    #region 會員簽到系統 ViewModels

    /// <summary>
    /// 簽到紀錄查詢結果視圖模型
    /// </summary>
    public class AdminSignInRecordsViewModel
    {
        public List<SignInRecordDetailModel> SignInRecords { get; set; } = new();
        public List<dynamic> Users { get; set; } = new();
        public SignInRecordQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// 簽到統計視圖模型
    /// </summary>
    public class AdminSignInStatsViewModel
    {
        public SignInStatisticsModel Statistics { get; set; } = new();
        public List<DailySignInCount> DailyCounts { get; set; } = new();
        public string Period { get; set; } = "today";
    }

    #endregion

    #region 寵物系統 ViewModels

    /// <summary>
    /// 寵物清單查詢結果視圖模型
    /// </summary>
    public class AdminPetListViewModel
    {
        public List<PetSettingModel> Pets { get; set; } = new();
        public List<dynamic> Users { get; set; } = new();
        public PetListQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// 寵物詳情視圖模型
    /// </summary>
    public class AdminPetDetailViewModel
    {
        public PetSettingModel Pet { get; set; } = new();
        public List<PetAppearanceChangeModel> AppearanceChanges { get; set; } = new();
        public PetSystemRulesViewModel SystemRules { get; set; } = new();
    }

    /// <summary>
    /// 寵物統計視圖模型
    /// </summary>
    public class AdminPetStatsViewModel
    {
        public int TotalPets { get; set; }
        public int ActivePets { get; set; }
        public int AverageLevel { get; set; }
        public int TotalInteractions { get; set; }
        public List<PetLevelDistribution> LevelDistribution { get; set; } = new();
        public List<PetSkinDistribution> SkinDistribution { get; set; } = new();
        public List<PetBackgroundDistribution> BackgroundDistribution { get; set; } = new();
    }

    /// <summary>
    /// 寵物等級分布
    /// </summary>
    public class PetLevelDistribution
    {
        public int Level { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// 寵物膚色分布
    /// </summary>
    public class PetSkinDistribution
    {
        public string SkinColor { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// 寵物背景分布
    /// </summary>
    public class PetBackgroundDistribution
    {
        public string BackgroundColor { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    #endregion

    #region 小遊戲系統 ViewModels

    /// <summary>
    /// 遊戲紀錄查詢結果視圖模型
    /// </summary>
    public class AdminGameRecordsViewModel
    {
        public List<GameRecordDetailModel> GameRecords { get; set; } = new();
        public List<dynamic> Users { get; set; } = new();
        public GameRecordQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// 遊戲詳情視圖模型
    /// </summary>
    public class AdminGameDetailViewModel
    {
        public GameRecordDetailModel GameRecord { get; set; } = new();
        public dynamic User { get; set; } = new();
        public dynamic Pet { get; set; } = new();
        public List<GameRecordDetailModel> RelatedGames { get; set; } = new();
    }

    /// <summary>
    /// 遊戲統計視圖模型
    /// </summary>
    public class AdminGameStatsViewModel
    {
        public GameStatisticsModel Statistics { get; set; } = new();
        public List<DailyGameCount> DailyCounts { get; set; } = new();
        public List<GameResultDistribution> ResultDistribution { get; set; } = new();
        public List<GameLevelDistribution> LevelDistribution { get; set; } = new();
        public string Period { get; set; } = "today";
    }

    /// <summary>
    /// 遊戲結果分布
    /// </summary>
    public class GameResultDistribution
    {
        public string Result { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// 遊戲等級分布
    /// </summary>
    public class GameLevelDistribution
    {
        public int Level { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    #endregion

    #region 通用 ViewModels

    /// <summary>
    /// 管理員儀表板視圖模型
    /// </summary>
    public class AdminDashboardViewModel
    {
        public StatisticsOverviewModel Overview { get; set; } = new();
        public List<RecentActivity> RecentActivities { get; set; } = new();
        public List<SystemAlert> SystemAlerts { get; set; } = new();
        public List<QuickStat> QuickStats { get; set; } = new();
    }

    /// <summary>
    /// 最近活動
    /// </summary>
    public class RecentActivity
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// 系統警告
    /// </summary>
    public class SystemAlert
    {
        public string Type { get; set; } = string.Empty; // Warning, Error, Info
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
    }

    /// <summary>
    /// 快速統計
    /// </summary>
    public class QuickStat
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Trend { get; set; } = string.Empty; // up, down, stable
        public double TrendValue { get; set; }
    }

    /// <summary>
    /// 搜尋結果視圖模型
    /// </summary>
    public class SearchResultViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public List<SearchResultItem> Users { get; set; } = new();
        public List<SearchResultItem> Pets { get; set; } = new();
        public List<SearchResultItem> Games { get; set; } = new();
        public List<SearchResultItem> Coupons { get; set; } = new();
        public int TotalResults { get; set; }
    }

    /// <summary>
    /// 搜尋結果項目
    /// </summary>
    public class SearchResultItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region 現有模型擴展

    /// <summary>
    /// 用戶錢包模型
    /// </summary>
    public class UserWalletModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int UserPoint { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 錢包查詢模型
    /// </summary>
    public class WalletQueryModel
    {
        public int? UserId { get; set; }
        public string? SearchTerm { get; set; }
        public int? MinPoints { get; set; }
        public int? MaxPoints { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 電子禮券查詢模型
    /// </summary>
    public class EVoucherQueryModel
    {
        public int? UserId { get; set; }
        public int? EVoucherTypeId { get; set; }
        public string? SearchTerm { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    #endregion
}
