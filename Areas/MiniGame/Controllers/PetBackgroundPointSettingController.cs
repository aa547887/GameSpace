using Areas.MiniGame.Models;
using Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物換背景所需點數設定控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize]
    public class PetBackgroundPointSettingController : Controller
    {
        private readonly IPetBackgroundPointSettingService _service;

        public PetBackgroundPointSettingController(IPetBackgroundPointSettingService service)
        {
            _service = service;
        }

        /// <summary>
        /// 設定列表頁面
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? searchKeyword = null, bool? isEnabledFilter = null)
        {
            var model = await _service.GetSettingsAsync(page, pageSize, searchKeyword, isEnabledFilter);
            return View(model);
        }

        /// <summary>
        /// 新增設定頁面
        /// </summary>
        public IActionResult Create()
        {
            return View(new PetBackgroundPointSettingViewModel());
        }

        /// <summary>
        /// 新增設定處理
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetBackgroundPointSettingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 檢查寵物等級是否已存在
            if (await _service.IsPetLevelExistsAsync(model.PetLevel))
            {
                ModelState.AddModelError(nameof(model.PetLevel), "此寵物等級的設定已存在");
                return View(model);
            }

            // 取得目前使用者ID（這裡暫時使用固定值，實際應從認證系統取得）
            var currentUserId = 30000001; // 實際應從 HttpContext.User 取得

            var success = await _service.CreateSettingAsync(model, currentUserId);
            if (success)
            {
                TempData["SuccessMessage"] = "設定新增成功";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "新增設定失敗，請稍後再試");
            return View(model);
        }

        /// <summary>
        /// 編輯設定頁面
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetSettingByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        /// <summary>
        /// 編輯設定處理
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetBackgroundPointSettingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 檢查寵物等級是否已存在（排除自己）
            if (await _service.IsPetLevelExistsAsync(model.PetLevel, model.Id))
            {
                ModelState.AddModelError(nameof(model.PetLevel), "此寵物等級的設定已存在");
                return View(model);
            }

            // 取得目前使用者ID（這裡暫時使用固定值，實際應從認證系統取得）
            var currentUserId = 30000001; // 實際應從 HttpContext.User 取得

            var success = await _service.UpdateSettingAsync(model, currentUserId);
            if (success)
            {
                TempData["SuccessMessage"] = "設定更新成功";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "更新設定失敗，請稍後再試");
            return View(model);
        }

        /// <summary>
        /// 刪除設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteSettingAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "設定刪除成功";
            }
            else
            {
                TempData["ErrorMessage"] = "刪除設定失敗，請稍後再試";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleEnabled(int id)
        {
            // 取得目前使用者ID（這裡暫時使用固定值，實際應從認證系統取得）
            var currentUserId = 30000001; // 實際應從 HttpContext.User 取得

            var success = await _service.ToggleEnabledAsync(id, currentUserId);
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

        /// <summary>
        /// 根據寵物等級取得所需點數（API）
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRequiredPoints(int petLevel)
        {
            var setting = await _service.GetSettingByPetLevelAsync(petLevel);
            if (setting == null)
            {
                return Json(new { success = false, message = "找不到對應的設定" });
            }

            return Json(new { success = true, requiredPoints = setting.RequiredPoints });
        }
    }
}
