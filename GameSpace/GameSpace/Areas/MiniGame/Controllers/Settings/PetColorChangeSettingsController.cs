using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models.Settings;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Controllers.Settings
{
    /// <summary>
    /// 寵物換色點數設定控制器
    /// </summary>
    [Area(""MiniGame"")]
    [Authorize]
    public class PetColorChangeSettingsController : Controller
    {
        private readonly ILogger<PetColorChangeSettingsController> _logger;

        public PetColorChangeSettingsController(ILogger<PetColorChangeSettingsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 寵物換色點數設定列表
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            // TODO: 實作從資料庫讀取設定列表
            var settings = new List<PetColorChangeSettingsViewModel>
            {
                new PetColorChangeSettingsViewModel
                {
                    Id = 1,
                    ColorName = "紅色",
                    RequiredPoints = 100,
                    ColorCode = "#FF0000",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new PetColorChangeSettingsViewModel
                {
                    Id = 2,
                    ColorName = "藍色",
                    RequiredPoints = 150,
                    ColorCode = "#0000FF",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            return View(settings);
        }

        /// <summary>
        /// 新增寵物換色點數設定
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new PetColorChangeSettingsViewModel());
        }

        /// <summary>
        /// 新增寵物換色點數設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PetColorChangeSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // TODO: 實作儲存到資料庫
                _logger.LogInformation("新增寵物換色點數設定: {ColorName}, 所需點數: {RequiredPoints}", 
                    model.ColorName, model.RequiredPoints);

                TempData["SuccessMessage"] = "寵物換色點數設定新增成功！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增寵物換色點數設定失敗");
                ModelState.AddModelError("", "新增失敗，請稍後再試");
                return View(model);
            }
        }

        /// <summary>
        /// 編輯寵物換色點數設定
        /// </summary>
        [HttpGet]
        public IActionResult Edit(int id)
        {
            // TODO: 實作從資料庫讀取設定
            var model = new PetColorChangeSettingsViewModel
            {
                Id = id,
                ColorName = "紅色",
                RequiredPoints = 100,
                ColorCode = "#FF0000",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            return View(model);
        }

        /// <summary>
        /// 編輯寵物換色點數設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, PetColorChangeSettingsViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // TODO: 實作更新到資料庫
                _logger.LogInformation("更新寵物換色點數設定: {ColorName}, 所需點數: {RequiredPoints}", 
                    model.ColorName, model.RequiredPoints);

                TempData["SuccessMessage"] = "寵物換色點數設定更新成功！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物換色點數設定失敗");
                ModelState.AddModelError("", "更新失敗，請稍後再試");
                return View(model);
            }
        }

        /// <summary>
        /// 刪除寵物換色點數設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                // TODO: 實作從資料庫刪除設定
                _logger.LogInformation("刪除寵物換色點數設定: {Id}", id);

                TempData["SuccessMessage"] = "寵物換色點數設定刪除成功！";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物換色點數設定失敗");
                TempData["ErrorMessage"] = "刪除失敗，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
