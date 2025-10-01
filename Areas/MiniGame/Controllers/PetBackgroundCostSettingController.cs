using Areas.MiniGame.Models;
using Areas.MiniGame.Models.ViewModels;
using Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物換背景所需點數設定控制器
    /// </summary>
    [Authorize]
    [Area("MiniGame")]
    public class PetBackgroundCostSettingController : Controller
    {
        private readonly IPetBackgroundCostSettingService _service;

        public PetBackgroundCostSettingController(IPetBackgroundCostSettingService service)
        {
            _service = service;
        }

        /// <summary>
        /// 寵物換背景所需點數設定列表
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, string? searchTerm = null, bool? isActive = null)
        {
            var model = await _service.GetPagedAsync(page, 10, searchTerm, isActive);
            return View(model);
        }

        /// <summary>
        /// 建立寵物換背景所需點數設定
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new PetBackgroundCostSettingViewModels.CreateViewModel());
        }

        /// <summary>
        /// 建立寵物換背景所需點數設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetBackgroundCostSettingViewModels.CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 檢查設定名稱是否已存在
                if (await _service.SettingNameExistsAsync(model.SettingName))
                {
                    ModelState.AddModelError(nameof(model.SettingName), "此設定名稱已存在");
                    return View(model);
                }

                var result = await _service.CreateAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "寵物換背景所需點數設定建立成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "建立失敗，請稍後再試");
                }
            }

            return View(model);
        }

        /// <summary>
        /// 編輯寵物換背景所需點數設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var setting = await _service.GetByIdAsync(id);
            if (setting == null)
            {
                return NotFound();
            }

            var model = new PetBackgroundCostSettingViewModels.EditViewModel
            {
                Id = setting.Id,
                SettingName = setting.SettingName,
                RequiredPoints = setting.RequiredPoints,
                IsActive = setting.IsActive,
                Description = setting.Description,
                SortOrder = setting.SortOrder
            };

            return View(model);
        }

        /// <summary>
        /// 編輯寵物換背景所需點數設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetBackgroundCostSettingViewModels.EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 檢查設定名稱是否已存在
                if (await _service.SettingNameExistsAsync(model.SettingName, model.Id))
                {
                    ModelState.AddModelError(nameof(model.SettingName), "此設定名稱已存在");
                    return View(model);
                }

                var result = await _service.UpdateAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "寵物換背景所需點數設定更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "更新失敗，請稍後再試");
                }
            }

            return View(model);
        }

        /// <summary>
        /// 查看寵物換背景所需點數設定詳情
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var setting = await _service.GetByIdAsync(id);
            if (setting == null)
            {
                return NotFound();
            }

            return View(setting);
        }

        /// <summary>
        /// 刪除寵物換背景所需點數設定
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var setting = await _service.GetByIdAsync(id);
            if (setting == null)
            {
                return NotFound();
            }

            return View(setting);
        }

        /// <summary>
        /// 確認刪除寵物換背景所需點數設定
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "寵物換背景所需點數設定刪除成功";
            }
            else
            {
                TempData["ErrorMessage"] = "刪除失敗，請稍後再試";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
