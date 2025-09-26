using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminController : MiniGameBaseController
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : base(context)
        {
            _adminService = adminService;
        }

        // MiniGame Admin 首頁儀表板
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await GetDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入儀表板時發生錯誤：{ex.Message}";
                return View(new AdminDashboardViewModel());
            }
        }

        // 獲取儀表板資料
        private async Task<AdminDashboardViewModel> GetDashboardDataAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            // 統計資料
            var totalUsers = await _context.Users.CountAsync();
            var totalPets = await _context.Pets.CountAsync();
            var totalPoints = await _context.UserWallets.SumAsync(w => w.UserPoint);
            var totalCoupons = await _context.Coupons.CountAsync();
            var totalEVouchers = await _context.Evouchers.CountAsync();

            // 今日簽到統計
            var todaySignIns = await _context.UserSignInStats
                .Where(s => s.SignTime.Date == today)
                .CountAsync();

            // 本月簽到統計
            var thisMonthSignIns = await _context.UserSignInStats
                .Where(s => s.SignTime >= thisMonth)
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
                .Take(10)
                .Select(s => new RecentSignInModel
                {
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    SignTime = s.SignTime,
                    PointsGained = s.PointsGained,
                    ExpGained = s.ExpGained
                })
                .ToListAsync();

            // 最近遊戲記錄
            var recentGameRecords = await _context.MiniGames
                .Include(m => m.User)
                .OrderByDescending(m => m.StartTime)
                .Take(10)
                .Select(m => new RecentGameRecordModel
                {
                    PlayId = m.PlayId,
                    UserId = m.UserId,
                    UserName = m.User.UserName,
                    Level = m.Level,
                    Result = m.Result,
                    PointsGained = m.PointsGained,
                    ExpGained = m.ExpGained,
                    StartTime = m.StartTime
                })
                .ToListAsync();

            // 最近錢包交易
            var recentWalletTransactions = await _context.WalletHistories
                .Include(w => w.User)
                .OrderByDescending(w => w.ChangeTime)
                .Take(10)
                .Select(w => new RecentWalletTransactionModel
                {
                    UserId = w.UserId,
                    UserName = w.User.UserName,
                    ChangeType = w.ChangeType,
                    PointsChanged = w.PointsChanged,
                    Description = w.Description,
                    ChangeTime = w.ChangeTime
                })
                .ToListAsync();

            // 系統統計
            var systemStats = new SystemStatsModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalPets = totalPets,
                TotalPoints = totalPoints,
                TotalCoupons = totalCoupons,
                TotalEVouchers = totalEVouchers,
                TodaySignIns = todaySignIns,
                ThisMonthSignIns = thisMonthSignIns
            };

            return new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalPets = totalPets,
                TotalPoints = totalPoints,
                TotalCoupons = totalCoupons,
                TotalEVouchers = totalEVouchers,
                RecentSignIns = recentSignIns,
                RecentGameRecords = recentGameRecords,
                RecentWalletTransactions = recentWalletTransactions,
                SystemStats = systemStats
            };
        }

        // 系統統計資料 API
        [HttpGet]
        public async Task<IActionResult> GetSystemStats()
        {
            try
            {
                var stats = await GetSystemStatsAsync();
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取系統統計資料
        private async Task<SystemStatsModel> GetSystemStatsAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            return new SystemStatsModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.UserSignInStats
                    .Where(s => s.SignTime >= today.AddDays(-7))
                    .Select(s => s.UserId)
                    .Distinct()
                    .CountAsync(),
                TotalPets = await _context.Pets.CountAsync(),
                TotalPoints = await _context.UserWallets.SumAsync(w => w.UserPoint),
                TotalCoupons = await _context.Coupons.CountAsync(),
                TotalEVouchers = await _context.Evouchers.CountAsync(),
                TodaySignIns = await _context.UserSignInStats
                    .Where(s => s.SignTime.Date == today)
                    .CountAsync(),
                ThisMonthSignIns = await _context.UserSignInStats
                    .Where(s => s.SignTime >= thisMonth)
                    .CountAsync()
            };
        }

        // 快速查詢用戶
        [HttpGet]
        public async Task<IActionResult> QuickSearchUser(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return Json(new { success = true, users = new List<object>() });

                var users = await _context.Users
                    .Where(u => u.UserName.Contains(query) || u.UserAccount.Contains(query))
                    .Take(10)
                    .Select(u => new
                    {
                        id = u.UserId,
                        name = u.UserName,
                        account = u.UserAccount,
                        email = u.UserEmailConfirmed ? "已驗證" : "未驗證"
                    })
                    .ToListAsync();

                return Json(new { success = true, users = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取用戶詳細資訊
        [HttpGet]
        public async Task<IActionResult> GetUserDetails(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserWallet)
                    .Include(u => u.Pets)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return Json(new { success = false, message = "找不到指定的用戶" });

                var userDetails = new
                {
                    userId = user.UserId,
                    userName = user.UserName,
                    userAccount = user.UserAccount,
                    emailConfirmed = user.UserEmailConfirmed,
                    phoneConfirmed = user.UserPhoneNumberConfirmed,
                    points = user.UserWallet?.UserPoint ?? 0,
                    petCount = user.Pets.Count(),
                    lastSignIn = await _context.UserSignInStats
                        .Where(s => s.UserId == userId)
                        .OrderByDescending(s => s.SignTime)
                        .Select(s => s.SignTime)
                        .FirstOrDefaultAsync()
                };

                return Json(new { success = true, data = userDetails });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取今日統計資料
        [HttpGet]
        public async Task<IActionResult> GetTodayStats()
        {
            try
            {
                var today = DateTime.Today;
                var stats = new
                {
                    signIns = await _context.UserSignInStats
                        .Where(s => s.SignTime.Date == today)
                        .CountAsync(),
                    gamesPlayed = await _context.MiniGames
                        .Where(m => m.StartTime.Date == today)
                        .CountAsync(),
                    pointsEarned = await _context.WalletHistories
                        .Where(w => w.ChangeTime.Date == today && w.PointsChanged > 0)
                        .SumAsync(w => w.PointsChanged),
                    newUsers = await _context.Users
                        .Where(u => u.UserId > 0) // 假設有創建時間欄位
                        .CountAsync()
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取圖表資料
        [HttpGet]
        public async Task<IActionResult> GetChartData(string type, int days = 7)
        {
            try
            {
                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-days);

                switch (type.ToLower())
                {
                    case "signins":
                        var signInData = await _context.UserSignInStats
                            .Where(s => s.SignTime >= startDate && s.SignTime <= endDate)
                            .GroupBy(s => s.SignTime.Date)
                            .Select(g => new { date = g.Key, count = g.Count() })
                            .OrderBy(x => x.date)
                            .ToListAsync();
                        return Json(new { success = true, data = signInData });

                    case "games":
                        var gameData = await _context.MiniGames
                            .Where(m => m.StartTime >= startDate && m.StartTime <= endDate)
                            .GroupBy(m => m.StartTime.Date)
                            .Select(g => new { date = g.Key, count = g.Count() })
                            .OrderBy(x => x.date)
                            .ToListAsync();
                        return Json(new { success = true, data = gameData });

                    case "points":
                        var pointsData = await _context.WalletHistories
                            .Where(w => w.ChangeTime >= startDate && w.ChangeTime <= endDate && w.PointsChanged > 0)
                            .GroupBy(w => w.ChangeTime.Date)
                            .Select(g => new { date = g.Key, points = g.Sum(x => x.PointsChanged) })
                            .OrderBy(x => x.date)
                            .ToListAsync();
                        return Json(new { success = true, data = pointsData });

                    default:
                        return Json(new { success = false, message = "不支援的圖表類型" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public List<RecentSignInModel> RecentSignIns { get; set; } = new();
        public List<RecentGameRecordModel> RecentGameRecords { get; set; } = new();
        public List<RecentWalletTransactionModel> RecentWalletTransactions { get; set; } = new();
        public SystemStatsModel SystemStats { get; set; } = new();
    }

    public class RecentSignInModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SignTime { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
    }

    public class RecentGameRecordModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Result { get; set; } = string.Empty;
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class RecentWalletTransactionModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public int PointsChanged { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ChangeTime { get; set; }
    }

    public class SystemStatsModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TodaySignIns { get; set; }
        public int ThisMonthSignIns { get; set; }
    }
}
