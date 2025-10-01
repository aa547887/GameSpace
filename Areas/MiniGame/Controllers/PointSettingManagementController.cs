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
        private readonly IPointSettingStorageService _storageService;

        public PointSettingManagementController(
            IPetSkinColorPointSettingService skinColorService,
            IPetBackgroundPointSettingService backgroundService,
            IPointSettingStorageService storageService)
        {
            _skinColorService = skinColorService;
            _backgroundService = backgroundService;
            _storageService = storageService;
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
                var statistics = await _storageService.GetPointSettingStatisticsAsync();

                var viewModel = new PointSettingStatisticsViewModel
                {
                    TotalSkinColorSettings = statistics.TotalSkinColorSettings,
                    TotalBackgroundSettings = statistics.TotalBackgroundSettings,
                    ActiveSkinColorSettings = statistics.ActiveSkinColorSettings,
                    ActiveBackgroundSettings = statistics.ActiveBackgroundSettings,
                    InactiveSkinColorSettings = statistics.TotalSkinColorSettings - statistics.ActiveSkinColorSettings,
                    InactiveBackgroundSettings = statistics.TotalBackgroundSettings - statistics.ActiveBackgroundSettings,
                    AverageSkinColorPoints = statistics.AverageSkinColorPoints,
                    AverageBackgroundPoints = statistics.AverageBackgroundPoints,
                    MaxSkinColorPoints = statistics.MaxSkinColorPoints,
                    MaxBackgroundPoints = statistics.MaxBackgroundPoints,
                    MinSkinColorPoints = statistics.MinSkinColorPoints,
                    MinBackgroundPoints = statistics.MinBackgroundPoints
                };

                return View(viewModel);
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

                var storageModels = new List<PointSettingStorageModel>();

                // 根據操作類型建立儲存模型
                foreach (var id in model.SelectedIds)
                {
                    var storageModel = new PointSettingStorageModel
                    {
                        Id = id,
                        UserId = GetCurrentUserId(),
                        Operation = model.OperationType switch
                        {
                            "Enable" => "Toggle",
                            "Disable" => "Toggle",
                            "Delete" => "Delete",
                            "UpdatePoints" => "Update",
                            _ => "Update"
                        }
                    };

                    // 根據 ID 判斷設定類型
                    var skinColorSetting = await _skinColorService.GetByIdAsync(id);
                    if (skinColorSetting != null)
                    {
                        storageModel.SettingType = "SkinColor";
                        storageModel.PetLevel = skinColorSetting.PetLevel;
                        storageModel.RequiredPoints = model.NewPointsValue ?? skinColorSetting.RequiredPoints;
                        storageModel.IsEnabled = model.OperationType == "Enable" ? true : 
                                               model.OperationType == "Disable" ? false : skinColorSetting.IsEnabled;
                    }
                    else
                    {
                        var backgroundSetting = await _backgroundService.GetSettingByIdAsync(id);
                        if (backgroundSetting != null)
                        {
                            storageModel.SettingType = "Background";
                            storageModel.PetLevel = backgroundSetting.PetLevel;
                            storageModel.RequiredPoints = model.NewPointsValue ?? backgroundSetting.RequiredPoints;
                            storageModel.IsEnabled = model.OperationType == "Enable" ? true : 
                                                   model.OperationType == "Disable" ? false : backgroundSetting.IsEnabled;
                            storageModel.Remarks = backgroundSetting.Remarks;
                        }
                    }

                    storageModels.Add(storageModel);
                }

                // 執行批量儲存
                var result = await _storageService.BatchSavePointSettingsAsync(storageModels);

                if (result.ErrorCount > 0)
                {
                    TempData["WarningMessage"] = $"批量操作完成，成功 {result.SuccessCount} 筆，失敗 {result.ErrorCount} 筆。錯誤詳情：{string.Join(", ", result.ErrorMessages)}";
                }
                else
                {
                    TempData["SuccessMessage"] = $"批量操作執行成功，共處理 {result.SuccessCount} 筆資料";
                }

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

        /// <summary>
        /// 儲存單一點數設定
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePointSetting(PointSettingStorageModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "模型驗證失敗" });
                }

                var result = await _storageService.SavePointSettingAsync(model);

                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message, affectedRows = result.AffectedRows });
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"儲存時發生錯誤：{ex.Message}" });
            }
        }

        /// <summary>
        /// 驗證點數設定
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ValidatePointSetting(PointSettingStorageModel model)
        {
            try
            {
                var result = await _storageService.ValidatePointSettingAsync(model);

                return Json(new
                {
                    isValid = result.IsValid,
                    validationErrors = result.ValidationErrors,
                    warnings = result.Warnings
                });
            }
            catch (Exception ex)
            {
                return Json(new { isValid = false, validationErrors = new[] { $"驗證時發生錯誤：{ex.Message}" } });
            }
        }

        #region 私有方法

        /// <summary>
        /// 取得當前使用者ID
        /// </summary>
        private int GetCurrentUserId()
        {
            // 這裡應該從認證系統取得當前使用者ID
            // 暫時返回預設值
            return 30000001; // 預設管理員ID
        }

        #endregion
    }
}
