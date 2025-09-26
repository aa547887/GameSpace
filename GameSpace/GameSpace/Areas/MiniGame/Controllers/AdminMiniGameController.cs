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

        // 小遊戲管理首頁
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await GetMiniGameDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入小遊戲管理頁面時發生錯誤：{ex.Message}";
                return View(new MiniGameDashboardViewModel());
            }
        }

        // 遊戲規則設定（獎勵規則、每日遊戲次數限制）
        [HttpGet]
        public async Task<IActionResult> GameRules()
        {
            try
            {
                var rules = await GetGameRulesAsync();
                var viewModel = new AdminMiniGameRulesViewModel
                {
                    Rules = rules
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入遊戲規則時發生錯誤：{ex.Message}";
                return View(new AdminMiniGameRulesViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GameRules(AdminMiniGameRulesViewModel model)
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

        // 查看會員遊戲紀錄
        public async Task<IActionResult> GameRecords(GameRecordQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QueryGameRecordsAsync(query);
                var users = await _context.Users.ToListAsync();
                var games = await _context.MiniGames.ToListAsync();

                var viewModel = new AdminGameRecordsViewModel
                {
                    GameRecords = result.Items,
                    Users = users,
                    Games = games,
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

        // 小遊戲清單管理
        public async Task<IActionResult> GameList(GameQueryModel query)
        {
            if (query.PageNumber <= 0) query.PageNumber = 1;
            if (query.PageSize <= 0) query.PageSize = 10;

            try
            {
                var result = await QueryGamesAsync(query);
                var viewModel = new AdminGameListViewModel
                {
                    Games = result.Items,
                    Query = query,
                    TotalCount = result.TotalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢小遊戲清單時發生錯誤：{ex.Message}";
                return View(new AdminGameListViewModel());
            }
        }

        // 新增小遊戲
        [HttpGet]
        public async Task<IActionResult> CreateGame()
        {
            try
            {
                var viewModel = new CreateGameViewModel();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入新增小遊戲頁面時發生錯誤：{ex.Message}";
                return View(new CreateGameViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGame(CreateGameViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await CreateGameAsync(model);
                TempData["SuccessMessage"] = "小遊戲創建成功！";
                return RedirectToAction(nameof(GameList));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"創建失敗：{ex.Message}");
                return View(model);
            }
        }

        // 編輯小遊戲
        [HttpGet]
        public async Task<IActionResult> EditGame(int id)
        {
            try
            {
                var game = await _context.MiniGames.FindAsync(id);
                if (game == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的小遊戲";
                    return RedirectToAction(nameof(GameList));
                }

                var viewModel = new EditGameViewModel
                {
                    GameId = game.GameId,
                    GameName = game.GameName,
                    Description = game.Description,
                    IsActive = game.IsActive,
                    PointsReward = game.PointsReward,
                    ExpReward = game.ExpReward,
                    Difficulty = game.Difficulty
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入編輯小遊戲頁面時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(GameList));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditGame(EditGameViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await UpdateGameAsync(model);
                TempData["SuccessMessage"] = "小遊戲更新成功！";
                return RedirectToAction(nameof(GameList));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                return View(model);
            }
        }

        // 刪除小遊戲
        [HttpPost]
        public async Task<IActionResult> DeleteGame(int id)
        {
            try
            {
                await DeleteGameAsync(id);
                TempData["SuccessMessage"] = "小遊戲刪除成功！";
                return RedirectToAction(nameof(GameList));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"刪除失敗：{ex.Message}";
                return RedirectToAction(nameof(GameList));
            }
        }

        // AJAX API 方法
        [HttpGet]
        public async Task<IActionResult> GetGameStats(string period = "today")
        {
            try
            {
                var stats = await GetGameStatisticsAsync(period);
                return Json(new { success = true, stats = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserGameRecords(int userId)
        {
            try
            {
                var records = await _context.GameRecords
                    .Include(g => g.Game)
                    .Where(g => g.UserId == userId)
                    .OrderByDescending(g => g.StartTime)
                    .Take(10)
                    .Select(g => new
                    {
                        playId = g.PlayId,
                        gameName = g.Game.GameName,
                        level = g.Level,
                        result = g.Result,
                        pointsGained = g.PointsGained,
                        expGained = g.ExpGained,
                        startTime = g.StartTime,
                        endTime = g.EndTime
                    })
                    .ToListAsync();

                return Json(new { success = true, records = records });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 私有方法
        private async Task<MiniGameDashboardViewModel> GetMiniGameDashboardDataAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            var totalGames = await _context.MiniGames.CountAsync();
            var activeGames = await _context.MiniGames.CountAsync(g => g.IsActive);
            var totalPlays = await _context.GameRecords.CountAsync();
            var todayPlays = await _context.GameRecords.CountAsync(g => g.StartTime.Date == today);
            var thisMonthPlays = await _context.GameRecords.CountAsync(g => g.StartTime >= thisMonth);

            var recentGames = await _context.GameRecords
                .Include(g => g.User)
                .Include(g => g.Game)
                .OrderByDescending(g => g.StartTime)
                .Take(5)
                .Select(g => new RecentGameModel
                {
                    PlayId = g.PlayId,
                    UserId = g.UserId,
                    UserName = g.User.UserName,
                    GameName = g.Game.GameName,
                    Level = g.Level,
                    Result = g.Result,
                    PointsGained = g.PointsGained,
                    ExpGained = g.ExpGained,
                    StartTime = g.StartTime
                })
                .ToListAsync();

            var topPlayers = await _context.GameRecords
                .Include(g => g.User)
                .GroupBy(g => g.UserId)
                .Select(g => new TopPlayerModel
                {
                    UserId = g.Key,
                    UserName = g.First().User.UserName,
                    TotalPlays = g.Count(),
                    TotalPoints = g.Sum(x => x.PointsGained),
                    TotalExp = g.Sum(x => x.ExpGained)
                })
                .OrderByDescending(p => p.TotalPlays)
                .Take(5)
                .ToListAsync();

            return new MiniGameDashboardViewModel
            {
                TotalGames = totalGames,
                ActiveGames = activeGames,
                TotalPlays = totalPlays,
                TodayPlays = todayPlays,
                ThisMonthPlays = thisMonthPlays,
                RecentGames = recentGames,
                TopPlayers = topPlayers
            };
        }

        private async Task<List<GameRuleModel>> GetGameRulesAsync()
        {
            var rules = await _context.GameRules
                .Include(g => g.Game)
                .Select(r => new GameRuleModel
                {
                    RuleId = r.RuleId,
                    GameId = r.GameId,
                    GameName = r.Game.GameName,
                    PointsReward = r.PointsReward,
                    ExpReward = r.ExpReward,
                    DailyLimit = r.DailyLimit,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            // 如果沒有規則，創建預設規則
            if (!rules.Any())
            {
                rules = CreateDefaultGameRules();
                await SaveDefaultGameRulesAsync(rules);
            }

            return rules;
        }

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
                        GameId = rule.GameId,
                        PointsReward = rule.PointsReward,
                        ExpReward = rule.ExpReward,
                        DailyLimit = rule.DailyLimit,
                        IsActive = rule.IsActive
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

        private async Task<PagedResult<GameRecordModel>> QueryGameRecordsAsync(GameRecordQueryModel query)
        {
            var queryable = _context.GameRecords
                .Include(g => g.User)
                .Include(g => g.Game)
                .AsQueryable();

            if (query.UserId.HasValue)
                queryable = queryable.Where(g => g.UserId == query.UserId.Value);

            if (!string.IsNullOrEmpty(query.UserName))
                queryable = queryable.Where(g => g.User.UserName.Contains(query.UserName));

            if (query.GameId.HasValue)
                queryable = queryable.Where(g => g.GameId == query.GameId.Value);

            if (!string.IsNullOrEmpty(query.Result))
                queryable = queryable.Where(g => g.Result == query.Result);

            if (query.StartDate.HasValue)
                queryable = queryable.Where(g => g.StartTime >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                queryable = queryable.Where(g => g.StartTime <= query.EndDate.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(g => g.StartTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(g => new GameRecordModel
                {
                    PlayId = g.PlayId,
                    UserId = g.UserId,
                    UserName = g.User.UserName,
                    GameName = g.Game.GameName,
                    Level = g.Level,
                    Result = g.Result,
                    PointsGained = g.PointsGained,
                    ExpGained = g.ExpGained,
                    StartTime = g.StartTime
                })
                .ToListAsync();

            return new PagedResult<GameRecordModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task<PagedResult<GameModel>> QueryGamesAsync(GameQueryModel query)
        {
            var queryable = _context.MiniGames.AsQueryable();

            if (!string.IsNullOrEmpty(query.GameName))
                queryable = queryable.Where(g => g.GameName.Contains(query.GameName));

            if (!string.IsNullOrEmpty(query.Difficulty))
                queryable = queryable.Where(g => g.Difficulty == query.Difficulty);

            if (query.IsActive.HasValue)
                queryable = queryable.Where(g => g.IsActive == query.IsActive.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(g => g.CreatedTime)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(g => new GameModel
                {
                    GameId = g.GameId,
                    GameName = g.GameName,
                    Description = g.Description,
                    IsActive = g.IsActive,
                    PointsReward = g.PointsReward,
                    ExpReward = g.ExpReward,
                    Difficulty = g.Difficulty,
                    CreatedTime = g.CreatedTime
                })
                .ToListAsync();

            return new PagedResult<GameModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        private async Task CreateGameAsync(CreateGameViewModel model)
        {
            var game = new MiniGame
            {
                GameName = model.GameName,
                Description = model.Description,
                IsActive = model.IsActive,
                PointsReward = model.PointsReward,
                ExpReward = model.ExpReward,
                Difficulty = model.Difficulty,
                CreatedTime = DateTime.Now
            };

            _context.MiniGames.Add(game);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateGameAsync(EditGameViewModel model)
        {
            var game = await _context.MiniGames.FindAsync(model.GameId);
            if (game != null)
            {
                game.GameName = model.GameName;
                game.Description = model.Description;
                game.IsActive = model.IsActive;
                game.PointsReward = model.PointsReward;
                game.ExpReward = model.ExpReward;
                game.Difficulty = model.Difficulty;

                await _context.SaveChangesAsync();
            }
        }

        private async Task DeleteGameAsync(int id)
        {
            var game = await _context.MiniGames.FindAsync(id);
            if (game != null)
            {
                _context.MiniGames.Remove(game);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<GameStatisticsModel> GetGameStatisticsAsync(string period)
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

            var totalPlays = await _context.GameRecords
                .Where(g => g.StartTime >= startDate)
                .CountAsync();

            var uniquePlayers = await _context.GameRecords
                .Where(g => g.StartTime >= startDate)
                .Select(g => g.UserId)
                .Distinct()
                .CountAsync();

            var totalPointsAwarded = await _context.GameRecords
                .Where(g => g.StartTime >= startDate)
                .SumAsync(g => g.PointsGained);

            var totalExpAwarded = await _context.GameRecords
                .Where(g => g.StartTime >= startDate)
                .SumAsync(g => g.ExpGained);

            return new GameStatisticsModel
            {
                Period = period,
                TotalPlays = totalPlays,
                UniquePlayers = uniquePlayers,
                TotalPointsAwarded = totalPointsAwarded,
                TotalExpAwarded = totalExpAwarded
            };
        }

        private List<GameRuleModel> CreateDefaultGameRules()
        {
            return new List<GameRuleModel>
            {
                new GameRuleModel
                {
                    GameId = 1,
                    GameName = "冒險遊戲",
                    PointsReward = 100,
                    ExpReward = 50,
                    DailyLimit = 3,
                    IsActive = true
                }
            };
        }

        private async Task SaveDefaultGameRulesAsync(List<GameRuleModel> rules)
        {
            foreach (var rule in rules)
            {
                var gameRule = new GameRule
                {
                    GameId = rule.GameId,
                    PointsReward = rule.PointsReward,
                    ExpReward = rule.ExpReward,
                    DailyLimit = rule.DailyLimit,
                    IsActive = rule.IsActive
                };
                _context.GameRules.Add(gameRule);
            }
            await _context.SaveChangesAsync();
        }
    }

    // ViewModels
    public class MiniGameDashboardViewModel
    {
        public int TotalGames { get; set; }
        public int ActiveGames { get; set; }
        public int TotalPlays { get; set; }
        public int TodayPlays { get; set; }
        public int ThisMonthPlays { get; set; }
        public List<RecentGameModel> RecentGames { get; set; } = new();
        public List<TopPlayerModel> TopPlayers { get; set; } = new();
    }

    public class AdminGameListViewModel
    {
        public List<GameModel> Games { get; set; } = new();
        public GameQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class AdminGameRecordsViewModel
    {
        public List<GameRecordModel> GameRecords { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<MiniGame> Games { get; set; } = new();
        public GameRecordQueryModel Query { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class AdminMiniGameRulesViewModel
    {
        public List<GameRuleModel> Rules { get; set; } = new();
    }

    public class CreateGameViewModel
    {
        public string GameName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int PointsReward { get; set; }
        public int ExpReward { get; set; }
        public string Difficulty { get; set; } = string.Empty;
    }

    public class EditGameViewModel
    {
        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int PointsReward { get; set; }
        public int ExpReward { get; set; }
        public string Difficulty { get; set; } = string.Empty;
    }

    public class GameModel
    {
        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int PointsReward { get; set; }
        public int ExpReward { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
    }

    public class GameRecordModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Result { get; set; } = string.Empty;
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class GameRuleModel
    {
        public int RuleId { get; set; }
        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public int PointsReward { get; set; }
        public int ExpReward { get; set; }
        public int DailyLimit { get; set; }
        public bool IsActive { get; set; }
    }

    public class RecentGameModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Result { get; set; } = string.Empty;
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class TopPlayerModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalPlays { get; set; }
        public int TotalPoints { get; set; }
        public int TotalExp { get; set; }
    }

    public class GameStatisticsModel
    {
        public string Period { get; set; } = string.Empty;
        public int TotalPlays { get; set; }
        public int UniquePlayers { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int TotalExpAwarded { get; set; }
    }

    public class GameQueryModel
    {
        public string GameName { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GameRecordQueryModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int? GameId { get; set; }
        public string Result { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
