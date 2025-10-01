using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using GameSpace.Areas.MiniGame.Models.Settings;
using GameSpace.Areas.MiniGame.Services;
using System.Threading.Tasks;
using System.Linq;

namespace GameSpace.Areas.MiniGame.Controllers.Settings
{
    [Area("MiniGame")]
    [Authorize(Roles = "Admin")]
    public class PointsSettingsController : Controller
    {
        private readonly ILogger<PointsSettingsController> _logger;
        private readonly IPetColorChangeSettingsService _petColorService;
        private readonly IPetBackgroundChangeSettingsService _petBackgroundService;

        public PointsSettingsController(
            ILogger<PointsSettingsController> logger,
            IPetColorChangeSettingsService petColorService,
            IPetBackgroundChangeSettingsService petBackgroundService)
        {
            _logger = logger;
            _petColorService = petColorService;
            _petBackgroundService = petBackgroundService;
        }

        // GET: MiniGame/PointsSettings
        public async Task<IActionResult> Index()
        {
            try
            {
                var colorSettings = await _petColorService.GetAllAsync();
                var backgroundSettings = await _petBackgroundService.GetAllAsync();

                var viewModel = new PointsSettingsIndexViewModel
                {
                    ColorSettings = colorSettings.ToList(),
                    BackgroundSettings = backgroundSettings.ToList()
                };

                return View(viewModel);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading points settings");
                TempData["ErrorMessage"] = "載入點數設定時發生錯誤";
                return View(new PointsSettingsIndexViewModel());
            }
        }

        // GET: MiniGame/PointsSettings/ColorSettings
        public IActionResult ColorSettings()
        {
            return RedirectToAction("Index", "PetColorChangeSettings");
        }

        // GET: MiniGame/PointsSettings/BackgroundSettings
        public IActionResult BackgroundSettings()
        {
            return RedirectToAction("Index", "PetBackgroundChangeSettings");
        }

        // GET: MiniGame/PointsSettings/Statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var colorSettings = await _petColorService.GetAllAsync();
                var backgroundSettings = await _petBackgroundService.GetAllAsync();

                var statistics = new PointsSettingsStatisticsViewModel
                {
                    TotalColorSettings = colorSettings.Count(),
                    ActiveColorSettings = colorSettings.Count(s => s.IsActive),
                    TotalBackgroundSettings = backgroundSettings.Count(),
                    ActiveBackgroundSettings = backgroundSettings.Count(s => s.IsActive),
                    TotalColorPoints = colorSettings.Where(s => s.IsActive).Sum(s => s.PointsRequired),
                    TotalBackgroundPoints = backgroundSettings.Where(s => s.IsActive).Sum(s => s.PointsRequired)
                };

                return View(statistics);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading points settings statistics");
                TempData["ErrorMessage"] = "載入統計資料時發生錯誤";
                return RedirectToAction("Index");
            }
        }
    }
}
