﻿using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

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
            try
            {
                var wallet = await GetUserWalletAsync(userId);

                // Check if user has enough points when deducting
                if (points < 0 && wallet.UserPoint < Math.Abs(points))
                {
                    return false;
                }

                wallet.UserPoint += points;

                // Record history
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = points > 0 ? "Admin_Add" : "Admin_Deduct",
                    PointsChanged = points,
                    ItemCode = "ADMIN_MANUAL",
                    Description = description,
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IssueCouponAsync(int userId, int couponTypeId, string description)
        {
            try
            {
                var couponType = await _context.CouponTypes
                    .FirstOrDefaultAsync(ct => ct.CouponTypeId == couponTypeId);

                if (couponType == null)
                {
                    return false;
                }

                // Generate unique coupon code
                var couponCode = $"CPN-{userId}-{DateTime.UtcNow.Ticks}";

                // Create coupon
                var coupon = new Coupon
                {
                    UserId = userId,
                    CouponTypeId = couponTypeId,
                    CouponCode = couponCode,
                    IsUsed = false
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
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IssueEVoucherAsync(int userId, int evoucherTypeId, string description)
        {
            try
            {
                var evoucherType = await _context.EvoucherTypes
                    .FirstOrDefaultAsync(et => et.EvoucherTypeId == evoucherTypeId);

                if (evoucherType == null)
                {
                    return false;
                }

                // Generate unique eVoucher code
                var evoucherCode = $"EVC-{userId}-{DateTime.UtcNow.Ticks}";

                // Create eVoucher
                var evoucher = new Evoucher
                {
                    UserId = userId,
                    EvoucherTypeId = evoucherTypeId,
                    EvoucherCode = evoucherCode,
                    IsUsed = false
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
                    ChangeTime = DateTime.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
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

        public async Task<bool> ExportWalletHistoryAsync(WalletHistoryQueryModel query, string filePath)
        {
            try
            {
                var queryable = _context.WalletHistories.AsQueryable();

                // Apply same filters
                if (query.UserId.HasValue)
                {
                    queryable = queryable.Where(h => h.UserId == query.UserId.Value);
                }
                if (!string.IsNullOrWhiteSpace(query.ChangeType))
                {
                    queryable = queryable.Where(h => h.ChangeType == query.ChangeType);
                }
                if (query.StartDate.HasValue)
                {
                    queryable = queryable.Where(h => h.ChangeTime >= query.StartDate.Value);
                }
                if (query.EndDate.HasValue)
                {
                    queryable = queryable.Where(h => h.ChangeTime <= query.EndDate.Value);
                }

                var histories = await queryable
                    .OrderByDescending(h => h.ChangeTime)
                    .ToListAsync();

                // Export to CSV file
                var csv = new StringBuilder();

                // Add header row
                csv.AppendLine("Log ID,User ID,Change Type,Points Changed,Item Code,Description,Change Time");

                // Add data rows
                foreach (var history in histories)
                {
                    csv.AppendLine($"{history.LogId},{history.UserId},{EscapeCsv(history.ChangeType)},{history.PointsChanged},{EscapeCsv(history.ItemCode ?? string.Empty)},{EscapeCsv(history.Description ?? string.Empty)},{history.ChangeTime:yyyy-MM-dd HH:mm:ss}");
                }

                // Write to file
                await File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }
}
