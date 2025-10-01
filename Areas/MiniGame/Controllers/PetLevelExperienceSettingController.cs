using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物等級經驗值設定控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize]
    public class PetLevelExperienceSettingController : Controller
    {
        private readonly IPetLevelExperienceSettingService _service;
        private readonly ILogger<PetLevelExperienceSettingController> _logger;

        public PetLevelExperienceSettingController(
            IPetLevelExperienceSettingService service,
            ILogger<PetLevelExperienceSettingController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// 等級經驗值設定列表頁面
        /// </summary>
        public async Task<IActionResult> Index(PetLevelExperienceSettingSearchViewModel? searchModel = null)
        {
            try
            {
                searchModel ??= new PetLevelExperienceSettingSearchViewModel();
                
                var (items, totalCount) = await _service.GetAllAsync(searchModel);
                var statistics = await _service.GetStatisticsAsync();

                ViewBag.SearchModel = searchModel;
                ViewBag.TotalCount = totalCount;
                ViewBag.Statistics = statistics;

                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入等級經驗值設定列表時發生錯誤");
                TempData["ErrorMessage"] = "載入資料時發生錯誤，請稍後再試";
                return View(new List<PetLevelExperienceSettingListViewModel>());
            }
        }

        /// <summary>
        /// 建立等級經驗值設定頁面
        /// </summary>
        public IActionResult Create()
        {
            return View(new PetLevelExperienceSettingCreateViewModel());
        }

        /// <summary>
        /// 建立等級經驗值設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetLevelExperienceSettingCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // 檢查等級是否已存在
                if (await _service.IsLevelExistsAsync(model.Level))
                {
                    ModelState.AddModelError(nameof(model.Level), "此等級已存在");
                    return View(model);
                }

                var success = await _service.CreateAsync(model, User.Identity?.Name);
                if (success)
                {
                    TempData["SuccessMessage"] = "等級經驗值設定建立成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "建立失敗，請檢查輸入資料";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立等級經驗值設定時發生錯誤");
                TempData["ErrorMessage"] = "建立時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 編輯等級經驗值設定頁面
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                if (entity == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的等級經驗值設定";
                    return RedirectToAction(nameof(Index));
                }

                var model = new PetLevelExperienceSettingEditViewModel
                {
                    Id = entity.Id,
                    Level = entity.Level,
                    RequiredExperience = entity.RequiredExperience,
                    LevelName = entity.LevelName,
                    Description = entity.Description,
                    IsEnabled = entity.IsEnabled
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入等級經驗值設定編輯頁面時發生錯誤");
                TempData["ErrorMessage"] = "載入時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 更新等級經驗值設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetLevelExperienceSettingEditViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // 檢查等級是否已存在（排除自己）
                if (await _service.IsLevelExistsAsync(model.Level, model.Id))
                {
                    ModelState.AddModelError(nameof(model.Level), "此等級已存在");
                    return View(model);
                }

                var success = await _service.UpdateAsync(model, User.Identity?.Name);
                if (success)
                {
                    TempData["SuccessMessage"] = "等級經驗值設定更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "更新失敗，請檢查輸入資料";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新等級經驗值設定時發生錯誤");
                TempData["ErrorMessage"] = "更新時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 等級經驗值設定詳細頁面
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                if (entity == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的等級經驗值設定";
                    return RedirectToAction(nameof(Index));
                }

                return View(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入等級經驗值設定詳細頁面時發生錯誤");
                TempData["ErrorMessage"] = "載入時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 刪除等級經驗值設定頁面
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                if (entity == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的等級經驗值設定";
                    return RedirectToAction(nameof(Index));
                }

                return View(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入等級經驗值設定刪除頁面時發生錯誤");
                TempData["ErrorMessage"] = "載入時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 確認刪除等級經驗值設定
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
                    TempData["SuccessMessage"] = "等級經驗值設定刪除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除失敗，請稍後再試";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除等級經驗值設定時發生錯誤");
                TempData["ErrorMessage"] = "刪除時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var success = await _service.ToggleStatusAsync(id, User.Identity?.Name);
                if (success)
                {
                    TempData["SuccessMessage"] = "狀態切換成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "狀態切換失敗，請稍後再試";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換等級經驗值設定狀態時發生錯誤");
                TempData["ErrorMessage"] = "狀態切換時發生錯誤，請稍後再試";
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
                return Json(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得等級統計資料時發生錯誤");
                return Json(new { error = "取得統計資料時發生錯誤" });
            }
        }
    }
}
