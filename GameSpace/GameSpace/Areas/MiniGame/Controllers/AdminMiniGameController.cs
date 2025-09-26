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
    public class AdminMiniGameController : MiniGameBaseController
    {
        private readonly IMiniGameAdminService _adminService;

        public AdminMiniGameController(GameSpacedatabaseContext context, IMiniGameAdminService adminService) : base(context)
        {
            _adminService = adminService;
        }

        // 1. 遊戲規則設定（獎勵規則、每日遊戲次數限制（預設 3 次/日））
        [HttpGet]
        public async Task<IActionResult> GameRules()
        {
            try
            {
                var rules = await GetGameRulesAsync();
                var viewModel = new GameRulesViewModel
                {
                    Rules = rules
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入遊戲規則時發生錯誤：{ex.Message}";
                return View(new GameRulesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GameRules(GameRulesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Rules = await GetGameRulesAsync();
                return View(model);
            }

            try
            {
                await UpdateGameRulesAsync(model.Rules);
                TempData["SuccessMessage"] = "遊戲規則更新成功！";
                return RedirectToAction(nameof(GameRules));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                model.Rules = await GetGameRulesAsync();
                return View(model);
            }
        }

        // 2. 查看會員遊戲紀錄（startTime、endTime、win/lose/abort、獲得獎勵）
        [HttpGet]
        public async Task<IActionResult> GameRecords(GameRecordQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 20;

            try
            {
                var result = await QueryGameRecordsAsync(query);
                var users = await _context.Users
                    .Select(u => new { u.Id, u.UserName, u.Email })
                    .ToListAsync();

                var viewModel = new AdminGameRecordsViewModel
                {
                    GameRecords = result.Items,
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
                TempData["ErrorMessage"] = $"查詢遊戲紀錄時發生錯誤：{ex.Message}";
                return View(new AdminGameRecordsViewModel());
            }
        }

        // 查看遊戲紀錄詳情
        [HttpGet]
        public async Task<IActionResult> GameRecordDetails(int recordId)
        {
            try
            {
                var record = await GetGameRecordDetailsAsync(recordId);
                if (record == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的遊戲紀錄！";
                    return RedirectToAction(nameof(GameRecords));
                }

                var rewards = await GetGameRecordRewardsAsync(recordId);
                var viewModel = new GameRecordDetailsViewModel
                {
                    GameRecord = record,
                    Rewards = rewards
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查看遊戲紀錄詳情時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(GameRecords));
            }
        }

        // 獲取遊戲統計資料
        [HttpGet]
        public async Task<IActionResult> GetGameStats(string period = "today")
        {
            try
            {
                var stats = await GetGameStatisticsAsync(period);
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取遊戲規則詳細信息
        [HttpGet]
        public async Task<IActionResult> GetGameRulesDetails()
        {
            try
            {
                var rules = await GetGameRulesAsync();
                return Json(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 更新遊戲規則
        [HttpPost]
        public async Task<IActionResult> UpdateGameRule([FromBody] GameRuleModel rule)
        {
            try
            {
                await UpdateGameRuleAsync(rule);
                return Json(new { success = true, message = "遊戲規則更新成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取用戶遊戲紀錄
        [HttpGet]
        public async Task<IActionResult> GetUserGameRecords(int userId, DateTime? startDate = null, DateTime? endDate = null)
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

                var records = await GetUserGameRecordsAsync(userId, startDate, endDate);
                var stats = await GetUserGameStatsAsync(userId, startDate, endDate);

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

        // 導出遊戲紀錄
        [HttpGet]
        public async Task<IActionResult> ExportGameRecords(GameRecordQueryModel query)
        {
            try
            {
                var records = await GetAllGameRecordsAsync(query);
                var csv = GenerateGameRecordsCSV(records);
                
                var fileName = $"遊戲紀錄_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"導出遊戲紀錄時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(GameRecords));
            }
        }

        // 重置用戶每日遊戲次數
        [HttpPost]
        public async Task<IActionResult> ResetUserDailyGames(int userId)
        {
            try
            {
                await ResetUserDailyGameCountAsync(userId);
                return Json(new { success = true, message = "用戶每日遊戲次數已重置！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 私有方法：獲取遊戲規則
        private async Task<List<GameRuleModel>> GetGameRulesAsync()
        {
            var rules = await _context.GameRules
                .OrderBy(r => r.RuleType)
                .ThenBy(r => r.Level)
                .Select(r => new GameRuleModel
                {
                    Id = r.Id,
                    RuleType = r.RuleType,
                    Level = r.Level,
                    Value = r.Value,
                    Description = r.Description,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            return rules;
        }

        // 私有方法：更新遊戲規則
        private async Task UpdateGameRulesAsync(List<GameRuleModel> rules)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 清除現有規則
                var existingRules = await _context.GameRules.ToListAsync();
                _context.GameRules.RemoveRange(existingRules);

                // 添加新規則
                foreach (var rule in rules)
                {
                    var gameRule = new GameRule
                    {
                        RuleType = rule.RuleType,
                        Level = rule.Level,
                        Value = rule.Value,
                        Description = rule.Description,
                        IsActive = rule.IsActive,
                        CreateTime = DateTime.Now,
                        LastUpdateTime = DateTime.Now
                    };
                    _context.GameRules.Add(gameRule);
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

        // 私有方法：更新單個遊戲規則
        private async Task UpdateGameRuleAsync(GameRuleModel rule)
        {
            var existingRule = await _context.GameRules
                .FirstOrDefaultAsync(r => r.Id == rule.Id);

            if (existingRule != null)
            {
                existingRule.RuleType = rule.RuleType;
                existingRule.Level = rule.Level;
                existingRule.Value = rule.Value;
                existingRule.Description = rule.Description;
                existingRule.IsActive = rule.IsActive;
                existingRule.LastUpdateTime = DateTime.Now;
            }
            else
            {
                var newRule = new GameRule
                {
                    RuleType = rule.RuleType,
                    Level = rule.Level,
                    Value = rule.Value,
                    Description = rule.Description,
                    IsActive = rule.IsActive,
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                };
                _context.GameRules.Add(newRule);
            }

            await _context.SaveChangesAsync();
        }

        // 私有方法：查詢遊戲紀錄
        private async Task<PagedResult<GameRecordModel>> QueryGameRecordsAsync(GameRecordQueryModel query)
        {
            var queryable = _context.MiniGames
                .Include(g => g.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(g => g.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(g => 
                    g.User.UserName.Contains(query.SearchTerm) || 
                    g.User.Email.Contains(query.SearchTerm));
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(g => g.StartTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(g => g.StartTime <= query.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(query.Result))
            {
                queryable = queryable.Where(g => g.Result == query.Result);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(g => g.StartTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(g => new GameRecordModel
                {
                    Id = g.Id,
                    UserId = g.UserId,
                    UserName = g.User.UserName,
                    Email = g.User.Email,
                    StartTime = g.StartTime,
                    EndTime = g.EndTime,
                    Result = g.Result,
                    Score = g.Score,
                    Duration = g.Duration,
                    Rewards = g.Rewards
                })
                .ToListAsync();

            return new PagedResult<GameRecordModel>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        // 私有方法：獲取遊戲紀錄詳情
        private async Task<GameRecordModel> GetGameRecordDetailsAsync(int recordId)
        {
            var record = await _context.MiniGames
                .Include(g => g.User)
                .Where(g => g.Id == recordId)
                .Select(g => new GameRecordModel
                {
                    Id = g.Id,
                    UserId = g.UserId,
                    UserName = g.User.UserName,
                    Email = g.User.Email,
                    StartTime = g.StartTime,
                    EndTime = g.EndTime,
                    Result = g.Result,
                    Score = g.Score,
                    Duration = g.Duration,
                    Rewards = g.Rewards,
                    SessionId = g.SessionId,
                    GameData = g.GameData
                })
                .FirstOrDefaultAsync();

            return record;
        }

        // 私有方法：獲取遊戲紀錄獎勵
        private async Task<List<GameRewardModel>> GetGameRecordRewardsAsync(int recordId)
        {
            var rewards = await _context.GameRewards
                .Where(r => r.GameId == recordId)
                .Select(r => new GameRewardModel
                {
                    Id = r.Id,
                    GameId = r.GameId,
                    RewardType = r.RewardType,
                    RewardValue = r.RewardValue,
                    Description = r.Description
                })
                .ToListAsync();

            return rewards;
        }

        // 私有方法：獲取遊戲統計
        private async Task<GameStatsModel> GetGameStatisticsAsync(string period)
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);

            var queryable = _context.MiniGames.AsQueryable();

            switch (period.ToLower())
            {
                case "today":
                    queryable = queryable.Where(g => g.StartTime.Date == today);
                    break;
                case "week":
                    queryable = queryable.Where(g => g.StartTime >= thisWeek);
                    break;
                case "month":
                    queryable = queryable.Where(g => g.StartTime >= thisMonth);
                    break;
            }

            var stats = new GameStatsModel
            {
                Period = period,
                TotalGames = await queryable.CountAsync(),
                UniquePlayers = await queryable.Select(g => g.UserId).Distinct().CountAsync(),
                WinGames = await queryable.Where(g => g.Result == "win").CountAsync(),
                LoseGames = await queryable.Where(g => g.Result == "lose").CountAsync(),
                AbortGames = await queryable.Where(g => g.Result == "abort").CountAsync(),
                AverageScore = await queryable.AverageAsync(g => g.Score),
                AverageDuration = await queryable.AverageAsync(g => g.Duration),
                TotalRewards = await queryable.SumAsync(g => g.Rewards)
            };

            return stats;
        }

        // 私有方法：獲取用戶遊戲紀錄
        private async Task<List<GameRecordModel>> GetUserGameRecordsAsync(int userId, DateTime? startDate, DateTime? endDate)
        {
            var queryable = _context.MiniGames
                .Include(g => g.User)
                .Where(g => g.UserId == userId);

            if (startDate.HasValue)
            {
                queryable = queryable.Where(g => g.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                queryable = queryable.Where(g => g.StartTime <= endDate.Value);
            }

            return await queryable
                .OrderByDescending(g => g.StartTime)
                .Select(g => new GameRecordModel
                {
                    Id = g.Id,
                    UserId = g.UserId,
                    UserName = g.User.UserName,
                    Email = g.User.Email,
                    StartTime = g.StartTime,
                    EndTime = g.EndTime,
                    Result = g.Result,
                    Score = g.Score,
                    Duration = g.Duration,
                    Rewards = g.Rewards
                })
                .ToListAsync();
        }

        // 私有方法：獲取用戶遊戲統計
        private async Task<UserGameStatsModel> GetUserGameStatsAsync(int userId, DateTime? startDate, DateTime? endDate)
        {
            var queryable = _context.MiniGames
                .Where(g => g.UserId == userId);

            if (startDate.HasValue)
            {
                queryable = queryable.Where(g => g.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                queryable = queryable.Where(g => g.StartTime <= endDate.Value);
            }

            var games = await queryable.ToListAsync();

            return new UserGameStatsModel
            {
                TotalGames = games.Count,
                WinGames = games.Count(g => g.Result == "win"),
                LoseGames = games.Count(g => g.Result == "lose"),
                AbortGames = games.Count(g => g.Result == "abort"),
                WinRate = games.Count > 0 ? (double)games.Count(g => g.Result == "win") / games.Count * 100 : 0,
                AverageScore = games.Count > 0 ? games.Average(g => g.Score) : 0,
                AverageDuration = games.Count > 0 ? games.Average(g => g.Duration) : 0,
                TotalRewards = games.Sum(g => g.Rewards),
                LastGameTime = games.OrderByDescending(g => g.StartTime).Select(g => g.StartTime).FirstOrDefault()
            };
        }

        // 私有方法：獲取所有遊戲紀錄（用於導出）
        private async Task<List<GameRecordModel>> GetAllGameRecordsAsync(GameRecordQueryModel query)
        {
            var queryable = _context.MiniGames
                .Include(g => g.User)
                .AsQueryable();

            if (query.UserId.HasValue)
            {
                queryable = queryable.Where(g => g.UserId == query.UserId.Value);
            }

            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(g => 
                    g.User.UserName.Contains(query.SearchTerm) || 
                    g.User.Email.Contains(query.SearchTerm));
            }

            if (query.StartDate.HasValue)
            {
                queryable = queryable.Where(g => g.StartTime >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                queryable = queryable.Where(g => g.StartTime <= query.EndDate.Value);
            }

            if (!string.IsNullOrEmpty(query.Result))
            {
                queryable = queryable.Where(g => g.Result == query.Result);
            }

            return await queryable
                .OrderByDescending(g => g.StartTime)
                .Select(g => new GameRecordModel
                {
                    Id = g.Id,
                    UserId = g.UserId,
                    UserName = g.User.UserName,
                    Email = g.User.Email,
                    StartTime = g.StartTime,
                    EndTime = g.EndTime,
                    Result = g.Result,
                    Score = g.Score,
                    Duration = g.Duration,
                    Rewards = g.Rewards
                })
                .ToListAsync();
        }

        // 私有方法：生成遊戲紀錄CSV
        private string GenerateGameRecordsCSV(List<GameRecordModel> records)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("遊戲ID,用戶ID,用戶名稱,開始時間,結束時間,結果,分數,持續時間,獎勵");

            foreach (var record in records)
            {
                csv.AppendLine($"{record.Id},{record.UserId},{record.UserName},{record.StartTime:yyyy-MM-dd HH:mm:ss},{record.EndTime:yyyy-MM-dd HH:mm:ss},{record.Result},{record.Score},{record.Duration},{record.Rewards}");
            }

            return csv.ToString();
        }

        // 私有方法：重置用戶每日遊戲次數
        private async Task ResetUserDailyGameCountAsync(int userId)
        {
            var userDailyGames = await _context.UserDailyGames
                .FirstOrDefaultAsync(udg => udg.UserId == userId && udg.GameDate.Date == DateTime.Today);

            if (userDailyGames != null)
            {
                userDailyGames.GameCount = 0;
                userDailyGames.LastUpdateTime = DateTime.Now;
            }
            else
            {
                var newDailyGame = new UserDailyGame
                {
                    UserId = userId,
                    GameDate = DateTime.Today,
                    GameCount = 0,
                    CreateTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now
                };
                _context.UserDailyGames.Add(newDailyGame);
            }

            await _context.SaveChangesAsync();
        }
    }
}
