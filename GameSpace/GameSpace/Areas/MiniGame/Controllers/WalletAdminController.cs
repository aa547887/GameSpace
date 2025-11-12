using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Infrastructure.Time;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class WalletAdminController : MiniGameBaseController
    {
        private readonly IAppClock _appClock;
        private readonly Services.IFuzzySearchService _fuzzySearchService;

        public WalletAdminController(GameSpacedatabaseContext context, IAppClock appClock, Services.IFuzzySearchService fuzzySearchService)
            : base(context)
        {
            _appClock = appClock;
            _fuzzySearchService = fuzzySearchService;
        }

        [HttpGet]
        public async Task<IActionResult> PointsQuery([FromQuery] WalletQueryModel query)
        {
            query ??= new WalletQueryModel();
            // 伺服器端保護：避免負數參數造成不合理查詢
            if (query.UserId.HasValue && query.UserId.Value < 0) query.UserId = 0;
            if (query.MinAmount.HasValue && query.MinAmount.Value < 0) query.MinAmount = 0;
            if (query.MaxAmount.HasValue && query.MaxAmount.Value < 0) query.MaxAmount = 0;
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 10, 200);

            var source = _context.UserWallets
                .AsNoTracking()
                .AsQueryable();

            // 模糊搜尋：UserId 或 SearchTerm（聯集OR邏輯，使用 FuzzySearchService）
            var hasUserId = query.UserId.HasValue;
            var hasSearchTerm = !string.IsNullOrWhiteSpace(query.SearchTerm);

            List<int> matchedUserIds = new List<int>();
            Dictionary<int, int> userPriority = new Dictionary<int, int>();

            if (hasUserId || hasSearchTerm)
            {
                var userIdStr = hasUserId ? query.UserId.Value.ToString() : "";
                var searchTerm = hasSearchTerm ? query.SearchTerm!.Trim() : "";

                // 查詢所有用戶並使用 FuzzySearchService 計算優先順序
                var allUsers = await _context.Users
                    .AsNoTracking()
                    .Select(u => new { u.UserId, u.UserAccount, u.UserName })
                    .ToListAsync();

                foreach (var user in allUsers)
                {
                    int priority = 0;

                    // 如果有 UserId 條件，優先匹配 UserId
                    if (hasUserId)
                    {
                        if (user.UserId == query.UserId.Value)
                        {
                            priority = 1; // 完全匹配 UserId
                        }
                        else if (user.UserId.ToString().Contains(userIdStr))
                        {
                            priority = 2; // 部分匹配 UserId
                        }
                    }

                    // 如果有 SearchTerm 條件，使用 FuzzySearchService
                    if (hasSearchTerm && priority == 0)
                    {
                        priority = _fuzzySearchService.CalculateMatchPriority(
                            searchTerm,
                            user.UserAccount,
                            user.UserName
                        );
                    }

                    // 如果匹配成功（priority > 0），加入結果
                    if (priority > 0)
                    {
                        matchedUserIds.Add(user.UserId);
                        userPriority[user.UserId] = priority;
                    }
                }

                source = source.Where(w => matchedUserIds.Contains(w.UserId));
            }

            if (query.MinAmount.HasValue)
            {
                source = source.Where(w => w.UserPoint >= query.MinAmount.Value);
            }

            if (query.MaxAmount.HasValue)
            {
                source = source.Where(w => w.UserPoint <= query.MaxAmount.Value);
            }

            // 計算篩選後的總數和統計數據
            var filteredWallets = await source.ToListAsync();
            var totalCount = filteredWallets.Count;

            // 動態計算統計數據（基於篩選後的資料）
            var queryMemberCount = totalCount;
            var totalPoints = filteredWallets.Sum(w => (long)w.UserPoint);
            var avgPoints = totalCount > 0 ? (int)(totalPoints / totalCount) : 0;
            var maxPoints = filteredWallets.Any() ? filteredWallets.Max(w => w.UserPoint) : 0;
            var minPoints = filteredWallets.Any() ? filteredWallets.Min(w => w.UserPoint) : 0;

            // 優先順序排序
            List<UserWallet> items;
            if (hasUserId || hasSearchTerm)
            {
                // 在記憶體中進行優先順序排序
                var ordered = filteredWallets.OrderBy(w => userPriority.ContainsKey(w.UserId) ? userPriority[w.UserId] : 99);

                // 次要排序
                var sorted = query.SortBy?.ToLowerInvariant() switch
                {
                    "points_asc" => ordered.ThenBy(w => w.UserPoint),
                    "points_desc" => ordered.ThenByDescending(w => w.UserPoint),
                    "userid_desc" => ordered.ThenByDescending(w => w.UserId),
                    "userid_asc" => ordered.ThenBy(w => w.UserId),
                    _ => ordered.ThenByDescending(w => w.UserPoint)
                };

                // 分頁
                items = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                // 沒有搜尋條件時使用資料庫排序
                var sorted = query.SortBy?.ToLowerInvariant() switch
                {
                    "points_asc" => filteredWallets.OrderBy(w => w.UserPoint),
                    "points_desc" => filteredWallets.OrderByDescending(w => w.UserPoint),
                    "userid_desc" => filteredWallets.OrderByDescending(w => w.UserId),
                    "userid_asc" => filteredWallets.OrderBy(w => w.UserId),
                    _ => filteredWallets.OrderByDescending(w => w.UserPoint)
                };

                items = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            var userIds = items.Select(i => i.UserId).Distinct().ToList();
            var userLookup = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => new { u.UserAccount, u.UserName });
            var introduceLookup = await _context.UserIntroduces
                .AsNoTracking()
                .Where(ui => userIds.Contains(ui.UserId))
                .ToDictionaryAsync(ui => ui.UserId, ui => ui.Email);

            var records = items.Select(w =>
            {
                userLookup.TryGetValue(w.UserId, out var u);
                introduceLookup.TryGetValue(w.UserId, out var email);
                return new WalletPointRecord
                {
                    UserId = w.UserId,
                    UserAccount = u?.UserAccount ?? string.Empty,
                    UserName = u?.UserName ?? string.Empty,
                    Email = email ?? string.Empty,
                    Points = w.UserPoint
                };
            }).ToList();

            var model = new WalletPointsQueryViewModel
            {
                Query = query,
                Results = new PagedResult<WalletPointRecord>
                {
                    Items = records,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                },
                QueryMemberCount = queryMemberCount,
                TotalPoints = totalPoints,
                AveragePoints = avgPoints,
                HighestPoints = maxPoints,
                LowestPoints = minPoints
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CouponsQuery([FromQuery] CouponQueryModel query)
        {
            query ??= new CouponQueryModel();
            // 伺服器端保護：避免負數參數
            if (query.UserId.HasValue && query.UserId.Value < 0) query.UserId = 0;
            if (query.CouponTypeId.HasValue && query.CouponTypeId.Value < 0) query.CouponTypeId = 0;
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 10, 200);

            var source = from c in _context.Coupons.AsNoTracking()
                         join u in _context.Users.AsNoTracking() on c.UserId equals u.UserId into uj
                         from u in uj.DefaultIfEmpty()
                         join ct in _context.CouponTypes.AsNoTracking() on c.CouponTypeId equals ct.CouponTypeId into ctj
                         from ct in ctj.DefaultIfEmpty()
                         select new { c, u, ct };

            // 模糊搜尋：UserId 或 SearchTerm（聯集OR邏輯，使用 FuzzySearchService）
            var hasUserId = query.UserId.HasValue;
            var hasSearchTerm = !string.IsNullOrWhiteSpace(query.SearchTerm);

            List<int> matchedUserIds = new List<int>();
            Dictionary<int, int> userPriority = new Dictionary<int, int>();

            if (hasUserId || hasSearchTerm)
            {
                var userIdStr = hasUserId ? query.UserId.Value.ToString() : "";
                var searchTerm = hasSearchTerm ? query.SearchTerm!.Trim() : "";

                // 查詢所有用戶並使用 FuzzySearchService 計算優先順序
                var allUsers = await _context.Users
                    .AsNoTracking()
                    .Select(u => new { u.UserId, u.UserAccount, u.UserName })
                    .ToListAsync();

                foreach (var user in allUsers)
                {
                    int priority = 0;

                    // 如果有 UserId 條件，優先匹配 UserId
                    if (hasUserId)
                    {
                        if (user.UserId == query.UserId.Value)
                        {
                            priority = 1; // 完全匹配 UserId
                        }
                        else if (user.UserId.ToString().Contains(userIdStr))
                        {
                            priority = 2; // 部分匹配 UserId
                        }
                    }

                    // 如果有 SearchTerm 條件，使用 FuzzySearchService
                    if (hasSearchTerm && priority == 0)
                    {
                        priority = _fuzzySearchService.CalculateMatchPriority(
                            searchTerm,
                            user.UserAccount,
                            user.UserName
                        );
                    }

                    // 如果匹配成功（priority > 0），加入結果
                    if (priority > 0)
                    {
                        matchedUserIds.Add(user.UserId);
                        userPriority[user.UserId] = priority;
                    }
                }

                source = source.Where(x => matchedUserIds.Contains(x.u.UserId) ||
                                           (hasSearchTerm && x.c.CouponCode.Contains(searchTerm)));
            }

            if (query.CouponTypeId.HasValue)
            {
                source = source.Where(x => x.c.CouponTypeId == query.CouponTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var now = _appClock.UtcNow;
                source = query.Status.ToLowerInvariant() switch
                {
                    "used" => source.Where(x => x.c.IsUsed),
                    "unused" => source.Where(x => !x.c.IsUsed && (x.ct == null || x.ct.ValidTo >= now)),
                    "expired" => source.Where(x => !x.c.IsUsed && x.ct != null && x.ct.ValidTo < now),
                    _ => source
                };
            }

            var totalCount = await source.CountAsync();

            // 優先順序排序：先取資料再排序（避免 EF 無法轉換 dynamic）
            var allItems = await source.ToListAsync();
            var items = allItems;

            if (hasUserId || hasSearchTerm)
            {
                var searchTerm = hasSearchTerm ? query.SearchTerm!.Trim() : "";

                // 在記憶體中進行優先順序排序（使用 userPriority 字典）
                var ordered = allItems.OrderBy(x =>
                {
                    // 如果用戶匹配，返回對應優先順序
                    if (x.u != null && userPriority.ContainsKey(x.u.UserId))
                    {
                        return userPriority[x.u.UserId];
                    }
                    // 如果優惠券代碼匹配
                    if (hasSearchTerm && x.c.CouponCode.Contains(searchTerm))
                    {
                        return 5;
                    }
                    return 99;
                });

                // 次要排序
                var sorted = query.SortBy?.ToLowerInvariant() switch
                {
                    "acquiredtime" => query.Descending ? ordered.ThenByDescending(x => x.c.AcquiredTime) : ordered.ThenBy(x => x.c.AcquiredTime),
                    "usetime" => query.Descending ? ordered.ThenByDescending(x => x.c.UsedTime) : ordered.ThenBy(x => x.c.UsedTime),
                    _ => ordered.ThenByDescending(x => x.c.AcquiredTime)
                };

                items = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                // 沒有搜尋條件時使用資料庫排序
                var sorted = query.SortBy?.ToLowerInvariant() switch
                {
                    "acquiredtime" => query.Descending ? source.OrderByDescending(x => x.c.AcquiredTime) : source.OrderBy(x => x.c.AcquiredTime),
                    "usetime" => query.Descending ? source.OrderByDescending(x => x.c.UsedTime) : source.OrderBy(x => x.c.UsedTime),
                    _ => source.OrderByDescending(x => x.c.AcquiredTime)
                };

                items = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            var records = items.Select(x => new UserCouponReadModel
            {
                CouponId = x.c.CouponId,
                CouponCode = x.c.CouponCode,
                UserId = x.c.UserId,
                UserName = x.u?.UserName ?? string.Empty,
                Email = string.Empty, // 若需 Email 可額外查 UserIntroduces
                CouponTypeId = x.c.CouponTypeId,
                CouponTypeName = x.ct?.Name ?? string.Empty,
                DiscountAmount = x.ct?.DiscountValue ?? 0,
                DiscountPercentage = x.ct?.DiscountType == "Percentage" ? x.ct?.DiscountValue : null,
                MinimumPurchase = x.ct?.MinSpend,
                AcquiredTime = x.c.AcquiredTime,
                UsedTime = x.c.UsedTime,
                ExpiryDate = x.ct?.ValidTo,
                IsUsed = x.c.IsUsed
            }).ToList();

            // 計算統計數據 - 從篩選後的 source 計算（而不是從全表）
            var nowTime = _appClock.UtcNow;

            // 使用已經篩選過的 allItems 來計算統計數據
            var totalCoupons = totalCount;

            // 已使用的優惠券
            var usedCount = allItems.Count(x => x.c.IsUsed);

            // 未使用的優惠券（未過期）
            var unusedButNotExpiredCount = allItems.Count(x => !x.c.IsUsed && (x.ct == null || x.ct.ValidTo >= nowTime));

            // 已過期的未使用優惠券
            var expiredCount = allItems.Count(x => !x.c.IsUsed && x.ct != null && x.ct.ValidTo < nowTime);

            var unusedCount = unusedButNotExpiredCount + expiredCount;

            // 動態讀取優惠券類型列表（用於下拉菜單）
            var couponTypeList = await _context.CouponTypes
                .AsNoTracking()
                .OrderBy(ct => ct.CouponTypeId)
                .Select(ct => new CouponTypeOption
                {
                    CouponTypeId = ct.CouponTypeId,
                    Name = ct.Name
                })
                .ToListAsync();

            var model = new WalletCouponsQueryViewModel
            {
                Query = query,
                Results = new PagedResult<UserCouponReadModel>
                {
                    Items = records,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                },
                TotalCoupons = totalCoupons,
                UnusedCount = unusedCount,
                UsedCount = usedCount,
                ExpiredCount = expiredCount,
                CouponTypeList = couponTypeList
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EVouchersQuery([FromQuery] EVoucherQueryModel query)
        {
            query ??= new EVoucherQueryModel();
            // 伺服器端保護：避免負數參數
            if (query.UserId.HasValue && query.UserId.Value < 0) query.UserId = 0;
            if (query.EVoucherTypeId.HasValue && query.EVoucherTypeId.Value < 0) query.EVoucherTypeId = 0;
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 10, 200);

            var source = from e in _context.Evouchers.AsNoTracking()
                         join u in _context.Users.AsNoTracking() on e.UserId equals u.UserId into uj
                         from u in uj.DefaultIfEmpty()
                         join et in _context.EvoucherTypes.AsNoTracking() on e.EvoucherTypeId equals et.EvoucherTypeId into etj
                         from et in etj.DefaultIfEmpty()
                         select new { e, u, et };

            // 模糊搜尋：UserId 或 SearchTerm（聯集OR邏輯，使用 FuzzySearchService）
            var hasUserId = query.UserId.HasValue;
            var hasSearchTerm = !string.IsNullOrWhiteSpace(query.SearchTerm);

            List<int> matchedUserIds = new List<int>();
            Dictionary<int, int> userPriority = new Dictionary<int, int>();

            if (hasUserId || hasSearchTerm)
            {
                var userIdStr = hasUserId ? query.UserId.Value.ToString() : "";
                var searchTerm = hasSearchTerm ? query.SearchTerm!.Trim() : "";

                // 查詢所有用戶並使用 FuzzySearchService 計算優先順序
                var allUsers = await _context.Users
                    .AsNoTracking()
                    .Select(u => new { u.UserId, u.UserAccount, u.UserName })
                    .ToListAsync();

                foreach (var user in allUsers)
                {
                    int priority = 0;

                    // 如果有 UserId 條件，優先匹配 UserId
                    if (hasUserId)
                    {
                        if (user.UserId == query.UserId.Value)
                        {
                            priority = 1; // 完全匹配 UserId
                        }
                        else if (user.UserId.ToString().Contains(userIdStr))
                        {
                            priority = 2; // 部分匹配 UserId
                        }
                    }

                    // 如果有 SearchTerm 條件，使用 FuzzySearchService
                    if (hasSearchTerm && priority == 0)
                    {
                        priority = _fuzzySearchService.CalculateMatchPriority(
                            searchTerm,
                            user.UserAccount,
                            user.UserName
                        );
                    }

                    // 如果匹配成功（priority > 0），加入結果
                    if (priority > 0)
                    {
                        matchedUserIds.Add(user.UserId);
                        userPriority[user.UserId] = priority;
                    }
                }

                source = source.Where(x => matchedUserIds.Contains(x.u.UserId) ||
                                           (hasSearchTerm && x.e.EvoucherCode.Contains(searchTerm)));
            }

            if (query.EVoucherTypeId.HasValue)
            {
                source = source.Where(x => x.e.EvoucherTypeId == query.EVoucherTypeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.TypeCode))
            {
                source = source.Where(x => x.et != null && x.et.Name.Contains(query.TypeCode));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var now2 = _appClock.UtcNow;
                source = query.Status.ToLowerInvariant() switch
                {
                    "used" => source.Where(x => x.e.IsUsed),
                    "unused" => source.Where(x => !x.e.IsUsed && x.et != null && x.et.ValidTo >= now2),
                    "expired" => source.Where(x => !x.e.IsUsed && x.et != null && x.et.ValidTo < now2),
                    _ => source
                };
            }

            var totalCount = await source.CountAsync();

            // 優先順序排序：先取資料再排序（避免 EF 無法轉換 dynamic）
            var allItems = await source.ToListAsync();
            var items = allItems;

            if (hasUserId || hasSearchTerm)
            {
                var userIdStr = hasUserId ? query.UserId.Value.ToString() : "";
                var term = hasSearchTerm ? query.SearchTerm!.Trim() : "";

                // 在記憶體中進行優先順序排序
                var ordered = allItems.OrderBy(x =>
                {
                    if (hasUserId && x.u != null && x.u.UserId == query.UserId.Value) return 1;
                    if (hasUserId && x.u != null && x.u.UserId.ToString().Contains(userIdStr)) return 2;
                    if (hasSearchTerm && x.u != null && x.u.UserAccount.Contains(term)) return 3;
                    if (hasSearchTerm && x.u != null && x.u.UserName.Contains(term)) return 4;
                    if (hasSearchTerm && x.e.EvoucherCode.Contains(term)) return 5;
                    return 6;
                });

                // 次要排序
                var sorted = query.SortBy?.ToLowerInvariant() switch
                {
                    "acquiredtime" => query.Descending ? ordered.ThenByDescending(x => x.e.AcquiredTime) : ordered.ThenBy(x => x.e.AcquiredTime),
                    "validto" => query.Descending ? ordered.ThenByDescending(x => x.et != null ? x.et.ValidTo : DateTime.MinValue) : ordered.ThenBy(x => x.et != null ? x.et.ValidTo : DateTime.MinValue),
                    _ => ordered.ThenByDescending(x => x.e.AcquiredTime)
                };

                items = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                // 沒有搜尋條件時使用資料庫排序
                var sorted = query.SortBy?.ToLowerInvariant() switch
                {
                    "acquiredtime" => query.Descending ? source.OrderByDescending(x => x.e.AcquiredTime) : source.OrderBy(x => x.e.AcquiredTime),
                    "validto" => query.Descending ? source.OrderByDescending(x => x.et != null ? x.et.ValidTo : DateTime.MinValue) : source.OrderBy(x => x.et != null ? x.et.ValidTo : DateTime.MinValue),
                    _ => source.OrderByDescending(x => x.e.AcquiredTime)
                };

                items = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            var records = items.Select(x => new Models.ViewModels.EVoucherReadModel
            {
                EVoucherId = x.e.EvoucherId,
                EVoucherCode = x.e.EvoucherCode,
                UserId = x.e.UserId,
                UserName = x.u?.UserName ?? string.Empty,
                UserEmail = string.Empty,
                EVoucherTypeId = x.e.EvoucherTypeId,
                EVoucherTypeName = x.et?.Name ?? string.Empty,
                VoucherValue = x.et?.ValueAmount ?? 0,
                MerchantName = x.et?.Description,
                IsUsed = x.e.IsUsed,
                AcquiredTime = x.e.AcquiredTime,
                UsedTime = x.e.UsedTime,
                ValidFrom = x.et?.ValidFrom ?? DateTime.MinValue,
                ValidTo = x.et?.ValidTo ?? DateTime.MinValue,
                UsedLocation = null
            }).ToList();

            // 計算統計數據 - 從篩選後的 allItems 計算（而不是從全表）
            var nowEVoucher = _appClock.UtcNow;

            // 使用已經篩選過的 allItems 來計算統計數據
            var totalEvouchers = totalCount;
            var unusedEVoucherCount = allItems.Count(x => !x.e.IsUsed && (x.et == null || x.et.ValidTo >= nowEVoucher));
            var usedEVoucherCount = allItems.Count(x => x.e.IsUsed);
            var expiredEVoucherCount = allItems.Count(x => !x.e.IsUsed && x.et != null && x.et.ValidTo < nowEVoucher);

            var model = new WalletEVouchersQueryViewModel
            {
                Query = query,
                Results = new PagedResult<Models.ViewModels.EVoucherReadModel>
                {
                    Items = records,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                },
                TotalEVouchers = totalEvouchers,
                UnusedCount = unusedEVoucherCount,
                UsedCount = usedEVoucherCount,
                ExpiredCount = expiredEVoucherCount
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> WalletHistory([FromQuery] WalletHistoryQueryModel query)
        {
            query ??= new WalletHistoryQueryModel();
            var page = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 10, 200);

            var source = _context.WalletHistories
                .AsNoTracking()
                .AsQueryable();

            // 模糊搜尋：UserId 或 SearchTerm（聯集OR邏輯）
            var hasUserId = query.UserId.HasValue;
            var hasSearchTerm = !string.IsNullOrWhiteSpace(query.SearchTerm);

            List<int> matchedUserIds = new List<int>();
            if (hasUserId || hasSearchTerm)
            {
                var userIdStr = hasUserId ? query.UserId.Value.ToString() : "";
                var term = hasSearchTerm ? query.SearchTerm!.Trim() : "";

                // 查詢所有符合條件的用戶（OR邏輯）
                var matchedUsers = await _context.Users
                    .AsNoTracking()
                    .Where(u =>
                        (hasUserId && u.UserId.ToString().Contains(userIdStr)) ||
                        (hasSearchTerm && (u.UserAccount.Contains(term) || u.UserName.Contains(term))))
                    .Select(u => new { u.UserId, u.UserAccount, u.UserName })
                    .ToListAsync();

                matchedUserIds = matchedUsers.Select(u => u.UserId).Distinct().ToList();

                // 應用用戶ID篩選或描述搜尋（OR邏輯）
                source = source.Where(h =>
                    matchedUserIds.Contains(h.UserId) ||
                    (hasSearchTerm && h.Description != null && h.Description.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(query.ChangeType))
            {
                source = source.Where(h => h.ChangeType == query.ChangeType);
            }

            if (query.StartDate.HasValue)
            {
                source = source.Where(h => h.ChangeTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                source = source.Where(h => h.ChangeTime <= query.EndDate.Value);
            }

            var totalCount = await source.CountAsync();

            // 計算統計數據（基於所有符合條件的記錄，而非分頁結果）
            var allResults = await source.ToListAsync();

            // 優先順序排序：先取資料再排序（避免 EF 無法處理複雜 lambda）
            List<WalletHistory> sortedResults;
            if (hasUserId || hasSearchTerm)
            {
                var userIdStr = hasUserId ? query.UserId.Value.ToString() : "";
                var term = hasSearchTerm ? query.SearchTerm!.Trim() : "";

                var userPriority = await _context.Users
                    .AsNoTracking()
                    .Where(u => matchedUserIds.Contains(u.UserId))
                    .Select(u => new
                    {
                        u.UserId,
                        Priority = hasUserId && u.UserId == query.UserId.Value ? 1 :
                                   hasUserId && u.UserId.ToString().Contains(userIdStr) ? 2 :
                                   hasSearchTerm && u.UserAccount.Contains(term) ? 3 :
                                   hasSearchTerm && u.UserName.Contains(term) ? 4 : 5
                    })
                    .ToDictionaryAsync(x => x.UserId, x => x.Priority);

                // 在記憶體中進行優先順序排序
                var ordered = allResults.OrderBy(h =>
                {
                    if (userPriority.ContainsKey(h.UserId))
                        return userPriority[h.UserId];
                    if (hasSearchTerm && h.Description != null && h.Description.Contains(term))
                        return 5;
                    return 6;
                }).ThenByDescending(h => h.ChangeTime);

                sortedResults = ordered.ToList();
            }
            else
            {
                // 沒有搜尋條件時的正常排序
                sortedResults = allResults.OrderByDescending(h => h.ChangeTime).ToList();
            }

            allResults = sortedResults;
            var totalIncome = allResults.Where(h => h.PointsChanged > 0).Sum(h => (long)h.PointsChanged);
            var totalExpense = allResults.Where(h => h.PointsChanged < 0).Sum(h => (long)Math.Abs(h.PointsChanged));
            var netChange = totalIncome - totalExpense;

            // 取得分頁的項目
            var items = allResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // 手動載入用戶資料
            var lookupIds = items.Select(h => h.UserId).Distinct().ToList();
            var userLookup = await _context.Users
                .AsNoTracking()
                .Where(u => lookupIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => new { u.UserAccount, u.UserName });

            var walletLookup = await _context.UserWallets
                .AsNoTracking()
                .Where(w => lookupIds.Contains(w.UserId))
                .ToDictionaryAsync(w => w.UserId, w => w.UserPoint);

            var records = items.Select(h =>
            {
                userLookup.TryGetValue(h.UserId, out var userData);
                walletLookup.TryGetValue(h.UserId, out var balance);

                return new WalletHistoryRecord
                {
                    LogId = h.LogId,
                    UserId = h.UserId,
                    UserAccount = userData?.UserAccount ?? string.Empty,
                    UserName = userData?.UserName ?? string.Empty,
                    ChangeType = h.ChangeType,
                    PointsChanged = h.PointsChanged,
                    BalanceAfter = balance,
                    Description = h.Description,
                    ChangeTime = h.ChangeTime
                };
            }).ToList();

            var model = new WalletHistoryViewModel
            {
                Query = query,
                Results = new PagedResult<WalletHistoryRecord>
                {
                    Items = records,
                    TotalCount = totalCount,
                    CurrentPage = page,
                    PageSize = pageSize
                },
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetChange = netChange
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GrantPoints()
        {
            var candidates = await _context.UserWallets
                .AsNoTracking()
                .OrderByDescending(w => w.UserPoint)
                .Take(20)
                .ToListAsync();

            var ids = candidates.Select(c => c.UserId).Distinct().ToList();
            var userLookup = await _context.Users
                .AsNoTracking()
                .Where(u => ids.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => new { u.UserAccount, u.UserName });
            var introduceLookup = await _context.UserIntroduces
                .AsNoTracking()
                .Where(ui => ids.Contains(ui.UserId))
                .ToDictionaryAsync(ui => ui.UserId, ui => ui.Email);

            var model = new WalletGrantPointsViewModel
            {
                Candidates = candidates.Select(w =>
                {
                    userLookup.TryGetValue(w.UserId, out var u);
                    introduceLookup.TryGetValue(w.UserId, out var email);
                    return new WalletPointRecord
                    {
                        UserId = w.UserId,
                        UserAccount = u?.UserAccount ?? string.Empty,
                        UserName = u?.UserName ?? string.Empty,
                        Email = email ?? string.Empty,
                        Points = w.UserPoint
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantPoints(int userId, int pointsToGrant, string reason)
        {
            if (userId < 0)
            {
                TempData["Error"] = "會員 ID 不可為負數";
                return RedirectToAction(nameof(GrantPoints));
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "請填寫發放原因";
                return RedirectToAction(nameof(GrantPoints));
            }

            var wallet = await _context.UserWallets.FindAsync(userId);
            if (wallet == null)
            {
                TempData["Error"] = "找不到該會員的錢包";
                return RedirectToAction(nameof(GrantPoints));
            }

            if (pointsToGrant < 0 && wallet.UserPoint + pointsToGrant < 0)
            {
                TempData["Error"] = $"會員點數不足。目前點數：{wallet.UserPoint}，欲扣除：{-pointsToGrant}";
                return RedirectToAction(nameof(GrantPoints));
            }

            if (pointsToGrant == 0)
            {
                TempData["Success"] = "未變更點數（輸入為 0）";
                return RedirectToAction(nameof(GrantPoints));
            }

            // 使用交易確保數據一致性
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 更新會員點數（可正負）
                wallet.UserPoint += pointsToGrant;

                // 記錄異動歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Point",
                    PointsChanged = pointsToGrant,
                    Description = reason,
                    ChangeTime = _appClock.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (pointsToGrant > 0)
                {
                    TempData["Success"] = $"成功發放 {pointsToGrant} 點數給會員 ID {userId}";
                }
                else
                {
                    TempData["Success"] = $"成功扣除會員 ID {userId} 的 {-pointsToGrant} 點數。剩餘點數：{wallet.UserPoint}";
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"調整點數失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(GrantPoints));
        }

        [HttpGet]
        public async Task<IActionResult> GrantCoupon()
        {
            var summaries = await _context.CouponTypes
                .AsNoTracking()
                .Select(ct => new CouponTypeSummary
                {
                    CouponTypeId = ct.CouponTypeId,
                    TypeName = ct.Name,
                    Description = ct.Description,
                    TotalIssued = ct.Coupons.Count,
                    UnusedCount = ct.Coupons.Count(c => !c.IsUsed)
                })
                .OrderBy(ct => ct.TypeName)
                .ToListAsync();

            var model = new WalletGrantCouponViewModel
            {
                CouponTypes = summaries
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GrantCoupon(int userId, int couponTypeId)
        {
            // 驗證會員存在
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                TempData["Error"] = $"找不到會員 ID {userId}";
                return RedirectToAction(nameof(GrantCoupon));
            }

            // 驗證優惠券類型存在
            var couponType = await _context.CouponTypes.FindAsync(couponTypeId);
            if (couponType == null)
            {
                TempData["Error"] = "找不到該優惠券類型";
                return RedirectToAction(nameof(GrantCoupon));
            }

            // 使用 UTC 時間儲存到資料庫
            var nowUtc = _appClock.UtcNow;
            var nowTaiwanTime = _appClock.ToAppTime(nowUtc);

            // 生成優惠券序號 CPN-YYYYMM-XXXXXX（使用台灣時間格式化）
            var random = new Random();
            var randomCode = new string(Enumerable.Range(0, 6)
                .Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[random.Next(36)])
                .ToArray());
            var couponCode = $"CPN-{nowTaiwanTime:yyMM}-{randomCode}";

            // 使用交易確保數據一致性
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 創建優惠券（資料庫時間使用 UTC）
                var coupon = new Coupon
                {
                    CouponCode = couponCode,
                    CouponTypeId = couponTypeId,
                    UserId = userId,
                    IsUsed = false,
                    AcquiredTime = nowUtc,  // 使用 UTC 時間
                    UsedTime = null,  // 明確設為 null (剛發放時未使用)
                    IsDeleted = false  // 必填字段：未刪除
                };
                _context.Coupons.Add(coupon);

                // 記錄異動歷史（資料庫時間使用 UTC）
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Coupon",
                    PointsChanged = 0,
                    ItemCode = couponCode,
                    Description = $"發放商城優惠券：{couponType.Name}",
                    ChangeTime = nowUtc  // 使用 UTC 時間
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"成功發放優惠券「{couponType.Name}」給會員 ID {userId}，序號：{couponCode}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"發放優惠券失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(GrantCoupon));
        }

        [HttpGet]
        public async Task<IActionResult> AdjustEVoucher()
        {
            var summaries = await _context.EvoucherTypes
                .AsNoTracking()
                .Select(et => new EVoucherTypeSummary
                {
                    EVoucherTypeId = et.EvoucherTypeId,
                    TypeName = et.Name,
                    MerchantName = et.Description,
                    VoucherValue = et.ValueAmount,
                    TotalIssued = et.Evouchers.Count,
                    UnusedCount = et.Evouchers.Count(ev => !ev.IsUsed)
                })
                .OrderBy(et => et.TypeName)
                .ToListAsync();

            var model = new WalletAdjustEVoucherViewModel
            {
                EVoucherTypes = summaries
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustEVoucher(int userId, int evoucherTypeId, string action)
        {
            // 驗證會員存在
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                TempData["Error"] = $"找不到會員 ID {userId}";
                return RedirectToAction(nameof(AdjustEVoucher));
            }

            // 驗證電子禮券類型存在
            var evoucherType = await _context.EvoucherTypes.FindAsync(evoucherTypeId);
            if (evoucherType == null)
            {
                TempData["Error"] = "找不到該電子禮券類型";
                return RedirectToAction(nameof(AdjustEVoucher));
            }

            // 使用台灣時間 (Asia/Taipei)
            var nowUtc = _appClock.UtcNow;
            var nowTaiwanTime = _appClock.ToAppTime(nowUtc);

            // 使用交易確保數據一致性
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (action == "grant")
                {
                    // 發放電子禮券 - 生成序號 EV-{類型}-{4位隨機碼}-{6位數字}
                    var random = new Random();
                    var randomCode = new string(Enumerable.Range(0, 4)
                        .Select(_ => "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[random.Next(26)])
                        .ToArray());
                    var randomDigits = random.Next(0, 1000000).ToString("D6");

                    // 從類型名稱提取類型代碼
                    string typeCode = evoucherType.Name.Contains("現金") ? "CASH" :
                                      evoucherType.Name.Contains("電影") ? "MOVIE" :
                                      evoucherType.Name.Contains("餐") ? "FOOD" :
                                      evoucherType.Name.Contains("加油") ? "GAS" :
                                      evoucherType.Name.Contains("咖啡") ? "COFFEE" : "STORE";

                    var evoucherCode = $"EV-{typeCode}-{randomCode}-{randomDigits}";

                    // 創建電子禮券
                    var evoucher = new Evoucher
                    {
                        EvoucherCode = evoucherCode,
                        EvoucherTypeId = evoucherTypeId,
                        UserId = userId,
                        IsUsed = false,
                        AcquiredTime = nowTaiwanTime,  // 使用台灣時間
                        UsedTime = null,  // 明確設為 null (剛發放時未使用)
                        IsDeleted = false  // 必填字段：未刪除
                    };
                    _context.Evouchers.Add(evoucher);

                    // 記錄異動歷史
                    var history = new WalletHistory
                    {
                        UserId = userId,
                        ChangeType = "EVoucher",
                        PointsChanged = 0,
                        ItemCode = evoucherCode,
                        Description = $"發放電子禮券：{evoucherType.Name}",
                        ChangeTime = nowTaiwanTime  // 使用台灣時間
                    };
                    _context.WalletHistories.Add(history);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"成功發放電子禮券「{evoucherType.Name}」給會員 ID {userId}，序號：{evoucherCode}";
                }
                else if (action == "revoke")
                {
                    // 撤銷電子禮券 - 找到該會員最近一張未使用的該類型禮券
                    var evoucherToRevoke = await _context.Evouchers
                        .Where(ev => ev.UserId == userId && ev.EvoucherTypeId == evoucherTypeId && !ev.IsUsed)
                        .OrderByDescending(ev => ev.AcquiredTime)
                        .FirstOrDefaultAsync();

                    if (evoucherToRevoke == null)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "找不到可撤銷的電子禮券";
                        return RedirectToAction(nameof(AdjustEVoucher));
                    }

                    _context.Evouchers.Remove(evoucherToRevoke);

                    // 記錄異動歷史
                    var history = new WalletHistory
                    {
                        UserId = userId,
                        ChangeType = "EVoucher",
                        PointsChanged = 0,
                        ItemCode = evoucherToRevoke.EvoucherCode,
                        Description = $"撤銷電子禮券：{evoucherType.Name}",
                        ChangeTime = nowTaiwanTime  // 使用台灣時間
                    };
                    _context.WalletHistories.Add(history);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = $"成功撤銷會員 ID {userId} 的電子禮券「{evoucherType.Name}」，序號：{evoucherToRevoke.EvoucherCode}";
                }
                else
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "無效的操作";
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"操作電子禮券失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(AdjustEVoucher));
        }

        [HttpGet]
        public async Task<IActionResult> DeductPoints()
        {
            var candidates = await _context.UserWallets
                .AsNoTracking()
                .Where(w => w.UserPoint > 0) // 只顯示有點數的會員
                .OrderByDescending(w => w.UserPoint)
                .Take(20)
                .ToListAsync();

            var ids2 = candidates.Select(c => c.UserId).Distinct().ToList();
            var userLookup2 = await _context.Users
                .AsNoTracking()
                .Where(u => ids2.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => new { u.UserAccount, u.UserName });
            var introduceLookup2 = await _context.UserIntroduces
                .AsNoTracking()
                .Where(ui => ids2.Contains(ui.UserId))
                .ToDictionaryAsync(ui => ui.UserId, ui => ui.Email);

            var model = new WalletDeductPointsViewModel
            {
                Candidates = candidates.Select(w =>
                {
                    userLookup2.TryGetValue(w.UserId, out var u);
                    introduceLookup2.TryGetValue(w.UserId, out var email);
                    return new WalletPointRecord
                    {
                        UserId = w.UserId,
                        UserAccount = u?.UserAccount ?? string.Empty,
                        UserName = u?.UserName ?? string.Empty,
                        Email = email ?? string.Empty,
                        Points = w.UserPoint
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeductPoints(int userId, int pointsToDeduct, string reason)
        {
            if (pointsToDeduct <= 0)
            {
                TempData["Error"] = "扣除點數必須大於 0";
                return RedirectToAction(nameof(DeductPoints));
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "請填寫扣除原因";
                return RedirectToAction(nameof(DeductPoints));
            }

            var wallet = await _context.UserWallets.FindAsync(userId);
            if (wallet == null)
            {
                TempData["Error"] = "找不到該會員的錢包";
                return RedirectToAction(nameof(DeductPoints));
            }

            if (wallet.UserPoint < pointsToDeduct)
            {
                TempData["Error"] = $"會員點數不足。目前點數：{wallet.UserPoint}，欲扣除：{pointsToDeduct}";
                return RedirectToAction(nameof(DeductPoints));
            }

            // 使用交易確保數據一致性
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 扣除會員點數
                wallet.UserPoint -= pointsToDeduct;

                // 記錄異動歷史（負值）
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Point",
                    PointsChanged = -pointsToDeduct, // 負值表示扣除
                    Description = reason,
                    ChangeTime = _appClock.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"成功扣除會員 ID {userId} 的 {pointsToDeduct} 點數。剩餘點數：{wallet.UserPoint}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"扣除點數失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(DeductPoints));
        }

        [HttpGet]
        public async Task<IActionResult> RevokeCoupon()
        {
            // 取得最近發放的優惠券（未使用）
            var recentCoupons = await _context.Coupons
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.CouponType)
                .Where(c => !c.IsUsed)
                .OrderByDescending(c => c.AcquiredTime)
                .Take(50)
                .ToListAsync();

            var records = recentCoupons.Select(c => new UserCouponReadModel
            {
                CouponId = c.CouponId,
                CouponCode = c.CouponCode,
                UserId = c.UserId,
                UserName = c.User?.UserName ?? string.Empty,
                Email = c.User?.UserIntroduce?.Email ?? string.Empty,
                CouponTypeId = c.CouponTypeId,
                CouponTypeName = c.CouponType?.Name ?? string.Empty,
                DiscountAmount = c.CouponType?.DiscountValue ?? 0,
                DiscountPercentage = c.CouponType?.DiscountType == "Percentage" ? c.CouponType?.DiscountValue : null,
                MinimumPurchase = c.CouponType?.MinSpend,
                AcquiredTime = c.AcquiredTime,
                UsedTime = c.UsedTime,
                ExpiryDate = c.CouponType?.ValidTo,
                IsUsed = c.IsUsed
            }).ToList();

            var model = new WalletRevokeCouponViewModel
            {
                UnusedCoupons = records
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeCoupon(int couponId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "請填寫撤銷原因";
                return RedirectToAction(nameof(RevokeCoupon));
            }

            var coupon = await _context.Coupons
                .Include(c => c.CouponType)
                .FirstOrDefaultAsync(c => c.CouponId == couponId);

            if (coupon == null)
            {
                TempData["Error"] = "找不到該優惠券";
                return RedirectToAction(nameof(RevokeCoupon));
            }

            if (coupon.IsUsed)
            {
                TempData["Error"] = "該優惠券已被使用，無法撤銷";
                return RedirectToAction(nameof(RevokeCoupon));
            }

            var couponCode = coupon.CouponCode;
            var userId = coupon.UserId;
            var couponTypeName = coupon.CouponType?.Name ?? "未知類型";

            // 使用交易確保數據一致性
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 刪除優惠券
                _context.Coupons.Remove(coupon);

                // 記錄異動歷史
                var history = new WalletHistory
                {
                    UserId = userId,
                    ChangeType = "Coupon",
                    PointsChanged = 0,
                    ItemCode = couponCode,
                    Description = $"撤銷商城優惠券：{couponTypeName}。原因：{reason}",
                    ChangeTime = _appClock.UtcNow
                };
                _context.WalletHistories.Add(history);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"成功撤銷會員 ID {userId} 的優惠券「{couponTypeName}」，序號：{couponCode}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"撤銷優惠券失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(RevokeCoupon));
        }
    }
}
