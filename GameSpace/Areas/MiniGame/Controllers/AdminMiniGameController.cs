using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class AdminMiniGameController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMiniGameController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MiniGame/AdminMiniGame
        public async Task<IActionResult> Index()
        {
            var viewModel = new MiniGameOverviewViewModel();

            // 統計數據
            viewModel.TotalGames = await _context.MiniGames.CountAsync();
            viewModel.TodayGames = await _context.MiniGames
                .Where(g => g.StartTime.Date == DateTime.Today)
                .CountAsync();
            viewModel.WinGames = await _context.MiniGames
                .Where(g => g.Result == "win")
                .CountAsync();
            viewModel.LoseGames = await _context.MiniGames
                .Where(g => g.Result == "lose")
                .CountAsync();
            viewModel.AbortGames = await _context.MiniGames
                .Where(g => g.Result == "abort")
                .CountAsync();

            // 遊戲結果分布
            viewModel.ResultDistribution = await _context.MiniGames
                .GroupBy(g => g.Result)
                .Select(g => new GameResultDistributionViewModel
                {
                    Result = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // 最近遊戲記錄
            viewModel.RecentGames = await _context.MiniGames
                .Include(g => g.User)
                .OrderByDescending(g => g.StartTime)
                .Take(50)
                .Select(g => new MiniGameViewModel
                {
                    GameID = g.GameID,
                    UserID = g.UserID,
                    UserName = g.User.User_name,
                    StartTime = g.StartTime,
                    EndTime = g.EndTime,
                    Result = g.Result,
                    PointsReward = g.PointsReward,
                    PetExpReward = g.PetExpReward,
                    CouponReward = g.CouponReward
                })
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/AdminMiniGame/GameRuleSettings
        public async Task<IActionResult> GameRuleSettings()
        {
            var settings = await _context.MiniGameRuleSettings
                .OrderBy(s => s.SettingID)
                .ToListAsync();

            return View(settings);
        }

        // POST: MiniGame/AdminMiniGame/UpdateGameRule
        [HttpPost]
        public async Task<IActionResult> UpdateGameRule([FromBody] UpdateGameRuleRequest request)
        {
            try
            {
                var setting = await _context.MiniGameRuleSettings
                    .FirstOrDefaultAsync(s => s.SettingID == request.SettingID);

                if (setting == null)
                {
                    return Json(new { success = false, message = "設定不存在" });
                }

                setting.SettingValue = request.SettingValue;
                setting.Description = request.Description;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedByManagerId = GetCurrentManagerId();

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "遊戲規則更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新失敗: {ex.Message}" });
            }
        }

        // POST: MiniGame/AdminMiniGame/CreateGameRule
        [HttpPost]
        public async Task<IActionResult> CreateGameRule([FromBody] CreateGameRuleRequest request)
        {
            try
            {
                var setting = new MiniGameRuleSettings
                {
                    SettingName = request.SettingName,
                    SettingValue = request.SettingValue,
                    Description = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedByManagerId = GetCurrentManagerId()
                };

                _context.MiniGameRuleSettings.Add(setting);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "遊戲規則創建成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"創建失敗: {ex.Message}" });
            }
        }

        // GET: MiniGame/AdminMiniGame/MemberGameRecords
        public async Task<IActionResult> MemberGameRecords(int? page, string searchTerm, string result)
        {
            var pageSize = 20;
            var pageNumber = page ?? 1;

            var query = _context.MiniGames
                .Include(g => g.User)
                .AsQueryable();

            // 搜尋功能
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(g => g.User.User_name.Contains(searchTerm) ||
                                       g.User.User_Account.Contains(searchTerm));
            }

            // 結果篩選
            if (!string.IsNullOrEmpty(result))
            {
                query = query.Where(g => g.Result == result);
            }

            var totalCount = await query.CountAsync();
            var games = await query
                .OrderByDescending(g => g.StartTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new MiniGameViewModel
                {
                    GameID = g.GameID,
                    UserID = g.UserID,
                    UserName = g.User.User_name,
                    UserAccount = g.User.User_Account,
                    StartTime = g.StartTime,
                    EndTime = g.EndTime,
                    Result = g.Result,
                    PointsReward = g.PointsReward,
                    PetExpReward = g.PetExpReward,
                    CouponReward = g.CouponReward,
                    SessionId = g.SessionId
                })
                .ToListAsync();

            var viewModel = new MemberGameRecordsViewModel
            {
                Games = games,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                TotalCount = totalCount,
                SearchTerm = searchTerm,
                Result = result
            };

            return View(viewModel);
        }

        // GET: MiniGame/AdminMiniGame/MemberGameDetail/{gameId}
        public async Task<IActionResult> MemberGameDetail(int gameId)
        {
            var game = await _context.MiniGames
                .Include(g => g.User)
                .Include(g => g.User.UserIntroduce)
                .FirstOrDefaultAsync(g => g.GameID == gameId);

            if (game == null)
            {
                return NotFound();
            }

            var viewModel = new MemberGameDetailViewModel
            {
                GameID = game.GameID,
                UserID = game.UserID,
                UserName = game.User.User_name,
                UserAccount = game.User.User_Account,
                NickName = game.User.UserIntroduce?.User_NickName ?? "",
                StartTime = game.StartTime,
                EndTime = game.EndTime,
                Result = game.Result,
                PointsReward = game.PointsReward,
                PetExpReward = game.PetExpReward,
                CouponReward = game.CouponReward,
                SessionId = game.SessionId,
                GameDuration = game.EndTime.HasValue ? 
                    (game.EndTime.Value - game.StartTime).TotalMinutes : 0
            };

            return View(viewModel);
        }

        // GET: MiniGame/AdminMiniGame/GameStatistics
        public async Task<IActionResult> GameStatistics()
        {
            var viewModel = new GameStatisticsViewModel();

            // 每日遊戲統計
            viewModel.DailyGameStats = await _context.MiniGames
                .Where(g => g.StartTime.Date >= DateTime.Today.AddDays(-30))
                .GroupBy(g => g.StartTime.Date)
                .Select(g => new DailyGameStatViewModel
                {
                    Date = g.Key,
                    GameCount = g.Count(),
                    WinCount = g.Count(gg => gg.Result == "win"),
                    LoseCount = g.Count(gg => gg.Result == "lose"),
                    AbortCount = g.Count(gg => gg.Result == "abort"),
                    TotalPointsReward = g.Sum(gg => gg.PointsReward ?? 0),
                    TotalPetExpReward = g.Sum(gg => gg.PetExpReward ?? 0)
                })
                .OrderBy(s => s.Date)
                .ToListAsync();

            // 用戶遊戲統計
            viewModel.UserGameStats = await _context.MiniGames
                .Include(g => g.User)
                .GroupBy(g => g.UserID)
                .Select(g => new UserGameStatViewModel
                {
                    UserID = g.Key,
                    UserName = g.First().User.User_name,
                    GameCount = g.Count(),
                    WinCount = g.Count(gg => gg.Result == "win"),
                    LoseCount = g.Count(gg => gg.Result == "lose"),
                    AbortCount = g.Count(gg => gg.Result == "abort"),
                    TotalPointsReward = g.Sum(gg => gg.PointsReward ?? 0),
                    TotalPetExpReward = g.Sum(gg => gg.PetExpReward ?? 0),
                    WinRate = g.Count() > 0 ? (double)g.Count(gg => gg.Result == "win") / g.Count() * 100 : 0
                })
                .OrderByDescending(s => s.GameCount)
                .Take(20)
                .ToListAsync();

            // 獎勵統計
            viewModel.RewardStats = await _context.MiniGames
                .Where(g => g.Result == "win")
                .GroupBy(g => g.StartTime.Date)
                .Select(g => new GameRewardStatViewModel
                {
                    Date = g.Key,
                    TotalPointsReward = g.Sum(gg => gg.PointsReward ?? 0),
                    TotalPetExpReward = g.Sum(gg => gg.PetExpReward ?? 0),
                    CouponRewardCount = g.Count(gg => !string.IsNullOrEmpty(gg.CouponReward))
                })
                .OrderByDescending(s => s.Date)
                .Take(30)
                .ToListAsync();

            return View(viewModel);
        }

        // GET: MiniGame/AdminMiniGame/UserGameLimits
        public async Task<IActionResult> UserGameLimits()
        {
            var viewModel = new UserGameLimitsViewModel();

            // 獲取每日遊戲次數限制設定
            var dailyLimitSetting = await _context.MiniGameRuleSettings
                .FirstOrDefaultAsync(s => s.SettingName == "DailyGameLimit");
            
            viewModel.DailyGameLimit = dailyLimitSetting != null ? 
                int.Parse(dailyLimitSetting.SettingValue) : 3;

            // 用戶今日遊戲次數統計
            viewModel.UserTodayGameCounts = await _context.MiniGames
                .Include(g => g.User)
                .Where(g => g.StartTime.Date == DateTime.Today)
                .GroupBy(g => g.UserID)
                .Select(g => new UserTodayGameCountViewModel
                {
                    UserID = g.Key,
                    UserName = g.First().User.User_name,
                    TodayGameCount = g.Count(),
                    HasReachedLimit = g.Count() >= viewModel.DailyGameLimit
                })
                .OrderByDescending(u => u.TodayGameCount)
                .ToListAsync();

            return View(viewModel);
        }

        // POST: MiniGame/AdminMiniGame/ResetUserGameLimit
        [HttpPost]
        public async Task<IActionResult> ResetUserGameLimit([FromBody] ResetUserGameLimitRequest request)
        {
            try
            {
                // 這裡可以實現重置用戶遊戲限制的邏輯
                // 例如：刪除今日的遊戲記錄或重置計數器
                
                var todayGames = await _context.MiniGames
                    .Where(g => g.UserID == request.UserId && g.StartTime.Date == DateTime.Today)
                    .ToListAsync();

                if (todayGames.Any())
                {
                    _context.MiniGames.RemoveRange(todayGames);
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "用戶遊戲限制已重置" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"重置失敗: {ex.Message}" });
            }
        }

        // GET: MiniGame/AdminMiniGame/GameSessionManagement
        public async Task<IActionResult> GameSessionManagement()
        {
            var viewModel = new GameSessionManagementViewModel();

            // 活躍遊戲會話
            viewModel.ActiveSessions = await _context.MiniGames
                .Include(g => g.User)
                .Where(g => g.EndTime == null)
                .OrderByDescending(g => g.StartTime)
                .Select(g => new GameSessionViewModel
                {
                    GameID = g.GameID,
                    UserID = g.UserID,
                    UserName = g.User.User_name,
                    SessionId = g.SessionId,
                    StartTime = g.StartTime,
                    Duration = (DateTime.UtcNow - g.StartTime).TotalMinutes
                })
                .ToListAsync();

            // 會話統計
            viewModel.TotalSessions = await _context.MiniGames.CountAsync();
            viewModel.ActiveSessionsCount = viewModel.ActiveSessions.Count;
            viewModel.CompletedSessionsCount = viewModel.TotalSessions - viewModel.ActiveSessionsCount;

            return View(viewModel);
        }

        // POST: MiniGame/AdminMiniGame/ForceEndGame
        [HttpPost]
        public async Task<IActionResult> ForceEndGame([FromBody] ForceEndGameRequest request)
        {
            try
            {
                var game = await _context.MiniGames
                    .FirstOrDefaultAsync(g => g.GameID == request.GameId);

                if (game == null)
                {
                    return Json(new { success = false, message = "遊戲不存在" });
                }

                if (game.EndTime.HasValue)
                {
                    return Json(new { success = false, message = "遊戲已經結束" });
                }

                game.EndTime = DateTime.UtcNow;
                game.Result = "abort"; // 強制結束標記為abort

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "遊戲已強制結束" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"強制結束失敗: {ex.Message}" });
            }
        }

        // 輔助方法
        private int GetCurrentManagerId()
        {
            // 這裡應該從當前登入的管理員中獲取ID
            // 實際應用中需要從Claims或Session中獲取
            return 1; // 暫時返回1，實際應用中需要修改
        }
    }

    // 請求模型
    public class UpdateGameRuleRequest
    {
        public int SettingID { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
    }

    public class CreateGameRuleRequest
    {
        public string SettingName { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
    }

    public class ResetUserGameLimitRequest
    {
        public int UserId { get; set; }
    }

    public class ForceEndGameRequest
    {
        public int GameId { get; set; }
    }
}
