using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物背景選項管理控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize]
    public class PetBackgroundOptionController : Controller
    {
        private readonly IPetBackgroundOptionService _backgroundOptionService;

        public PetBackgroundOptionController(IPetBackgroundOptionService backgroundOptionService)
        {
            _backgroundOptionService = backgroundOptionService;
        }

        /// <summary>
        /// 背景選項列表頁面
        /// </summary>
        public async Task<IActionResult> Index(string? searchKeyword = null, bool? isActiveFilter = null, int page = 1)
        {
            var model = await _backgroundOptionService.GetBackgroundOptionsAsync(searchKeyword, isActiveFilter, page);
            return View(model);
        }

        /// <summary>
        /// 背景選項詳情頁面
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var backgroundOption = await _backgroundOptionService.GetBackgroundOptionByIdAsync(id);
            if (backgroundOption == null)
            {
                return NotFound();
            }

            return View(backgroundOption);
        }

        /// <summary>
        /// 新增背景選項頁面
        /// </summary>
        public IActionResult Create()
        {
            return View(new PetBackgroundOptionFormViewModel());
        }

        /// <summary>
        /// 處理新增背景選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetBackgroundOptionFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 檢查名稱是否重複
            if (await _backgroundOptionService.IsNameDuplicateAsync(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "背景名稱已存在，請使用其他名稱");
                return View(model);
            }

            // 檢查顏色代碼是否重複
            if (await _backgroundOptionService.IsColorCodeDuplicateAsync(model.BackgroundColorCode))
            {
                ModelState.AddModelError(nameof(model.BackgroundColorCode), "背景顏色代碼已存在，請使用其他代碼");
                return View(model);
            }

            var success = await _backgroundOptionService.CreateBackgroundOptionAsync(model);
            if (success)
            {
                TempData["SuccessMessage"] = "背景選項新增成功";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "新增背景選項時發生錯誤，請稍後再試");
            return View(model);
        }

        /// <summary>
        /// 編輯背景選項頁面
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var backgroundOption = await _backgroundOptionService.GetBackgroundOptionByIdAsync(id);
            if (backgroundOption == null)
            {
                return NotFound();
            }

            var model = new PetBackgroundOptionFormViewModel
            {
                Id = backgroundOption.Id,
                Name = backgroundOption.Name,
                Description = backgroundOption.Description,
                BackgroundColorCode = backgroundOption.BackgroundColorCode,
                IsActive = backgroundOption.IsActive,
                SortOrder = backgroundOption.SortOrder
            };

            return View(model);
        }

        /// <summary>
        /// 處理編輯背景選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetBackgroundOptionFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 檢查名稱是否重複
            if (await _backgroundOptionService.IsNameDuplicateAsync(model.Name, id))
            {
                ModelState.AddModelError(nameof(model.Name), "背景名稱已存在，請使用其他名稱");
                return View(model);
            }

            // 檢查顏色代碼是否重複
            if (await _backgroundOptionService.IsColorCodeDuplicateAsync(model.BackgroundColorCode, id))
            {
                ModelState.AddModelError(nameof(model.BackgroundColorCode), "背景顏色代碼已存在，請使用其他代碼");
                return View(model);
            }

            var success = await _backgroundOptionService.UpdateBackgroundOptionAsync(id, model);
            if (success)
            {
                TempData["SuccessMessage"] = "背景選項更新成功";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "更新背景選項時發生錯誤，請稍後再試");
            return View(model);
        }

        /// <summary>
        /// 刪除背景選項頁面
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            var backgroundOption = await _backgroundOptionService.GetBackgroundOptionByIdAsync(id);
            if (backgroundOption == null)
            {
                return NotFound();
            }

            return View(backgroundOption);
        }

        /// <summary>
        /// 處理刪除背景選項
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _backgroundOptionService.DeleteBackgroundOptionAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "背景選項刪除成功";
            }
            else
            {
                TempData["ErrorMessage"] = "刪除背景選項時發生錯誤，請稍後再試";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 檢查背景選項名稱是否可用
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckNameAvailability(string name, int? excludeId = null)
        {
            var isDuplicate = await _backgroundOptionService.IsNameDuplicateAsync(name, excludeId);
            return Json(new { isAvailable = !isDuplicate });
        }

        /// <summary>
        /// 檢查背景選項顏色代碼是否可用
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CheckColorCodeAvailability(string colorCode, int? excludeId = null)
        {
            var isDuplicate = await _backgroundOptionService.IsColorCodeDuplicateAsync(colorCode, excludeId);
            return Json(new { isAvailable = !isDuplicate });
        }
    }
}
