using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理員儀表板視圖模型
    /// </summary>
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalMiniGames { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TotalPoints { get; set; }
        public int TodaySignIns { get; set; }
        public int TodayGames { get; set; }
        public int ThisMonthSignIns { get; set; }
        public int ThisMonthGames { get; set; }
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new List<RecentActivityViewModel>();
    }

    /// <summary>
    /// 最近活動視圖模型
    /// </summary>
    public class RecentActivityViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public DateTime ActivityTime { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用戶統計視圖模型
    /// </summary>
    public class UserStatisticsViewModel
    {
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int ActiveUsersToday { get; set; }
        public int ActiveUsersThisWeek { get; set; }
        public int ConfirmedUsers { get; set; }
        public int UnconfirmedUsers { get; set; }
        public int LockedUsers { get; set; }
    }

    /// <summary>
    /// 遊戲統計視圖模型
    /// </summary>
    public class GameStatisticsViewModel
    {
        public int TotalGames { get; set; }
        public int GamesToday { get; set; }
        public int GamesThisWeek { get; set; }
        public int GamesThisMonth { get; set; }
        public List<GameTypeStatisticsViewModel> GameTypeStatistics { get; set; } = new List<GameTypeStatisticsViewModel>();
        public List<TopPlayerViewModel> TopPlayers { get; set; } = new List<TopPlayerViewModel>();
    }

    /// <summary>
    /// 遊戲類型統計視圖模型
    /// </summary>
    public class GameTypeStatisticsViewModel
    {
        public string GameType { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageScore { get; set; }
        public int MaxScore { get; set; }
    }

    /// <summary>
    /// 頂級玩家視圖模型
    /// </summary>
    public class TopPlayerViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalGames { get; set; }
        public int TotalScore { get; set; }
        public double AverageScore { get; set; }
        public int MaxScore { get; set; }
    }

    /// <summary>
    /// 錢包統計視圖模型
    /// </summary>
    public class WalletStatisticsViewModel
    {
        public int TotalWallets { get; set; }
        public int TotalPoints { get; set; }
        public double AveragePoints { get; set; }
        public int TodayTransactions { get; set; }
        public int TodayPointsAdded { get; set; }
        public int TodayPointsDeducted { get; set; }
        public int ThisWeekTransactions { get; set; }
        public int ThisWeekPointsAdded { get; set; }
        public int ThisWeekPointsDeducted { get; set; }
        public int ThisMonthTransactions { get; set; }
        public int ThisMonthPointsAdded { get; set; }
        public int ThisMonthPointsDeducted { get; set; }
        public List<TransactionTypeStatisticsViewModel> TransactionTypeStatistics { get; set; } = new List<TransactionTypeStatisticsViewModel>();
    }

    /// <summary>
    /// 交易類型統計視圖模型
    /// </summary>
    public class TransactionTypeStatisticsViewModel
    {
        public string ChangeType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int TotalPoints { get; set; }
        public double AveragePoints { get; set; }
    }

    /// <summary>
    /// 系統健康狀態視圖模型
    /// </summary>
    public class SystemHealthViewModel
    {
        public bool DatabaseConnectionHealthy { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsersToday { get; set; }
        public int TotalGames { get; set; }
        public int GamesToday { get; set; }
        public int ErrorCount { get; set; }
        public string SystemStatus { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
    }

    /// <summary>
    /// 分頁結果模型
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }

    /// <summary>
    /// 查詢模型基類
    /// </summary>
    public class BaseQueryModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public string SortBy { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// 用戶查詢模型
    /// </summary>
    public class UserQueryModel : BaseQueryModel
    {
        public string Status { get; set; } = string.Empty;
        public bool? EmailConfirmed { get; set; }
        public bool? IsLocked { get; set; }
    }

    /// <summary>
    /// 管理員查詢模型
    /// </summary>
    public class ManagerQueryModel : BaseQueryModel
    {
        public string Status { get; set; } = string.Empty;
        public int? RoleId { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? IsLocked { get; set; }
    }

    /// <summary>
    /// 錢包歷史查詢模型
    /// </summary>
    public class WalletHistoryQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public int? MinPoints { get; set; }
        public int? MaxPoints { get; set; }
    }

    /// <summary>
    /// 簽到查詢模型
    /// </summary>
    public class SignInQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public DateTime? SignDate { get; set; }
        public int? MinConsecutiveDays { get; set; }
        public int? MaxConsecutiveDays { get; set; }
    }

    /// <summary>
    /// 遊戲查詢模型
    /// </summary>
    public class GameQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public int? PetId { get; set; }
        public string GameType { get; set; } = string.Empty;
        public int? MinScore { get; set; }
        public int? MaxScore { get; set; }
    }

    /// <summary>
    /// 優惠券查詢模型
    /// </summary>
    public class CouponQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public int? CouponTypeId { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>
    /// 電子券查詢模型
    /// </summary>
    public class EVoucherQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public int? EVoucherTypeId { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>
    /// 寵物查詢模型
    /// </summary>
    public class PetQueryModel : BaseQueryModel
    {
        public int? UserId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public string SkinColor { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
    }
}
