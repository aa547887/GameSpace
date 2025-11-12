using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.social_hub.Auth;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class AdminMiniGameController : MiniGameBaseController
    {
        private readonly IGameQueryService _gameQueryService;
        private readonly IGameMutationService _gameMutationService;
        private readonly IFuzzySearchService _fuzzySearchService;
        private readonly IGameRulesConfigService _gameRulesConfigService;

        public AdminMiniGameController(
            GameSpacedatabaseContext context,
            IGameQueryService gameQueryService,
            IGameMutationService gameMutationService,
            IFuzzySearchService fuzzySearchService,
            IGameRulesConfigService gameRulesConfigService)
            : base(context)
        {
            _gameQueryService = gameQueryService;
            _gameMutationService = gameMutationService;
            _fuzzySearchService = fuzzySearchService;
            _gameRulesConfigService = gameRulesConfigService;
        }

        // GET: AdminMiniGame
        // Note: MiniGame entity represents game play records, not game definitions
        public async Task<IActionResult> Index(string searchTerm = "", string result = "", string sortBy = "recent", int page = 1, int pageSize = 10)
        {
            var query = _context.MiniGames
                .Include(g => g.User)
                .Where(g => !g.IsDeleted)
                .AsQueryable();

            // 模糊搜尋：SearchTerm（聯集OR邏輯，使用 FuzzySearchService）
            var hasSearchTerm = !string.IsNullOrWhiteSpace(searchTerm);

            List<int> matchedGameIds = new List<int>();
            Dictionary<int, int> gamePriority = new Dictionary<int, int>();

            if (hasSearchTerm)
            {
                var term = searchTerm.Trim();

                // 查詢所有遊戲記錄並使用 FuzzySearchService 計算優先順序
                var allGames = await _context.MiniGames
                    .Include(g => g.User)
                    .AsNoTracking()
                    .Select(g => new {
                        g.PlayId,
                        g.UserId,
                        UserName = g.User != null ? g.User.UserName : "",
                        UserAccount = g.User != null ? g.User.UserAccount : ""
                    })
                    .ToListAsync();

                foreach (var game in allGames)
                {
                    int priority = 0;

                    // PlayId精確匹配優先
                    if (game.PlayId.ToString().Equals(term, StringComparison.OrdinalIgnoreCase))
                    {
                        priority = 1; // 完全匹配 PlayId
                    }
                    else if (game.PlayId.ToString().Contains(term))
                    {
                        priority = 2; // 部分匹配 PlayId
                    }

                    // 如果ID沒有匹配，檢查UserId
                    if (priority == 0 && int.TryParse(term, out int userId))
                    {
                        if (game.UserId == userId)
                        {
                            priority = 1; // 完全匹配 UserId
                        }
                        else if (game.UserId.ToString().Contains(term))
                        {
                            priority = 2; // 部分匹配 UserId
                        }
                    }

                    // 如果還沒匹配，使用模糊搜尋用戶信息
                    if (priority == 0)
                    {
                        priority = _fuzzySearchService.CalculateMatchPriority(
                            term,
                            game.UserAccount,
                            game.UserName
                        );
                    }

                    // 如果匹配成功（priority > 0），加入結果
                    if (priority > 0)
                    {
                        matchedGameIds.Add(game.PlayId);
                        gamePriority[game.PlayId] = priority;
                    }
                }

                query = query.Where(g => matchedGameIds.Contains(g.PlayId));
            }

            // 遊戲結果篩選
            if (!string.IsNullOrEmpty(result))
            {
                query = query.Where(g => g.Result == result);
            }

            // 計算統計數據（從篩選後的查詢）
            var totalCount = await query.CountAsync();

            // 優先順序排序：先取資料再排序
            var allItems = await query.ToListAsync();
            var miniGames = allItems;

            if (hasSearchTerm)
            {
                // 在記憶體中進行優先順序排序
                var ordered = allItems.OrderBy(g =>
                {
                    // 如果遊戲記錄匹配，返回對應優先順序
                    if (gamePriority.ContainsKey(g.PlayId))
                    {
                        return gamePriority[g.PlayId];
                    }
                    return 99;
                });

                // 次要排序
                var sorted = sortBy switch
                {
                    "level" => ordered.ThenByDescending(g => g.Level),
                    "points" => ordered.ThenByDescending(g => g.PointsGained),
                    "exp" => ordered.ThenByDescending(g => g.ExpGained),
                    _ => ordered.ThenByDescending(g => g.StartTime)
                };

                miniGames = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                // 沒有搜尋條件時使用預設排序
                var sorted = sortBy switch
                {
                    "level" => allItems.OrderByDescending(g => g.Level),
                    "points" => allItems.OrderByDescending(g => g.PointsGained),
                    "exp" => allItems.OrderByDescending(g => g.ExpGained),
                    _ => allItems.OrderByDescending(g => g.StartTime)
                };

                miniGames = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

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

            // 計算統計數據（從篩選後的 allItems）
            ViewBag.CompletedGames = allItems.Count(g => g.Result == "勝利" || g.Result == "Win");
            ViewBag.AbortedGames = allItems.Count(g => g.Aborted);
            ViewBag.TotalPointsAwarded = allItems.Sum(g => (int?)g.PointsGained) ?? 0;
            ViewBag.TotalExpAwarded = allItems.Sum(g => (int?)g.ExpGained) ?? 0;

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
        [ValidateAntiForgeryToken]
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

        // ==================== Game Rules Configuration ====================

        /// <summary>
        /// GET: AdminMiniGame/GameRules
        /// Display game rules configuration page with complete settings
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GameRules()
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    TempData["ErrorMessage"] = "您沒有權限查看遊戲規則設定";
                    return RedirectToAction(nameof(Index));
                }

                // Load complete game rules configuration
                var gameRulesConfig = await _gameRulesConfigService.GetCompleteGameRulesAsync();

                return View(gameRulesConfig);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入遊戲規則時發生錯誤: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/UpdateGameRules
        /// Save game rules configuration
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGameRules(GameRulesInputModel model)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    TempData["ErrorMessage"] = "您沒有權限修改遊戲規則";
                    return RedirectToAction(nameof(GameRules));
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var gameRules = await _gameQueryService.GetGameRulesAsync();
                    var stats = await _gameQueryService.GetGameStatisticsAsync();
                    ViewBag.Statistics = stats;
                    TempData["ErrorMessage"] = "表單資料驗證失敗，請檢查輸入內容";
                    return View("GameRules", gameRules);
                }

                // Validate duration range
                if (model.MinDurationSeconds.HasValue && model.MaxDurationSeconds.HasValue)
                {
                    if (model.MinDurationSeconds.Value > model.MaxDurationSeconds.Value)
                    {
                        ModelState.AddModelError("MinDurationSeconds", "最小時長不能大於最大時長");
                        var gameRules = await _gameQueryService.GetGameRulesAsync();
                        var stats = await _gameQueryService.GetGameStatisticsAsync();
                        ViewBag.Statistics = stats;
                        TempData["ErrorMessage"] = "最小時長不能大於最大時長";
                        return View("GameRules", gameRules);
                    }
                }

                // Get current manager ID
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue)
                {
                    TempData["ErrorMessage"] = "無法取得管理員資訊";
                    return RedirectToAction(nameof(GameRules));
                }

                // Update game rules
                var (success, message) = await _gameMutationService.UpdateGameRulesAsync(model, managerId.Value);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    await LogOperationAsync("UpdateGameRules", $"更新遊戲規則: {model.GameName}");
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }

                return RedirectToAction(nameof(GameRules));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"更新遊戲規則時發生錯誤: {ex.Message}";
                return RedirectToAction(nameof(GameRules));
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/UpdateLevelSettings
        /// Save individual level configuration
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLevelSettings(LevelSettingsInputModel model)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    return Json(new { success = false, message = "您沒有權限修改關卡設定" });
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = $"驗證失敗: {string.Join(", ", errors)}" });
                }

                // Validate level settings
                var (isValid, errorMessage) = _gameMutationService.ValidateLevelSettings(
                    model.Level,
                    model.MonsterCount,
                    model.SpeedMultiplier,
                    model.WinPointsReward,
                    model.WinExpReward
                );

                if (!isValid)
                {
                    return Json(new { success = false, message = errorMessage });
                }

                // Get current manager ID
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue)
                {
                    return Json(new { success = false, message = "無法取得管理員資訊" });
                }

                // Update level settings
                var (success, message) = await _gameMutationService.UpdateLevelSettingsAsync(model, managerId.Value);

                if (success)
                {
                    await LogOperationAsync("UpdateLevelSettings", $"更新第 {model.Level} 關設定");
                }

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新關卡設定時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/UpdateDailyLimit
        /// Update daily play limit
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDailyLimit(DailyLimitInputModel model)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    return Json(new { success = false, message = "您沒有權限修改每日次數限制" });
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = $"驗證失敗: {string.Join(", ", errors)}" });
                }

                // Get current manager ID
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue)
                {
                    return Json(new { success = false, message = "無法取得管理員資訊" });
                }

                // Update daily limit
                var (success, message) = await _gameMutationService.UpdateDailyLimitAsync(model, managerId.Value);

                if (success)
                {
                    await LogOperationAsync("UpdateDailyLimit", $"更新每日次數限制為 {model.MaxPlaysPerDay} 次");
                }

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新每日次數限制時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/UpdateLevelConfig
        /// Update individual level configuration using new system
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLevelConfig(LevelConfigInputModel model)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    return Json(new { success = false, message = "您沒有權限修改關卡設定" });
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = $"驗證失敗: {string.Join(", ", errors)}" });
                }

                // Get current manager ID
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue)
                {
                    return Json(new { success = false, message = "無法取得管理員資訊" });
                }

                // Update level config
                var (success, message) = await _gameRulesConfigService.UpdateLevelConfigAsync(model, managerId.Value);

                if (success)
                {
                    await LogOperationAsync("UpdateLevelConfig", $"更新第 {model.Level} 關設定");
                }

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新關卡設定時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/UpdateAdventureImpact
        /// Update adventure result impact (pet status changes)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAdventureImpact(AdventureResultImpactInputModel model)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    return Json(new { success = false, message = "您沒有權限修改冒險結果影響設定" });
                }

                // Validate model state
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = $"驗證失敗: {string.Join(", ", errors)}" });
                }

                // Get current manager ID
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue)
                {
                    return Json(new { success = false, message = "無法取得管理員資訊" });
                }

                // Update adventure impact
                var (success, message) = await _gameRulesConfigService.UpdateAdventureResultImpactAsync(model, managerId.Value);

                if (success)
                {
                    await LogOperationAsync("UpdateAdventureImpact", $"更新冒險{model.ResultType}結果影響設定");
                }

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新冒險結果影響時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/UpdateGameDailyLimit
        /// Update daily game limit using new system
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGameDailyLimit(int dailyLimit)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    return Json(new { success = false, message = "您沒有權限修改每日次數限制" });
                }

                // Validate
                if (dailyLimit < 1 || dailyLimit > 100)
                {
                    return Json(new { success = false, message = "每日次數必須在1-100之間" });
                }

                // Get current manager ID
                var managerId = GetCurrentManagerId();
                if (!managerId.HasValue)
                {
                    return Json(new { success = false, message = "無法取得管理員資訊" });
                }

                // Update daily limit
                var (success, message) = await _gameRulesConfigService.UpdateDailyLimitAsync(dailyLimit, managerId.Value);

                if (success)
                {
                    await LogOperationAsync("UpdateGameDailyLimit", $"更新每日遊戲次數限制為 {dailyLimit} 次");
                }

                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"更新每日次數限制時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: AdminMiniGame/QueryRecords
        /// Alias for ViewGameRecords for backward compatibility
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QueryRecords(GameRecordQueryModel query)
        {
            return await ViewGameRecords(query);
        }

        /// <summary>
        /// POST: AdminMiniGame/SearchGameRecords
        /// AJAX endpoint for searching game records with filters
        /// Returns JSON response with records and statistics
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SearchGameRecords([FromForm] GameRecordQueryModel query)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.View"))
                {
                    return Json(new { success = false, message = "您沒有權限查看遊戲紀錄" });
                }

                // Validate date range
                if (query.StartDate.HasValue && query.EndDate.HasValue)
                {
                    if (query.StartDate.Value > query.EndDate.Value)
                    {
                        return Json(new { success = false, message = "開始日期不能晚於結束日期" });
                    }
                }

                // Set default values if not provided
                if (query.PageNumber < 1) query.PageNumber = 1;
                if (query.PageSize < 1) query.PageSize = 100; // Return all results for AJAX
                if (query.PageSize > 1000) query.PageSize = 1000;

                // Query game records - will return ALL results if no filters provided
                var result = await _gameQueryService.QueryGameRecordsAsync(query);

                // Transform records to match frontend expectations
                var data = result.Records.Select(r => new
                {
                    recordId = r.PlayId,
                    userId = r.UserId,
                    userName = r.UserName,
                    score = r.PointsGained,
                    status = MapResultToStatus(r.Result, r.Aborted),
                    difficulty = $"Level {r.Level}",
                    gameTime = r.StartTime.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList();

                // Calculate statistics from filtered results (not just all records)
                var statistics = new
                {
                    totalGames = result.TotalCount,
                    completedGames = result.Records.Count(r => r.Result == "Win" || r.Result == "勝利"),
                    avgScore = result.Records.Any() ? (decimal)result.Records.Average(r => r.PointsGained) : 0,
                    maxScore = result.Records.Any() ? result.Records.Max(r => r.PointsGained) : 0
                };

                return Json(new
                {
                    success = true,
                    data = data,
                    statistics = statistics,
                    totalCount = result.TotalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"查詢遊戲紀錄時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// Helper method to map Result and Aborted status to frontend status display
        /// </summary>
        private string MapResultToStatus(string result, bool aborted)
        {
            if (aborted) return "放棄";
            return result switch
            {
                "Win" => "已完成",
                "Lose" => "失敗",
                "Abort" => "放棄",
                _ => "進行中"
            };
        }

        /// <summary>
        /// GET: AdminMiniGame/GetGameRecordDetail
        /// Get detailed information for a specific game record
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetGameRecordDetail(int recordId)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.View"))
                {
                    return Json(new { success = false, message = "您沒有權限查看遊戲紀錄" });
                }

                var detail = await _gameQueryService.GetGameRecordDetailAsync(recordId);

                if (detail == null)
                {
                    return Json(new { success = false, message = "找不到該遊戲記錄" });
                }

                var data = new
                {
                    userId = detail.UserId,
                    userName = detail.UserName,
                    difficulty = $"Level {detail.Level}",
                    score = detail.PointsGained,
                    status = MapResultToStatus(detail.Result, detail.Aborted),
                    gameTime = detail.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    rewardPoints = detail.PointsGained,
                    rewardItems = !string.IsNullOrEmpty(detail.CouponGained) ? detail.CouponGained : "無",
                    startTime = detail.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    endTime = detail.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未結束",
                    duration = detail.Duration.HasValue ? $"{detail.Duration.Value} 秒" : "N/A",
                    ipAddress = "未記錄",
                    deviceType = "未記錄",
                    browser = "未記錄"
                };

                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"獲取詳細資訊時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/AdjustScore
        /// Adjust score for a game record (admin correction)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AdjustScore(int recordId, int newScore)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Edit"))
                {
                    return Json(new { success = false, message = "您沒有權限調整分數" });
                }

                // Validate score
                if (newScore < 0)
                {
                    return Json(new { success = false, message = "分數不能為負數" });
                }

                // Find the game record
                var miniGame = await _context.MiniGames.FindAsync(recordId);
                if (miniGame == null)
                {
                    return Json(new { success = false, message = "找不到該遊戲記錄" });
                }

                // Update points gained
                var oldScore = miniGame.PointsGained;
                miniGame.PointsGained = newScore;
                miniGame.PointsGainedTime = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Log the operation
                await LogOperationAsync("AdjustScore", $"調整遊戲記錄 {recordId} 分數從 {oldScore} 到 {newScore}");

                return Json(new { success = true, message = "分數調整成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"調整分數時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// POST: AdminMiniGame/DeleteGameRecord
        /// Soft delete a game record
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteGameRecord(int recordId)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.Delete"))
                {
                    return Json(new { success = false, message = "您沒有權限刪除遊戲記錄" });
                }

                // Find the game record
                var miniGame = await _context.MiniGames.FindAsync(recordId);
                if (miniGame == null)
                {
                    return Json(new { success = false, message = "找不到該遊戲記錄" });
                }

                // Soft delete
                miniGame.IsDeleted = true;
                miniGame.DeletedAt = DateTime.UtcNow;
                miniGame.DeletedBy = GetCurrentManagerId();
                miniGame.DeleteReason = "管理員刪除";

                await _context.SaveChangesAsync();

                // Log the operation
                await LogOperationAsync("DeleteGameRecord", $"刪除遊戲記錄 {recordId}");

                return Json(new { success = true, message = "遊戲記錄刪除成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"刪除遊戲記錄時發生錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: AdminMiniGame/ViewGameRecords
        /// Query and display game records with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ViewGameRecords(GameRecordQueryModel query)
        {
            try
            {
                // Check permission
                if (!await HasPermissionAsync("MiniGame.View"))
                {
                    TempData["ErrorMessage"] = "您沒有權限查看遊戲紀錄";
                    return RedirectToAction(nameof(Index));
                }

                // Validate date range
                if (query.StartDate.HasValue && query.EndDate.HasValue)
                {
                    if (query.StartDate.Value > query.EndDate.Value)
                    {
                        ModelState.AddModelError("StartDate", "開始日期不能晚於結束日期");
                        TempData["ErrorMessage"] = "開始日期不能晚於結束日期";
                        query.StartDate = null;
                        query.EndDate = null;
                    }
                }

                // Set default values if not provided
                if (query.PageNumber < 1) query.PageNumber = 1;
                if (query.PageSize < 1) query.PageSize = 20;
                if (query.PageSize > 100) query.PageSize = 100;

                // Query game records
                var result = await _gameQueryService.QueryGameRecordsAsync(query);

                // Get statistics for display
                var stats = await _gameQueryService.GetGameStatisticsAsync(query.StartDate, query.EndDate);

                // Pass data to view
                ViewBag.Statistics = stats;
                ViewBag.Query = query;
                ViewBag.TotalRecords = result.TotalCount;
                ViewBag.FilteredRecords = result.Records.Count;

                // Prepare result options for dropdown
                ViewBag.ResultOptions = new List<string> { "Win", "Lose", "Abort" };

                // Prepare level options for dropdown
                ViewBag.LevelOptions = Enumerable.Range(1, 10).ToList();

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"查詢遊戲紀錄時發生錯誤: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==================== End of Game Rules Configuration ====================

        private bool MiniGameExists(int id)
        {
            return _context.MiniGames.Any(e => e.PlayId == id);
        }
    }
}

