using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物顏色選項管理控制器
    /// </summary>
    [Area(\
MiniGame\)]
    [Authorize]
    public class PetColorOptionController : Controller
    {
        private readonly IPetColorOptionService _colorOptionService;
        private readonly ILogger<PetColorOptionController> _logger;

        public PetColorOptionController(
            IPetColorOptionService colorOptionService,
            ILogger<PetColorOptionController> logger)
        {
            _colorOptionService = colorOptionService;
            _logger = logger;
        }

        /// <summary>
        /// 顏色選項列表頁面
        /// </summary>
        public async Task<IActionResult> Index(string? searchKeyword = null, bool showActiveOnly = false)
        {
            try
            {
                var colorOptions = new List<PetColorOption>();
                
                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    colorOptions = await _colorOptionService.SearchColorOptionsAsync(searchKeyword, showActiveOnly);
                }
                else if (showActiveOnly)
                {
                    colorOptions = await _colorOptionService.GetActiveColorOptionsAsync();
                }
                else
                {
                    colorOptions = await _colorOptionService.GetAllColorOptionsAsync();
                }

                var statistics = await _colorOptionService.GetColorOptionStatisticsAsync();

                var viewModel = new PetColorOptionListViewModel
                {
                    ColorOptions = colorOptions,
                    TotalCount = statistics.total,
                    ActiveCount = statistics.active,
                    InactiveCount = statistics.inactive,
                    SearchKeyword = searchKeyword,
                    ShowActiveOnly = showActiveOnly
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \取得顏色選項列表時發生錯誤\);
                TempData[\ErrorMessage\] = \取得顏色選項列表時發生錯誤，請稍後再試\;
                return View(new PetColorOptionListViewModel());
            }
        }

        /// <summary>
        /// 建立顏色選項頁面
        /// </summary>
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

                // 這裡應該從認證系統取得使用者ID，暫時使用預設值
                var createdBy = 1;
                
                var success = await _colorOptionService.CreateColorOptionAsync(model, createdBy);
                
                if (success)
                {
                    TempData[\SuccessMessage\] = \顏色選項建立成功\;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData[\ErrorMessage\] = \顏色選項建立失敗，請稍後再試\;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \建立顏色選項時發生錯誤\);
                TempData[\ErrorMessage\] = \建立顏色選項時發生錯誤，請稍後再試\;
                return View(model);
            }
        }

        /// <summary>
        /// 編輯顏色選項頁面
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var colorOption = await _colorOptionService.GetColorOptionByIdAsync(id);
                if (colorOption == null)
                {
                    TempData[\ErrorMessage\] = \找不到指定的顏色選項\;
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new PetColorOptionViewModel
                {
                    ColorOptionId = colorOption.ColorOptionId,
                    ColorName = colorOption.ColorName,
                    ColorCode = colorOption.ColorCode,
                    IsActive = colorOption.IsActive,
                    SortOrder = colorOption.SortOrder,
                    Remarks = colorOption.Remarks
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \取得顏色選項時發生錯誤，ID:
Id
\, id);
                TempData[\ErrorMessage\] = \取得顏色選項時發生錯誤，請稍後再試\;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 更新顏色選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetColorOptionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // 這裡應該從認證系統取得使用者ID，暫時使用預設值
                var updatedBy = 1;
                
                var success = await _colorOptionService.UpdateColorOptionAsync(model, updatedBy);
                
                if (success)
                {
                    TempData[\SuccessMessage\] = \顏色選項更新成功\;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData[\ErrorMessage\] = \顏色選項更新失敗，請稍後再試\;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \更新顏色選項時發生錯誤，ID:
Id
\, model.ColorOptionId);
                TempData[\ErrorMessage\] = \更新顏色選項時發生錯誤，請稍後再試\;
                return View(model);
            }
        }

        /// <summary>
        /// 刪除顏色選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _colorOptionService.DeleteColorOptionAsync(id);
                
                if (success)
                {
                    TempData[\SuccessMessage\] = \顏色選項刪除成功\;
                }
                else
                {
                    TempData[\ErrorMessage\] = \顏色選項刪除失敗，請稍後再試\;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \刪除顏色選項時發生錯誤，ID:
Id
\, id);
                TempData[\ErrorMessage\] = \刪除顏色選項時發生錯誤，請稍後再試\;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 切換顏色選項啟用狀態
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                // 這裡應該從認證系統取得使用者ID，暫時使用預設值
                var updatedBy = 1;
                
                var success = await _colorOptionService.ToggleColorOptionStatusAsync(id, updatedBy);
                
                if (success)
                {
                    TempData[\SuccessMessage\] = \顏色選項狀態更新成功\;
                }
                else
                {
                    TempData[\ErrorMessage\] = \顏色選項狀態更新失敗，請稍後再試\;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \切換顏色選項狀態時發生錯誤，ID:
Id
\, id);
                TempData[\ErrorMessage\] = \切換顏色選項狀態時發生錯誤，請稍後再試\;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
