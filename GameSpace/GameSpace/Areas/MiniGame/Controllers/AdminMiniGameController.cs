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
                var game = await GetGameByIdAsync(id);
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
                return Json(new { success = true, message = "小遊戲刪除成功！" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 遊戲記錄查詢
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
                TempData["ErrorMessage"] = $"查詢遊戲記錄時發生錯誤：{ex.Message}";
                return View(new AdminGameRecordsViewModel());
            }
        }

        // 遊戲統計資料
        [HttpGet]
        public async Task<IActionResult> GameStats(string period = "today")
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

        // 獲取遊戲詳情
        [HttpGet]
        public async Task<IActionResult> GetGameDetails(int gameId)
        {
            try
            {
                var game = await GetGameByIdAsync(gameId);
                if (game == null)
                    return Json(new { success = false, message = "找不到指定的小遊戲" });

                var gameDetails = new
                {
                    gameId = game.GameId,
                    gameName = game.GameName,
                    description = game.Description,
                    isActive = game.IsActive,
                    pointsReward = game.PointsReward,
                    expReward = game.ExpReward,
                    difficulty = game.Difficulty,
                    createdTime = game.CreatedTime
                };

                return Json(new { success = true, data = gameDetails });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 獲取用戶遊戲記錄
        [HttpGet]
        public async Task<IActionResult> GetUserGameRecords(int userId, int days = 30)
        {
            try
            {
                var records = await GetUserGameRecordsAsync(userId, days);
                return Json(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 切換遊戲狀態
        [HttpPost]
        public async Task<IActionResult> ToggleGameStatus(int gameId)
        {
            try
            {
                await ToggleGameStatusAsync(gameId);
                return Json(new { success = true, message = "遊戲狀態切換成功！" });
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
            var totalPlays = await _context.MiniGames.CountAsync();
            var totalPointsAwarded = await _context.MiniGames.SumAsync(g => g.PointsGained);
            var totalExpAwarded = await _context.MiniGames.SumAsync(g => g.ExpGained);

            var todayPlays = await _context.MiniGames
                .Where(g => g.StartTime.Date == today)
                .CountAsync();

            var thisMonthPlays = await _context.MiniGames
                .Where(g => g.StartTime >= thisMonth)
                .CountAsync();

            var recentGames = await _context.MiniGames
                .Include(g => g.User)
                .OrderByDescending(g => g.StartTime)
                .Take(10)
                .Select(g => new RecentGameModel
                {
                    PlayId = g.PlayId,
                    UserId = g.UserId,
                    UserName = g.User.UserName,
                    GameName = g.GameName,
                    Level = g.Level,
                    Result = g.Result,
                    PointsGained = g.PointsGained,
                    ExpGained = g.ExpGained,
                    StartTime = g.StartTime
                })
                .ToListAsync();

            var topPlayers = await _context.MiniGames
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
                .OrderByDescending(p => p.TotalPoints)
                .Take(10)
                .ToListAsync();

            return new MiniGameDashboardViewModel
            {
                TotalGames = totalGames,
                ActiveGames = activeGames,
                TotalPlays = totalPlays,
                TotalPointsAwarded = totalPointsAwarded,
                TotalExpAwarded = totalExpAwarded,
                TodayPlays = todayPlays,
                ThisMonthPlays = thisMonthPlays,
                RecentGames = recentGames,
                TopPlayers = topPlayers
            };
        }

        private async Task<PagedResult<GameModel>> QueryGamesAsync(GameQueryModel query)
        {
            var queryable = _context.MiniGames.AsQueryable();

            if (!string.IsNullOrEmpty(query.GameName))
                queryable = queryable.Where(g => g.GameName.Contains(query.GameName));

            if (query.IsActive.HasValue)
                queryable = queryable.Where(g => g.IsActive == query.IsActive.Value);

            if (query.MinPointsReward.HasValue)
                queryable = queryable.Where(g => g.PointsReward >= query.MinPointsReward.Value);

            if (query.MaxPointsReward.HasValue)
                queryable = queryable.Where(g => g.PointsReward <= query.MaxPointsReward.Value);

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

        private async Task<PagedResult<GameRecordModel>> QueryGameRecordsAsync(GameRecordQueryModel query)
        {
            var queryable = _context.MiniGames
                .Include(g => g.User)
                .AsQueryable();

            if (query.UserId.HasValue)
                queryable = queryable.Where(g => g.UserId == query.UserId.Value);

            if (!string.IsNullOrEmpty(query.UserName))
                queryable = queryable.Where(g => g.User.UserName.Contains(query.UserName));

            if (!string.IsNullOrEmpty(query.GameName))
                queryable = queryable.Where(g => g.GameName.Contains(query.GameName));

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
                    GameName = g.GameName,
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

        private async Task<GameModel> GetGameByIdAsync(int gameId)
        {
            var game = await _context.MiniGames.FindAsync(gameId);
            if (game == null)
                return null;

            return new GameModel
            {
                GameId = game.GameId,
                GameName = game.GameName,
                Description = game.Description,
                IsActive = game.IsActive,
                PointsReward = game.PointsReward,
                ExpReward = game.ExpReward,
                Difficulty = game.Difficulty,
                CreatedTime = game.CreatedTime
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
            if (game == null)
                throw new Exception("找不到指定的小遊戲");

            game.GameName = model.GameName;
            game.Description = model.Description;
            game.IsActive = model.IsActive;
            game.PointsReward = model.PointsReward;
            game.ExpReward = model.ExpReward;
            game.Difficulty = model.Difficulty;

            await _context.SaveChangesAsync();
        }

        private async Task DeleteGameAsync(int gameId)
        {
            var game = await _context.MiniGames.FindAsync(gameId);
            if (game == null)
                throw new Exception("找不到指定的小遊戲");

            _context.MiniGames.Remove(game);
            await _context.SaveChangesAsync();
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

            var totalPlays = await _context.MiniGames
                .Where(g => g.StartTime >= startDate)
                .CountAsync();

            var uniquePlayers = await _context.MiniGames
                .Where(g => g.StartTime >= startDate)
                .Select(g => g.UserId)
                .Distinct()
                .CountAsync();

            var totalPointsAwarded = await _context.MiniGames
                .Where(g => g.StartTime >= startDate)
                .SumAsync(g => g.PointsGained);

            var totalExpAwarded = await _context.MiniGames
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

        private async Task<List<GameRecordModel>> GetUserGameRecordsAsync(int userId, int days)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            var records = await _context.MiniGames
                .Where(g => g.UserId == userId && g.StartTime >= startDate)
                .OrderByDescending(g => g.StartTime)
                .Select(g => new GameRecordModel
                {
                    PlayId = g.PlayId,
                    UserId = g.UserId,
                    UserName = g.User.UserName,
                    GameName = g.GameName,
                    Level = g.Level,
                    Result = g.Result,
                    PointsGained = g.PointsGained,
                    ExpGained = g.ExpGained,
                    StartTime = g.StartTime
                })
                .ToListAsync();

            return records;
        }

        private async Task ToggleGameStatusAsync(int gameId)
        {
            var game = await _context.MiniGames.FindAsync(gameId);
            if (game == null)
                throw new Exception("找不到指定的小遊戲");

            game.IsActive = !game.IsActive;
            await _context.SaveChangesAsync();
        }
    }

    // ViewModels
    public class GameQueryModel
    {
        public string GameName { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public int? MinPointsReward { get; set; }
        public int? MaxPointsReward { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GameRecordQueryModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class MiniGameDashboardViewModel
    {
        public int TotalGames { get; set; }
        public int ActiveGames { get; set; }
        public int TotalPlays { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int TotalExpAwarded { get; set; }
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
}
