using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物顏色選項管理控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize]
    public class PetColorOptionController : Controller
    {
        private readonly IPetColorOptionService _colorOptionService;
        private readonly ILogger<PetColorOptionController> _logger;

        public PetColorOptionController(IPetColorOptionService colorOptionService, ILogger<PetColorOptionController> logger)
        {
            _colorOptionService = colorOptionService;
            _logger = logger;
        }

        /// <summary>
        /// 顏色選項列表頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? isActiveFilter = null)
        {
            try
            {
                var model = await _colorOptionService.GetColorOptionsAsync(pageNumber, pageSize, searchTerm, isActiveFilter);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得顏色選項列表時發生錯誤");
                TempData["ErrorMessage"] = "取得顏色選項列表時發生錯誤，請稍後再試";
                return View(new PetColorOptionListViewModel());
            }
        }

        /// <summary>
        /// 顏色選項詳情頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var colorOption = await _colorOptionService.GetColorOptionByIdAsync(id);
                if (colorOption == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的顏色選項";
                    return RedirectToAction(nameof(Index));
                }

                return View(colorOption);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得顏色選項詳情時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "取得顏色選項詳情時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 建立顏色選項頁面
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new PetColorOptionViewModel());
        }

        /// <summary>
        /// 建立顏色選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetColorOptionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // 檢查顏色代碼是否重複
                if (await _colorOptionService.IsColorCodeExistsAsync(model.ColorCode))
                {
                    ModelState.AddModelError(nameof(model.ColorCode), "此顏色代碼已存在");
                    return View(model);
                }

                // 檢查顏色名稱是否重複
                if (await _colorOptionService.IsColorNameExistsAsync(model.ColorName))
                {
                    ModelState.AddModelError(nameof(model.ColorName), "此顏色名稱已存在");
                    return View(model);
                }

                var result = await _colorOptionService.CreateColorOptionAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "顏色選項建立成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "建立顏色選項失敗，請稍後再試";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立顏色選項時發生錯誤");
                TempData["ErrorMessage"] = "建立顏色選項時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 編輯顏色選項頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var colorOption = await _colorOptionService.GetColorOptionByIdAsync(id);
                if (colorOption == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的顏色選項";
                    return RedirectToAction(nameof(Index));
                }

                var model = new PetColorOptionViewModel
                {
                    Id = colorOption.Id,
                    ColorName = colorOption.ColorName,
                    ColorCode = colorOption.ColorCode,
                    Description = colorOption.Description,
                    IsActive = colorOption.IsActive,
                    SortOrder = colorOption.SortOrder
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得編輯顏色選項時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "取得編輯顏色選項時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 更新顏色選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetColorOptionViewModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    TempData["ErrorMessage"] = "請求參數錯誤";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // 檢查顏色代碼是否重複（排除自己）
                if (await _colorOptionService.IsColorCodeExistsAsync(model.ColorCode, id))
                {
                    ModelState.AddModelError(nameof(model.ColorCode), "此顏色代碼已存在");
                    return View(model);
                }

                // 檢查顏色名稱是否重複（排除自己）
                if (await _colorOptionService.IsColorNameExistsAsync(model.ColorName, id))
                {
                    ModelState.AddModelError(nameof(model.ColorName), "此顏色名稱已存在");
                    return View(model);
                }

                var result = await _colorOptionService.UpdateColorOptionAsync(id, model);
                if (result)
                {
                    TempData["SuccessMessage"] = "顏色選項更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "更新顏色選項失敗，請稍後再試";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新顏色選項時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "更新顏色選項時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 刪除顏色選項頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var colorOption = await _colorOptionService.GetColorOptionByIdAsync(id);
                if (colorOption == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的顏色選項";
                    return RedirectToAction(nameof(Index));
                }

                return View(colorOption);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得刪除顏色選項時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "取得刪除顏色選項時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 確認刪除顏色選項
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _colorOptionService.DeleteColorOptionAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "顏色選項刪除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除顏色選項失敗，請稍後再試";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除顏色選項時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "刪除顏色選項時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 檢查顏色代碼是否可用
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckColorCode(string colorCode, int? id = null)
        {
            try
            {
                var exists = await _colorOptionService.IsColorCodeExistsAsync(colorCode, id);
                return Json(new { available = !exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查顏色代碼時發生錯誤");
                return Json(new { available = false });
            }
        }

        /// <summary>
        /// 檢查顏色名稱是否可用
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckColorName(string colorName, int? id = null)
        {
            try
            {
                var exists = await _colorOptionService.IsColorNameExistsAsync(colorName, id);
                return Json(new { available = !exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查顏色名稱時發生錯誤");
                return Json(new { available = false });
            }
        }
    }
}
