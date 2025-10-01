using Microsoft.AspNetCore.Mvc;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.ViewModels;
using GameSpace.Areas.MiniGame.Filters;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 簽到管理控制器
    /// </summary>
    [Area("MiniGame")]
    [MiniGameAdminAuthorize("UserStatusManagement")]
    public class SignInStatsController : Controller
    {
        private readonly ISignInStatsService _signInStatsService;
        private readonly ILogger<SignInStatsController> _logger;

        public SignInStatsController(ISignInStatsService signInStatsService, ILogger<SignInStatsController> logger)
        {
            _signInStatsService = signInStatsService;
            _logger = logger;
        }

        /// <summary>
        /// 簽到管理首頁
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            try
            {
                var signInStats = await _signInStatsService.GetAllSignInStatsAsync(page, pageSize);
                
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = 1; // 實際實作時需要計算總頁數

                return View(signInStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得簽到統計列表失敗");
                TempData["ErrorMessage"] = "取得簽到統計列表失敗";
                return View(new List<SignInStatsViewModel>());
            }
        }

        /// <summary>
        /// 會員簽到詳情
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int userId)
        {
            try
            {
                var signInStats = await _signInStatsService.GetUserSignInStatsAsync(userId);
                if (signInStats == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的會員簽到記錄";
                    return RedirectToAction(nameof(Index));
                }

                // 取得簽到歷史記錄
                signInStats.SignInHistory = await _signInStatsService.GetSignInHistoryAsync(userId, 1, 50);

                return View(signInStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得會員簽到詳情失敗，UserID: {UserId}", userId);
                TempData["ErrorMessage"] = "取得會員簽到詳情失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 手動執行會員簽到
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessSignIn(int userId)
        {
            try
            {
                var success = await _signInStatsService.ProcessUserSignInAsync(userId);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功處理會員簽到";
                }
                else
                {
                    TempData["ErrorMessage"] = "處理會員簽到失敗";
                }

                return RedirectToAction(nameof(Details), new { userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理會員簽到失敗，UserID: {UserId}", userId);
                TempData["ErrorMessage"] = "處理會員簽到失敗";
                return RedirectToAction(nameof(Details), new { userId });
            }
        }

        /// <summary>
        /// 簽到規則設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Rules()
        {
            try
            {
                var rules = await _signInStatsService.GetSignInRulesAsync();
                return View(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得簽到規則設定失敗");
                TempData["ErrorMessage"] = "取得簽到規則設定失敗";
                return View(new SignInRulesViewModel());
            }
        }

        /// <summary>
        /// 更新簽到規則設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRules(SignInRulesViewModel rules)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Rules", rules);
                }

                var success = await _signInStatsService.UpdateSignInRulesAsync(rules);
                if (success)
                {
                    TempData["SuccessMessage"] = "成功更新簽到規則設定";
                }
                else
                {
                    TempData["ErrorMessage"] = "更新簽到規則設定失敗";
                }

                return RedirectToAction(nameof(Rules));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新簽到規則設定失敗");
                TempData["ErrorMessage"] = "更新簽到規則設定失敗";
                return View("Rules", rules);
            }
        }

        /// <summary>
        /// 匯出簽到統計資料
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
                _logger.LogError(ex, "匯出簽到統計資料失敗");
                TempData["ErrorMessage"] = "匯出簽到統計資料失敗";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}