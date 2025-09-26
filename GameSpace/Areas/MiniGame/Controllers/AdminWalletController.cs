using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminWalletController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminWalletController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/AdminWallet
        public async Task<IActionResult> Index()
        {
            var viewModel = new WalletOverviewViewModel();

            // 統計數據
            viewModel.TotalPoints = await _context.UserWallets.SumAsync(uw => uw.User_Point);
            viewModel.TodayPointChanges = await _context.WalletHistories
                .Where(w => w.ChangeTime.Date == DateTime.Today)
                .SumAsync(w => w.PointsChanged);
            viewModel.TotalCoupons = await _context.Coupons.CountAsync();
            viewModel.TotalEVouchers = await _context.EVouchers.CountAsync();
            viewModel.ActiveCoupons = await _context.Coupons
                .Where(c => !c.IsUsed && c.CouponType.ValidTo > DateTime.Now)
                .CountAsync();
            viewModel.ActiveEVouchers = await _context.EVouchers
                .Where(e => !e.IsUsed && e.EVoucherType.ValidTo > DateTime.Now)
                .CountAsync();

            // 錢包異動記錄
            viewModel.WalletHistory = await _context.WalletHistories
                .Include(w => w.User)
                .OrderByDescending(w => w.ChangeTime)
                .Take(50)
                .Select(w => new WalletHistoryViewModel
                {
                    LogId = w.LogID,
                    UserId = w.UserID,
                    UserName = w.User.User_name,
                    ChangeType = w.ChangeType,
                    Amount = w.PointsChanged,
                    Description = w.Description,
                    ChangeTime = w.ChangeTime,
                    Status = "成功"
                })
                .ToListAsync();

            // 用戶錢包列表
            viewModel.UserWallets = await _context.Users
                .Include(u => u.UserWallet)
                .Include(u => u.UserIntroduce)
                .Include(u => u.Coupons)
                .Include(u => u.EVouchers)
                .OrderByDescending(u => u.UserWallet.User_Point)
                .Select(u => new UserWalletViewModel
                {
                    UserId = u.User_ID,
                    UserName = u.User_name,
                    NickName = u.UserIntroduce.User_NickName,
                    CurrentPoints = u.UserWallet.User_Point,
                    TotalCoupons = u.Coupons.Count(),
                    TotalEVouchers = u.EVouchers.Count(),
                    LastActivity = u.WalletHistories.Any() ? 
                        u.WalletHistories.Max(wh => wh.ChangeTime) : DateTime.MinValue
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/AdminWallet/GetWalletOverview
        [HttpGet]
        public async Task<IActionResult> GetWalletOverview()
        {
            try
            {
                var data = new
                {
                    totalPoints = await _context.UserWallets.SumAsync(uw => uw.User_Point),
                    todayPointChanges = await _context.WalletHistories
                        .Where(w => w.ChangeTime.Date == DateTime.Today)
                        .SumAsync(w => w.PointsChanged),
                    totalCoupons = await _context.Coupons.CountAsync(),
                    totalEVouchers = await _context.EVouchers.CountAsync(),
                    walletHistory = await _context.WalletHistories
                        .Include(w => w.User)
                        .OrderByDescending(w => w.ChangeTime)
                        .Take(50)
                        .Select(w => new
                        {
                            logId = w.LogID,
                            userId = w.UserID,
                            userName = w.User.User_name,
                            changeType = w.ChangeType,
                            amount = w.PointsChanged,
                            description = w.Description,
                            changeTime = w.ChangeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            status = "成功"
                        })
                        .ToListAsync()
                };

                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MiniGame/AdminWallet/CouponManagement
        public async Task<IActionResult> CouponManagement()
        {
            var viewModel = new CouponManagementViewModel();

            // 統計數據
            viewModel.TotalCouponTypes = await _context.CouponTypes.CountAsync();
            viewModel.ActiveCouponTypes = await _context.CouponTypes
                .Where(ct => ct.ValidTo > DateTime.Now)
                .CountAsync();
            viewModel.TotalCouponsIssued = await _context.Coupons.CountAsync();
            viewModel.ActiveCoupons = await _context.Coupons
                .Where(c => !c.IsUsed && c.CouponType.ValidTo > DateTime.Now)
                .CountAsync();
            viewModel.UsedCoupons = await _context.Coupons
                .Where(c => c.IsUsed)
                .CountAsync();
            viewModel.ExpiredCoupons = await _context.Coupons
                .Where(c => !c.IsUsed && c.CouponType.ValidTo <= DateTime.Now)
                .CountAsync();

            // 優惠券類型
            viewModel.CouponTypes = await _context.CouponTypes
                .Include(ct => ct.Coupons)
                .OrderByDescending(ct => ct.CouponTypeID)
                .Select(ct => new CouponTypeViewModel
                {
                    CouponTypeId = ct.CouponTypeID,
                    Name = ct.Name,
                    DiscountType = ct.DiscountType,
                    DiscountValue = ct.DiscountValue,
                    MinSpend = ct.MinSpend,
                    ValidFrom = ct.ValidFrom,
                    ValidTo = ct.ValidTo,
                    PointsCost = ct.PointsCost,
                    Description = ct.Description,
                    TotalIssued = ct.Coupons.Count(),
                    TotalUsed = ct.Coupons.Count(c => c.IsUsed),
                    IsActive = ct.ValidTo > DateTime.Now
                })
                .ToListAsync();

            // 優惠券列表
            viewModel.Coupons = await _context.Coupons
                .Include(c => c.CouponType)
                .Include(c => c.User)
                .OrderByDescending(c => c.AcquiredTime)
                .Take(100)
                .Select(c => new CouponViewModel
                {
                    CouponId = c.CouponID,
                    CouponCode = c.CouponCode,
                    CouponTypeId = c.CouponTypeID,
                    CouponTypeName = c.CouponType.Name,
                    UserId = c.UserID,
                    UserName = c.User.User_name,
                    IsUsed = c.IsUsed,
                    AcquiredTime = c.AcquiredTime,
                    UsedTime = c.UsedTime,
                    UsedInOrderId = c.UsedInOrderID,
                    IsExpired = c.CouponType.ValidTo <= DateTime.Now
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/AdminWallet/EVoucherManagement
        public async Task<IActionResult> EVoucherManagement()
        {
            var viewModel = new EVoucherManagementViewModel();

            // 統計數據
            viewModel.TotalEVoucherTypes = await _context.EVoucherTypes.CountAsync();
            viewModel.ActiveEVoucherTypes = await _context.EVoucherTypes
                .Where(evt => evt.ValidTo > DateTime.Now)
                .CountAsync();
            viewModel.TotalEVouchersIssued = await _context.EVouchers.CountAsync();
            viewModel.ActiveEVouchers = await _context.EVouchers
                .Where(e => !e.IsUsed && e.EVoucherType.ValidTo > DateTime.Now)
                .CountAsync();
            viewModel.UsedEVouchers = await _context.EVouchers
                .Where(e => e.IsUsed)
                .CountAsync();
            viewModel.ExpiredEVouchers = await _context.EVouchers
                .Where(e => !e.IsUsed && e.EVoucherType.ValidTo <= DateTime.Now)
                .CountAsync();

            // 電子禮券類型
            viewModel.EVoucherTypes = await _context.EVoucherTypes
                .Include(evt => evt.EVouchers)
                .OrderByDescending(evt => evt.EVoucherTypeID)
                .Select(evt => new EVoucherTypeViewModel
                {
                    EVoucherTypeId = evt.EVoucherTypeID,
                    Name = evt.Name,
                    ValueAmount = evt.ValueAmount,
                    ValidFrom = evt.ValidFrom,
                    ValidTo = evt.ValidTo,
                    PointsCost = evt.PointsCost,
                    TotalAvailable = evt.TotalAvailable,
                    Description = evt.Description,
                    TotalIssued = evt.EVouchers.Count(),
                    TotalUsed = evt.EVouchers.Count(e => e.IsUsed),
                    IsActive = evt.ValidTo > DateTime.Now
                })
                .ToListAsync();

            // 電子禮券列表
            viewModel.EVouchers = await _context.EVouchers
                .Include(e => e.EVoucherType)
                .Include(e => e.User)
                .OrderByDescending(e => e.AcquiredTime)
                .Take(100)
                .Select(e => new EVoucherViewModel
                {
                    EVoucherId = e.EVoucherID,
                    EVoucherCode = e.EVoucherCode,
                    EVoucherTypeId = e.EVoucherTypeID,
                    EVoucherTypeName = e.EVoucherType.Name,
                    UserId = e.UserID,
                    UserName = e.User.User_name,
                    IsUsed = e.IsUsed,
                    AcquiredTime = e.AcquiredTime,
                    UsedTime = e.UsedTime,
                    IsExpired = e.EVoucherType.ValidTo <= DateTime.Now
                })
                .ToListAsync();

            return View(viewModel);
        }

        // POST: MiniGame/AdminWallet/IssueCoupon
        [HttpPost]
        public async Task<IActionResult> IssueCoupon(int userId, int couponTypeId)
        {
            try
            {
                var couponType = await _context.CouponTypes
                    .FirstOrDefaultAsync(ct => ct.CouponTypeID == couponTypeId);

                if (couponType == null)
                {
                    return Json(new { success = false, message = "找不到優惠券類型" });
                }

                var user = await _context.Users
                    .Include(u => u.UserWallet)
                    .FirstOrDefaultAsync(u => u.User_ID == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "找不到用戶" });
                }

                // 檢查用戶點數是否足夠
                if (user.UserWallet.User_Point < couponType.PointsCost)
                {
                    return Json(new { success = false, message = "用戶點數不足" });
                }

                // 扣除點數
                user.UserWallet.User_Point -= couponType.PointsCost;

                // 生成優惠券
                var coupon = new Coupon
                {
                    CouponCode = GenerateCouponCode(),
                    CouponTypeID = couponTypeId,
                    UserID = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now
                };

                _context.Coupons.Add(coupon);

                // 記錄錢包異動
                var walletHistory = new WalletHistory
                {
                    UserID = userId,
                    ChangeType = "兌換優惠券",
                    PointsChanged = -couponType.PointsCost,
                    ItemCode = coupon.CouponCode,
                    Description = $"兌換優惠券: {couponType.Name}",
                    ChangeTime = DateTime.Now
                };

                _context.WalletHistories.Add(walletHistory);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "優惠券發放成功", couponCode = coupon.CouponCode });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MiniGame/AdminWallet/IssueEVoucher
        [HttpPost]
        public async Task<IActionResult> IssueEVoucher(int userId, int evoucherTypeId)
        {
            try
            {
                var evoucherType = await _context.EVoucherTypes
                    .FirstOrDefaultAsync(evt => evt.EVoucherTypeID == evoucherTypeId);

                if (evoucherType == null)
                {
                    return Json(new { success = false, message = "找不到電子禮券類型" });
                }

                var user = await _context.Users
                    .Include(u => u.UserWallet)
                    .FirstOrDefaultAsync(u => u.User_ID == userId);

                if (user == null)
                {
                    return Json(new { success = false, message = "找不到用戶" });
                }

                // 檢查用戶點數是否足夠
                if (user.UserWallet.User_Point < evoucherType.PointsCost)
                {
                    return Json(new { success = false, message = "用戶點數不足" });
                }

                // 扣除點數
                user.UserWallet.User_Point -= evoucherType.PointsCost;

                // 生成電子禮券
                var evoucher = new EVoucher
                {
                    EVoucherCode = GenerateEVoucherCode(),
                    EVoucherTypeID = evoucherTypeId,
                    UserID = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now
                };

                _context.EVouchers.Add(evoucher);

                // 記錄錢包異動
                var walletHistory = new WalletHistory
                {
                    UserID = userId,
                    ChangeType = "兌換電子禮券",
                    PointsChanged = -evoucherType.PointsCost,
                    ItemCode = evoucher.EVoucherCode,
                    Description = $"兌換電子禮券: {evoucherType.Name}",
                    ChangeTime = DateTime.Now
                };

                _context.WalletHistories.Add(walletHistory);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "電子禮券發放成功", evoucherCode = evoucher.EVoucherCode });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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
