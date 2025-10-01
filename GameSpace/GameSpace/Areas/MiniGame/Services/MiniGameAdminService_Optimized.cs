using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// MiniGame管理員服務 - 完整業務邏輯實現
    /// </summary>
    public class MiniGameAdminService : IMiniGameAdminService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<MiniGameAdminService> _logger;

        public MiniGameAdminService(MiniGameDbContext context, ILogger<MiniGameAdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 獲取管理員儀表板資料
        /// </summary>
        public async Task<AdminDashboardViewModel> GetDashboardDataAsync()
        {
            try
            {
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                // 基本統計
                var totalUsers = await _context.Users.CountAsync();
                var totalPets = await _context.Pets.CountAsync();
                var totalPoints = await _context.UserWallets.SumAsync(w => w.User_Point);
                var totalCoupons = await _context.Coupons.CountAsync();
                var totalEVouchers = await _context.EVouchers.CountAsync();

                // 今日統計
                var todaySignIns = await _context.UserSignInStats
                    .Where(s => s.SignTime.Date == today)
                    .CountAsync();

                var todayGames = await _context.MiniGames
                    .Where(g => g.StartTime.Date == today)
                    .CountAsync();

                // 本月統計
                var thisMonthSignIns = await _context.UserSignInStats
                    .Where(s => s.SignTime >= thisMonth)
                    .CountAsync();

                var thisMonthGames = await _context.MiniGames
                    .Where(g => g.StartTime >= thisMonth)
                    .CountAsync();

                // 活躍用戶統計（最近7天）
                var sevenDaysAgo = today.AddDays(-7);
                var activeUsers = await _context.Users
                    .Where(u => u.User_registration_date >= sevenDaysAgo)
                    .CountAsync();

                // 最近活動
                var recentSignIns = await _context.UserSignInStats
                    .Include(s => s.User)
                    .OrderByDescending(s => s.SignTime)
                    .Take(5)
                    .Select(s => new RecentActivityViewModel
                    {
                        UserName = s.User.User_name,
                        ActivityType = ""簽到"",
                        ActivityTime = s.SignTime,
                        Description = $""連續簽到 {s.ConsecutiveDays} 天""
                    })
                    .ToListAsync();

                var recentGames = await _context.MiniGames
                    .Include(g => g.User)
                    .OrderByDescending(g => g.StartTime)
                    .Take(5)
                    .Select(g => new RecentActivityViewModel
                    {
                        UserName = g.User.User_name,
                        ActivityType = ""遊戲"",
                        ActivityTime = g.StartTime,
                        Description = $""遊戲類型: {g.GameType}, 得分: {g.Score}""
                    })
                    .ToListAsync();

                var recentActivities = recentSignIns.Concat(recentGames)
                    .OrderByDescending(a => a.ActivityTime)
                    .Take(10)
                    .ToList();

                return new AdminDashboardViewModel
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    TotalPets = totalPets,
                    TotalMiniGames = await _context.MiniGames.CountAsync(),
                    TotalCoupons = totalCoupons,
                    TotalEVouchers = totalEVouchers,
                    TotalPoints = totalPoints,
                    TodaySignIns = todaySignIns,
                    TodayGames = todayGames,
                    ThisMonthSignIns = thisMonthSignIns,
                    ThisMonthGames = thisMonthGames,
                    RecentActivities = recentActivities
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""獲取儀表板資料時發生錯誤"");
                throw;
            }
        }

        /// <summary>
        /// 獲取用戶統計資料
        /// </summary>
        public async Task<UserStatisticsViewModel> GetUserStatisticsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var thisWeek = today.AddDays(-7);
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                // 用戶註冊統計
                var totalUsers = await _context.Users.CountAsync();
                var newUsersToday = await _context.Users
                    .Where(u => u.User_registration_date.Date == today)
                    .CountAsync();
                var newUsersThisWeek = await _context.Users
                    .Where(u => u.User_registration_date >= thisWeek)
                    .CountAsync();
                var newUsersThisMonth = await _context.Users
                    .Where(u => u.User_registration_date >= thisMonth)
                    .CountAsync();

                // 活躍用戶統計
                var activeUsersToday = await _context.UserSignInStats
                    .Where(s => s.SignTime.Date == today)
                    .Select(s => s.User_Id)
                    .Distinct()
                    .CountAsync();

                var activeUsersThisWeek = await _context.UserSignInStats
                    .Where(s => s.SignTime >= thisWeek)
                    .Select(s => s.User_Id)
                    .Distinct()
                    .CountAsync();

                // 用戶狀態統計
                var confirmedUsers = await _context.Users
                    .Where(u => u.User_EmailConfirmed)
                    .CountAsync();
                var unconfirmedUsers = totalUsers - confirmedUsers;

                var lockedUsers = await _context.Users
                    .Where(u => u.User_LockoutEnabled && 
                              u.User_LockoutEnd.HasValue && 
                              u.User_LockoutEnd > DateTime.Now)
                    .CountAsync();

                return new UserStatisticsViewModel
                {
                    TotalUsers = totalUsers,
                    NewUsersToday = newUsersToday,
                    NewUsersThisWeek = newUsersThisWeek,
                    NewUsersThisMonth = newUsersThisMonth,
                    ActiveUsersToday = activeUsersToday,
                    ActiveUsersThisWeek = activeUsersThisWeek,
                    ConfirmedUsers = confirmedUsers,
                    UnconfirmedUsers = unconfirmedUsers,
                    LockedUsers = lockedUsers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""獲取用戶統計資料時發生錯誤"");
                throw;
            }
        }

        /// <summary>
        /// 獲取遊戲統計資料
        /// </summary>
        public async Task<GameStatisticsViewModel> GetGameStatisticsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var thisWeek = today.AddDays(-7);
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                // 遊戲總數統計
                var totalGames = await _context.MiniGames.CountAsync();
                var gamesToday = await _context.MiniGames
                    .Where(g => g.StartTime.Date == today)
                    .CountAsync();
                var gamesThisWeek = await _context.MiniGames
                    .Where(g => g.StartTime >= thisWeek)
                    .CountAsync();
                var gamesThisMonth = await _context.MiniGames
                    .Where(g => g.StartTime >= thisMonth)
                    .CountAsync();

                // 遊戲類型統計
                var gameTypeStats = await _context.MiniGames
                    .GroupBy(g => g.GameType)
                    .Select(g => new GameTypeStatisticsViewModel
                    {
                        GameType = g.Key,
                        Count = g.Count(),
                        AverageScore = g.Average(x => x.Score),
                        MaxScore = g.Max(x => x.Score)
                    })
                    .ToListAsync();

                // 排行榜統計
                var topPlayers = await _context.MiniGames
                    .Include(g => g.User)
                    .GroupBy(g => g.User_Id)
                    .Select(g => new TopPlayerViewModel
                    {
                        UserId = g.Key,
                        UserName = g.First().User.User_name,
                        TotalGames = g.Count(),
                        TotalScore = g.Sum(x => x.Score),
                        AverageScore = g.Average(x => x.Score),
                        MaxScore = g.Max(x => x.Score)
                    })
                    .OrderByDescending(p => p.TotalScore)
                    .Take(10)
                    .ToListAsync();

                return new GameStatisticsViewModel
                {
                    TotalGames = totalGames,
                    GamesToday = gamesToday,
                    GamesThisWeek = gamesThisWeek,
                    GamesThisMonth = gamesThisMonth,
                    GameTypeStatistics = gameTypeStats,
                    TopPlayers = topPlayers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""獲取遊戲統計資料時發生錯誤"");
                throw;
            }
        }

        /// <summary>
        /// 獲取錢包統計資料
        /// </summary>
        public async Task<WalletStatisticsViewModel> GetWalletStatisticsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var thisWeek = today.AddDays(-7);
                var thisMonth = new DateTime(today.Year, today.Month, 1);

                // 錢包總數統計
                var totalWallets = await _context.UserWallets.CountAsync();
                var totalPoints = await _context.UserWallets.SumAsync(w => w.User_Point);
                var averagePoints = await _context.UserWallets.AverageAsync(w => w.User_Point);

                // 今日交易統計
                var todayTransactions = await _context.WalletHistories
                    .Where(w => w.ChangeTime.Date == today)
                    .CountAsync();
                var todayPointsAdded = await _context.WalletHistories
                    .Where(w => w.ChangeTime.Date == today && w.PointsChanged > 0)
                    .SumAsync(w => w.PointsChanged);
                var todayPointsDeducted = await _context.WalletHistories
                    .Where(w => w.ChangeTime.Date == today && w.PointsChanged < 0)
                    .SumAsync(w => Math.Abs(w.PointsChanged));

                // 本週交易統計
                var thisWeekTransactions = await _context.WalletHistories
                    .Where(w => w.ChangeTime >= thisWeek)
                    .CountAsync();
                var thisWeekPointsAdded = await _context.WalletHistories
                    .Where(w => w.ChangeTime >= thisWeek && w.PointsChanged > 0)
                    .SumAsync(w => w.PointsChanged);
                var thisWeekPointsDeducted = await _context.WalletHistories
                    .Where(w => w.ChangeTime >= thisWeek && w.PointsChanged < 0)
                    .SumAsync(w => Math.Abs(w.PointsChanged));

                // 本月交易統計
                var thisMonthTransactions = await _context.WalletHistories
                    .Where(w => w.ChangeTime >= thisMonth)
                    .CountAsync();
                var thisMonthPointsAdded = await _context.WalletHistories
                    .Where(w => w.ChangeTime >= thisMonth && w.PointsChanged > 0)
                    .SumAsync(w => w.PointsChanged);
                var thisMonthPointsDeducted = await _context.WalletHistories
                    .Where(w => w.ChangeTime >= thisMonth && w.PointsChanged < 0)
                    .SumAsync(w => Math.Abs(w.PointsChanged));

                // 交易類型統計
                var transactionTypeStats = await _context.WalletHistories
                    .GroupBy(w => w.ChangeType)
                    .Select(w => new TransactionTypeStatisticsViewModel
                    {
                        ChangeType = w.Key,
                        Count = w.Count(),
                        TotalPoints = w.Sum(x => x.PointsChanged),
                        AveragePoints = w.Average(x => x.PointsChanged)
                    })
                    .ToListAsync();

                return new WalletStatisticsViewModel
                {
                    TotalWallets = totalWallets,
                    TotalPoints = totalPoints,
                    AveragePoints = averagePoints,
                    TodayTransactions = todayTransactions,
                    TodayPointsAdded = todayPointsAdded,
                    TodayPointsDeducted = todayPointsDeducted,
                    ThisWeekTransactions = thisWeekTransactions,
                    ThisWeekPointsAdded = thisWeekPointsAdded,
                    ThisWeekPointsDeducted = thisWeekPointsDeducted,
                    ThisMonthTransactions = thisMonthTransactions,
                    ThisMonthPointsAdded = thisMonthPointsAdded,
                    ThisMonthPointsDeducted = thisMonthPointsDeducted,
                    TransactionTypeStatistics = transactionTypeStats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""獲取錢包統計資料時發生錯誤"");
                throw;
            }
        }

        /// <summary>
        /// 獲取系統健康狀態
        /// </summary>
        public async Task<SystemHealthViewModel> GetSystemHealthAsync()
        {
            try
            {
                var today = DateTime.Today;
                var thisWeek = today.AddDays(-7);

                // 資料庫連接測試
                var dbConnectionHealthy = await TestDatabaseConnectionAsync();

                // 系統負載統計
                var totalUsers = await _context.Users.CountAsync();
                var activeUsersToday = await _context.UserSignInStats
                    .Where(s => s.SignTime.Date == today)
                    .Select(s => s.User_Id)
                    .Distinct()
                    .CountAsync();

                var totalGames = await _context.MiniGames.CountAsync();
                var gamesToday = await _context.MiniGames
                    .Where(g => g.StartTime.Date == today)
                    .CountAsync();

                // 錯誤統計
                var errorCount = await _context.WalletHistories
                    .Where(w => w.ChangeType == ""ERROR"" || w.Description.Contains(""錯誤""))
                    .CountAsync();

                // 系統狀態評估
                var systemStatus = EvaluateSystemStatus(
                    dbConnectionHealthy, 
                    activeUsersToday, 
                    gamesToday, 
                    errorCount);

                return new SystemHealthViewModel
                {
                    DatabaseConnectionHealthy = dbConnectionHealthy,
                    TotalUsers = totalUsers,
                    ActiveUsersToday = activeUsersToday,
                    TotalGames = totalGames,
                    GamesToday = gamesToday,
                    ErrorCount = errorCount,
                    SystemStatus = systemStatus,
                    LastChecked = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""獲取系統健康狀態時發生錯誤"");
                throw;
            }
        }

        /// <summary>
        /// 測試資料庫連接
        /// </summary>
        private async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(""SELECT 1"");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""資料庫連接測試失敗"");
                return false;
            }
        }

        /// <summary>
        /// 評估系統狀態
        /// </summary>
        private string EvaluateSystemStatus(bool dbHealthy, int activeUsers, int gamesToday, int errorCount)
        {
            if (!dbHealthy) return ""Critical"";
            if (errorCount > 100) return ""Warning"";
            if (activeUsers > 1000 && gamesToday > 500) return ""High Load"";
            if (activeUsers > 100 && gamesToday > 50) return ""Normal"";
            return ""Low Load"";
        }
    }
}
