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

        // 1. 簽到規則設定
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

        // 2. 查看會員簽到紀錄
        [HttpGet]
        public async Task<IActionResult> SignInRecords(SignInRecordQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await QuerySignInRecordsAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

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

        // 獲取簽到規則詳細信息
        [HttpGet]
        public async Task<IActionResult> GetSignInRulesDetails()
        {
            try
            {
                var rules = await GetSignInRulesAsync();
                return Json(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新簽到規則
        [HttpPost]
        public async Task<IActionResult> UpdateSignInRules([FromBody] SignInRuleModel rule)
        {
            try
            {
                await UpdateSignInRuleAsync(rule);
                return Json(new { success = true, message = "簽到規則更新成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取用戶簽到詳情
        [HttpGet]
        public async Task<IActionResult> GetUserSignInDetails(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return Json(new { success = false, message = "找不到指定的用戶" });
                }

                var records = await GetUserSignInRecordsAsync(userId, startDate, endDate);
                var stats = await GetUserSignInStatsAsync(userId, startDate, endDate);

                return Json(new { 
                    success = true, 
                    data = new { 
                        User = user, 
                        Records = records, 
                        Stats = stats 
                    } 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 導出簽到紀錄
        [HttpGet]
        public async Task<IActionResult> ExportSignInRecords(SignInRecordQueryModel query)
        {
            try
            {
                var records = await GetAllSignInRecordsAsync(query);
                var csv = GenerateSignInRecordsCSV(records);
                
                var fileName = $"簽到紀錄_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"導出簽到紀錄時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(SignInRecords));
            }
        }

        // 私有方法：獲取簽到規則
        private async Task<List<SignInRuleModel>> GetSignInRulesAsync()
        {
            var rules = await _context.SignInRules
                .OrderBy(r => r.DayNumber)
                .Select(r => new SignInRuleModel
                {
                    Id = r.Id,
                    DayNumber = r.DayNumber,
                    RewardType = r.RewardType,
                    RewardValue = r.RewardValue,
                    Description = r.Description,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            return rules;
        }

        // 私有方法：更新簽到規則
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
                        RewardType = rule.RewardType,
                        RewardValue = rule.RewardValue,
                        Description = rule.Description,
                        IsActive = rule.IsActive,
                        CreateTime = DateTime.Now,
                        LastUpdateTime = DateTime.Now
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

        // 私有方法：更新單個簽到規則
        private async Task UpdateSignInRuleAsync(SignInRuleModel rule)
        {
            var existingRule = await _context.SignInRules
                .FirstOrDefaultAsync(r => r.Id == rule.Id);

            if (existingRule != null)
            {
                existingRule.DayNumber = rule.DayNumber;
                existingRule.RewardType = rule.RewardType;
                existingRule.RewardValue = rule.RewardValue;
                existingRule.Description = rule.Description;
                existingRule.IsActive = rule.IsActive;
                existingRule.LastUpdateTime = DateTime.Now;
            }
            else
            {
                var newRule = new SignInRule
                {
                    DayNumber = rule.DayNumber,
                    RewardType = rule.RewardType,
                    RewardValue = rule.RewardValue,
                    Description = rule.Description,
                    IsActive = rule.IsActive,
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                };
                _context.SignInRules.Add(newRule);
            }

            await _context.SaveChangesAsync();
        }

        // 私有方法：查詢簽到紀錄
        private async Task<PagedResult<SignInRecordModel>> QuerySignInRecordsAsync(SignInRecordQueryModel query)
        {
            var queryable = _context.UserSignInStats
                .Include(s => s.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(s => s.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(s => 
                    s.User.UserName.Contains(query.SearchTerm) || 
                    s.User.Email.Contains(query.SearchTerm));
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(s => s.SignInDate >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(s => s.SignInDate <= query.EndDate.Value);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(s => s.SignInDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new SignInRecordModel
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    Email = s.User.Email,
                    SignInDate = s.SignInDate,
                    RewardType = s.RewardType,
                    RewardValue = s.RewardValue,
                    Description = s.Description
                })
                .ToListAsync();

            return new PagedResult<SignInRecordModel>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        // 私有方法：獲取簽到統計
        private async Task<SignInStatsModel> GetSignInStatisticsAsync(string period)
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);

            var queryable = _context.UserSignInStats.AsQueryable();

            switch (period.ToLower())
            {
                case "today":
                    queryable = queryable.Where(s => s.SignInDate.Date == today);
                    break;
                case "week":
                    queryable = queryable.Where(s => s.SignInDate >= thisWeek);
                    break;
                case "month":
                    queryable = queryable.Where(s => s.SignInDate >= thisMonth);
                    break;
            }

            var stats = new SignInStatsModel
            {
                Period = period,
                TotalSignIns = await queryable.CountAsync(),
                UniqueUsers = await queryable.Select(s => s.UserId).Distinct().CountAsync(),
                TotalPointsRewarded = await queryable
                    .Where(s => s.RewardType == "Points")
                    .SumAsync(s => s.RewardValue),
                TotalCouponsRewarded = await queryable
                    .Where(s => s.RewardType == "Coupon")
                    .CountAsync()
            };

            return stats;
        }

        // 私有方法：獲取用戶簽到紀錄
        private async Task<List<SignInRecordModel>> GetUserSignInRecordsAsync(int userId, DateTime? startDate, DateTime? endDate)
        {
            var queryable = _context.UserSignInStats
                .Include(s => s.User)
                .Where(s => s.UserId == userId);

            if (startDate.HasValue)
            {
                queryable = queryable.Where(s => s.SignInDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                queryable = queryable.Where(s => s.SignInDate <= endDate.Value);
            }

            return await queryable
                .OrderByDescending(s => s.SignInDate)
                .Select(s => new SignInRecordModel
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    Email = s.User.Email,
                    SignInDate = s.SignInDate,
                    RewardType = s.RewardType,
                    RewardValue = s.RewardValue,
                    Description = s.Description
                })
                .ToListAsync();
        }

        // 私有方法：獲取用戶簽到統計
        private async Task<UserSignInStatsModel> GetUserSignInStatsAsync(int userId, DateTime? startDate, DateTime? endDate)
        {
            var queryable = _context.UserSignInStats
                .Where(s => s.UserId == userId);

            if (startDate.HasValue)
            {
                queryable = queryable.Where(s => s.SignInDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                queryable = queryable.Where(s => s.SignInDate <= endDate.Value);
            }

            var records = await queryable.ToListAsync();

            return new UserSignInStatsModel
            {
                TotalSignIns = records.Count,
                TotalPointsEarned = records
                    .Where(r => r.RewardType == "Points")
                    .Sum(r => r.RewardValue),
                TotalCouponsEarned = records
                    .Where(r => r.RewardType == "Coupon")
                    .Count(),
                LastSignInDate = records
                    .OrderByDescending(r => r.SignInDate)
                    .Select(r => r.SignInDate)
                    .FirstOrDefault()
            };
        }

        // 私有方法：獲取所有簽到紀錄（用於導出）
        private async Task<List<SignInRecordModel>> GetAllSignInRecordsAsync(SignInRecordQueryModel query)
        {
            var queryable = _context.UserSignInStats
                .Include(s => s.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(s => s.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(s => 
                    s.User.UserName.Contains(query.SearchTerm) || 
                    s.User.Email.Contains(query.SearchTerm));
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(s => s.SignInDate >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(s => s.SignInDate <= query.EndDate.Value);
            }

            return await queryable
                .OrderByDescending(s => s.SignInDate)
                .Select(s => new SignInRecordModel
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    Email = s.User.Email,
                    SignInDate = s.SignInDate,
                    RewardType = s.RewardType,
                    RewardValue = s.RewardValue,
                    Description = s.Description
                })
                .ToListAsync();
        }

        // 私有方法：生成CSV
        private string GenerateSignInRecordsCSV(List<SignInRecordModel> records)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("用戶ID,用戶名稱,電子郵件,簽到日期,獎勵類型,獎勵數值,描述");

            foreach (var record in records)
            {
                csv.AppendLine($"{record.UserId},{record.UserName},{record.Email},{record.SignInDate:yyyy-MM-dd},{record.RewardType},{record.RewardValue},{record.Description}");
            }

            return csv.ToString();
        }
    }
}
