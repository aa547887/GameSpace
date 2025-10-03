using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly GameSpacedatabaseContext _context;

        public DashboardService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // 總覽統計
        public async Task<DashboardOverview> GetDashboardOverviewAsync()
        {
            var today = DateTime.UtcNow.Date;

            var overview = new DashboardOverview
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.User_Status == "Active"),
                TotalGames = 0, // 需要 Game 表
                TodayGames = 0, // 需要 GameHistory 表
                TotalRevenue = 0, // 需要收入記錄
                TodayRevenue = 0, // 需要收入記錄
                OnlineUsers = await _context.Users.CountAsync(u => u.User_Status == "Active")
            };

            return overview;
        }

        public async Task<UserStatistics> GetUserStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddMonths(-1);

            var stats = new UserStatistics
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(u => u.User_Status == "Active"),
                NewUsersToday = await _context.Users.CountAsync(u => u.User_CreatedAt >= today),
                NewUsersThisWeek = await _context.Users.CountAsync(u => u.User_CreatedAt >= weekAgo),
                NewUsersThisMonth = await _context.Users.CountAsync(u => u.User_CreatedAt >= monthAgo),
                AverageSessionTime = 0 // 需要 Session 記錄表
            };

            return stats;
        }

        public async Task<GameStatistics> GetGameStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddMonths(-1);

            // 簡化版：使用簽到作為遊戲活動指標
            var stats = new GameStatistics
            {
                TotalGamesPlayed = await _context.UserSignInStats.CountAsync(),
                GamesToday = await _context.UserSignInStats.CountAsync(s => s.SignInTime >= today),
                GamesThisWeek = await _context.UserSignInStats.CountAsync(s => s.SignInTime >= weekAgo),
                GamesThisMonth = await _context.UserSignInStats.CountAsync(s => s.SignInTime >= monthAgo),
                AverageGameDuration = 0,
                MostPopularGame = "寵物養成"
            };

            return stats;
        }

        public async Task<RevenueStatistics> GetRevenueStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddMonths(-1);

            // 使用點數消耗作為收入指標
            var allHistory = await _context.WalletHistories
                .Where(h => h.PointsChanged < 0)
                .ToListAsync();

            var stats = new RevenueStatistics
            {
                TotalRevenue = Math.Abs(allHistory.Sum(h => h.PointsChanged)),
                RevenueToday = Math.Abs(allHistory.Where(h => h.ChangeTime >= today).Sum(h => h.PointsChanged)),
                RevenueThisWeek = Math.Abs(allHistory.Where(h => h.ChangeTime >= weekAgo).Sum(h => h.PointsChanged)),
                RevenueThisMonth = Math.Abs(allHistory.Where(h => h.ChangeTime >= monthAgo).Sum(h => h.PointsChanged)),
                AverageRevenuePerUser = 0
            };

            var userCount = await _context.Users.CountAsync();
            if (userCount > 0)
            {
                stats.AverageRevenuePerUser = stats.TotalRevenue / userCount;
            }

            return stats;
        }

        // 趨勢數據
        public async Task<Dictionary<string, int>> GetUserGrowthTrendAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var users = await _context.Users
                .Where(u => u.User_CreatedAt >= startDate)
                .GroupBy(u => u.User_CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            return users.ToDictionary(
                x => x.Date.ToString("yyyy-MM-dd"),
                x => x.Count
            );
        }

        public async Task<Dictionary<string, int>> GetGamePlayTrendAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var signIns = await _context.UserSignInStats
                .Where(s => s.SignInTime >= startDate)
                .GroupBy(s => s.SignInTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            return signIns.ToDictionary(
                x => x.Date.ToString("yyyy-MM-dd"),
                x => x.Count
            );
        }

        public async Task<Dictionary<string, decimal>> GetRevenueTrendAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var revenue = await _context.WalletHistories
                .Where(h => h.ChangeTime >= startDate && h.PointsChanged < 0)
                .GroupBy(h => h.ChangeTime.Date)
                .Select(g => new { Date = g.Key, Total = Math.Abs(g.Sum(h => h.PointsChanged)) })
                .ToListAsync();

            return revenue.ToDictionary(
                x => x.Date.ToString("yyyy-MM-dd"),
                x => (decimal)x.Total
            );
        }

        // 排行榜
        public async Task<IEnumerable<TopUser>> GetTopUsersAsync(int count = 10)
        {
            var topUsers = await _context.Users
                .Join(_context.UserWallets,
                    u => u.User_Id,
                    w => w.UserId,
                    (u, w) => new { User = u, Wallet = w })
                .GroupJoin(_context.Pets,
                    uw => uw.User.User_Id,
                    p => p.UserID,
                    (uw, pets) => new { uw.User, uw.Wallet, Pet = pets.FirstOrDefault() })
                .GroupJoin(_context.UserSignInStats,
                    uwp => uwp.User.User_Id,
                    s => s.UserID,
                    (uwp, signIns) => new TopUser
                    {
                        UserId = uwp.User.User_Id,
                        UserName = uwp.User.User_Account,
                        Points = uwp.Wallet.UserPoint,
                        Level = uwp.Pet != null ? uwp.Pet.Level : 1,
                        GamesPlayed = signIns.Count()
                    })
                .OrderByDescending(u => u.Points)
                .ThenByDescending(u => u.Level)
                .Take(count)
                .ToListAsync();

            return topUsers;
        }

        public async Task<IEnumerable<TopGame>> GetTopGamesAsync(int count = 10)
        {
            // 簡化版：使用寵物互動作為遊戲指標
            var topGames = new List<TopGame>
            {
                new TopGame
                {
                    GameId = 1,
                    GameName = "寵物養成",
                    PlayCount = await _context.Pets.CountAsync(),
                    AverageScore = 0
                },
                new TopGame
                {
                    GameId = 2,
                    GameName = "每日簽到",
                    PlayCount = await _context.UserSignInStats.CountAsync(),
                    AverageScore = 0
                }
            };

            return topGames.OrderByDescending(g => g.PlayCount).Take(count);
        }

        // 最近活動
        public async Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(int count = 20)
        {
            var activities = new List<RecentActivity>();

            // 最近簽到
            var recentSignIns = await _context.UserSignInStats
                .Include(s => s.Users)
                .OrderByDescending(s => s.SignInTime)
                .Take(count / 2)
                .ToListAsync();

            activities.AddRange(recentSignIns.Select(s => new RecentActivity
            {
                ActivityType = "SignIn",
                Description = $"完成簽到，獲得 {s.PointsGained} 點數",
                UserName = s.Users.User_Account,
                Timestamp = s.SignInTime
            }));

            // 最近錢包變動
            var recentWallet = await _context.WalletHistories
                .Include(h => h.User)
                .OrderByDescending(h => h.ChangeTime)
                .Take(count / 2)
                .ToListAsync();

            activities.AddRange(recentWallet.Select(h => new RecentActivity
            {
                ActivityType = h.ChangeType,
                Description = $"{h.Description} ({(h.PointsChanged > 0 ? "+" : "")}{h.PointsChanged} 點數)",
                UserName = h.User.User_Account,
                Timestamp = h.ChangeTime
            }));

            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(count);
        }

        // 警報
        public async Task<IEnumerable<SystemAlert>> GetActiveAlertsAsync()
        {
            var alerts = new List<SystemAlert>();

            // 檢查資料庫連線
            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                alerts.Add(new SystemAlert
                {
                    Level = "Critical",
                    Message = "資料庫連線失敗",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 檢查低點數使用者
            var lowPointUsers = await _context.UserWallets
                .CountAsync(w => w.User_Point < 10);
            if (lowPointUsers > 0)
            {
                alerts.Add(new SystemAlert
                {
                    Level = "Warning",
                    Message = $"有 {lowPointUsers} 位使用者點數低於 10",
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 檢查優惠券庫存
            var expiringSoon = await _context.Coupons
                .CountAsync(c => !c.IsUsed && c.ExpiryTime <= DateTime.UtcNow.AddDays(7));
            if (expiringSoon > 0)
            {
                alerts.Add(new SystemAlert
                {
                    Level = "Info",
                    Message = $"有 {expiringSoon} 張優惠券將在 7 天內到期",
                    CreatedAt = DateTime.UtcNow
                });
            }

            return alerts.OrderByDescending(a => a.Level).ThenByDescending(a => a.CreatedAt);
        }
    }
}

