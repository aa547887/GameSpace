using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminHomeController : MiniGameBaseController
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminHomeController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : base(context)
        {
            _adminService = adminService;
        }

        // MiniGame Admin 首頁
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await GetMiniGameAdminDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入 MiniGame Admin 首頁時發生錯誤：{ex.Message}";
                return View(new MiniGameAdminDashboardViewModel());
            }
        }

        // 獲取儀表板統計資料
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = await GetDashboardStatisticsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取圖表資料
        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType, int days = 7)
        {
            try
            {
                var chartData = await GetChartDataAsync(chartType, days);
                return Json(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取最近活動
        [HttpGet]
        public async Task<IActionResult> GetRecentActivity()
        {
            try
            {
                var activities = await GetRecentActivitiesAsync();
                return Json(new { success = true, data = activities });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取系統狀態
        [HttpGet]
        public async Task<IActionResult> GetSystemStatus()
        {
            try
            {
                var status = await GetSystemStatusAsync();
                return Json(new { success = true, data = status });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 私有方法
        private async Task<MiniGameAdminDashboardViewModel> GetMiniGameAdminDashboardDataAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            // 基本統計
            var totalUsers = await _context.Users.CountAsync();
            var totalPets = await _context.Pets.CountAsync();
            var totalGames = await _context.MiniGames.CountAsync();
            var totalCoupons = await _context.Coupons.CountAsync();
            var totalEVouchers = await _context.EVouchers.CountAsync();

            // 今日統計
            var todaySignIns = await _context.UserSignInStats
                .Where(s => s.SignTime.Date == today)
                .CountAsync();

            var todayGames = await _context.MiniGames
                .Where(g => g.StartTime.Date == today)
                .CountAsync();

            var todayPointsEarned = await _context.WalletHistories
                .Where(w => w.ChangeTime.Date == today && w.PointsChanged > 0)
                .SumAsync(w => w.PointsChanged);

            // 本月統計
            var thisMonthSignIns = await _context.UserSignInStats
                .Where(s => s.SignTime >= thisMonth)
                .CountAsync();

            var thisMonthGames = await _context.MiniGames
                .Where(s => s.StartTime >= thisMonth)
                .CountAsync();

            // 活躍用戶（最近7天有活動）
            var activeUsers = await _context.UserSignInStats
                .Where(s => s.SignTime >= today.AddDays(-7))
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync();

            // 最近簽到記錄
            var recentSignIns = await _context.UserSignInStats
                .Include(s => s.User)
                .OrderByDescending(s => s.SignTime)
                .Take(5)
                .Select(s => new RecentActivityModel
                {
                    Id = s.SignInId,
                    Type = "SignIn",
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    Description = $"簽到獲得 {s.PointsGained} 點",
                    Timestamp = s.SignTime
                })
                .ToListAsync();

            // 最近遊戲記錄
            var recentGames = await _context.MiniGames
                .Include(g => g.User)
                .OrderByDescending(g => g.StartTime)
                .Take(5)
                .Select(g => new RecentActivityModel
                {
                    Id = g.PlayId,
                    Type = "Game",
                    UserId = g.UserId,
                    UserName = g.User.UserName,
                    Description = $"遊戲 {g.GameName} 獲得 {g.PointsGained} 點",
                    Timestamp = g.StartTime
                })
                .ToListAsync();

            // 最近錢包交易
            var recentWalletTransactions = await _context.WalletHistories
                .Include(w => w.User)
                .OrderByDescending(w => w.ChangeTime)
                .Take(5)
                .Select(w => new RecentActivityModel
                {
                    Id = w.LogId,
                    Type = "Wallet",
                    UserId = w.UserId,
                    UserName = w.User.UserName,
                    Description = $"{w.ChangeType} {w.PointsChanged} 點",
                    Timestamp = w.ChangeTime
                })
                .ToListAsync();

            // 合併最近活動
            var allRecentActivities = recentSignIns
                .Concat(recentGames)
                .Concat(recentWalletTransactions)
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();

            // 系統狀態
            var systemStatus = new SystemStatusModel
            {
                DatabaseConnection = await CheckDatabaseConnectionAsync(),
                MemoryUsage = GetMemoryUsage(),
                LastBackup = await GetLastBackupTimeAsync(),
                ErrorCount = await GetTodayErrorCountAsync()
            };

            return new MiniGameAdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalPets = totalPets,
                TotalGames = totalGames,
                TotalCoupons = totalCoupons,
                TotalEVouchers = totalEVouchers,
                TodaySignIns = todaySignIns,
                TodayGames = todayGames,
                TodayPointsEarned = todayPointsEarned,
                ThisMonthSignIns = thisMonthSignIns,
                ThisMonthGames = thisMonthGames,
                ActiveUsers = activeUsers,
                RecentActivities = allRecentActivities,
                SystemStatus = systemStatus
            };
        }

        private async Task<DashboardStatisticsModel> GetDashboardStatisticsAsync()
        {
            var today = DateTime.Today;
            var thisWeek = today.AddDays(-7);
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            return new DashboardStatisticsModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.UserSignInStats
                    .Where(s => s.SignTime >= thisWeek)
                    .Select(s => s.UserId)
                    .Distinct()
                    .CountAsync(),
                TotalPets = await _context.Pets.CountAsync(),
                TotalGames = await _context.MiniGames.CountAsync(),
                TodaySignIns = await _context.UserSignInStats
                    .Where(s => s.SignTime.Date == today)
                    .CountAsync(),
                TodayGames = await _context.MiniGames
                    .Where(g => g.StartTime.Date == today)
                    .CountAsync(),
                ThisMonthSignIns = await _context.UserSignInStats
                    .Where(s => s.SignTime >= thisMonth)
                    .CountAsync(),
                ThisMonthGames = await _context.MiniGames
                    .Where(g => g.StartTime >= thisMonth)
                    .CountAsync()
            };
        }

        private async Task<List<ChartDataModel>> GetChartDataAsync(string chartType, int days)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            switch (chartType.ToLower())
            {
                case "signins":
                    var signInData = await _context.UserSignInStats
                        .Where(s => s.SignTime >= startDate && s.SignTime <= endDate)
                        .GroupBy(s => s.SignTime.Date)
                        .Select(g => new ChartDataModel
                        {
                            Date = g.Key,
                            Value = g.Count()
                        })
                        .OrderBy(x => x.Date)
                        .ToListAsync();
                    return signInData;

                case "games":
                    var gameData = await _context.MiniGames
                        .Where(g => g.StartTime >= startDate && g.StartTime <= endDate)
                        .GroupBy(g => g.StartTime.Date)
                        .Select(g => new ChartDataModel
                        {
                            Date = g.Key,
                            Value = g.Count()
                        })
                        .OrderBy(x => x.Date)
                        .ToListAsync();
                    return gameData;

                case "points":
                    var pointsData = await _context.WalletHistories
                        .Where(w => w.ChangeTime >= startDate && w.ChangeTime <= endDate && w.PointsChanged > 0)
                        .GroupBy(w => w.ChangeTime.Date)
                        .Select(g => new ChartDataModel
                        {
                            Date = g.Key,
                            Value = g.Sum(x => x.PointsChanged)
                        })
                        .OrderBy(x => x.Date)
                        .ToListAsync();
                    return pointsData;

                default:
                    return new List<ChartDataModel>();
            }
        }

        private async Task<List<RecentActivityModel>> GetRecentActivitiesAsync()
        {
            var activities = new List<RecentActivityModel>();

            // 最近簽到
            var recentSignIns = await _context.UserSignInStats
                .Include(s => s.User)
                .OrderByDescending(s => s.SignTime)
                .Take(3)
                .Select(s => new RecentActivityModel
                {
                    Id = s.SignInId,
                    Type = "SignIn",
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    Description = $"簽到獲得 {s.PointsGained} 點",
                    Timestamp = s.SignTime
                })
                .ToListAsync();

            // 最近遊戲
            var recentGames = await _context.MiniGames
                .Include(g => g.User)
                .OrderByDescending(g => g.StartTime)
                .Take(3)
                .Select(g => new RecentActivityModel
                {
                    Id = g.PlayId,
                    Type = "Game",
                    UserId = g.UserId,
                    UserName = g.User.UserName,
                    Description = $"遊戲 {g.GameName} 獲得 {g.PointsGained} 點",
                    Timestamp = g.StartTime
                })
                .ToListAsync();

            activities.AddRange(recentSignIns);
            activities.AddRange(recentGames);

            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(5)
                .ToList();
        }

        private async Task<SystemStatusModel> GetSystemStatusAsync()
        {
            return new SystemStatusModel
            {
                DatabaseConnection = await CheckDatabaseConnectionAsync(),
                MemoryUsage = GetMemoryUsage(),
                LastBackup = await GetLastBackupTimeAsync(),
                ErrorCount = await GetTodayErrorCountAsync()
            };
        }

        private async Task<bool> CheckDatabaseConnectionAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        private string GetMemoryUsage()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var memoryMB = process.WorkingSet64 / 1024 / 1024;
            return $"{memoryMB} MB";
        }

        private async Task<DateTime?> GetLastBackupTimeAsync()
        {
            // 假設有備份記錄
            return DateTime.Now.AddDays(-1);
        }

        private async Task<int> GetTodayErrorCountAsync()
        {
            var today = DateTime.Today;
            return await _context.ErrorLogs
                .Where(e => e.Timestamp.Date == today)
                .CountAsync();
        }
    }

    // ViewModels
    public class MiniGameAdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalGames { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TodaySignIns { get; set; }
        public int TodayGames { get; set; }
        public int TodayPointsEarned { get; set; }
        public int ThisMonthSignIns { get; set; }
        public int ThisMonthGames { get; set; }
        public int ActiveUsers { get; set; }
        public List<RecentActivityModel> RecentActivities { get; set; } = new();
        public SystemStatusModel SystemStatus { get; set; } = new();
    }

    public class DashboardStatisticsModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalGames { get; set; }
        public int TodaySignIns { get; set; }
        public int TodayGames { get; set; }
        public int ThisMonthSignIns { get; set; }
        public int ThisMonthGames { get; set; }
    }

    public class ChartDataModel
    {
        public DateTime Date { get; set; }
        public int Value { get; set; }
    }

    public class RecentActivityModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class SystemStatusModel
    {
        public bool DatabaseConnection { get; set; }
        public string MemoryUsage { get; set; } = string.Empty;
        public DateTime? LastBackup { get; set; }
        public int ErrorCount { get; set; }
    }
}
