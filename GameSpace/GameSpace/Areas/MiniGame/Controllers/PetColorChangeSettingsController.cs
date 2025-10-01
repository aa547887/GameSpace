using GameSpace.Areas.MiniGame.Models.Settings;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物換色點數設定控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize]
    public class PetColorChangeSettingsController : Controller
    {
        private readonly IPetColorChangeSettingsService _settingsService;

        public PetColorChangeSettingsController(IPetColorChangeSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        /// 寵物換色點數設定列表
        /// </summary>
        /// <returns>設定列表頁面</returns>
        public async Task<IActionResult> Index()
        {
            var settings = await _settingsService.GetAllSettingsAsync();
            return View(settings);
        }

        /// <summary>
        /// 寵物換色點數設定詳情
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>設定詳情頁面</returns>
        public async Task<IActionResult> Details(int id)
        {
            var setting = await _settingsService.GetSettingByIdAsync(id);
            if (setting == null)
            {
                return NotFound();
            }

            return View(setting);
        }

        /// <summary>
        /// 新增寵物換色點數設定頁面
        /// </summary>
        /// <returns>新增頁面</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// 新增寵物換色點數設定
        /// </summary>
        /// <param name="model">設定資料</param>
        /// <returns>新增結果</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetColorChangeSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var setting = new PetColorChangeSettings
                {
                    SettingName = model.SettingName,
                    PointsRequired = model.PointsRequired,
                    IsActive = model.IsActive,
                    CreatedBy = 1, // TODO: 從登入使用者取得
                    Remarks = model.Remarks
                };

                await _settingsService.CreateSettingAsync(setting);
                TempData["SuccessMessage"] = "寵物換色點數設定新增成功！";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        /// <summary>
        /// 編輯寵物換色點數設定頁面
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>編輯頁面</returns>
        public async Task<IActionResult> Edit(int id)
        {
            var setting = await _settingsService.GetSettingByIdAsync(id);
            if (setting == null)
            {
                return NotFound();
            }

            var model = new PetColorChangeSettingsViewModel
            {
                SettingId = setting.SettingId,
                SettingName = setting.SettingName,
                PointsRequired = setting.PointsRequired,
                IsActive = setting.IsActive,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt,
                CreatedBy = setting.CreatedBy,
                UpdatedBy = setting.UpdatedBy,
                Remarks = setting.Remarks
            };

            return View(model);
        }

        /// <summary>
        /// 編輯寵物換色點數設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <param name="model">設定資料</param>
        /// <returns>編輯結果</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetColorChangeSettingsViewModel model)
        {
            if (id != model.SettingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var setting = await _settingsService.GetSettingByIdAsync(id);
                if (setting == null)
                {
                    return NotFound();
                }

                setting.SettingName = model.SettingName;
                setting.PointsRequired = model.PointsRequired;
                setting.IsActive = model.IsActive;
                setting.UpdatedBy = 1; // TODO: 從登入使用者取得
                setting.Remarks = model.Remarks;

                await _settingsService.UpdateSettingAsync(setting);
                TempData["SuccessMessage"] = "寵物換色點數設定更新成功！";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        /// <summary>
        /// 刪除寵物換色點數設定頁面
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>刪除確認頁面</returns>
        public async Task<IActionResult> Delete(int id)
        {
            var setting = await _settingsService.GetSettingByIdAsync(id);
            if (setting == null)
            {
                return NotFound();
            }

            return View(setting);
        }

        /// <summary>
        /// 刪除寵物換色點數設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>刪除結果</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _settingsService.DeleteSettingAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "寵物換色點數設定刪除成功！";
            }
            else
            {
                TempData["ErrorMessage"] = "寵物換色點數設定刪除失敗！";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
