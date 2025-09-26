using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]
    public class AdminSignInController : MiniGameBaseController
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminSignInController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : base(context)
        {
            _adminService = adminService;
        }

        // 簽到規則設定
        [HttpGet]
        public async Task<IActionResult> SignInRules()
        {
            try
            {
                var rules = await GetSignInRulesAsync();
                var viewModel = new SignInRulesViewModel
                {
                    Rules = rules
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入簽到規則時發生錯誤：{ex.Message}";
                return View(new SignInRulesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SignInRules(SignInRulesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Rules = await GetSignInRulesAsync();
                return View(model);
            }

            try
            {
                await UpdateSignInRulesAsync(model.Rules);
                TempData["SuccessMessage"] = "簽到規則更新成功！";
                return RedirectToAction(nameof(SignInRules));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                model.Rules = await GetSignInRulesAsync();
                return View(model);
            }
        }

        // 查看會員簽到紀錄
        public async Task<IActionResult> SignInRecords(SignInRecordQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QuerySignInRecordsAsync(query);
                var users = await _context.Users.ToListAsync();

                var viewModel = new AdminSignInRecordsViewModel
                {
                    SignInRecords = result.Items,
                    Users = users,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢簽到紀錄時發生錯誤：{ex.Message}";
                return View(new AdminSignInRecordsViewModel());
            }
        }

        // 獲取簽到統計資料
        [HttpGet]
        public async Task<IActionResult> GetSignInStats(string period = "today")
        {
            try
            {
                var stats = await GetSignInStatisticsAsync(period);
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取簽到趨勢圖表資料
        [HttpGet]
        public async Task<IActionResult> GetSignInTrends(int days = 30)
        {
            try
            {
                var trends = await GetSignInTrendDataAsync(days);
                return Json(new { success = true, data = trends });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取用戶簽到詳情
        [HttpGet]
        public async Task<IActionResult> GetUserSignInDetails(int userId, int days = 30)
        {
            try
            {
                var details = await GetUserSignInDetailsAsync(userId, days);
                return Json(new { success = true, data = details });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 手動為用戶添加簽到記錄
        [HttpPost]
        public async Task<IActionResult> AddManualSignIn(ManualSignInModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "請填寫所有必要欄位" });

            try
            {
                await AddManualSignInRecordAsync(model);
                return Json(new { success = true, message = "手動簽到記錄添加成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 刪除簽到記錄
        [HttpPost]
        public async Task<IActionResult> DeleteSignInRecord(int recordId)
        {
            try
            {
                await DeleteSignInRecordAsync(recordId);
                return Json(new { success = true, message = "簽到記錄刪除成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 私有方法
        private async Task<List<SignInRuleModel>> GetSignInRulesAsync()
        {
            var rules = await _context.SignInRules
                .OrderBy(r => r.DayNumber)
                .Select(r => new SignInRuleModel
                {
                    RuleId = r.RuleId,
                    DayNumber = r.DayNumber,
                    PointsReward = r.PointsReward,
                    ExpReward = r.ExpReward,
                    CouponReward = r.CouponReward,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            // 如果沒有規則，創建預設規則
            if (!rules.Any())
            {
                rules = CreateDefaultSignInRules();
                await SaveDefaultSignInRulesAsync(rules);
            }

            return rules;
        }

        private async Task UpdateSignInRulesAsync(List<SignInRuleModel> rules)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 清除現有規則
                var existingRules = await _context.SignInRules.ToListAsync();
                _context.SignInRules.RemoveRange(existingRules);

                // 添加新規則
                foreach (var rule in rules)
                {
                    var signInRule = new SignInRule
                    {
                        DayNumber = rule.DayNumber,
                        PointsReward = rule.PointsReward,
                        ExpReward = rule.ExpReward,
                        CouponReward = rule.CouponReward,
                        IsActive = rule.IsActive
                    };
                    _context.SignInRules.Add(signInRule);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<PagedResult<SignInRecordModel>> QuerySignInRecordsAsync(SignInRecordQueryModel query)
        {
            var queryable = _context.UserSignInStats
                .Include(s => s.User)
                .AsQueryable();

            if (query.UserId.HasValue)
                queryable = queryable.Where(s => s.UserId == query.UserId.Value);

            if (!string.IsNullOrEmpty(query.UserName))
                queryable = queryable.Where(s => s.User.UserName.Contains(query.UserName));

            if (query.StartDate.HasValue)
                queryable = queryable.Where(s => s.SignTime >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                queryable = queryable.Where(s => s.SignTime <= query.EndDate.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(s => s.SignTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new SignInRecordModel
                {
                    SignInId = s.SignInId,
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    SignTime = s.SignTime,
                    PointsGained = s.PointsGained,
                    ExpGained = s.ExpGained,
                    ConsecutiveDays = s.ConsecutiveDays
                })
                .ToListAsync();

            return new PagedResult<SignInRecordModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task<SignInStatisticsModel> GetSignInStatisticsAsync(string period)
        {
            var today = DateTime.Today;
            var startDate = period switch
            {
                "today" => today,
                "week" => today.AddDays(-7),
                "month" => new DateTime(today.Year, today.Month, 1),
                "year" => new DateTime(today.Year, 1, 1),
                _ => today
            };

            var totalSignIns = await _context.UserSignInStats
                .Where(s => s.SignTime >= startDate)
                .CountAsync();

            var uniqueUsers = await _context.UserSignInStats
                .Where(s => s.SignTime >= startDate)
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync();

            var totalPoints = await _context.UserSignInStats
                .Where(s => s.SignTime >= startDate)
                .SumAsync(s => s.PointsGained);

            var totalExp = await _context.UserSignInStats
                .Where(s => s.SignTime >= startDate)
                .SumAsync(s => s.ExpGained);

            return new SignInStatisticsModel
            {
                Period = period,
                TotalSignIns = totalSignIns,
                UniqueUsers = uniqueUsers,
                TotalPoints = totalPoints,
                TotalExp = totalExp
            };
        }

        private async Task<List<SignInTrendModel>> GetSignInTrendDataAsync(int days)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            var trends = await _context.UserSignInStats
                .Where(s => s.SignTime >= startDate && s.SignTime <= endDate)
                .GroupBy(s => s.SignTime.Date)
                .Select(g => new SignInTrendModel
                {
                    Date = g.Key,
                    SignInCount = g.Count(),
                    UniqueUsers = g.Select(s => s.UserId).Distinct().Count()
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            return trends;
        }

        private async Task<UserSignInDetailsModel> GetUserSignInDetailsAsync(int userId, int days)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("找不到指定的用戶");

            var signInRecords = await _context.UserSignInStats
                .Where(s => s.UserId == userId && s.SignTime >= startDate)
                .OrderByDescending(s => s.SignTime)
                .Select(s => new SignInRecordModel
                {
                    SignInId = s.SignInId,
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    SignTime = s.SignTime,
                    PointsGained = s.PointsGained,
                    ExpGained = s.ExpGained,
                    ConsecutiveDays = s.ConsecutiveDays
                })
                .ToListAsync();

            var totalSignIns = signInRecords.Count;
            var totalPoints = signInRecords.Sum(s => s.PointsGained);
            var totalExp = signInRecords.Sum(s => s.ExpGained);
            var maxConsecutive = signInRecords.Max(s => s.ConsecutiveDays);

            return new UserSignInDetailsModel
            {
                UserId = userId,
                UserName = user.UserName,
                SignInRecords = signInRecords,
                TotalSignIns = totalSignIns,
                TotalPoints = totalPoints,
                TotalExp = totalExp,
                MaxConsecutive = maxConsecutive
            };
        }

        private async Task AddManualSignInRecordAsync(ManualSignInModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null)
                    throw new Exception("找不到指定的用戶");

                var signInRecord = new UserSignInStat
                {
                    UserId = model.UserId,
                    SignTime = model.SignTime,
                    PointsGained = model.PointsGained,
                    ExpGained = model.ExpGained,
                    ConsecutiveDays = model.ConsecutiveDays,
                    IsManual = true
                };
                _context.UserSignInStats.Add(signInRecord);

                // 更新用戶錢包
                var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == model.UserId);
                if (wallet == null)
                {
                    wallet = new UserWallet { UserId = model.UserId, UserPoint = 0 };
                    _context.UserWallets.Add(wallet);
                }
                wallet.UserPoint += model.PointsGained;

                // 添加錢包歷史記錄
                var walletHistory = new WalletHistory
                {
                    UserId = model.UserId,
                    ChangeType = "SignIn",
                    PointsChanged = model.PointsGained,
                    Description = $"手動簽到獎勵：{model.PointsGained} 點",
                    ChangeTime = DateTime.Now
                };
                _context.WalletHistories.Add(walletHistory);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task DeleteSignInRecordAsync(int recordId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var record = await _context.UserSignInStats.FindAsync(recordId);
                if (record == null)
                    throw new Exception("找不到指定的簽到記錄");

                // 如果是手動添加的記錄，可以刪除
                if (record.IsManual)
                {
                    // 扣除相應的點數
                    var wallet = await _context.UserWallets.FirstOrDefaultAsync(w => w.UserId == record.UserId);
                    if (wallet != null)
                    {
                        wallet.UserPoint = Math.Max(0, wallet.UserPoint - record.PointsGained);
                    }

                    _context.UserSignInStats.Remove(record);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("只能刪除手動添加的簽到記錄");
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private List<SignInRuleModel> CreateDefaultSignInRules()
        {
            return new List<SignInRuleModel>
            {
                new() { DayNumber = 1, PointsReward = 10, ExpReward = 5, CouponReward = 0, IsActive = true },
                new() { DayNumber = 2, PointsReward = 15, ExpReward = 8, CouponReward = 0, IsActive = true },
                new() { DayNumber = 3, PointsReward = 20, ExpReward = 10, CouponReward = 0, IsActive = true },
                new() { DayNumber = 4, PointsReward = 25, ExpReward = 12, CouponReward = 0, IsActive = true },
                new() { DayNumber = 5, PointsReward = 30, ExpReward = 15, CouponReward = 0, IsActive = true },
                new() { DayNumber = 6, PointsReward = 35, ExpReward = 18, CouponReward = 0, IsActive = true },
                new() { DayNumber = 7, PointsReward = 50, ExpReward = 25, CouponReward = 1, IsActive = true }
            };
        }

        private async Task SaveDefaultSignInRulesAsync(List<SignInRuleModel> rules)
        {
            foreach (var rule in rules)
            {
                var signInRule = new SignInRule
                {
                    DayNumber = rule.DayNumber,
                    PointsReward = rule.PointsReward,
                    ExpReward = rule.ExpReward,
                    CouponReward = rule.CouponReward,
                    IsActive = rule.IsActive
                };
                _context.SignInRules.Add(signInRule);
            }
            await _context.SaveChangesAsync();
        }
    }

    // ViewModels
    public class SignInRulesViewModel
    {
        public List<SignInRuleModel> Rules { get; set; } = new();
    }

    public class SignInRecordQueryModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class AdminSignInRecordsViewModel
    {
        public List<SignInRecordModel> SignInRecords { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public SignInRecordQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class SignInRuleModel
    {
        public int RuleId { get; set; }
        public int DayNumber { get; set; }
        public int PointsReward { get; set; }
        public int ExpReward { get; set; }
        public int CouponReward { get; set; }
        public bool IsActive { get; set; }
    }

    public class SignInRecordModel
    {
        public int SignInId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SignTime { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public int ConsecutiveDays { get; set; }
    }

    public class SignInStatisticsModel
    {
        public string Period { get; set; } = string.Empty;
        public int TotalSignIns { get; set; }
        public int UniqueUsers { get; set; }
        public int TotalPoints { get; set; }
        public int TotalExp { get; set; }
    }

    public class SignInTrendModel
    {
        public DateTime Date { get; set; }
        public int SignInCount { get; set; }
        public int UniqueUsers { get; set; }
    }

    public class UserSignInDetailsModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<SignInRecordModel> SignInRecords { get; set; } = new();
        public int TotalSignIns { get; set; }
        public int TotalPoints { get; set; }
        public int TotalExp { get; set; }
        public int MaxConsecutive { get; set; }
    }

    public class ManualSignInModel
    {
        public int UserId { get; set; }
        public DateTime SignTime { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public int ConsecutiveDays { get; set; }
    }
}
