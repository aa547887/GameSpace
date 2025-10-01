using Areas.MiniGame.Models;
using Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Areas.MiniGame.Controllers
{
    /// <summary>
    /// 每日遊戲次數限制設定控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize]
    public class DailyGameLimitSettingController : Controller
    {
        private readonly IDailyGameLimitSettingService _service;

        public DailyGameLimitSettingController(IDailyGameLimitSettingService service)
        {
            _service = service;
        }

        /// <summary>
        /// 設定列表頁面
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var settings = await _service.GetAllAsync();
            var viewModel = settings.Select(s => new DailyGameLimitSettingListViewModel
            {
                Id = s.Id,
                SettingName = s.SettingName,
                DailyLimit = s.DailyLimit,
                IsEnabled = s.IsEnabled,
                Description = s.Description,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToList();

            return View(viewModel);
        }

        /// <summary>
        /// 新增設定頁面
        /// </summary>
        public IActionResult Create()
        {
            return View(new DailyGameLimitSettingFormViewModel());
        }

        /// <summary>
        /// 新增設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DailyGameLimitSettingFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 檢查名稱是否重複
            if (await _service.IsNameDuplicateAsync(model.SettingName))
            {
                ModelState.AddModelError(nameof(model.SettingName), "設定名稱已存在");
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst("Manager_Id")?.Value ?? "1");
                await _service.CreateAsync(model, userId);
                TempData["SuccessMessage"] = "設定新增成功";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"新增失敗：{ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// 編輯設定頁面
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            var setting = await _service.GetByIdAsync(id);
            if (setting == null)
                return NotFound();

            var model = new DailyGameLimitSettingFormViewModel
            {
                Id = setting.Id,
                SettingName = setting.SettingName,
                DailyLimit = setting.DailyLimit,
                IsEnabled = setting.IsEnabled,
                Description = setting.Description
            };

            return View(model);
        }

        /// <summary>
        /// 更新設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DailyGameLimitSettingFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            // 檢查名稱是否重複
            if (await _service.IsNameDuplicateAsync(model.SettingName, id))
            {
                ModelState.AddModelError(nameof(model.SettingName), "設定名稱已存在");
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst("Manager_Id")?.Value ?? "1");
                var success = await _service.UpdateAsync(id, model, userId);
                
                if (!success)
                {
                    ModelState.AddModelError("", "更新失敗，設定不存在");
                    return View(model);
                }

                TempData["SuccessMessage"] = "設定更新成功";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                return View(model);
            }
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
                    TempData["SuccessMessage"] = "設定刪除成功";
                else
                    TempData["ErrorMessage"] = "刪除失敗，設定不存在";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"刪除失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
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
                var success = await _service.ToggleStatusAsync(id);
                if (success)
                    TempData["SuccessMessage"] = "狀態切換成功";
                else
                    TempData["ErrorMessage"] = "狀態切換失敗，設定不存在";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"狀態切換失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// 設定詳情頁面
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var setting = await _service.GetByIdAsync(id);
            if (setting == null)
                return NotFound();

            var viewModel = new DailyGameLimitSettingListViewModel
            {
                Id = setting.Id,
                SettingName = setting.SettingName,
                DailyLimit = setting.DailyLimit,
                IsEnabled = setting.IsEnabled,
                Description = setting.Description,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };

            return View(viewModel);
        }
    }
}
