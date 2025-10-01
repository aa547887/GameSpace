using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class PetLevelUpRuleController : Controller
    {
        private readonly IPetLevelUpRuleService _levelUpRuleService;
        private readonly ILogger<PetLevelUpRuleController> _logger;

        public PetLevelUpRuleController(IPetLevelUpRuleService levelUpRuleService, ILogger<PetLevelUpRuleController> logger)
        {
            _levelUpRuleService = levelUpRuleService;
            _logger = logger;
        }

        // GET: PetLevelUpRule
        public async Task<IActionResult> Index()
        {
            try
            {
                var rules = await _levelUpRuleService.GetAllLevelUpRulesAsync();
                return View(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得升級規則列表時發生錯誤");
                TempData["ErrorMessage"] = "載入升級規則列表失敗";
                return View(new List<PetLevelUpRule>());
            }
        }

        // GET: PetLevelUpRule/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var rule = await _levelUpRuleService.GetLevelUpRuleByIdAsync(id);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的升級規則";
                    return RedirectToAction(nameof(Index));
                }

                return View(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得升級規則詳情時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "載入升級規則詳情失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: PetLevelUpRule/Create
        public IActionResult Create()
        {
            return View(new PetLevelUpRuleCreateViewModel());
        }

        // POST: PetLevelUpRule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetLevelUpRuleCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // 驗證升級規則
                if (!await _levelUpRuleService.ValidateLevelUpRuleAsync(model))
                {
                    ModelState.AddModelError("", "等級已存在或資料不正確");
                    return View(model);
                }

                var createdBy = 1; // 這裡應該從當前使用者取得
                var success = await _levelUpRuleService.CreateLevelUpRuleAsync(model, createdBy);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "升級規則建立成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "建立升級規則失敗");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立升級規則時發生錯誤");
                ModelState.AddModelError("", "建立升級規則時發生錯誤");
                return View(model);
            }
        }

        // GET: PetLevelUpRule/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var rule = await _levelUpRuleService.GetLevelUpRuleByIdAsync(id);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的升級規則";
                    return RedirectToAction(nameof(Index));
                }

                var model = new PetLevelUpRuleEditViewModel
                {
                    Id = rule.Id,
                    Level = rule.Level,
                    ExperienceRequired = rule.ExperienceRequired,
                    PointsReward = rule.PointsReward,
                    ExpReward = rule.ExpReward,
                    IsActive = rule.IsActive,
                    Remarks = rule.Remarks
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得升級規則編輯資料時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "載入升級規則編輯資料失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: PetLevelUpRule/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetLevelUpRuleEditViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    TempData["ErrorMessage"] = "升級規則ID不匹配";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var updatedBy = 1; // 這裡應該從當前使用者取得
                var success = await _levelUpRuleService.UpdateLevelUpRuleAsync(model, updatedBy);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "升級規則更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "更新升級規則失敗");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新升級規則時發生錯誤，ID: {Id}", id);
                ModelState.AddModelError("", "更新升級規則時發生錯誤");
                return View(model);
            }
        }

        // GET: PetLevelUpRule/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var rule = await _levelUpRuleService.GetLevelUpRuleByIdAsync(id);
                if (rule == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的升級規則";
                    return RedirectToAction(nameof(Index));
                }

                return View(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得升級規則刪除資料時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "載入升級規則刪除資料失敗";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: PetLevelUpRule/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _levelUpRuleService.DeleteLevelUpRuleAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "升級規則刪除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除升級規則失敗";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除升級規則時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "刪除升級規則時發生錯誤";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: PetLevelUpRule/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var success = await _levelUpRuleService.ToggleLevelUpRuleStatusAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "升級規則狀態切換成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "切換升級規則狀態失敗";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換升級規則狀態時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "切換升級規則狀態時發生錯誤";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: PetLevelUpRule/GetActiveRules
        [HttpGet]
        public async Task<IActionResult> GetActiveRules()
        {
            try
            {
                var rules = await _levelUpRuleService.GetActiveLevelUpRulesAsync();
                return Json(new { success = true, data = rules });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得啟用升級規則時發生錯誤");
                return Json(new { success = false, message = "取得啟用升級規則失敗" });
            }
        }

        // POST: PetLevelUpRule/BulkUpdate
        [HttpPost]
        public async Task<IActionResult> BulkUpdate([FromBody] List<PetLevelUpRuleEditViewModel> models)
        {
            try
            {
                if (models == null || !models.Any())
                {
                    return Json(new { success = false, message = "更新資料不能為空" });
                }

                var updatedBy = 1; // 這裡應該從當前使用者取得
                var success = await _levelUpRuleService.BulkUpdateLevelUpRulesAsync(models, updatedBy);
                
                if (success)
                {
                    return Json(new { success = true, message = "批量更新成功" });
                }
                else
                {
                    return Json(new { success = false, message = "批量更新失敗" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新升級規則時發生錯誤");
                return Json(new { success = false, message = "批量更新升級規則時發生錯誤" });
            }
        }
    }
}
