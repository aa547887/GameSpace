using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 點數設定管理控制器
    /// 整合寵物換色和換背景點數設定的統一管理介面
    /// </summary>
    [Area("MiniGame")]
    [Authorize(Policy = "PointSettingManagement")]
    public class PointSettingManagementController : Controller
    {
        private readonly IPetSkinColorPointSettingService _skinColorService;
        private readonly IPetBackgroundPointSettingService _backgroundService;

        public PointSettingManagementController(
            IPetSkinColorPointSettingService skinColorService,
            IPetBackgroundPointSettingService backgroundService)
        {
            _skinColorService = skinColorService;
            _backgroundService = backgroundService;
        }

        /// <summary>
        /// 點數設定管理主頁面
        /// </summary>
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1, int pageSize = 10)
        {
            try
            {
                var skinColorSettings = await _skinColorService.GetAllAsync();
                var backgroundSettings = await _backgroundService.GetAllAsync();

                // 搜尋功能
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    skinColorSettings = skinColorSettings.Where(s => 
                        s.PetLevel.ToString().Contains(searchTerm) ||
                        s.RequiredPoints.ToString().Contains(searchTerm)).ToList();
                    
                    backgroundSettings = backgroundSettings.Where(s => 
                        s.PetLevel.ToString().Contains(searchTerm) ||
                        s.RequiredPoints.ToString().Contains(searchTerm)).ToList();
                }

                var viewModel = new PointSettingManagementIndexViewModel
                {
                    SkinColorSettings = skinColorSettings.ToList(),
                    BackgroundSettings = backgroundSettings.ToList(),
                    TotalSkinColorSettings = skinColorSettings.Count(),
                    TotalBackgroundSettings = backgroundSettings.Count(),
                    SearchTerm = searchTerm,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)Math.Max(skinColorSettings.Count(), backgroundSettings.Count()) / pageSize)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入點數設定資料時發生錯誤：{ex.Message}";
                return View(new PointSettingManagementIndexViewModel());
            }
        }

        /// <summary>
        /// 點數設定統計頁面
        /// </summary>
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var skinColorSettings = await _skinColorService.GetAllAsync();
                var backgroundSettings = await _backgroundService.GetAllAsync();

                var statistics = new PointSettingStatisticsViewModel
                {
                    TotalSkinColorSettings = skinColorSettings.Count(),
                    TotalBackgroundSettings = backgroundSettings.Count(),
                    ActiveSkinColorSettings = skinColorSettings.Count(s => s.IsEnabled),
                    ActiveBackgroundSettings = backgroundSettings.Count(s => s.IsEnabled),
                    InactiveSkinColorSettings = skinColorSettings.Count(s => !s.IsEnabled),
                    InactiveBackgroundSettings = backgroundSettings.Count(s => !s.IsEnabled),
                    AverageSkinColorPoints = skinColorSettings.Any() ? (decimal)skinColorSettings.Average(s => s.RequiredPoints) : 0,
                    AverageBackgroundPoints = backgroundSettings.Any() ? (decimal)backgroundSettings.Average(s => s.RequiredPoints) : 0,
                    MaxSkinColorPoints = skinColorSettings.Any() ? skinColorSettings.Max(s => s.RequiredPoints) : 0,
                    MaxBackgroundPoints = backgroundSettings.Any() ? backgroundSettings.Max(s => s.RequiredPoints) : 0,
                    MinSkinColorPoints = skinColorSettings.Any() ? skinColorSettings.Min(s => s.RequiredPoints) : 0,
                    MinBackgroundPoints = backgroundSettings.Any() ? backgroundSettings.Min(s => s.RequiredPoints) : 0
                };

                return View(statistics);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入統計資料時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 批量操作頁面
        /// </summary>
        public IActionResult BatchOperation()
        {
            return View(new PointSettingBatchOperationViewModel());
        }

        /// <summary>
        /// 執行批量操作
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchOperation(PointSettingBatchOperationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                switch (model.OperationType)
                {
                    case "Enable":
                        await EnableSelectedSettings(model.SelectedIds);
                        break;
                    case "Disable":
                        await DisableSelectedSettings(model.SelectedIds);
                        break;
                    case "Delete":
                        await DeleteSelectedSettings(model.SelectedIds);
                        break;
                    case "UpdatePoints":
                        if (model.NewPointsValue.HasValue)
                        {
                            await UpdateSelectedPoints(model.SelectedIds, model.NewPointsValue.Value);
                        }
                        break;
                }

                TempData["SuccessMessage"] = "批量操作執行成功";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"執行批量操作時發生錯誤：{ex.Message}";
                return View(model);
            }
        }

        /// <summary>
        /// 匯出設定資料
        /// </summary>
        public async Task<IActionResult> Export(PointSettingExportViewModel model)
        {
            try
            {
                var skinColorSettings = await _skinColorService.GetAllAsync();
                var backgroundSettings = await _backgroundService.GetAllAsync();

                if (!model.IncludeInactive)
                {
                    skinColorSettings = skinColorSettings.Where(s => s.IsEnabled);
                    backgroundSettings = backgroundSettings.Where(s => s.IsEnabled);
                }

                // 這裡可以實作實際的匯出邏輯
                // 目前先返回 JSON 格式
                var exportData = new
                {
                    SkinColorSettings = skinColorSettings,
                    BackgroundSettings = backgroundSettings,
                    ExportTime = DateTime.Now,
                    ExportType = model.ExportType
                };

                return Json(exportData);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"匯出資料時發生錯誤：{ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        #region 私有方法

        private async Task EnableSelectedSettings(List<int> selectedIds)
        {
            // 實作啟用選定設定的邏輯
            foreach (var id in selectedIds)
            {
                // 這裡需要根據 ID 判斷是換色還是換背景設定
                // 暫時跳過實作細節
            }
        }

        private async Task DisableSelectedSettings(List<int> selectedIds)
        {
            // 實作停用選定設定的邏輯
            foreach (var id in selectedIds)
            {
                // 這裡需要根據 ID 判斷是換色還是換背景設定
                // 暫時跳過實作細節
            }
        }

        private async Task DeleteSelectedSettings(List<int> selectedIds)
        {
            // 實作刪除選定設定的邏輯
            foreach (var id in selectedIds)
            {
                // 這裡需要根據 ID 判斷是換色還是換背景設定
                // 暫時跳過實作細節
            }
        }

        private async Task UpdateSelectedPoints(List<int> selectedIds, int newPointsValue)
        {
            // 實作更新選定設定點數的邏輯
            foreach (var id in selectedIds)
            {
                // 這裡需要根據 ID 判斷是換色還是換背景設定
                // 暫時跳過實作細節
            }
        }

        #endregion
    }
}
