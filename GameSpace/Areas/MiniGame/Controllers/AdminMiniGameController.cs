using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Route("MiniGame/[controller]")]
    public class AdminMiniGameController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMiniGameController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminMiniGameViewModel
            {
                TotalGamesPlayed = await _context.MiniGame.CountAsync(),
                TodayGamesPlayed = await _context.MiniGame
                    .Where(g => g.StartTime.Date == DateTime.Today)
                    .CountAsync(),
                WinRate = await CalculateWinRateAsync(),
                AverageGameDuration = await CalculateAverageGameDurationAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetGameOverview()
        {
            try
            {
                var data = new
                {
                    totalGamesPlayed = await _context.MiniGame.CountAsync(),
                    todayGamesPlayed = await _context.MiniGame
                        .Where(g => g.StartTime.Date == DateTime.Today)
                        .CountAsync(),
                    winRate = await CalculateWinRateAsync(),
                    abortRate = await CalculateAbortRateAsync(),
                    averageLevel = await _context.MiniGame.AverageAsync(g => (double)g.Level),
                    totalExpAwarded = await _context.MiniGame.SumAsync(g => g.ExpGained),
                    totalPointsAwarded = await _context.MiniGame.SumAsync(g => g.PointsGained),
                    gameRecords = await GetGameRecordsAsync()
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetGameRecords(int page = 1, int pageSize = 50)
        {
            try
            {
                var games = await _context.MiniGame
                    .Include(g => g.User)
                    .Include(g => g.Pet)
                    .OrderByDescending(g => g.StartTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => new
                    {
                        playId = g.PlayID,
                        userId = g.UserID,
                        userName = g.User.User_name,
                        petId = g.PetID,
                        petName = g.Pet.PetName,
                        level = g.Level,
                        monsterCount = g.MonsterCount,
                        speedMultiplier = g.SpeedMultiplier,
                        result = g.Result,
                        expGained = g.ExpGained,
                        pointsGained = g.PointsGained,
                        couponGained = g.CouponGained,
                        startTime = g.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        endTime = g.EndTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                        duration = g.EndTime.HasValue 
                            ? (g.EndTime.Value - g.StartTime).TotalMinutes.ToString("F1") + " 分鐘"
                            : "未完成",
                        aborted = g.Aborted,
                        petEffects = new
                        {
                            hungerDelta = g.HungerDelta,
                            moodDelta = g.MoodDelta,
                            staminaDelta = g.StaminaDelta,
                            cleanlinessDelta = g.CleanlinessDelta
                        }
                    })
                    .ToListAsync();

                return Json(new { success = true, data = games });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetGameStatistics()
        {
            try
            {
                var totalGames = await _context.MiniGame.CountAsync();
                var winCount = await _context.MiniGame.Where(g => g.Result == "Win").CountAsync();
                var loseCount = await _context.MiniGame.Where(g => g.Result == "Lose").CountAsync();
                var abortCount = await _context.MiniGame.Where(g => g.Aborted).CountAsync();

                // Level distribution
                var levelDistribution = await _context.MiniGame
                    .GroupBy(g => g.Level)
                    .Select(group => new
                    {
                        level = group.Key,
                        count = group.Count(),
                        winRate = group.Count(g => g.Result == "Win") * 100.0 / group.Count()
                    })
                    .OrderBy(x => x.level)
                    .ToListAsync();

                // Daily play statistics for the last 30 days
                var thirtyDaysAgo = DateTime.Today.AddDays(-30);
                var dailyStats = await _context.MiniGame
                    .Where(g => g.StartTime.Date >= thirtyDaysAgo)
                    .GroupBy(g => g.StartTime.Date)
                    .Select(group => new
                    {
                        date = group.Key.ToString("yyyy-MM-dd"),
                        totalGames = group.Count(),
                        winCount = group.Count(g => g.Result == "Win"),
                        loseCount = group.Count(g => g.Result == "Lose"),
                        abortCount = group.Count(g => g.Aborted)
                    })
                    .OrderBy(x => x.date)
                    .ToListAsync();

                // Top performing players
                var topPlayers = await _context.MiniGame
                    .Include(g => g.User)
                    .GroupBy(g => new { g.UserID, g.User.User_name })
                    .Select(group => new
                    {
                        userId = group.Key.UserID,
                        userName = group.Key.User_name,
                        totalGames = group.Count(),
                        winCount = group.Count(g => g.Result == "Win"),
                        winRate = group.Count(g => g.Result == "Win") * 100.0 / group.Count(),
                        totalExp = group.Sum(g => g.ExpGained),
                        totalPoints = group.Sum(g => g.PointsGained)
                    })
                    .OrderByDescending(x => x.winRate)
                    .ThenByDescending(x => x.totalGames)
                    .Take(10)
                    .ToListAsync();

                var data = new
                {
                    overview = new
                    {
                        totalGames = totalGames,
                        winCount = winCount,
                        loseCount = loseCount,
                        abortCount = abortCount,
                        winRate = totalGames > 0 ? winCount * 100.0 / totalGames : 0,
                        abortRate = totalGames > 0 ? abortCount * 100.0 / totalGames : 0
                    },
                    levelDistribution = levelDistribution,
                    dailyStats = dailyStats,
                    topPlayers = topPlayers
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetGameDetails(int playId)
        {
            try
            {
                var game = await _context.MiniGame
                    .Include(g => g.User)
                    .Include(g => g.Pet)
                    .FirstOrDefaultAsync(g => g.PlayID == playId);

                if (game == null)
                {
                    return Json(new { success = false, message = "遊戲記錄不存在" });
                }

                var data = new
                {
                    playId = game.PlayID,
                    user = new
                    {
                        userId = game.UserID,
                        userName = game.User.User_name
                    },
                    pet = new
                    {
                        petId = game.PetID,
                        petName = game.Pet.PetName,
                        level = game.Pet.Level,
                        beforeGame = new
                        {
                            hunger = game.Pet.Hunger + game.HungerDelta,
                            mood = game.Pet.Mood + game.MoodDelta,
                            stamina = game.Pet.Stamina + game.StaminaDelta,
                            cleanliness = game.Pet.Cleanliness + game.CleanlinessDelta
                        },
                        afterGame = new
                        {
                            hunger = game.Pet.Hunger,
                            mood = game.Pet.Mood,
                            stamina = game.Pet.Stamina,
                            cleanliness = game.Pet.Cleanliness
                        }
                    },
                    gameSettings = new
                    {
                        level = game.Level,
                        monsterCount = game.MonsterCount,
                        speedMultiplier = game.SpeedMultiplier
                    },
                    result = new
                    {
                        outcome = game.Result,
                        aborted = game.Aborted,
                        expGained = game.ExpGained,
                        pointsGained = game.PointsGained,
                        couponGained = game.CouponGained != "0" ? game.CouponGained : null
                    },
                    timing = new
                    {
                        startTime = game.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        endTime = game.EndTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                        duration = game.EndTime.HasValue 
                            ? (game.EndTime.Value - game.StartTime).TotalSeconds.ToString("F1") + " 秒"
                            : "未完成"
                    },
                    petEffects = new
                    {
                        hungerDelta = game.HungerDelta,
                        moodDelta = game.MoodDelta,
                        staminaDelta = game.StaminaDelta,
                        cleanlinessDelta = game.CleanlinessDelta
                    }
                };

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> UpdateGameRewards(int playId, int expGained, int pointsGained)
        {
            try
            {
                var game = await _context.MiniGame
                    .Include(g => g.User)
                    .Include(g => g.Pet)
                    .FirstOrDefaultAsync(g => g.PlayID == playId);

                if (game == null)
                {
                    return Json(new { success = false, message = "遊戲記錄不存在" });
                }

                if (expGained < 0 || pointsGained < 0)
                {
                    return Json(new { success = false, message = "獎勵值不能為負數" });
                }

                var oldExp = game.ExpGained;
                var oldPoints = game.PointsGained;

                game.ExpGained = expGained;
                game.PointsGained = pointsGained;

                // Update pet experience
                if (expGained != oldExp)
                {
                    game.Pet.Experience += (expGained - oldExp);
                    if (game.Pet.Experience < 0) game.Pet.Experience = 0;
                }

                // Update user wallet
                if (pointsGained != oldPoints)
                {
                    var wallet = await _context.User_Wallet.FirstOrDefaultAsync(w => w.User_Id == game.UserID);
                    if (wallet != null)
                    {
                        var pointsDiff = pointsGained - oldPoints;
                        wallet.User_Point += pointsDiff;
                        if (wallet.User_Point < 0) wallet.User_Point = 0;

                        // Add wallet history
                        var history = new WalletHistory
                        {
                            UserID = game.UserID,
                            ChangeType = "Game",
                            PointsChanged = pointsDiff,
                            Description = $"遊戲獎勵調整 (PlayID: {playId})",
                            ChangeTime = DateTime.UtcNow
                        };
                        _context.WalletHistory.Add(history);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "遊戲獎勵更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<double> CalculateWinRateAsync()
        {
            var totalGames = await _context.MiniGame.CountAsync();
            if (totalGames == 0) return 0;

            var winCount = await _context.MiniGame.Where(g => g.Result == "Win").CountAsync();
            return (double)winCount / totalGames * 100;
        }

        private async Task<double> CalculateAbortRateAsync()
        {
            var totalGames = await _context.MiniGame.CountAsync();
            if (totalGames == 0) return 0;

            var abortCount = await _context.MiniGame.Where(g => g.Aborted).CountAsync();
            return (double)abortCount / totalGames * 100;
        }

        private async Task<double> CalculateAverageGameDurationAsync()
        {
            var completedGames = await _context.MiniGame
                .Where(g => g.EndTime.HasValue && !g.Aborted)
                .ToListAsync();

            if (!completedGames.Any()) return 0;

            var totalMinutes = completedGames
                .Sum(g => (g.EndTime.Value - g.StartTime).TotalMinutes);

            return totalMinutes / completedGames.Count;
        }

        private async Task<List<object>> GetGameRecordsAsync()
        {
            return await _context.MiniGame
                .Include(g => g.User)
                .Include(g => g.Pet)
                .OrderByDescending(g => g.StartTime)
                .Take(20)
                .Select(g => new
                {
                    playId = g.PlayID,
                    userName = g.User.User_name,
                    petName = g.Pet.PetName,
                    level = g.Level,
                    result = g.Result,
                    expGained = g.ExpGained,
                    pointsGained = g.PointsGained,
                    startTime = g.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    aborted = g.Aborted
                })
                .Cast<object>()
                .ToListAsync();
        }
    }
}
