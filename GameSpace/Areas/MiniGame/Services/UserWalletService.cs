using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 會員錢包服務實作
    /// </summary>
    public class UserWalletService : IUserWalletService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<UserWalletService> _logger;

        public UserWalletService(MiniGameDbContext context, ILogger<UserWalletService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserWalletViewModel?> GetUserWalletAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.UserID == userId)
                    .FirstOrDefaultAsync();

                if (user == null) return null;

                var wallet = await _context.User_Wallet
                    .Where(w => w.UserID == userId)
                    .FirstOrDefaultAsync();

                var viewModel = new UserWalletViewModel
                {
                    UserID = user.UserID,
                    UserAccount = user.UserAccount,
                    UserName = user.UserName,
                    CurrentPoints = wallet?.Points ?? 0
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得會員錢包資訊失敗，UserID: {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> AdjustUserPointsAsync(int userId, int points, string reason)
        {
            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // 取得或建立錢包記錄
                var wallet = await _context.User_Wallet
                    .Where(w => w.UserID == userId)
                    .FirstOrDefaultAsync();

                if (wallet == null)
                {
                    wallet = new User_Wallet
                    {
                        UserID = userId,
                        Points = 0
                    };
                    _context.User_Wallet.Add(wallet);
                }

                // 更新點數
                wallet.Points += points;

                // 記錄錢包歷史
                var history = new WalletHistory
                {
                    UserID = userId,
                    ChangeTime = DateTime.Now,
                    ChangeType = points > 0 ? "增加" : "減少",
                    ChangeAmount = Math.Abs(points),
                    Description = reason
                };
                _context.WalletHistory.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "調整會員點數失敗，UserID: {UserId}, Points: {Points}", userId, points);
                return false;
            }
        }

        public async Task<List<WalletHistoryViewModel>> GetWalletHistoryAsync(int userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var histories = await _context.WalletHistory
                    .Where(h => h.UserID == userId)
                    .OrderByDescending(h => h.ChangeTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(h => new WalletHistoryViewModel
                    {
                        HistoryID = h.HistoryID,
                        ChangeTime = h.ChangeTime,
                        ChangeType = h.ChangeType,
                        ChangeAmount = h.ChangeAmount,
                        Description = h.Description
                    })
                    .ToListAsync();

                return histories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得錢包歷史記錄失敗，UserID: {UserId}", userId);
                return new List<WalletHistoryViewModel>();
            }
        }

        public async Task<List<CouponViewModel>> GetUserCouponsAsync(int userId)
        {
            try
            {
                var coupons = await _context.Coupon
                    .Where(c => c.UserID == userId)
                    .Include(c => c.CouponType)
                    .Select(c => new CouponViewModel
                    {
                        CouponID = c.CouponID,
                        CouponCode = c.CouponCode,
                        CouponTypeName = c.CouponType.CouponTypeName,
                        IsUsed = c.IsUsed,
                        AcquiredTime = c.AcquiredTime,
                        UsedTime = c.UsedTime
                    })
                    .ToListAsync();

                return coupons;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得會員優惠券失敗，UserID: {UserId}", userId);
                return new List<CouponViewModel>();
            }
        }

        public async Task<List<EVoucherViewModel>> GetUserEVouchersAsync(int userId)
        {
            try
            {
                var evouchers = await _context.EVoucher
                    .Where(e => e.UserID == userId)
                    .Include(e => e.EVoucherType)
                    .Select(e => new EVoucherViewModel
                    {
                        EVoucherID = e.EVoucherID,
                        EVoucherCode = e.EVoucherCode,
                        EVoucherTypeName = e.EVoucherType.EVoucherTypeName,
                        IsUsed = e.IsUsed,
                        AcquiredTime = e.AcquiredTime,
                        UsedTime = e.UsedTime
                    })
                    .ToListAsync();

                return evouchers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得會員電子禮券失敗，UserID: {UserId}", userId);
                return new List<EVoucherViewModel>();
            }
        }

        public async Task<bool> IssueCouponToUserAsync(int userId, int couponTypeId)
        {
            try
            {
                var couponType = await _context.CouponType
                    .Where(ct => ct.CouponTypeID == couponTypeId)
                    .FirstOrDefaultAsync();

                if (couponType == null) return false;

                var coupon = new Coupon
                {
                    UserID = userId,
                    CouponTypeID = couponTypeId,
                    CouponCode = GenerateCouponCode(),
                    IsUsed = false,
                    AcquiredTime = DateTime.Now
                };

                _context.Coupon.Add(coupon);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發放優惠券失敗，UserID: {UserId}, CouponTypeID: {CouponTypeId}", userId, couponTypeId);
                return false;
            }
        }

        public async Task<bool> IssueEVoucherToUserAsync(int userId, int evoucherTypeId)
        {
            try
            {
                var evoucherType = await _context.EVoucherType
                    .Where(et => et.EVoucherTypeID == evoucherTypeId)
                    .FirstOrDefaultAsync();

                if (evoucherType == null) return false;

                var evoucher = new EVoucher
                {
                    UserID = userId,
                    EVoucherTypeID = evoucherTypeId,
                    EVoucherCode = GenerateEVoucherCode(),
                    IsUsed = false,
                    AcquiredTime = DateTime.Now
                };

                _context.EVoucher.Add(evoucher);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發放電子禮券失敗，UserID: {UserId}, EVoucherTypeID: {EVoucherTypeId}", userId, evoucherTypeId);
                return false;
            }
        }

        public async Task<bool> RemoveUserCouponAsync(int couponId)
        {
            try
            {
                var coupon = await _context.Coupon
                    .Where(c => c.CouponID == couponId)
                    .FirstOrDefaultAsync();

                if (coupon == null) return false;

                _context.Coupon.Remove(coupon);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除會員優惠券失敗，CouponID: {CouponId}", couponId);
                return false;
            }
        }

        public async Task<bool> RemoveUserEVoucherAsync(int evoucherId)
        {
            try
            {
                var evoucher = await _context.EVoucher
                    .Where(e => e.EVoucherID == evoucherId)
                    .FirstOrDefaultAsync();

                if (evoucher == null) return false;

                _context.EVoucher.Remove(evoucher);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除會員電子禮券失敗，EVoucherID: {EVoucherId}", evoucherId);
                return false;
            }
        }

        private string GenerateCouponCode()
        {
            return "COUPON" + DateTime.Now.Ticks.ToString("X")[^8..];
        }

        private string GenerateEVoucherCode()
        {
            return "EVOUCHER" + DateTime.Now.Ticks.ToString("X")[^8..];
        }
    }
}