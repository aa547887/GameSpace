using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GameSpace.Areas.MiniGame.Models.Settings;
using GameSpace.Areas.MiniGame.Data;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Controllers.Settings
{
    [Area("MiniGame")]
    [Authorize]
    public class PetBackgroundChangeSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PetBackgroundChangeSettingsController> _logger;

        public PetBackgroundChangeSettingsController(ApplicationDbContext context, ILogger<PetBackgroundChangeSettingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: MiniGame/Settings/PetBackgroundChangeSettings
        public async Task<IActionResult> Index()
        {
            try
            {
                var settings = await _context.PetBackgroundChangeSettings
                    .OrderBy(s => s.Id)
                    .ToListAsync();

                var viewModels = settings.Select(s => new PetBackgroundChangeSettingsViewModel
                {
                    Id = s.Id,
                    BackgroundColor = s.BackgroundColor,
                    PointsRequired = s.PointsRequired,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToList();

                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pet background change settings");
                TempData["ErrorMessage"] = "載入寵物換背景點數設定時發生錯誤";
                return View(new List<PetBackgroundChangeSettingsViewModel>());
            }
        }

        // GET: MiniGame/Settings/PetBackgroundChangeSettings/Create
        public IActionResult Create()
        {
            return View(new PetBackgroundChangeSettingsViewModel());
        }

        // POST: MiniGame/Settings/PetBackgroundChangeSettings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PetBackgroundChangeSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var setting = new PetBackgroundChangeSettings
                {
                    BackgroundColor = model.BackgroundColor,
                    PointsRequired = model.PointsRequired,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PetBackgroundChangeSettings.Add(setting);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new pet background change setting: {BackgroundColor}, Points: {PointsRequired}", 
                    setting.BackgroundColor, setting.PointsRequired);

                TempData["SuccessMessage"] = "寵物換背景點數設定已成功新增";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pet background change setting");
                ModelState.AddModelError("", "新增寵物換背景點數設定時發生錯誤");
                return View(model);
            }
        }

        // GET: MiniGame/Settings/PetBackgroundChangeSettings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var setting = await _context.PetBackgroundChangeSettings.FindAsync(id);
                if (setting == null)
                {
                    return NotFound();
                }

                var model = new PetBackgroundChangeSettingsViewModel
                {
                    Id = setting.Id,
                    BackgroundColor = setting.BackgroundColor,
                    PointsRequired = setting.PointsRequired,
                    IsActive = setting.IsActive,
                    CreatedAt = setting.CreatedAt,
                    UpdatedAt = setting.UpdatedAt
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pet background change setting for edit: {Id}", id);
                TempData["ErrorMessage"] = "載入寵物換背景點數設定時發生錯誤";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: MiniGame/Settings/PetBackgroundChangeSettings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PetBackgroundChangeSettingsViewModel model)
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
                var setting = await _context.PetBackgroundChangeSettings.FindAsync(id);
                if (setting == null)
                {
                    return NotFound();
                }

                setting.BackgroundColor = model.BackgroundColor;
                setting.PointsRequired = model.PointsRequired;
                setting.IsActive = model.IsActive;
                setting.UpdatedAt = DateTime.UtcNow;

                _context.Update(setting);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated pet background change setting: {Id}, BackgroundColor: {BackgroundColor}, Points: {PointsRequired}", 
                    setting.Id, setting.BackgroundColor, setting.PointsRequired);

                TempData["SuccessMessage"] = "寵物換背景點數設定已成功更新";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PetBackgroundChangeSettingsExists(model.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pet background change setting: {Id}", id);
                ModelState.AddModelError("", "更新寵物換背景點數設定時發生錯誤");
                return View(model);
            }
        }

        // GET: MiniGame/Settings/PetBackgroundChangeSettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var setting = await _context.PetBackgroundChangeSettings.FindAsync(id);
                if (setting == null)
                {
                    return NotFound();
                }

                var model = new PetBackgroundChangeSettingsViewModel
                {
                    Id = setting.Id,
                    BackgroundColor = setting.BackgroundColor,
                    PointsRequired = setting.PointsRequired,
                    IsActive = setting.IsActive,
                    CreatedAt = setting.CreatedAt,
                    UpdatedAt = setting.UpdatedAt
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pet background change setting for delete: {Id}", id);
                TempData["ErrorMessage"] = "載入寵物換背景點數設定時發生錯誤";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: MiniGame/Settings/PetBackgroundChangeSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var setting = await _context.PetBackgroundChangeSettings.FindAsync(id);
                if (setting != null)
                {
                    _context.PetBackgroundChangeSettings.Remove(setting);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Deleted pet background change setting: {Id}, BackgroundColor: {BackgroundColor}", 
                        setting.Id, setting.BackgroundColor);
                }

                TempData["SuccessMessage"] = "寵物換背景點數設定已成功刪除";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting pet background change setting: {Id}", id);
                TempData["ErrorMessage"] = "刪除寵物換背景點數設定時發生錯誤";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<bool> PetBackgroundChangeSettingsExists(int id)
        {
            return await _context.PetBackgroundChangeSettings.AnyAsync(e => e.Id == id);
        }
    }
}
