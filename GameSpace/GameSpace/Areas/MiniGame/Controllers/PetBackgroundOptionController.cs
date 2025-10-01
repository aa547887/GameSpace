using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物背景選項管理控制器
    /// </summary>
    [Area(\
MiniGame\)]
    [Authorize]
    public class PetBackgroundOptionController : Controller
    {
        private readonly IPetBackgroundOptionService _backgroundOptionService;
        private readonly ILogger<PetBackgroundOptionController> _logger;

        public PetBackgroundOptionController(
            IPetBackgroundOptionService backgroundOptionService,
            ILogger<PetBackgroundOptionController> logger)
        {
            _backgroundOptionService = backgroundOptionService;
            _logger = logger;
        }

        /// <summary>
        /// 背景選項列表頁面
        /// </summary>
        public async Task<IActionResult> Index(string? searchKeyword = null, bool showActiveOnly = false)
        {
            try
            {
                var backgroundOptions = new List<PetBackgroundOption>();
                
                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    backgroundOptions = await _backgroundOptionService.SearchBackgroundOptionsAsync(searchKeyword, showActiveOnly);
                }
                else if (showActiveOnly)
                {
                    backgroundOptions = await _backgroundOptionService.GetActiveBackgroundOptionsAsync();
                }
                else
                {
                    backgroundOptions = await _backgroundOptionService.GetAllBackgroundOptionsAsync();
                }

                var statistics = await _backgroundOptionService.GetBackgroundOptionStatisticsAsync();

                var viewModel = new PetBackgroundOptionListViewModel
                {
                    BackgroundOptions = backgroundOptions,
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
                _logger.LogError(ex, \取得背景選項列表時發生錯誤\);
                TempData[\ErrorMessage\] = \取得背景選項列表時發生錯誤，請稍後再試\;
                return View(new PetBackgroundOptionListViewModel());
            }
        }

        /// <summary>
        /// 建立背景選項頁面
        /// </summary>
        public IActionResult Create()
        {
            return View(new PetBackgroundOptionViewModel());
        }

        /// <summary>
        /// 建立背景選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetBackgroundOptionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // 這裡應該從認證系統取得使用者ID，暫時使用預設值
                var createdBy = 1;
                
                var success = await _backgroundOptionService.CreateBackgroundOptionAsync(model, createdBy);
                
                if (success)
                {
                    TempData[\SuccessMessage\] = \背景選項建立成功\;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData[\ErrorMessage\] = \背景選項建立失敗，請稍後再試\;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \建立背景選項時發生錯誤\);
                TempData[\ErrorMessage\] = \建立背景選項時發生錯誤，請稍後再試\;
                return View(model);
            }
        }

        /// <summary>
        /// 編輯背景選項頁面
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var backgroundOption = await _backgroundOptionService.GetBackgroundOptionByIdAsync(id);
                if (backgroundOption == null)
                {
                    TempData[\ErrorMessage\] = \找不到指定的背景選項\;
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new PetBackgroundOptionViewModel
                {
                    BackgroundOptionId = backgroundOption.BackgroundOptionId,
                    BackgroundName = backgroundOption.BackgroundName,
                    Description = backgroundOption.Description,
                    ImagePath = backgroundOption.ImagePath,
                    IsActive = backgroundOption.IsActive,
                    SortOrder = backgroundOption.SortOrder,
                    Remarks = backgroundOption.Remarks
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \取得背景選項時發生錯誤，ID:
Id
\, id);
                TempData[\ErrorMessage\] = \取得背景選項時發生錯誤，請稍後再試\;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 更新背景選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetBackgroundOptionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // 這裡應該從認證系統取得使用者ID，暫時使用預設值
                var updatedBy = 1;
                
                var success = await _backgroundOptionService.UpdateBackgroundOptionAsync(model, updatedBy);
                
                if (success)
                {
                    TempData[\SuccessMessage\] = \背景選項更新成功\;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData[\ErrorMessage\] = \背景選項更新失敗，請稍後再試\;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \更新背景選項時發生錯誤，ID:
Id
\, model.BackgroundOptionId);
                TempData[\ErrorMessage\] = \更新背景選項時發生錯誤，請稍後再試\;
                return View(model);
            }
        }

        /// <summary>
        /// 刪除背景選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _backgroundOptionService.DeleteBackgroundOptionAsync(id);
                
                if (success)
                {
                    TempData[\SuccessMessage\] = \背景選項刪除成功\;
                }
                else
                {
                    TempData[\ErrorMessage\] = \背景選項刪除失敗，請稍後再試\;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \刪除背景選項時發生錯誤，ID:
Id
\, id);
                TempData[\ErrorMessage\] = \刪除背景選項時發生錯誤，請稍後再試\;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 切換背景選項啟用狀態
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                // 這裡應該從認證系統取得使用者ID，暫時使用預設值
                var updatedBy = 1;
                
                var success = await _backgroundOptionService.ToggleBackgroundOptionStatusAsync(id, updatedBy);
                
                if (success)
                {
                    TempData[\SuccessMessage\] = \背景選項狀態更新成功\;
                }
                else
                {
                    TempData[\ErrorMessage\] = \背景選項狀態更新失敗，請稍後再試\;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \切換背景選項狀態時發生錯誤，ID:
Id
\, id);
                TempData[\ErrorMessage\] = \切換背景選項狀態時發生錯誤，請稍後再試\;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
