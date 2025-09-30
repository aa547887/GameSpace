using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        // GET: MiniGame/AdminWallet/MemberWalletDetail/{userId}
        public async Task<IActionResult> MemberWalletDetail(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserWallet)
                .Include(u => u.UserIntroduce)
                .Include(u => u.Coupons)
                    .ThenInclude(c => c.CouponType)
                .Include(u => u.EVouchers)
                    .ThenInclude(e => e.EVoucherType)
                .Include(u => u.WalletHistories)
                .FirstOrDefaultAsync(u => u.User_ID == userId);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new MemberWalletDetailViewModel
            {
                UserID = user.User_ID,
                UserName = user.User_name,
                NickName = user.UserIntroduce?.User_NickName ?? "",
                CurrentPoints = user.UserWallet?.User_Point ?? 0,
                TotalCoupons = user.Coupons?.Count() ?? 0,
                ActiveCoupons = user.Coupons?.Count(c => !c.IsUsed && c.CouponType.ValidTo > DateTime.Now) ?? 0,
                UsedCoupons = user.Coupons?.Count(c => c.IsUsed) ?? 0,
                TotalEVouchers = user.EVouchers?.Count() ?? 0,
                ActiveEVouchers = user.EVouchers?.Count(e => !e.IsUsed && e.EVoucherType.ValidTo > DateTime.Now) ?? 0,
                UsedEVouchers = user.EVouchers?.Count(e => e.IsUsed) ?? 0,
                LastActivity = user.WalletHistories?.Any() == true ? 
                    user.WalletHistories.Max(wh => wh.ChangeTime) : DateTime.MinValue,
                RecentWalletHistory = user.WalletHistories?
                    .OrderByDescending(wh => wh.ChangeTime)
                    .Take(20)
                    .Select(wh => new WalletHistoryDetailViewModel
                    {
                        LogID = wh.LogID,
                        ChangeType = wh.ChangeType,
                        PointsChanged = wh.PointsChanged,
                        ItemCode = wh.ItemCode,
                        Description = wh.Description,
                        ChangeTime = wh.ChangeTime,
                        Status = "成功"
                    })
                    .ToList() ?? new List<WalletHistoryDetailViewModel>(),
                UserCoupons = user.Coupons?
                    .Select(c => new CouponDetailViewModel
                    {
                        CouponID = c.CouponID,
                        CouponCode = c.CouponCode,
                        CouponTypeName = c.CouponType.Name,
                        DiscountType = c.CouponType.DiscountType,
                        DiscountValue = c.CouponType.DiscountValue,
                        MinSpend = c.CouponType.MinSpend,
                        IsUsed = c.IsUsed,
                        AcquiredTime = c.AcquiredTime,
                        UsedTime = c.UsedTime,
                        IsExpired = c.CouponType.ValidTo <= DateTime.Now
                    })
                    .ToList() ?? new List<CouponDetailViewModel>(),
                UserEVouchers = user.EVouchers?
                    .Select(e => new EVoucherDetailViewModel
                    {
                        EVoucherID = e.EVoucherID,
                        EVoucherCode = e.EVoucherCode,
                        EVoucherTypeName = e.EVoucherType.Name,
                        ValueAmount = e.EVoucherType.ValueAmount,
                        IsUsed = e.IsUsed,
                        AcquiredTime = e.AcquiredTime,
                        UsedTime = e.UsedTime,
                        IsExpired = e.EVoucherType.ValidTo <= DateTime.Now
                    })
                    .ToList() ?? new List<EVoucherDetailViewModel>()
            };

            return View(viewModel);
        }

        // POST: MiniGame/AdminWallet/IssuePoints
        [HttpPost]
        public async Task<IActionResult> IssuePoints([FromBody] IssuePointsRequest request)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserWallet)
                    .FirstOrDefaultAsync(u => u.User_ID == request.UserId);

                if (user == null)
                {
                    return Json(new { success = false, message = "用戶不存在" });
                }

                if (request.Amount <= 0)
                {
                    return Json(new { success = false, message = "點數金額必須大於0" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 更新用戶點數
                    user.UserWallet.User_Point += request.Amount;

                    // 記錄錢包異動
                    var walletHistory = new WalletHistory
                    {
                        UserID = request.UserId,
                        ChangeType = "AdminIssue",
                        PointsChanged = request.Amount,
                        Description = $"管理員發放點數: {request.Description}",
                        ChangeTime = DateTime.UtcNow
                    };

                    _context.WalletHistories.Add(walletHistory);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "點數發放成功" });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"發放點數失敗: {ex.Message}" });
            }
        }

        // POST: MiniGame/AdminWallet/IssueCoupon
        [HttpPost]
        public async Task<IActionResult> IssueCoupon([FromBody] IssueCouponRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.User_ID == request.UserId);
                var couponType = await _context.CouponTypes.FirstOrDefaultAsync(ct => ct.CouponTypeID == request.CouponTypeId);

                if (user == null)
                {
                    return Json(new { success = false, message = "用戶不存在" });
                }

                if (couponType == null)
                {
                    return Json(new { success = false, message = "優惠券類型不存在" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 生成優惠券代碼
                    var couponCode = GenerateCouponCode();

                    // 創建優惠券
                    var coupon = new Coupon
                    {
                        CouponCode = couponCode,
                        CouponTypeID = request.CouponTypeId,
                        UserID = request.UserId,
                        IsUsed = false,
                        AcquiredTime = DateTime.UtcNow
                    };

                    _context.Coupons.Add(coupon);

                    // 記錄錢包異動
                    var walletHistory = new WalletHistory
                    {
                        UserID = request.UserId,
                        ChangeType = "Coupon",
                        PointsChanged = 0,
                        ItemCode = couponCode,
                        Description = $"管理員發放優惠券: {couponType.Name}",
                        ChangeTime = DateTime.UtcNow
                    };

                    _context.WalletHistories.Add(walletHistory);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "優惠券發放成功", couponCode = couponCode });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"發放優惠券失敗: {ex.Message}" });
            }
        }

        // POST: MiniGame/AdminWallet/IssueEVoucher
        [HttpPost]
        public async Task<IActionResult> IssueEVoucher([FromBody] IssueEVoucherRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.User_ID == request.UserId);
                var evoucherType = await _context.EVoucherTypes.FirstOrDefaultAsync(evt => evt.EVoucherTypeID == request.EVoucherTypeId);

                if (user == null)
                {
                    return Json(new { success = false, message = "用戶不存在" });
                }

                if (evoucherType == null)
                {
                    return Json(new { success = false, message = "電子禮券類型不存在" });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 生成電子禮券代碼
                    var evoucherCode = GenerateEVoucherCode();

                    // 創建電子禮券
                    var evoucher = new EVoucher
                    {
                        EVoucherCode = evoucherCode,
                        EVoucherTypeID = request.EVoucherTypeId,
                        UserID = request.UserId,
                        IsUsed = false,
                        AcquiredTime = DateTime.UtcNow
                    };

                    _context.EVouchers.Add(evoucher);

                    // 記錄錢包異動
                    var walletHistory = new WalletHistory
                    {
                        UserID = request.UserId,
                        ChangeType = "EVoucher",
                        PointsChanged = 0,
                        ItemCode = evoucherCode,
                        Description = $"管理員發放電子禮券: {evoucherType.Name}",
                        ChangeTime = DateTime.UtcNow
                    };

                    _context.WalletHistories.Add(walletHistory);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "電子禮券發放成功", evoucherCode = evoucherCode });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"發放電子禮券失敗: {ex.Message}" });
            }
        }

        // GET: MiniGame/AdminWallet/EVoucherTokenManagement
        public async Task<IActionResult> EVoucherTokenManagement()
        {
            var viewModel = new EVoucherTokenManagementViewModel();

            // 統計數據
            viewModel.TotalTokens = await _context.EVoucherTokens.CountAsync();
            viewModel.ActiveTokens = await _context.EVoucherTokens
                .Where(evt => !evt.IsRevoked && evt.ExpiresAt > DateTime.UtcNow)
                .CountAsync();
            viewModel.ExpiredTokens = await _context.EVoucherTokens
                .Where(evt => evt.ExpiresAt <= DateTime.UtcNow)
                .CountAsync();
            viewModel.RevokedTokens = await _context.EVoucherTokens
                .Where(evt => evt.IsRevoked)
                .CountAsync();

            // Token列表
            viewModel.EVoucherTokens = await _context.EVoucherTokens
                .Include(evt => evt.EVoucher)
                    .ThenInclude(ev => ev.User)
                .Include(evt => evt.EVoucher)
                    .ThenInclude(ev => ev.EVoucherType)
                .OrderByDescending(evt => evt.TokenID)
                .Select(evt => new EVoucherTokenViewModel
                {
                    TokenID = evt.TokenID,
                    EVoucherID = evt.EVoucherID,
                    EVoucherCode = evt.EVoucher.EVoucherCode,
                    Token = evt.Token,
                    ExpiresAt = evt.ExpiresAt,
                    IsRevoked = evt.IsRevoked,
                    IsExpired = evt.ExpiresAt <= DateTime.UtcNow,
                    UserID = evt.EVoucher.UserID,
                    UserName = evt.EVoucher.User.User_name
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/AdminWallet/EVoucherRedeemLogManagement
        public async Task<IActionResult> EVoucherRedeemLogManagement()
        {
            var viewModel = new EVoucherRedeemLogManagementViewModel();

            // 統計數據
            viewModel.TotalRedeemLogs = await _context.EVoucherRedeemLogs.CountAsync();
            viewModel.ApprovedLogs = await _context.EVoucherRedeemLogs
                .Where(evrl => evrl.Status == "Approved")
                .CountAsync();
            viewModel.RejectedLogs = await _context.EVoucherRedeemLogs
                .Where(evrl => evrl.Status == "Rejected")
                .CountAsync();
            viewModel.ExpiredLogs = await _context.EVoucherRedeemLogs
                .Where(evrl => evrl.Status == "Expired")
                .CountAsync();
            viewModel.AlreadyUsedLogs = await _context.EVoucherRedeemLogs
                .Where(evrl => evrl.Status == "AlreadyUsed")
                .CountAsync();

            // 核銷記錄列表
            viewModel.EVoucherRedeemLogs = await _context.EVoucherRedeemLogs
                .Include(evrl => evrl.EVoucher)
                    .ThenInclude(ev => ev.User)
                .Include(evrl => evrl.EVoucherToken)
                .OrderByDescending(evrl => evrl.ScannedAt)
                .Select(evrl => new EVoucherRedeemLogViewModel
                {
                    RedeemID = evrl.RedeemID,
                    EVoucherID = evrl.EVoucherID,
                    EVoucherCode = evrl.EVoucher.EVoucherCode,
                    TokenID = evrl.TokenID,
                    Token = evrl.EVoucherToken != null ? evrl.EVoucherToken.Token : null,
                    UserID = evrl.UserID,
                    UserName = evrl.EVoucher.User.User_name,
                    ScannedAt = evrl.ScannedAt,
                    Status = evrl.Status,
                    StatusDescription = GetStatusDescription(evrl.Status)
                })
                .ToListAsync();

            return View(viewModel);
        }

        // 輔助方法
        private string GenerateCouponCode()
        {
            return "ADMIN" + DateTime.UtcNow.Ticks.ToString().Substring(10);
        }

        private string GenerateEVoucherCode()
        {
            return "EV" + DateTime.UtcNow.Ticks.ToString().Substring(8);
        }

        private string GetStatusDescription(string status)
        {
            return status switch
            {
                "Approved" => "已成功核銷",
                "Rejected" => "核銷被拒絕",
                "Expired" => "Token已過期",
                "AlreadyUsed" => "禮券已使用",
                "Revoked" => "Token已撤銷",
                _ => "未知狀態"
            };
        }
    }

    // 請求模型
    public class IssuePointsRequest
    {
        public int UserId { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
    }

    public class IssueCouponRequest
    {
        public int UserId { get; set; }
        public int CouponTypeId { get; set; }
    }

    public class IssueEVoucherRequest
    {
        public int UserId { get; set; }
        public int EVoucherTypeId { get; set; }
    }
}
