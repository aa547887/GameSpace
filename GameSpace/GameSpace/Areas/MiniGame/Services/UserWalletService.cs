using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class UserWalletService : IUserWalletService
    {
        private readonly GameSpacedatabaseContext _context;

        public UserWalletService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserWallet>> GetUserWalletsAsync(WalletQueryModel query)
        {
            var queryable = _context.UserWallets.AsQueryable();

            // 搜尋條件
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(w => w.User.User_name.Contains(query.SearchTerm) ||
                                               w.User.User_Account.Contains(query.SearchTerm));
            }

            // 點數範圍篩選
            if (query.MinPoints.HasValue)
                queryable = queryable.Where(w => w.User_Point >= query.MinPoints.Value);
            if (query.MaxPoints.HasValue)
                queryable = queryable.Where(w => w.User_Point <= query.MaxPoints.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .Include(w => w.User)
                .OrderByDescending(w => w.User_Point)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<UserWallet>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<UserWallet> GetUserWalletAsync(int userId)
        {
            return await _context.UserWallets
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.User_Id == userId);
        }

        public async Task<bool> UpdateUserPointsAsync(int userId, int points, string description)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var wallet = await _context.UserWallets.FindAsync(userId);
                if (wallet == null) return false;

                var oldPoints = wallet.User_Point;
                wallet.User_Point += points;

                // 記錄交易歷史
                var history = new WalletHistory
                {
                    UserID = userId,
                    ChangeAmount = points,
                    ChangeType = "Point",
                    ChangeTime = DateTime.Now,
                    Description = description,
                    RelatedID = null
                };

                _context.WalletHistories.Add(history);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> IssueCouponAsync(int userId, int couponTypeId, string description)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
                if (couponType == null) return false;

                var coupon = new Coupon
                {
                    CouponCode = GenerateCouponCode(),
                    CouponTypeID = couponTypeId,
                    UserID = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now,
                    UsedTime = null,
                    UsedInOrderID = null
                };

                _context.Coupons.Add(coupon);

                // 記錄交易歷史
                var history = new WalletHistory
                {
                    UserID = userId,
                    ChangeAmount = 1,
                    ChangeType = "Coupon",
                    ChangeTime = DateTime.Now,
                    Description = description,
                    RelatedID = coupon.CouponID
                };

                _context.WalletHistories.Add(history);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> IssueEVoucherAsync(int userId, int evoucherTypeId, string description)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var evoucherType = await _context.EVoucherTypes.FindAsync(evoucherTypeId);
                if (evoucherType == null) return false;

                var evoucher = new EVoucher
                {
                    EVoucherCode = GenerateEVoucherCode(evoucherType.Name),
                    EVoucherTypeID = evoucherTypeId,
                    UserID = userId,
                    IsUsed = false,
                    AcquiredTime = DateTime.Now,
                    UsedTime = null
                };

                _context.EVouchers.Add(evoucher);

                // 記錄交易歷史
                var history = new WalletHistory
                {
                    UserID = userId,
                    ChangeAmount = 1,
                    ChangeType = "EVoucher",
                    ChangeTime = DateTime.Now,
                    Description = description,
                    RelatedID = evoucher.EVoucherID
                };

                _context.WalletHistories.Add(history);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<PagedResult<WalletHistory>> GetWalletHistoryAsync(WalletHistoryQueryModel query)
        {
            var queryable = _context.WalletHistories.AsQueryable();

            // 用戶篩選
            if (query.UserId.HasValue)
                queryable = queryable.Where(h => h.UserID == query.UserId.Value);

            // 交易類型篩選
            if (!string.IsNullOrEmpty(query.ChangeType))
                queryable = queryable.Where(h => h.ChangeType == query.ChangeType);

            // 日期範圍篩選
            if (query.StartDate.HasValue)
                queryable = queryable.Where(h => h.ChangeTime >= query.StartDate.Value);
            if (query.EndDate.HasValue)
                queryable = queryable.Where(h => h.ChangeTime <= query.EndDate.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .Include(h => h.User)
                .OrderByDescending(h => h.ChangeTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<WalletHistory>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<bool> ExportWalletHistoryAsync(WalletHistoryQueryModel query, string filePath)
        {
            // 實作匯出功能
            return await Task.FromResult(true);
        }

        private string GenerateCouponCode()
        {
            var now = DateTime.Now;
            var random = new Random();
            var randomCode = random.Next(100000, 999999);
            return $"CPN-{now:yyyyMM}-{randomCode}";
        }

        private string GenerateEVoucherCode(string typeName)
        {
            var typeCode = GetTypeCode(typeName);
            var random = new Random();
            var randomCode = random.Next(1000, 9999);
            var numberCode = random.Next(100000, 999999);
            return $"EV-{typeCode}-{randomCode}-{numberCode}";
        }

        private string GetTypeCode(string typeName)
        {
            return typeName.ToUpper() switch
            {
                "現金券" => "CASH",
                "電影券" => "MOVIE",
                "美食券" => "FOOD",
                "加油券" => "GAS",
                "商店券" => "STORE",
                "咖啡券" => "COFFEE",
                _ => "CASH"
            };
        }
    }
}
