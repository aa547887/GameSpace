using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class SignInAdminController : MiniGameBaseController
    {
        private readonly IInMemorySignInRuleService _ruleService;
        private readonly IFuzzySearchService _fuzzySearchService;

        public SignInAdminController(
            GameSpacedatabaseContext context,
            IInMemorySignInRuleService ruleService,
            IFuzzySearchService fuzzySearchService)
            : base(context)
        {
            _ruleService = ruleService;
            _fuzzySearchService = fuzzySearchService;
        }

        [HttpGet]
        public async Task<IActionResult> RuleSettings()
        {
            var rules = await _ruleService.GetAllRulesAsync();

            // 動態查詢 CouponTypes（不可硬編碼）
            var couponTypes = await _context.CouponTypes
                .AsNoTracking()
                .Where(ct => !ct.IsDeleted)
                .OrderBy(ct => ct.CouponTypeId)
                .Select(ct => new { ct.CouponTypeId, ct.Name })
                .ToListAsync();

            ViewBag.CouponTypes = couponTypes;

            // 建立 Name → CouponTypeId 的映射（用於 JavaScript）
            var couponTypeMapping = couponTypes.ToDictionary(ct => ct.Name, ct => ct.CouponTypeId);
            ViewBag.CouponTypeMapping = System.Text.Json.JsonSerializer.Serialize(couponTypeMapping);

            var model = new SignInRuleSettingsViewModel
            {
                Rules = rules.OrderBy(r => r.DayNumber).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRule(int ruleId, int points, int experience, bool hasCoupon, int? couponTypeId, bool isActive, string? description)
        {
            if (points < 0 || experience < 0)
            {
                TempData["Error"] = "點數和經驗值不能為負數";
                return RedirectToAction(nameof(RuleSettings));
            }

            // 將 CouponTypeId 映射為 CouponType.Name（因為資料庫 FK 使用 Name）
            string? couponTypeCode = null;
            if (hasCoupon && couponTypeId.HasValue)
            {
                var couponType = await _context.CouponTypes
                    .AsNoTracking()
                    .Where(ct => ct.CouponTypeId == couponTypeId.Value && !ct.IsDeleted)
                    .Select(ct => ct.Name)
                    .FirstOrDefaultAsync();

                if (couponType == null)
                {
                    TempData["Error"] = "找不到指定的優惠券類型";
                    return RedirectToAction(nameof(RuleSettings));
                }

                couponTypeCode = couponType;
            }

            // 使用記憶體服務更新規則
            var success = await _ruleService.UpdateRuleAsync(ruleId, points, experience, hasCoupon, couponTypeCode, isActive, description);

            if (!success)
            {
                TempData["Error"] = "找不到該簽到規則";
                return RedirectToAction(nameof(RuleSettings));
            }

            var rule = await _ruleService.GetRuleByIdAsync(ruleId);
            TempData["Success"] = $"成功更新第 {rule?.DayNumber} 天的簽到規則";
            return RedirectToAction(nameof(RuleSettings));
        }

        /// <summary>
        /// 快速切換規則啟用/停用狀態
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int ruleId)
        {
            var rule = await _ruleService.GetRuleByIdAsync(ruleId);
            if (rule == null)
            {
                TempData["Error"] = "找不到該簽到規則";
                return RedirectToAction(nameof(RuleSettings));
            }

            // 切換狀態
            var newStatus = !rule.IsActive;
            var success = await _ruleService.UpdateRuleAsync(
                rule.Id,
                rule.Points,
                rule.Experience,
                rule.HasCoupon,
                rule.CouponTypeCode,
                newStatus,
                rule.Description
            );

            if (success)
            {
                var statusText = newStatus ? "啟用" : "停用";
                TempData["Success"] = $"成功{statusText}第 {rule.DayNumber} 天的簽到規則";
            }
            else
            {
                TempData["Error"] = "更新規則狀態失敗";
            }

            return RedirectToAction(nameof(RuleSettings));
        }

        [HttpGet]
        public async Task<IActionResult> Records(
            int? userId = null,
            string? userAccount = null,
            string? userName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? minConsecutiveDays = null,
            int? maxConsecutiveDays = null,
            string? searchTerm = null,
            int page = 1,
            int pageSize = 20,
            string sortBy = "SignTime",
            bool descending = true)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 10, 100);

            // 建立查詢模型
            var query = new SignInRecordsQueryModel
            {
                UserId = userId,
                UserAccount = userAccount,
                UserName = userName,
                StartDate = startDate,
                EndDate = endDate,
                MinConsecutiveDays = minConsecutiveDays,
                MaxConsecutiveDays = maxConsecutiveDays,
                SearchTerm = searchTerm,
                PageNumber = page,
                PageSize = pageSize,
                SortBy = sortBy,
                Descending = descending
            };

            // 基礎查詢（不使用 .Include 避免 UserId1 錯誤）
            var baseQuery = _context.UserSignInStats
                .AsNoTracking()
                .Where(s => !s.IsDeleted);

            // 應用篩選條件
            if (userId.HasValue)
            {
                baseQuery = baseQuery.Where(s => s.UserId == userId.Value);
            }

            if (startDate.HasValue)
            {
                baseQuery = baseQuery.Where(s => s.SignTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                baseQuery = baseQuery.Where(s => s.SignTime <= endOfDay);
            }

            // 取得所有符合條件的記錄（用於統計和模糊搜尋）
            var allMatchingRecords = await baseQuery.ToListAsync();

            // 手動載入用戶資料
            var userIds = allMatchingRecords.Select(s => s.UserId).Distinct().ToList();
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId);

            // 手動載入優惠券類型資料（JOIN Coupon and CouponType）
            var couponCodes = allMatchingRecords
                .Where(s => !string.IsNullOrWhiteSpace(s.CouponGained))
                .Select(s => s.CouponGained)
                .Distinct()
                .ToList();

            var couponTypes = await _context.Coupons
                .AsNoTracking()
                .Where(c => couponCodes.Contains(c.CouponCode) && !c.IsDeleted)
                .Include(c => c.CouponType)
                .Where(c => !c.CouponType.IsDeleted)
                .Select(c => new { c.CouponCode, c.CouponType.Name, c.CouponTypeId })
                .Distinct()
                .ToDictionaryAsync(c => c.CouponCode, c => new { c.Name, c.CouponTypeId });

            // 轉換為 ViewModel（包含連續天數計算）
            var recordsWithUserData = allMatchingRecords.Select(s =>
            {
                users.TryGetValue(s.UserId, out var userData);
                couponTypes.TryGetValue(s.CouponGained ?? string.Empty, out var couponTypeData);

                // 計算連續天數（簡化版：從數據庫中同一用戶的記錄計算）
                var consecutiveDays = _context.UserSignInStats
                    .AsNoTracking()
                    .Where(x => x.UserId == s.UserId && x.SignTime <= s.SignTime && !x.IsDeleted)
                    .OrderByDescending(x => x.SignTime)
                    .ToList()
                    .TakeWhile((x, index) => index == 0 || (x.SignTime.Date == s.SignTime.Date.AddDays(-index)))
                    .Count();

                return new SignInRecordViewModel
                {
                    LogId = s.LogId,
                    RecordId = s.LogId,
                    UserId = s.UserId,
                    UserAccount = userData?.UserAccount ?? string.Empty,
                    UserName = userData?.UserName ?? string.Empty,
                    SignTime = s.SignTime,
                    SignInDate = s.SignTime.Date,
                    ConsecutiveDays = consecutiveDays,
                    PointsGained = s.PointsGained,
                    PointsEarned = s.PointsGained,
                    PointsRewarded = s.PointsGained,
                    ExpGained = s.ExpGained,
                    PetExpRewarded = s.ExpGained,
                    CouponCode = string.IsNullOrWhiteSpace(s.CouponGained) ? null : s.CouponGained,
                    CouponTypeName = couponTypeData?.Name,
                    CouponTypeId = couponTypeData?.CouponTypeId,
                    CreatedAt = s.SignTime
                };
            }).ToList();

            // 應用額外篩選條件（用戶帳號、用戶名稱）
            var filteredRecords = recordsWithUserData;

            if (!string.IsNullOrWhiteSpace(userAccount))
            {
                filteredRecords = filteredRecords
                    .Where(r => r.UserAccount.Contains(userAccount, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                filteredRecords = filteredRecords
                    .Where(r => r.UserName.Contains(userName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (minConsecutiveDays.HasValue)
            {
                filteredRecords = filteredRecords.Where(r => r.ConsecutiveDays >= minConsecutiveDays.Value).ToList();
            }

            if (maxConsecutiveDays.HasValue)
            {
                filteredRecords = filteredRecords.Where(r => r.ConsecutiveDays <= maxConsecutiveDays.Value).ToList();
            }

            // 應用模糊搜尋（5級優先順序）
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchResults = filteredRecords
                    .Select(r => new
                    {
                        Record = r,
                        Priority = _fuzzySearchService.CalculateMatchPriority(
                            searchTerm,
                            r.UserId.ToString(),
                            r.UserAccount,
                            r.UserName,
                            r.CouponCode,
                            r.CouponTypeName
                        )
                    })
                    .Where(x => x.Priority > 0)
                    .OrderBy(x => x.Priority)
                    .Select(x => x.Record)
                    .ToList();

                filteredRecords = searchResults;
            }

            // 計算統計數據（基於篩選後的結果）
            var totalFilteredCount = filteredRecords.Count;
            var totalPoints = filteredRecords.Sum(r => (long)r.PointsGained);
            var totalExp = filteredRecords.Sum(r => (long)r.ExpGained);
            var totalCoupons = filteredRecords.Count(r => !string.IsNullOrEmpty(r.CouponCode));

            // 應用排序
            filteredRecords = sortBy.ToLower() switch
            {
                "signtime" => descending
                    ? filteredRecords.OrderByDescending(r => r.SignTime).ToList()
                    : filteredRecords.OrderBy(r => r.SignTime).ToList(),
                "signindate" => descending
                    ? filteredRecords.OrderByDescending(r => r.SignInDate).ToList()
                    : filteredRecords.OrderBy(r => r.SignInDate).ToList(),
                "consecutivedays" => descending
                    ? filteredRecords.OrderByDescending(r => r.ConsecutiveDays).ToList()
                    : filteredRecords.OrderBy(r => r.ConsecutiveDays).ToList(),
                "pointsgained" => descending
                    ? filteredRecords.OrderByDescending(r => r.PointsGained).ToList()
                    : filteredRecords.OrderBy(r => r.PointsGained).ToList(),
                "pointsrewarded" => descending
                    ? filteredRecords.OrderByDescending(r => r.PointsGained).ToList()
                    : filteredRecords.OrderBy(r => r.PointsGained).ToList(),
                _ => descending
                    ? filteredRecords.OrderByDescending(r => r.SignTime).ToList()
                    : filteredRecords.OrderBy(r => r.SignTime).ToList()
            };

            // 分頁
            var pagedRecords = filteredRecords
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new SignInRecordsViewModel
            {
                Query = query,
                Records = new PagedResult<SignInRecordViewModel>
                {
                    Items = pagedRecords,
                    TotalCount = totalFilteredCount,
                    CurrentPage = page,
                    PageSize = pageSize
                },
                FilteredRecordCount = totalFilteredCount,
                TotalRewardedPoints = totalPoints,
                TotalRewardedExp = totalExp,
                TotalCouponsGranted = totalCoupons
            };

            return View(model);
        }
    }
}
