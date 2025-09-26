using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Route("MiniGame/[controller]")]
    public class AdminSignInController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSignInController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminSignInViewModel
            {
                TotalSignIns = await _context.UserSignInStats.CountAsync(),
                TodaySignIns = await _context.UserSignInStats
                    .Where(s => s.SignTime.Date == DateTime.Today)
                    .CountAsync(),
                UniqueUsersToday = await _context.UserSignInStats
                    .Where(s => s.SignTime.Date == DateTime.Today)
                    .Select(s => s.UserID)
                    .Distinct()
                    .CountAsync(),
                AveragePointsPerSignIn = await CalculateAveragePointsPerSignInAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetSignInOverview()
        {
            try
            {
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);
                var thisWeek = today.AddDays(-(int)today.DayOfWeek);
                var lastWeek = thisWeek.AddDays(-7);

                var data = new
                {
                    totalSignIns = await _context.UserSignInStats.CountAsync(),
                    todaySignIns = await _context.UserSignInStats
                        .Where(s => s.SignTime.Date == today)
                        .CountAsync(),
                    yesterdaySignIns = await _context.UserSignInStats
                        .Where(s => s.SignTime.Date == yesterday)
                        .CountAsync(),
                    thisWeekSignIns = await _context.UserSignInStats
                        .Where(s => s.SignTime.Date >= thisWeek)
                        .CountAsync(),
                    lastWeekSignIns = await _context.UserSignInStats
                        .Where(s => s.SignTime.Date >= lastWeek && s.SignTime.Date < thisWeek)
                        .CountAsync(),
                    uniqueUsersToday = await _context.UserSignInStats
                        .Where(s => s.SignTime.Date == today)
                        .Select(s => s.UserID)
                        .Distinct()
                        .CountAsync(),
                    totalPointsAwarded = await _context.UserSignInStats
                        .SumAsync(s => s.PointsGained),
                    totalExpAwarded = await _context.UserSignInStats
                        .SumAsync(s => s.ExpGained),
                    totalCouponsAwarded = await _context.UserSignInStats
                        .Where(s => s.CouponGained != "0")
                        .CountAsync(),
                    signInRecords = await GetSignInRecordsAsync()
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetSignInRecords(int page = 1, int pageSize = 50)
        {
            try
            {
                var records = await _context.UserSignInStats
                    .Include(s => s.User)
                    .OrderByDescending(s => s.SignTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new
                    {
                        logId = s.LogID,
                        userId = s.UserID,
                        userName = s.User.User_name,
                        signTime = s.SignTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        pointsGained = s.PointsGained,
                        pointsGainedTime = s.PointsGainedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        expGained = s.ExpGained,
                        expGainedTime = s.ExpGainedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        couponGained = s.CouponGained != "0" ? s.CouponGained : null,
                        couponGainedTime = s.CouponGained != "0" ? s.CouponGainedTime.ToString("yyyy-MM-dd HH:mm:ss") : null,
                        consecutiveDays = await CalculateConsecutiveDaysAsync(s.UserID, s.SignTime.Date)
                    })
                    .ToListAsync();

                return Json(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetSignInStatistics()
        {
            try
            {
                // Daily sign-in statistics for the last 30 days
                var thirtyDaysAgo = DateTime.Today.AddDays(-30);
                var dailyStats = await _context.UserSignInStats
                    .Where(s => s.SignTime.Date >= thirtyDaysAgo)
                    .GroupBy(s => s.SignTime.Date)
                    .Select(group => new
                    {
                        date = group.Key.ToString("yyyy-MM-dd"),
                        signInCount = group.Count(),
                        uniqueUsers = group.Select(s => s.UserID).Distinct().Count(),
                        totalPoints = group.Sum(s => s.PointsGained),
                        totalExp = group.Sum(s => s.ExpGained),
                        couponsAwarded = group.Count(s => s.CouponGained != "0")
                    })
                    .OrderBy(x => x.date)
                    .ToListAsync();

                // User engagement analysis
                var userEngagement = await _context.UserSignInStats
                    .GroupBy(s => s.UserID)
                    .Select(group => new
                    {
                        userId = group.Key,
                        totalSignIns = group.Count(),
                        firstSignIn = group.Min(s => s.SignTime),
                        lastSignIn = group.Max(s => s.SignTime),
                        totalPoints = group.Sum(s => s.PointsGained),
                        totalExp = group.Sum(s => s.ExpGained),
                        couponsReceived = group.Count(s => s.CouponGained != "0")
                    })
                    .ToListAsync();

                // Calculate consecutive sign-in streaks
                var streakAnalysis = userEngagement.Select(u => new
                {
                    u.userId,
                    u.totalSignIns,
                    daysSinceFirstSignIn = (DateTime.Today - u.firstSignIn.Date).Days + 1,
                    daysSinceLastSignIn = (DateTime.Today - u.lastSignIn.Date).Days,
                    signInRate = u.totalSignIns * 100.0 / Math.Max(1, (DateTime.Today - u.firstSignIn.Date).Days + 1),
                    u.totalPoints,
                    u.totalExp,
                    u.couponsReceived
                }).ToList();

                // Top users by sign-in frequency
                var topUsers = await _context.UserSignInStats
                    .Include(s => s.User)
                    .GroupBy(s => new { s.UserID, s.User.User_name })
                    .Select(group => new
                    {
                        userId = group.Key.UserID,
                        userName = group.Key.User_name,
                        totalSignIns = group.Count(),
                        totalPoints = group.Sum(s => s.PointsGained),
                        totalExp = group.Sum(s => s.ExpGained),
                        lastSignIn = group.Max(s => s.SignTime).ToString("yyyy-MM-dd"),
                        couponsReceived = group.Count(s => s.CouponGained != "0")
                    })
                    .OrderByDescending(x => x.totalSignIns)
                    .Take(10)
                    .ToListAsync();

                // Reward distribution analysis
                var rewardStats = new
                {
                    pointsDistribution = await _context.UserSignInStats
                        .GroupBy(s => s.PointsGained)
                        .Select(group => new
                        {
                            points = group.Key,
                            count = group.Count()
                        })
                        .OrderBy(x => x.points)
                        .ToListAsync(),
                    expDistribution = await _context.UserSignInStats
                        .GroupBy(s => s.ExpGained)
                        .Select(group => new
                        {
                            exp = group.Key,
                            count = group.Count()
                        })
                        .OrderBy(x => x.exp)
                        .ToListAsync()
                };

                var data = new
                {
                    dailyStats = dailyStats,
                    streakAnalysis = new
                    {
                        averageSignInRate = streakAnalysis.Average(s => s.signInRate),
                        highEngagementUsers = streakAnalysis.Where(s => s.signInRate >= 80).Count(),
                        mediumEngagementUsers = streakAnalysis.Where(s => s.signInRate >= 50 && s.signInRate < 80).Count(),
                        lowEngagementUsers = streakAnalysis.Where(s => s.signInRate < 50).Count()
                    },
                    topUsers = topUsers,
                    rewardStats = rewardStats
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetUserSignInHistory(int userId, int days = 30)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "用戶不存在" });
                }

                var startDate = DateTime.Today.AddDays(-days);
                var history = await _context.UserSignInStats
                    .Where(s => s.UserID == userId && s.SignTime.Date >= startDate)
                    .OrderByDescending(s => s.SignTime)
                    .Select(s => new
                    {
                        logId = s.LogID,
                        signTime = s.SignTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        pointsGained = s.PointsGained,
                        expGained = s.ExpGained,
                        couponGained = s.CouponGained != "0" ? s.CouponGained : null,
                        consecutiveDays = CalculateConsecutiveDaysAsync(s.UserID, s.SignTime.Date).Result
                    })
                    .ToListAsync();

                var stats = new
                {
                    totalSignIns = history.Count,
                    totalPoints = history.Sum(h => h.pointsGained),
                    totalExp = history.Sum(h => h.expGained),
                    couponsReceived = history.Count(h => h.couponGained != null),
                    signInRate = days > 0 ? history.Count * 100.0 / days : 0
                };

                var data = new
                {
                    userId = userId,
                    userName = user.User_name,
                    period = $"最近 {days} 天",
                    stats = stats,
                    history = history
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ManualSignIn(int userId, int pointsGained, int expGained, string couponCode = null)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "用戶不存在" });
                }

                // Check if user already signed in today
                var todaySignIn = await _context.UserSignInStats
                    .FirstOrDefaultAsync(s => s.UserID == userId && s.SignTime.Date == DateTime.Today);

                if (todaySignIn != null)
                {
                    return Json(new { success = false, message = "用戶今日已簽到" });
                }

                if (pointsGained < 0 || expGained < 0)
                {
                    return Json(new { success = false, message = "獎勵值不能為負數" });
                }

                var signInRecord = new UserSignInStats
                {
                    UserID = userId,
                    SignTime = DateTime.UtcNow,
                    PointsGained = pointsGained,
                    PointsGainedTime = DateTime.UtcNow,
                    ExpGained = expGained,
                    ExpGainedTime = DateTime.UtcNow,
                    CouponGained = string.IsNullOrEmpty(couponCode) ? "0" : couponCode,
                    CouponGainedTime = DateTime.UtcNow
                };

                _context.UserSignInStats.Add(signInRecord);

                // Update user wallet
                var wallet = await _context.User_Wallet.FirstOrDefaultAsync(w => w.User_Id == userId);
                if (wallet == null)
                {
                    wallet = new User_Wallet { User_Id = userId, User_Point = 0 };
                    _context.User_Wallet.Add(wallet);
                }
                wallet.User_Point += pointsGained;

                // Add wallet history
                if (pointsGained > 0)
                {
                    var walletHistory = new WalletHistory
                    {
                        UserID = userId,
                        ChangeType = "Manual",
                        PointsChanged = pointsGained,
                        Description = "管理員手動簽到",
                        ChangeTime = DateTime.UtcNow
                    };
                    _context.WalletHistory.Add(walletHistory);
                }

                // Update pet experience if user has a pet and exp > 0
                if (expGained > 0)
                {
                    var pet = await _context.Pet.FirstOrDefaultAsync(p => p.UserID == userId);
                    if (pet != null)
                    {
                        pet.Experience += expGained;
                    }
                }

                // Add coupon if specified
                if (!string.IsNullOrEmpty(couponCode) && couponCode != "0")
                {
                    // This would require coupon type lookup and creation
                    // For now, just record in the sign-in stats
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "手動簽到成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DeleteSignInRecord(int logId)
        {
            try
            {
                var record = await _context.UserSignInStats.FindAsync(logId);
                if (record == null)
                {
                    return Json(new { success = false, message = "簽到記錄不存在" });
                }

                _context.UserSignInStats.Remove(record);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "簽到記錄刪除成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<double> CalculateAveragePointsPerSignInAsync()
        {
            var totalSignIns = await _context.UserSignInStats.CountAsync();
            if (totalSignIns == 0) return 0;

            var totalPoints = await _context.UserSignInStats.SumAsync(s => s.PointsGained);
            return (double)totalPoints / totalSignIns;
        }

        private async Task<int> CalculateConsecutiveDaysAsync(int userId, DateTime signInDate)
        {
            var userSignIns = await _context.UserSignInStats
                .Where(s => s.UserID == userId && s.SignTime.Date <= signInDate)
                .OrderByDescending(s => s.SignTime.Date)
                .Select(s => s.SignTime.Date)
                .Distinct()
                .ToListAsync();

            int consecutiveDays = 0;
            DateTime currentDate = signInDate;

            foreach (var date in userSignIns)
            {
                if (date == currentDate)
                {
                    consecutiveDays++;
                    currentDate = currentDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return consecutiveDays;
        }

        private async Task<List<object>> GetSignInRecordsAsync()
        {
            return await _context.UserSignInStats
                .Include(s => s.User)
                .OrderByDescending(s => s.SignTime)
                .Take(20)
                .Select(s => new
                {
                    logId = s.LogID,
                    userName = s.User.User_name,
                    signTime = s.SignTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    pointsGained = s.PointsGained,
                    expGained = s.ExpGained,
                    couponGained = s.CouponGained != "0" ? s.CouponGained : null
                })
                .Cast<object>()
                .ToListAsync();
        }
    }
}
