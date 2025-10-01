using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.MiniGame.Controllers
{
    /// <summary>
    /// 寵物選項統一管理控制器
    /// </summary>
    [Area("MiniGame")]
    [Authorize]
    public class PetOptionManagementController : Controller
    {
        private readonly IPetColorOptionService _colorOptionService;
        private readonly IPetBackgroundOptionService _backgroundOptionService;
        private readonly ILogger<PetOptionManagementController> _logger;

        public PetOptionManagementController(
            IPetColorOptionService colorOptionService,
            IPetBackgroundOptionService backgroundOptionService,
            ILogger<PetOptionManagementController> logger)
        {
            _colorOptionService = colorOptionService;
            _backgroundOptionService = backgroundOptionService;
            _logger = logger;
        }

        /// <summary>
        /// 選項管理首頁
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = new PetOptionManagementViewModel
                {
                    ColorOptions = await _colorOptionService.GetColorOptionsAsync(1, 10, null, null),
                    BackgroundOptions = await _backgroundOptionService.GetBackgroundOptionsAsync(null, null, 1)
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得選項管理資料時發生錯誤");
                TempData["ErrorMessage"] = "取得選項管理資料時發生錯誤，請稍後再試";
                return View(new PetOptionManagementViewModel());
            }
        }

        /// <summary>
        /// 顏色選項管理頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ColorOptions(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? isActiveFilter = null)
        {
            try
            {
                var model = await _colorOptionService.GetColorOptionsAsync(pageNumber, pageSize, searchTerm, isActiveFilter);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得顏色選項列表時發生錯誤");
                TempData["ErrorMessage"] = "取得顏色選項列表時發生錯誤，請稍後再試";
                return View(new PetColorOptionListViewModel());
            }
        }

        /// <summary>
        /// 背景選項管理頁面
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> BackgroundOptions(string? searchKeyword = null, bool? isActiveFilter = null, int page = 1)
        {
            try
            {
                var model = await _backgroundOptionService.GetBackgroundOptionsAsync(searchKeyword, isActiveFilter, page);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得背景選項列表時發生錯誤");
                TempData["ErrorMessage"] = "取得背景選項列表時發生錯誤，請稍後再試";
                return View(new PetBackgroundOptionListViewModel());
            }
        }

        /// <summary>
        /// 快速新增顏色選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickAddColor([FromBody] QuickAddColorRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "輸入資料驗證失敗" });
                }

                var result = await _colorOptionService.CreateColorOptionAsync(new PetColorOptionFormViewModel
                {
                    ColorName = request.ColorName,
                    ColorCode = request.ColorCode,
                    Description = request.Description,
                    IsActive = request.IsActive,
                    SortOrder = request.SortOrder
                });

                if (result)
                {
                    return Json(new { success = true, message = "顏色選項新增成功" });
                }
                else
                {
                    return Json(new { success = false, message = "顏色選項新增失敗" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "快速新增顏色選項時發生錯誤");
                return Json(new { success = false, message = "新增失敗，請稍後再試" });
            }
        }

        /// <summary>
        /// 快速新增背景選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickAddBackground([FromBody] QuickAddBackgroundRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "輸入資料驗證失敗" });
                }

                var result = await _backgroundOptionService.CreateBackgroundOptionAsync(new PetBackgroundOptionFormViewModel
                {
                    Name = request.Name,
                    BackgroundColorCode = request.BackgroundColorCode,
                    Description = request.Description,
                    IsActive = request.IsActive,
                    SortOrder = request.SortOrder
                });

                if (result)
                {
                    return Json(new { success = true, message = "背景選項新增成功" });
                }
                else
                {
                    return Json(new { success = false, message = "背景選項新增失敗" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "快速新增背景選項時發生錯誤");
                return Json(new { success = false, message = "新增失敗，請稍後再試" });
            }
        }

        /// <summary>
        /// 切換選項狀態
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string optionType, int id)
        {
            try
            {
                bool result = false;
                string message = "";

                switch (optionType.ToLower())
                {
                    case "color":
                        result = await _colorOptionService.ToggleColorOptionStatusAsync(id);
                        message = result ? "顏色選項狀態更新成功" : "顏色選項狀態更新失敗";
                        break;
                    case "background":
                        result = await _backgroundOptionService.ToggleBackgroundOptionStatusAsync(id);
                        message = result ? "背景選項狀態更新成功" : "背景選項狀態更新失敗";
                        break;
                    default:
                        return Json(new { success = false, message = "無效的選項類型" });
                }

                return Json(new { success = result, message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換選項狀態時發生錯誤");
                return Json(new { success = false, message = "狀態更新失敗，請稍後再試" });
            }
        }

        /// <summary>
        /// 刪除選項
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOption(string optionType, int id)
        {
            try
            {
                bool result = false;
                string message = "";

                switch (optionType.ToLower())
                {
                    case "color":
                        result = await _colorOptionService.DeleteColorOptionAsync(id);
                        message = result ? "顏色選項刪除成功" : "顏色選項刪除失敗";
                        break;
                    case "background":
                        result = await _backgroundOptionService.DeleteBackgroundOptionAsync(id);
                        message = result ? "背景選項刪除成功" : "背景選項刪除失敗";
                        break;
                    default:
                        return Json(new { success = false, message = "無效的選項類型" });
                }

                return Json(new { success = result, message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除選項時發生錯誤");
                return Json(new { success = false, message = "刪除失敗，請稍後再試" });
            }
        }
    }

    /// <summary>
    /// 快速新增顏色選項請求
    /// </summary>
    public class QuickAddColorRequest
    {
        public string ColorName { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// 快速新增背景選項請求
    /// </summary>
    public class QuickAddBackgroundRequest
    {
        public string Name { get; set; } = string.Empty;
        public string BackgroundColorCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }
}
