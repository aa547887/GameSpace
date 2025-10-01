using Areas.MiniGame.Models;
using Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物換色所需點數設定控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize]
    public class PetSkinColorPointSettingController : Controller
    {
        private readonly IPetSkinColorPointSettingService _service;
        private readonly ILogger<PetSkinColorPointSettingController> _logger;

        public PetSkinColorPointSettingController(
            IPetSkinColorPointSettingService service,
            ILogger<PetSkinColorPointSettingController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// 首頁 - 顯示設定列表
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var model = await _service.GetAllAsync(page, pageSize);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物換色所需點數設定列表時發生錯誤");
                TempData["ErrorMessage"] = "取得設定列表時發生錯誤，請稍後再試";
                return View(new PetSkinColorPointSettingListViewModel());
            }
        }

        /// <summary>
        /// 新增設定頁面
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new PetSkinColorPointSettingViewModel());
        }

        /// <summary>
        /// 新增設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetSkinColorPointSettingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 取得目前使用者ID (這裡需要根據實際的認證系統調整)
                var userId = GetCurrentUserId();
                
                var success = await _service.CreateAsync(model, userId);
                if (success)
                {
                    TempData["SuccessMessage"] = "寵物換色所需點數設定新增成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "新增設定失敗，請稍後再試";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增寵物換色所需點數設定時發生錯誤");
                TempData["ErrorMessage"] = "新增設定時發生錯誤，請稍後再試";
            }

            return View(model);
        }

        /// <summary>
        /// 編輯設定頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _service.GetByIdAsync(id);
                if (model == null)
                {
                    TempData["ErrorMessage"] = "找不到指定的設定";
                    return RedirectToAction(nameof(Index));
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物換色所需點數設定時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "取得設定時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 更新設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PetSkinColorPointSettingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // 取得目前使用者ID (這裡需要根據實際的認證系統調整)
                var userId = GetCurrentUserId();
                
                var success = await _service.UpdateAsync(model, userId);
                if (success)
                {
                    TempData["SuccessMessage"] = "寵物換色所需點數設定更新成功";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "更新設定失敗，請稍後再試";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物換色所需點數設定時發生錯誤，ID: {Id}", model.Id);
                TempData["ErrorMessage"] = "更新設定時發生錯誤，請稍後再試";
            }

            return View(model);
        }

        /// <summary>
        /// 刪除設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "寵物換色所需點數設定刪除成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除設定失敗，請稍後再試";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物換色所需點數設定時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "刪除設定時發生錯誤，請稍後再試";
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
            try
            {
                // 取得目前使用者ID (這裡需要根據實際的認證系統調整)
                var userId = GetCurrentUserId();
                
                var success = await _service.ToggleEnabledAsync(id, userId);
                if (success)
                {
                    TempData["SuccessMessage"] = "寵物換色所需點數設定狀態切換成功";
                }
                else
                {
                    TempData["ErrorMessage"] = "切換狀態失敗，請稍後再試";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換寵物換色所需點數設定啟用狀態時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "切換狀態時發生錯誤，請稍後再試";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 取得目前使用者ID
        /// </summary>
        private int GetCurrentUserId()
        {
            // 這裡需要根據實際的認證系統調整
            // 暫時回傳預設值，實際實作時需要從 Claims 或 Session 中取得
            return 30000001; // 預設管理員ID
        }
    }
}
