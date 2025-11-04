using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace GameSpace.Areas.MiniGame.Services
{
    public class UserWalletService : IUserWalletService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ILogger<UserWalletService> _logger;
        private readonly IAppClock _appClock;

        public UserWalletService(GameSpacedatabaseContext context, ILogger<UserWalletService> logger, IAppClock appClock)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appClock = appClock ?? throw new ArgumentNullException(nameof(appClock));
        }

        public async Task<PagedResult<UserWallet>> GetUserWalletsAsync(WalletQueryModel query)
        {
            var queryable = _context.UserWallets.AsQueryable();

            // Filter by user ID
            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(w => w.UserId == query.UserId.Value);
            }

            // Filter by amount range
            if (query.MinAmount.HasValue)
            {
                queryable = queryable.Where(w => w.UserPoint >= query.MinAmount.Value);
            }
            if (query.MaxAmount.HasValue)
            {
                queryable = queryable.Where(w => w.UserPoint <= query.MaxAmount.Value);
            }

            // Calculate total count
            var totalCount = await queryable.CountAsync();

            // Sorting
            switch (query.SortBy?.ToLower())
            {
                case "points_desc":
                    queryable = queryable.OrderByDescending(w => w.UserPoint);
                    break;
                case "points_asc":
                    queryable = queryable.OrderBy(w => w.UserPoint);
                    break;
                case "userid_desc":
                    queryable = queryable.OrderByDescending(w => w.UserId);
                    break;
                case "userid_asc":
                default:
                    queryable = queryable.OrderBy(w => w.UserId);
                    break;
            }

            // Pagination
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<UserWallet>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<UserWallet> GetUserWalletAsync(int userId)
        {
            var wallet = await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                // Create new wallet if it does not exist
                wallet = new UserWallet
                {
                    UserId = userId,
                    UserPoint = 0
                };
                _context.UserWallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            return wallet;
        }

        public async Task<bool> UpdateUserPointsAsync(int userId, int points, string description)
        {
            var wallet = await GetUserWalletAsync(userId);

            // Check if user has enough points when deducting
            if (points < 0 && wallet.UserPoint < Math.Abs(points))
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                wallet.UserPoint += points;

                // Record history - 使用台灣時間
                var nowUtc = _appClock.UtcNow;
                var nowTaiwanTime = _appClock.ToAppTime(nowUtc);
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = points > 0 ? "Admin_Add" : "Admin_Deduct",
                    PointsChanged = points,
                    ItemCode = "ADMIN_MANUAL",
                    Description = description,
                    ChangeTime = nowTaiwanTime
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("用戶點數更新成功: UserId={UserId}, Points={Points}, Description={Description}", userId, points, description);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用戶點數失敗: UserId={UserId}, Points={Points}", userId, points);
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> IssueCouponAsync(int userId, int couponTypeId, string description)
        {
            var couponType = await _context.CouponTypes
                .FirstOrDefaultAsync(ct => ct.CouponTypeId == couponTypeId);

            if (couponType == null)
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 使用台灣時間
                var nowUtc = _appClock.UtcNow;
                var nowTaiwanTime = _appClock.ToAppTime(nowUtc);

                // Generate unique coupon code
                var couponCode = $"CPN-{userId}-{nowUtc.Ticks}";

                // Create coupon
                var coupon = new Coupon
                {
                    UserId = userId,
                    CouponTypeId = couponTypeId,
                    CouponCode = couponCode,
                    IsUsed = false,
                    AcquiredTime = nowTaiwanTime,  // 使用台灣時間
                    UsedTime = null,  // 明確設為 null (剛發放時未使用)
                    IsDeleted = false  // 必填字段：未刪除
                };
                _context.Coupons.Add(coupon);

                // Record history
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Coupon_Issue",
                    PointsChanged = 0,
                    ItemCode = $"COUPON_{couponTypeId}",
                    Description = description,
                    ChangeTime = nowTaiwanTime  // 使用台灣時間
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("發放優惠券成功: UserId={UserId}, CouponTypeId={CouponTypeId}", userId, couponTypeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發放優惠券失敗: UserId={UserId}, CouponTypeId={CouponTypeId}", userId, couponTypeId);
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> IssueEVoucherAsync(int userId, int evoucherTypeId, string description)
        {
            var evoucherType = await _context.EvoucherTypes
                .FirstOrDefaultAsync(et => et.EvoucherTypeId == evoucherTypeId);

            if (evoucherType == null)
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 使用台灣時間
                var nowUtc = _appClock.UtcNow;
                var nowTaiwanTime = _appClock.ToAppTime(nowUtc);

                // Generate unique eVoucher code
                var evoucherCode = $"EVC-{userId}-{nowUtc.Ticks}";

                // Create eVoucher
                var evoucher = new Evoucher
                {
                    UserId = userId,
                    EvoucherTypeId = evoucherTypeId,
                    EvoucherCode = evoucherCode,
                    IsUsed = false,
                    AcquiredTime = nowTaiwanTime,  // 使用台灣時間
                    UsedTime = null,  // 明確設為 null (剛發放時未使用)
                    IsDeleted = false  // 必填字段：未刪除
                };
                _context.Evouchers.Add(evoucher);

                // Record history
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "EVoucher_Issue",
                    PointsChanged = 0,
                    ItemCode = $"EVOUCHER_{evoucherTypeId}",
                    Description = description,
                    ChangeTime = nowTaiwanTime  // 使用台灣時間
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("發放電子優惠券成功: UserId={UserId}, EVoucherTypeId={EVoucherTypeId}", userId, evoucherTypeId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發放電子優惠券失敗: UserId={UserId}, EVoucherTypeId={EVoucherTypeId}", userId, evoucherTypeId);
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<PagedResult<WalletHistory>> GetWalletHistoryAsync(WalletHistoryQueryModel query)
        {
            var queryable = _context.WalletHistories.AsQueryable();

            // Filter by user ID
            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(h => h.UserId == query.UserId.Value);
            }

            // Filter by change type
            if (!string.IsNullOrWhiteSpace(query.ChangeType))
            {
                queryable = queryable.Where(h => h.ChangeType == query.ChangeType);
            }

            // Filter by date range
            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(h => h.ChangeTime >= query.StartDate.Value);
            }
            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(h => h.ChangeTime <= query.EndDate.Value);
            }

            // Calculate total count
            var totalCount = await queryable.CountAsync();

            // Sort by time descending
            queryable = queryable.OrderByDescending(h => h.ChangeTime);

            // Pagination
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<WalletHistory>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = query.PageNumber,
                PageSize = query.PageSize
            };
        }

    }
}
