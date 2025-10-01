using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物互動狀態增益規則控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize]
    public class PetInteractionBonusRuleController : Controller
    {
        private readonly IPetInteractionBonusRuleService _service;
        private readonly ILogger<PetInteractionBonusRuleController> _logger;

        public PetInteractionBonusRuleController(
            IPetInteractionBonusRuleService service,
            ILogger<PetInteractionBonusRuleController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// 互動狀態增益規則列表頁面
        /// </summary>
        public async Task<IActionResult> Index(PetInteractionBonusRuleSearchViewModel? searchModel = null)
        {
            try
            {
                searchModel ??= new PetInteractionBonusRuleSearchViewModel();
                
                var (items, totalCount) = await _service.SearchAsync(searchModel);
                var statistics = await _service.GetStatisticsAsync();

                ViewBag.SearchModel = searchModel;
                ViewBag.TotalCount = totalCount;
                ViewBag.Statistics = statistics;

                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入互動狀態增益規則列表時發生錯誤");
                TempData["ErrorMessage"] = "載入資料時發生錯誤，請稍後再試";
                return View(new List<PetInteractionBonusRuleListViewModel>());
            }
        }

        /// <summary>
        /// 互動狀態增益規則詳情頁面
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var rule = await _service.GetByIdAsync(id);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的互動狀態增益規則";
                    return RedirectToAction(nameof(Index));
                }

                return View(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入互動狀態增益規則詳情時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "載入詳情時發生錯誤";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 新增互動狀態增益規則頁面
        /// </summary>
        public IActionResult Create()
        {
            return View(new PetInteractionBonusRuleCreateViewModel());
        }

        /// <summary>
        /// 新增互動狀態增益規則
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetInteractionBonusRuleCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var success = await _service.CreateAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "互動狀態增益規則建立成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "互動狀態增益規則建立失敗，請檢查互動類型是否已存在";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立互動狀態增益規則時發生錯誤");
                TempData["ErrorMessage"] = "建立時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 編輯互動狀態增益規則頁面
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var rule = await _service.GetByIdAsync(id);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的互動狀態增益規則";
                    return RedirectToAction(nameof(Index));
                }

                var model = new PetInteractionBonusRuleViewModel
                {
                    Id = rule.Id,
                    InteractionType = rule.InteractionType,
                    InteractionName = rule.InteractionName,
                    PointsCost = rule.PointsCost,
                    HappinessGain = rule.HappinessGain,
                    ExpGain = rule.ExpGain,
                    CooldownMinutes = rule.CooldownMinutes,
                    IsActive = rule.IsActive,
                    Description = rule.Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入互動狀態增益規則編輯頁面時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "載入編輯頁面時發生錯誤";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 更新互動狀態增益規則
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetInteractionBonusRuleViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var success = await _service.UpdateAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "互動狀態增益規則更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "互動狀態增益規則更新失敗，請檢查互動類型是否已被其他規則使用";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新互動狀態增益規則時發生錯誤，ID: {Id}", model.Id);
                TempData["ErrorMessage"] = "更新時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 刪除互動狀態增益規則頁面
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var rule = await _service.GetByIdAsync(id);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的互動狀態增益規則";
                    return RedirectToAction(nameof(Index));
                }

                return View(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入互動狀態增益規則刪除頁面時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "載入刪除頁面時發生錯誤";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 確認刪除互動狀態增益規則
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "互動狀態增益規則刪除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "互動狀態增益規則刪除失敗";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除互動狀態增益規則時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "刪除時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 切換規則啟用狀態
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var success = await _service.ToggleStatusAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "規則狀態切換成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "規則狀態切換失敗";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換互動狀態增益規則狀態時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "切換狀態時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 取得統計資料 (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var statistics = await _service.GetStatisticsAsync();
                return Json(new { success = true, data = statistics });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得互動狀態增益規則統計資料時發生錯誤");
                return Json(new { success = false, message = "取得統計資料時發生錯誤" });
            }
        }
    }
}
