using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.social_hub.Auth;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme)]
    public class AdminMiniGameController : MiniGameBaseController
    {
        public AdminMiniGameController(GameSpacedatabaseContext context)
            : base(context)
        {
        }

        // GET: AdminMiniGame
        // Note: MiniGame entity represents game play records, not game definitions
        public async Task<IActionResult> Index(string searchTerm = "", string result = "", string sortBy = "recent", int page = 1, int pageSize = 10)
        {
            var query = _context.MiniGames.Include(g => g.User).AsQueryable();

            // 搜尋功能 - 依使用者ID或結果
            if (!string.IsNullOrEmpty(searchTerm) && int.TryParse(searchTerm, out int userId))
            {
                query = query.Where(g => g.UserId == userId);
            }

            // 遊戲結果篩選
            if (!string.IsNullOrEmpty(result))
            {
                query = query.Where(g => g.Result == result);
            }

            // 排序
            query = sortBy switch
            {
                "level" => query.OrderByDescending(g => g.Level),
                "points" => query.OrderByDescending(g => g.PointsGained),
                "exp" => query.OrderByDescending(g => g.ExpGained),
                _ => query.OrderByDescending(g => g.StartTime)
            };

            // 分頁
            var totalCount = await query.CountAsync();
            var miniGames = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new AdminMiniGameIndexViewModel
            {
                MiniGames = new PagedResult<GameSpace.Models.MiniGame>
                {
                    Items = miniGames,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                }
            };

            // 設定 ViewBag 用於搜尋和篩選
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Result = result;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalGames = totalCount;
            ViewBag.CompletedGames = await _context.MiniGames.CountAsync(g => g.Result == "勝利" || g.Result == "Win");
            ViewBag.AbortedGames = await _context.MiniGames.CountAsync(g => g.Aborted);
            ViewBag.TotalPointsAwarded = await _context.MiniGames.SumAsync(g => (int?)g.PointsGained) ?? 0;
            ViewBag.TotalExpAwarded = await _context.MiniGames.SumAsync(g => (int?)g.ExpGained) ?? 0;

            return View(viewModel);
        }

        // GET: AdminMiniGame/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGames
                .Include(g => g.User)
                .FirstOrDefaultAsync(m => m.PlayId == id);

            if (miniGame == null)
            {
                return NotFound();
            }

            return View(miniGame);
        }

        // GET: AdminMiniGame/Create
        public IActionResult Create()
        {
            ViewBag.Users = _context.Users.OrderBy(u => u.UserId).ToList();
            return View();
        }

        // POST: AdminMiniGame/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminMiniGameCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var miniGame = new GameSpace.Models.MiniGame
                {
                    UserId = model.UserId,
                    PetId = model.PetID,
                    Level = 1,
                    MonsterCount = 0,
                    SpeedMultiplier = 1.0m,
                    Result = model.Result ?? "進行中",
                    ExpGained = model.ExpEarned,
                    ExpGainedTime = DateTime.Now,
                    PointsGained = model.PointsEarned,
                    PointsGainedTime = DateTime.Now,
                    CouponGained = "",
                    CouponGainedTime = DateTime.Now,
                    HungerDelta = 0,
                    MoodDelta = 0,
                    StaminaDelta = 0,
                    CleanlinessDelta = 0,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Aborted = false
                };

                _context.Add(miniGame);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "遊戲記錄建立成功";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = _context.Users.OrderBy(u => u.UserId).ToList();
            return View(model);
        }

        // GET: AdminMiniGame/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGames.FindAsync(id);
            if (miniGame == null)
            {
                return NotFound();
            }

            var model = new AdminMiniGameCreateViewModel
            {
                UserId = miniGame.UserId,
                PetID = miniGame.PetId,
                StartTime = miniGame.StartTime,
                EndTime = miniGame.EndTime,
                Result = miniGame.Result,
                PointsEarned = miniGame.PointsGained,
                ExpEarned = miniGame.ExpGained,
                CouponEarned = 0
            };

            ViewBag.Users = _context.Users.OrderBy(u => u.UserId).ToList();
            return View(model);
        }

        // POST: AdminMiniGame/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminMiniGameCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var miniGame = await _context.MiniGames.FindAsync(id);
                    if (miniGame == null)
                    {
                        return NotFound();
                    }

                    miniGame.UserId = model.UserId;
                    miniGame.PetId = model.PetID;
                    miniGame.StartTime = model.StartTime;
                    miniGame.EndTime = model.EndTime;
                    miniGame.Result = model.Result ?? miniGame.Result;
                    miniGame.PointsGained = model.PointsEarned;
                    miniGame.PointsGainedTime = DateTime.Now;
                    miniGame.ExpGained = model.ExpEarned;
                    miniGame.ExpGainedTime = DateTime.Now;

                    _context.Update(miniGame);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "遊戲記錄更新成功";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MiniGameExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Users = _context.Users.OrderBy(u => u.UserId).ToList();
            return View(model);
        }

        // GET: AdminMiniGame/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var miniGame = await _context.MiniGames
                .Include(g => g.User)
                .FirstOrDefaultAsync(m => m.PlayId == id);

            if (miniGame == null)
            {
                return NotFound();
            }

            return View(miniGame);
        }

        // POST: AdminMiniGame/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var miniGame = await _context.MiniGames.FindAsync(id);
            if (miniGame != null)
            {
                _context.MiniGames.Remove(miniGame);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "遊戲記錄刪除成功";
            }

            return RedirectToAction(nameof(Index));
        }

        // 切換遊戲狀態 - 改為標記遊戲為中止
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var miniGame = await _context.MiniGames.FindAsync(id);
            if (miniGame != null)
            {
                miniGame.Aborted = !miniGame.Aborted;
                if (miniGame.Aborted && miniGame.EndTime == null)
                {
                    miniGame.EndTime = DateTime.Now;
                }
                _context.Update(miniGame);
                await _context.SaveChangesAsync();

                return Json(new { success = true, isAborted = miniGame.Aborted });
            }

            return Json(new { success = false });
        }

        // 獲取遊戲統計數據
        [HttpGet]
        public async Task<IActionResult> GetGameStats()
        {
            var stats = new
            {
                total = await _context.MiniGames.CountAsync(),
                completed = await _context.MiniGames.CountAsync(g => g.Result == "勝利" || g.Result == "Win"),
                aborted = await _context.MiniGames.CountAsync(g => g.Aborted),
                inProgress = await _context.MiniGames.CountAsync(g => g.EndTime == null && !g.Aborted),
                totalPointsAwarded = await _context.MiniGames.SumAsync(g => (int?)g.PointsGained) ?? 0,
                totalExpAwarded = await _context.MiniGames.SumAsync(g => (int?)g.ExpGained) ?? 0
            };

            return Json(stats);
        }

        // 獲取遊戲結果分佈
        [HttpGet]
        public async Task<IActionResult> GetGameTypeDistribution()
        {
            var distribution = await _context.MiniGames
                .GroupBy(g => g.Result)
                .Select(g => new
                {
                    type = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(g => g.count)
                .ToListAsync();

            return Json(distribution);
        }

        // 獲取遊戲遊玩統計 (前10名使用者)
        [HttpGet]
        public async Task<IActionResult> GetGamePlayStats()
        {
            var stats = await _context.MiniGames
                .GroupBy(g => g.UserId)
                .Select(g => new
                {
                    userId = g.Key,
                    userName = _context.Users.Where(u => u.UserId == g.Key).Select(u => u.UserName).FirstOrDefault(),
                    playCount = g.Count(),
                    totalPoints = g.Sum(x => x.PointsGained),
                    totalExp = g.Sum(x => x.ExpGained),
                    avgLevel = g.Average(x => (double)x.Level)
                })
                .OrderByDescending(g => g.playCount)
                .Take(10)
                .ToListAsync();

            return Json(stats);
        }

        // 獲取遊戲獎勵統計
        [HttpGet]
        public async Task<IActionResult> GetGameRevenueStats()
        {
            var stats = await _context.MiniGames
                .Where(g => g.EndTime != null)
                .GroupBy(g => g.EndTime.Value.Date)
                .Select(g => new
                {
                    date = g.Key,
                    playCount = g.Count(),
                    totalPoints = g.Sum(x => x.PointsGained),
                    totalExp = g.Sum(x => x.ExpGained),
                    completedGames = g.Count(x => x.Result == "勝利" || x.Result == "Win")
                })
                .OrderByDescending(g => g.date)
                .Take(30)
                .ToListAsync();

            return Json(stats);
        }

        // 新增：每日遊戲次數限制設定
        // NOTE: DailyGameLimits DbSet doesn't exist - commented out
        // TODO: Create DailyGameLimit entity and add DbSet if this functionality is needed
        /*
        [HttpGet]
        public async Task<IActionResult> GetDailyGameLimit()
        {
            try
            {
                var limit = await _context.DailyGameLimits.FirstOrDefaultAsync();
                if (limit == null)
                {
                    // 如果沒有設定，返回預設值（一天三次）
                    return Json(new { success = true, data = new { dailyLimit = 3 } });
                }

                return Json(new { success = true, data = new { dailyLimit = limit.DailyLimit } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDailyGameLimit(int dailyLimit)
        {
            try
            {
                if (dailyLimit < 1)
                {
                    return Json(new { success = false, message = "每日遊戲次數限制不能小於1" });
                }

                var limit = await _context.DailyGameLimits.FirstOrDefaultAsync();
                if (limit == null)
                {
                    limit = new DailyGameLimit { DailyLimit = dailyLimit };
                    _context.DailyGameLimits.Add(limit);
                }
                else
                {
                    limit.DailyLimit = dailyLimit;
                    _context.DailyGameLimits.Update(limit);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "每日遊戲次數限制設定成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        */

        // 新增：獎勵種類詳細設定
        // NOTE: GameRewardSettings DbSet doesn't exist - commented out
        // TODO: Create GameRewardSettings entity and add DbSet if this functionality is needed
        /*
        [HttpGet]
        public async Task<IActionResult> GetGameRewardSettings()
        {
            try
            {
                var settings = await _context.GameRewardSettings.FirstOrDefaultAsync();
                if (settings == null)
                {
                    // 如果沒有設定，返回預設值
                    return Json(new {
                        success = true,
                        data = new {
                            pointsRewardRate = 0.1,
                            expRewardRate = 0.05,
                            couponRewardRate = 0.02,
                            pointsRewardEnabled = true,
                            expRewardEnabled = true,
                            couponRewardEnabled = true
                        }
                    });
                }

                return Json(new {
                    success = true,
                    data = new {
                        pointsRewardRate = settings.PointsRewardRate,
                        expRewardRate = settings.ExpRewardRate,
                        couponRewardRate = settings.CouponRewardRate,
                        pointsRewardEnabled = settings.PointsRewardEnabled,
                        expRewardEnabled = settings.ExpRewardEnabled,
                        couponRewardEnabled = settings.CouponRewardEnabled
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGameRewardSettings(
            decimal pointsRewardRate,
            decimal expRewardRate,
            decimal couponRewardRate,
            bool pointsRewardEnabled,
            bool expRewardEnabled,
            bool couponRewardEnabled)
        {
            try
            {
                if (pointsRewardRate < 0 || expRewardRate < 0 || couponRewardRate < 0)
                {
                    return Json(new { success = false, message = "獎勵比例不能為負數" });
                }

                var settings = await _context.GameRewardSettings.FirstOrDefaultAsync();
                if (settings == null)
                {
                    settings = new GameRewardSettings
                    {
                        PointsRewardRate = pointsRewardRate,
                        ExpRewardRate = expRewardRate,
                        CouponRewardRate = couponRewardRate,
                        PointsRewardEnabled = pointsRewardEnabled,
                        ExpRewardEnabled = expRewardEnabled,
                        CouponRewardEnabled = couponRewardEnabled
                    };
                    _context.GameRewardSettings.Add(settings);
                }
                else
                {
                    settings.PointsRewardRate = pointsRewardRate;
                    settings.ExpRewardRate = expRewardRate;
                    settings.CouponRewardRate = couponRewardRate;
                    settings.PointsRewardEnabled = pointsRewardEnabled;
                    settings.ExpRewardEnabled = expRewardEnabled;
                    settings.CouponRewardEnabled = couponRewardEnabled;
                    _context.GameRewardSettings.Update(settings);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "遊戲獎勵設定更新成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        */

        private bool MiniGameExists(int id)
        {
            return _context.MiniGames.Any(e => e.PlayId == id);
        }
    }
}

