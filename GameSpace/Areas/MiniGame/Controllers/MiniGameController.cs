using Microsoft.AspNetCore.Mvc;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.ViewModels;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 小遊戲管理控制器
    /// </summary>
    [Area("MiniGame")]
    [MiniGameAdminAuthorize("UserStatusManagement")]
    public class MiniGameController : Controller
    {
        private readonly IMiniGameService _miniGameService;
        private readonly ILogger<MiniGameController> _logger;

        public MiniGameController(IMiniGameService miniGameService, ILogger<MiniGameController> logger)
        {
            _miniGameService = miniGameService;
            _logger = logger;
        }

        /// <summary>
        /// 小遊戲管理首頁
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            try
            {
                var games = await _miniGameService.GetAllGameHistoryAsync(page, pageSize);
                
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 1; // 實際實作時需要計算總頁數

                return View(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得小遊戲記錄列表失敗");
                TempData["ErrorMessage"] = "取得小遊戲記錄列表失敗";
                return View(new List<MiniGameViewModel>());
            }
        }

        /// <summary>
        /// 遊戲記錄詳情
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int gameId)
        {
            try
            {
                // 這裡需要根據 GameID 查詢遊戲記錄
                // 暫時使用假資料，實際實作時需要查詢資料庫
                var game = new MiniGameViewModel
                {
                    GameID = gameId,
                    UserID = 1,
                    PetID = 1,
                    UserAccount = "test@example.com",
                    UserName = "測試會員",
                    PetName = "測試寵物",
                    GameType = "打怪遊戲",
                    StartTime = DateTime.Now.AddMinutes(-30),
                    EndTime = DateTime.Now.AddMinutes(-25),
                    GameResult = "勝利",
                    PointsEarned = 20,
                    PetExpEarned = 200,
                    SessionID = "SESSION_" + gameId
                };

                return View(game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得遊戲記錄詳情失敗，GameID: {GameId}", gameId);
                TempData["ErrorMessage"] = "取得遊戲記錄詳情失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 開始新遊戲
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartGame(int userId, int petId, string gameType)
        {
            try
            {
                // 檢查每日遊戲次數限制
                var canPlay = await _miniGameService.CheckDailyGameLimitAsync(userId);
                if (!canPlay)
                {
                    TempData["ErrorMessage"] = "今日遊戲次數已達上限";
                    return RedirectToAction(nameof(Index));
                }

                var sessionId = await _miniGameService.StartGameAsync(userId, petId, gameType);
                if (!string.IsNullOrEmpty(sessionId))
                {
                    TempData["SuccessMessage"] = "遊戲開始成功";
                    TempData["SessionId"] = sessionId;
                }
                else
                {
                    TempData["ErrorMessage"] = "開始遊戲失敗";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "開始遊戲失敗，UserID: {UserId}, PetID: {PetId}", userId, petId);
                TempData["ErrorMessage"] = "開始遊戲失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 結束遊戲
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndGame(string sessionId, string gameResult, int pointsEarned, int petExpEarned, int? couponEarned)
        {
            try
            {
                var success = await _miniGameService.EndGameAsync(sessionId, gameResult, pointsEarned, petExpEarned, couponEarned);
                if (success)
                {
                    TempData["SuccessMessage"] = "遊戲結束，獎勵已發放";
                }
                else
                {
                    TempData["ErrorMessage"] = "結束遊戲失敗";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "結束遊戲失敗，SessionID: {SessionId}", sessionId);
                TempData["ErrorMessage"] = "結束遊戲失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 會員遊戲記錄
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UserHistory(int userId, int page = 1, int pageSize = 20)
        {
            try
            {
                var games = await _miniGameService.GetUserGameHistoryAsync(userId, page, pageSize);
                
                ViewBag.UserId = userId;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 1; // 實際實作時需要計算總頁數

                return View(games);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得會員遊戲記錄失敗，UserID: {UserId}", userId);
                TempData["ErrorMessage"] = "取得會員遊戲記錄失敗";
                return View(new List<MiniGameViewModel>());
            }
        }

        /// <summary>
        /// 遊戲規則設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Rules()
        {
            try
            {
                var rules = await _miniGameService.GetGameRulesAsync();
                return View(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得遊戲規則設定失敗");
                TempData["ErrorMessage"] = "取得遊戲規則設定失敗";
                return View(new GameRulesViewModel());
            }
        }

        /// <summary>
        /// 更新遊戲規則設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRules(GameRulesViewModel rules)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Rules", rules);
                }

                var success = await _miniGameService.UpdateGameRulesAsync(rules);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功更新遊戲規則設定";
                }
                else
                {
                    TempData["ErrorMessage"] = "更新遊戲規則設定失敗";
                }

                return RedirectToAction(nameof(Rules));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新遊戲規則設定失敗");
                TempData["ErrorMessage"] = "更新遊戲規則設定失敗";
                return View("Rules", rules);
            }
        }

        /// <summary>
        /// 遊戲統計資料
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var statistics = await _miniGameService.GetGameStatisticsAsync();
                return View(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得遊戲統計資料失敗");
                TempData["ErrorMessage"] = "取得遊戲統計資料失敗";
                return View(new object());
            }
        }

        /// <summary>
        /// 匯出遊戲記錄
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Export()
        {
            try
            {
                // 這裡應該實作匯出功能
                // 暫時回傳空結果
                TempData["SuccessMessage"] = "匯出功能開發中";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "匯出遊戲記錄失敗");
                TempData["ErrorMessage"] = "匯出遊戲記錄失敗";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}