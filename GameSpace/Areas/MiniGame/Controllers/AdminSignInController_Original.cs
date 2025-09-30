using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminSignInController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSignInController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/AdminSignIn
        public async Task<IActionResult> Index()
        {
            var viewModel = new SignInOverviewViewModel();

            // 統計數據
            viewModel.TotalSignIns = await _context.UserSignInStats.CountAsync();
            viewModel.TodaySignIns = await _context.UserSignInStats
                .Where(s => s.LastSignInTime.Date == DateTime.Today)
                .CountAsync();
            viewModel.WeeklySignIns = await _context.UserSignInStats
                .Where(s => s.LastSignInTime >= DateTime.Today.AddDays(-7))
                .CountAsync();
            viewModel.MonthlySignIns = await _context.UserSignInStats
                .Where(s => s.LastSignInTime >= DateTime.Today.AddDays(-30))
                .CountAsync();

            // 連續簽到統計
            viewModel.AverageConsecutiveDays = await _context.UserSignInStats
                .AverageAsync(s => s.ConsecutiveDays);
            viewModel.MaxConsecutiveDays = await _context.UserSignInStats
                .MaxAsync(s => s.ConsecutiveDays);
            viewModel.TotalConsecutiveDays = await _context.UserSignInStats
                .SumAsync(s => s.ConsecutiveDays);

            // 簽到統計
            viewModel.SignInStats = await _context.UserSignInStats
                .Include(s => s.User)
                .Include(s => s.User.UserIntroduce)
                .OrderByDescending(s => s.ConsecutiveDays)
                .ThenByDescending(s => s.LastSignInTime)
                .Select(s => new SignInStatsViewModel
                {
                    UserId = s.UserID,
                    UserName = s.User.User_name,
                    NickName = s.UserIntroduce.User_NickName,
                    ConsecutiveDays = s.ConsecutiveDays,
                    TotalSignIns = s.TotalSignIns,
                    LastSignInTime = s.LastSignInTime,
                    LastSignInPoints = s.LastSignInPoints,
                    LastSignInCoupon = s.LastSignInCoupon,
                    LastSignInEVoucher = s.LastSignInEVoucher,
                    TotalPointsEarned = s.TotalPointsEarned,
                    TotalCouponsEarned = s.TotalCouponsEarned,
                    TotalEVouchersEarned = s.TotalEVouchersEarned
                })
                .ToListAsync();

            // 每日簽到統計
            viewModel.DailySignInStats = await _context.UserSignInStats
                .GroupBy(s => s.LastSignInTime.Date)
                .Select(g => new DailySignInStatsViewModel
                {
                    Date = g.Key,
                    SignInCount = g.Count(),
                    TotalPointsEarned = g.Sum(s => s.LastSignInPoints),
                    TotalCouponsEarned = g.Sum(s => s.TotalCouponsEarned),
                    TotalEVouchersEarned = g.Sum(s => s.TotalEVouchersEarned)
                })
                .OrderByDescending(s => s.Date)
                .Take(30)
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/AdminSignIn/GetSignInOverview
        [HttpGet]
        public async Task<IActionResult> GetSignInOverview()
        {
            try
            {
                var data = new
                {
                    totalSignIns = await _context.UserSignInStats.CountAsync(),
                    todaySignIns = await _context.UserSignInStats
                        .Where(s => s.LastSignInTime.Date == DateTime.Today)
                        .CountAsync(),
                    weeklySignIns = await _context.UserSignInStats
                        .Where(s => s.LastSignInTime >= DateTime.Today.AddDays(-7))
                        .CountAsync(),
                    monthlySignIns = await _context.UserSignInStats
                        .Where(s => s.LastSignInTime >= DateTime.Today.AddDays(-30))
                        .CountAsync(),
                    averageConsecutiveDays = await _context.UserSignInStats
                        .AverageAsync(s => s.ConsecutiveDays),
                    maxConsecutiveDays = await _context.UserSignInStats
                        .MaxAsync(s => s.ConsecutiveDays),
                    totalConsecutiveDays = await _context.UserSignInStats
                        .SumAsync(s => s.ConsecutiveDays),
                    signInStats = await _context.UserSignInStats
                        .Include(s => s.User)
                        .Include(s => s.User.UserIntroduce)
                        .OrderByDescending(s => s.ConsecutiveDays)
                        .ThenByDescending(s => s.LastSignInTime)
                        .Take(50)
                        .Select(s => new
                        {
                            userId = s.UserID,
                            userName = s.User.User_name,
                            nickName = s.UserIntroduce.User_NickName,
                            consecutiveDays = s.ConsecutiveDays,
                            totalSignIns = s.TotalSignIns,
                            lastSignInTime = s.LastSignInTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            lastSignInPoints = s.LastSignInPoints,
                            lastSignInCoupon = s.LastSignInCoupon,
                            lastSignInEVoucher = s.LastSignInEVoucher,
                            totalPointsEarned = s.TotalPointsEarned,
                            totalCouponsEarned = s.TotalCouponsEarned,
                            totalEVouchersEarned = s.TotalEVouchersEarned
                        })
                        .ToListAsync(),
                    dailySignInStats = await _context.UserSignInStats
                        .GroupBy(s => s.LastSignInTime.Date)
                        .Select(g => new
                        {
                            date = g.Key.ToString("yyyy-MM-dd"),
                            signInCount = g.Count(),
                            totalPointsEarned = g.Sum(s => s.LastSignInPoints),
                            totalCouponsEarned = g.Sum(s => s.TotalCouponsEarned),
                            totalEVouchersEarned = g.Sum(s => s.TotalEVouchersEarned)
                        })
                        .OrderByDescending(s => s.date)
                        .Take(30)
                        .ToListAsync()
                };

                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminSignIn/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var signInStats = await _context.UserSignInStats
                .Include(s => s.User)
                .Include(s => s.User.UserIntroduce)
                .Include(s => s.User.UserWallet)
                .FirstOrDefaultAsync(m => m.UserID == id);

            if (signInStats == null)
            {
                return NotFound();
            }

            return View(signInStats);
        }

        // POST: MiniGame/AdminSignIn/ManualSignIn
        [HttpPost]
        public async Task<IActionResult> ManualSignIn(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserWallet)
                    .Include(u => u.UserSignInStats)
                    .FirstOrDefaultAsync(u => u.User_ID == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "找不到用戶" });
                }

                var signInStats = user.UserSignInStats.FirstOrDefault();
                if (signInStats == null)
                {
                    // 創建新的簽到統計記錄
                    signInStats = new UserSignInStats
                    {
                        UserID = userId,
                        ConsecutiveDays = 1,
                        TotalSignIns = 1,
                        LastSignInTime = DateTime.Now,
                        LastSignInPoints = 10,
                        LastSignInCoupon = "0",
                        LastSignInEVoucher = "0",
                        TotalPointsEarned = 10,
                        TotalCouponsEarned = 0,
                        TotalEVouchersEarned = 0
                    };

                    _context.UserSignInStats.Add(signInStats);
                }
                else
                {
                    // 檢查是否已經簽到
                    if (signInStats.LastSignInTime.Date == DateTime.Today)
                    {
                        return Json(new { success = false, message = "用戶今天已經簽到過了" });
                    }

                    // 檢查連續簽到
                    if (signInStats.LastSignInTime.Date == DateTime.Today.AddDays(-1))
                    {
                        signInStats.ConsecutiveDays++;
                    }
                    else
                    {
                        signInStats.ConsecutiveDays = 1;
                    }

                    // 計算獎勵
                    var pointsReward = CalculateSignInPoints(signInStats.ConsecutiveDays);
                    var couponReward = CalculateSignInCoupon(signInStats.ConsecutiveDays);
                    var eVoucherReward = CalculateSignInEVoucher(signInStats.ConsecutiveDays);

                    // 更新簽到統計
                    signInStats.TotalSignIns++;
                    signInStats.LastSignInTime = DateTime.Now;
                    signInStats.LastSignInPoints = pointsReward;
                    signInStats.LastSignInCoupon = couponReward;
                    signInStats.LastSignInEVoucher = eVoucherReward;
                    signInStats.TotalPointsEarned += pointsReward;
                    signInStats.TotalCouponsEarned += couponReward != "0" ? 1 : 0;
                    signInStats.TotalEVouchersEarned += eVoucherReward != "0" ? 1 : 0;

                    // 更新用戶點數
                    user.UserWallet.User_Point += pointsReward;

                    // 記錄錢包異動
                    var walletHistory = new WalletHistory
                    {
                        UserID = userId,
                        ChangeType = "每日簽到獎勵",
                        PointsChanged = pointsReward,
                        Description = $"連續簽到 {signInStats.ConsecutiveDays} 天獎勵",
                        ChangeTime = DateTime.Now
                    };

                    _context.WalletHistories.Add(walletHistory);

                    // 處理優惠券獎勵
                    if (couponReward != "0")
                    {
                        var couponType = await _context.CouponTypes
                            .FirstOrDefaultAsync(ct => ct.Name.Contains(couponReward));

                        if (couponType != null)
                        {
                            var coupon = new Coupon
                            {
                                CouponCode = GenerateCouponCode(),
                                CouponTypeID = couponType.CouponTypeID,
                                UserID = userId,
                                IsUsed = false,
                                AcquiredTime = DateTime.Now
                            };

                            _context.Coupons.Add(coupon);
                        }
                    }

                    // 處理電子券獎勵
                    if (eVoucherReward != "0")
                    {
                        var eVoucherType = await _context.EVoucherTypes
                            .FirstOrDefaultAsync(evt => evt.Name.Contains(eVoucherReward));

                        if (eVoucherType != null)
                        {
                            var eVoucher = new EVoucher
                            {
                                EVoucherCode = GenerateEVoucherCode(),
                                EVoucherTypeID = eVoucherType.EVoucherTypeID,
                                UserID = userId,
                                IsUsed = false,
                                AcquiredTime = DateTime.Now
                            };

                            _context.EVouchers.Add(eVoucher);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "手動簽到成功",
                    consecutiveDays = signInStats.ConsecutiveDays,
                    pointsReward = signInStats.LastSignInPoints,
                    couponReward = signInStats.LastSignInCoupon,
                    eVoucherReward = signInStats.LastSignInEVoucher
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminSignIn/ResetSignInStats
        [HttpPost]
        public async Task<IActionResult> ResetSignInStats(int userId)
        {
            try
            {
                var signInStats = await _context.UserSignInStats
                    .FirstOrDefaultAsync(s => s.UserID == userId);

                if (signInStats == null)
                {
                    return Json(new { success = false, message = "找不到簽到統計記錄" });
                }

                // 重置簽到統計
                signInStats.ConsecutiveDays = 0;
                signInStats.TotalSignIns = 0;
                signInStats.LastSignInTime = DateTime.MinValue;
                signInStats.LastSignInPoints = 0;
                signInStats.LastSignInCoupon = "0";
                signInStats.LastSignInEVoucher = "0";
                signInStats.TotalPointsEarned = 0;
                signInStats.TotalCouponsEarned = 0;
                signInStats.TotalEVouchersEarned = 0;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "簽到統計已重置" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminSignIn/AdjustConsecutiveDays
        [HttpPost]
        public async Task<IActionResult> AdjustConsecutiveDays(int userId, int consecutiveDays)
        {
            try
            {
                var signInStats = await _context.UserSignInStats
                    .FirstOrDefaultAsync(s => s.UserID == userId);

                if (signInStats == null)
                {
                    return Json(new { success = false, message = "找不到簽到統計記錄" });
                }

                signInStats.ConsecutiveDays = Math.Max(0, consecutiveDays);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "連續簽到天數已調整" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private int CalculateSignInPoints(int consecutiveDays)
        {
            // 基礎點數 + 連續簽到獎勵
            var basePoints = 10;
            var bonusPoints = consecutiveDays * 2;
            return Math.Min(basePoints + bonusPoints, 100); // 最高100點
        }

        private string CalculateSignInCoupon(int consecutiveDays)
        {
            // 每7天給一次優惠券
            if (consecutiveDays % 7 == 0 && consecutiveDays > 0)
            {
                return "7天簽到優惠券";
            }
            return "0";
        }

        private string CalculateSignInEVoucher(int consecutiveDays)
        {
            // 每30天給一次電子券
            if (consecutiveDays % 30 == 0 && consecutiveDays > 0)
            {
                return "30天簽到電子券";
            }
            return "0";
        }

        private string GenerateCouponCode()
        {
            return "COUPON" + DateTime.Now.Ticks.ToString().Substring(10);
        }

        private string GenerateEVoucherCode()
        {
            return "EVOUCHER" + DateTime.Now.Ticks.ToString().Substring(10);
        }
    }
}
