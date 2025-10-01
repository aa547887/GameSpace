using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Authorize]
    public class PetColorCostSettingController : Controller
    {
        private readonly IPetColorCostSettingService _service;

        public PetColorCostSettingController(IPetColorCostSettingService service)
        {
            _service = service;
        }

        // GET: PetColorCostSetting
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool? isActiveFilter = null)
        {
            var model = await _service.GetListAsync(pageNumber, pageSize, searchTerm, isActiveFilter);
            return View(model);
        }

        // GET: PetColorCostSetting/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var setting = await _service.GetByIdAsync(id.Value);
            if (setting == null)
                return NotFound();

            var model = new PetColorCostSettingViewModel
            {
                Id = setting.Id,
                SettingName = setting.SettingName,
                RequiredPoints = setting.RequiredPoints,
                IsActive = setting.IsActive,
                Description = setting.Description,
                SortOrder = setting.SortOrder,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };

            return View(model);
        }

        // GET: PetColorCostSetting/Create
        public IActionResult Create()
        {
            return View(new PetColorCostSettingViewModel());
        }

        // POST: PetColorCostSetting/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetColorCostSettingViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _service.CreateAsync(model);
                    TempData["SuccessMessage"] = "寵物換色所需點數設定已成功建立";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"建立失敗：{ex.Message}");
                }
            }

            return View(model);
        }

        // GET: PetColorCostSetting/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var setting = await _service.GetByIdAsync(id.Value);
            if (setting == null)
                return NotFound();

            var model = new PetColorCostSettingViewModel
            {
                Id = setting.Id,
                SettingName = setting.SettingName,
                RequiredPoints = setting.RequiredPoints,
                IsActive = setting.IsActive,
                Description = setting.Description,
                SortOrder = setting.SortOrder,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };

            return View(model);
        }

        // POST: PetColorCostSetting/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetColorCostSettingViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _service.UpdateAsync(id, model);
                    TempData["SuccessMessage"] = "寵物換色所需點數設定已成功更新";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    return NotFound();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"更新失敗：{ex.Message}");
                }
            }

            return View(model);
        }

        // GET: PetColorCostSetting/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var setting = await _service.GetByIdAsync(id.Value);
            if (setting == null)
                return NotFound();

            var model = new PetColorCostSettingViewModel
            {
                Id = setting.Id,
                SettingName = setting.SettingName,
                RequiredPoints = setting.RequiredPoints,
                IsActive = setting.IsActive,
                Description = setting.Description,
                SortOrder = setting.SortOrder,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };

            return View(model);
        }

        // POST: PetColorCostSetting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "寵物換色所需點數設定已成功刪除";
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除失敗，找不到指定的設定項目";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"刪除失敗：{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
