using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.social_hub.Auth;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Services;

namespace GameSpace.Areas.MiniGame.Controllers.Settings
{
    /// <summary>
    /// 系統設定管理控制器 - 提供 SystemSettings 表的完整 CRUD 功能
    /// 讓管理員可以透過後台 UI 直接調整所有商業規則配置
    /// </summary>
    [Area("MiniGame")]
    [Authorize(AuthenticationSchemes = AuthConstants.AdminCookieScheme, Policy = "AdminOnly")]
    public class SystemSettingsController : MiniGameBaseController
    {
        private readonly ILogger<SystemSettingsController> _logger;
        private readonly IFuzzySearchService _fuzzySearchService;

        public SystemSettingsController(
            GameSpacedatabaseContext context,
            ILogger<SystemSettingsController> logger,
            IFuzzySearchService fuzzySearchService) : base(context)
        {
            _logger = logger;
            _fuzzySearchService = fuzzySearchService;
        }

        /// <summary>
        /// 系統設定列表頁 (Index)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? category = null, string? searchKey = null)
        {
            try
            {
                var query = _context.SystemSettings
                    .Where(s => !s.IsDeleted)
                    .AsQueryable();

                // 篩選：依 Category
                if (!string.IsNullOrWhiteSpace(category))
                {
                    query = query.Where(s => s.Category == category);
                }

                List<SystemSetting> settings;

                // 篩選：依 SettingKey with fuzzy search
                if (!string.IsNullOrWhiteSpace(searchKey))
                {
                    // Apply fuzzy search with 5-level priority on SettingKey and Description
                    var allSettings = await query.ToListAsync();

                    settings = allSettings
                        .Select(s => new
                        {
                            Setting = s,
                            Priority = _fuzzySearchService.CalculateMatchPriority(searchKey, s.SettingKey ?? "", s.Description ?? "")
                        })
                        .Where(x => x.Priority > 0)
                        .OrderBy(x => x.Priority)
                        .ThenBy(x => x.Setting.Category)
                        .ThenBy(x => x.Setting.SettingKey)
                        .Select(x => x.Setting)
                        .ToList();
                }
                else
                {
                    settings = await query
                        .OrderBy(s => s.Category)
                        .ThenBy(s => s.SettingKey)
                        .ToListAsync();
                }

                // 取得所有 Category 供篩選使用
                var categories = await _context.SystemSettings
                    .Where(s => !s.IsDeleted)
                    .Select(s => s.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                ViewBag.Categories = categories;
                ViewBag.CurrentCategory = category;
                ViewBag.SearchKey = searchKey;

                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得系統設定列表時發生錯誤");
                TempData["ErrorMessage"] = "取得系統設定列表時發生錯誤，請稍後再試";
                return View(new List<SystemSetting>());
            }
        }

        /// <summary>
        /// 顯示單一設定詳情 (Details)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingId == id && !s.IsDeleted);

                if (setting == null)
                {
                    TempData["ErrorMessage"] = "找不到該設定項目";
                    return RedirectToAction(nameof(Index));
                }

                return View(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得設定詳情時發生錯誤 (SettingId: {SettingId})", id);
                TempData["ErrorMessage"] = "取得設定詳情時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 顯示新增設定表單 (Create GET)
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new SystemSetting
            {
                Category = "General",
                SettingType = "String",
                IsActive = true,
                IsReadOnly = false
            });
        }

        /// <summary>
        /// 處理新增設定 (Create POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SystemSetting model)
        {
            try
            {
                // 移除不需要驗證的欄位
                ModelState.Remove("SettingId");
                ModelState.Remove("CreatedAt");
                ModelState.Remove("UpdatedAt");
                ModelState.Remove("UpdatedBy");
                ModelState.Remove("DeletedAt");
                ModelState.Remove("DeletedBy");
                ModelState.Remove("DeleteReason");

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "表單驗證失敗，請檢查輸入資料";
                    return View(model);
                }

                // 檢查 SettingKey 是否已存在
                var exists = await _context.SystemSettings
                    .AnyAsync(s => s.SettingKey == model.SettingKey && !s.IsDeleted);

                if (exists)
                {
                    ModelState.AddModelError("SettingKey", "此設定鍵 (SettingKey) 已存在");
                    return View(model);
                }

                // 設定建立時間
                model.CreatedAt = DateTime.UtcNow;
                model.IsDeleted = false;

                _context.SystemSettings.Add(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功新增系統設定：{SettingKey}", model.SettingKey);
                TempData["SuccessMessage"] = $"成功新增系統設定：{model.SettingKey}";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增系統設定時發生錯誤");
                TempData["ErrorMessage"] = "新增系統設定時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 顯示編輯設定表單 (Edit GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingId == id && !s.IsDeleted);

                if (setting == null)
                {
                    TempData["ErrorMessage"] = "找不到該設定項目";
                    return RedirectToAction(nameof(Index));
                }

                // 檢查是否為唯讀設定
                if (setting.IsReadOnly == true)
                {
                    TempData["WarningMessage"] = "此設定標記為唯讀，請謹慎修改";
                }

                return View(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得編輯設定時發生錯誤 (SettingId: {SettingId})", id);
                TempData["ErrorMessage"] = "取得編輯設定時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 處理編輯設定 (Edit POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SystemSetting model)
        {
            if (id != model.SettingId)
            {
                TempData["ErrorMessage"] = "設定 ID 不符";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // 移除不需要驗證的欄位
                ModelState.Remove("CreatedAt");
                ModelState.Remove("UpdatedAt");
                ModelState.Remove("UpdatedBy");
                ModelState.Remove("DeletedAt");
                ModelState.Remove("DeletedBy");
                ModelState.Remove("DeleteReason");

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "表單驗證失敗，請檢查輸入資料";
                    return View(model);
                }

                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingId == id && !s.IsDeleted);

                if (setting == null)
                {
                    TempData["ErrorMessage"] = "找不到該設定項目";
                    return RedirectToAction(nameof(Index));
                }

                // 更新欄位
                setting.SettingKey = model.SettingKey;
                setting.SettingValue = model.SettingValue;
                setting.Description = model.Description;
                setting.Category = model.Category;
                setting.SettingType = model.SettingType;
                setting.IsReadOnly = model.IsReadOnly;
                setting.IsActive = model.IsActive;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功更新系統設定：{SettingKey}", setting.SettingKey);
                TempData["SuccessMessage"] = $"成功更新系統設定：{setting.SettingKey}";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "更新系統設定時發生並行衝突 (SettingId: {SettingId})", id);
                TempData["ErrorMessage"] = "更新失敗：資料已被其他使用者修改，請重新整理後再試";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新系統設定時發生錯誤 (SettingId: {SettingId})", id);
                TempData["ErrorMessage"] = "更新系統設定時發生錯誤，請稍後再試";
                return View(model);
            }
        }

        /// <summary>
        /// 顯示刪除確認頁 (Delete GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingId == id && !s.IsDeleted);

                if (setting == null)
                {
                    TempData["ErrorMessage"] = "找不到該設定項目";
                    return RedirectToAction(nameof(Index));
                }

                // 檢查是否為唯讀設定
                if (setting.IsReadOnly == true)
                {
                    TempData["ErrorMessage"] = "此設定標記為唯讀，無法刪除";
                    return RedirectToAction(nameof(Index));
                }

                return View(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得刪除確認頁時發生錯誤 (SettingId: {SettingId})", id);
                TempData["ErrorMessage"] = "取得刪除確認頁時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 處理刪除 (Delete POST) - 軟刪除
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? deleteReason)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingId == id && !s.IsDeleted);

                if (setting == null)
                {
                    TempData["ErrorMessage"] = "找不到該設定項目";
                    return RedirectToAction(nameof(Index));
                }

                // 檢查是否為唯讀設定
                if (setting.IsReadOnly == true)
                {
                    TempData["ErrorMessage"] = "此設定標記為唯讀，無法刪除";
                    return RedirectToAction(nameof(Index));
                }

                // 執行軟刪除
                setting.IsDeleted = true;
                setting.DeletedAt = DateTime.UtcNow;
                setting.DeleteReason = deleteReason ?? "管理員手動刪除";

                await _context.SaveChangesAsync();

                _logger.LogWarning("系統設定已刪除：{SettingKey}, 原因：{Reason}",
                    setting.SettingKey, setting.DeleteReason);
                TempData["SuccessMessage"] = $"成功刪除系統設定：{setting.SettingKey}";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除系統設定時發生錯誤 (SettingId: {SettingId})", id);
                TempData["ErrorMessage"] = "刪除系統設定時發生錯誤，請稍後再試";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 切換設定啟用狀態 (AJAX)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingId == id && !s.IsDeleted);

                if (setting == null)
                {
                    return Json(new { success = false, message = "找不到該設定項目" });
                }

                setting.IsActive = !setting.IsActive;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("系統設定 {SettingKey} 啟用狀態已切換為：{IsActive}",
                    setting.SettingKey, setting.IsActive);

                return Json(new {
                    success = true,
                    isActive = setting.IsActive,
                    message = $"設定 {setting.SettingKey} 已{(setting.IsActive ? "啟用" : "停用")}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換設定啟用狀態時發生錯誤 (SettingId: {SettingId})", id);
                return Json(new { success = false, message = "操作失敗，請稍後再試" });
            }
        }

        /// <summary>
        /// 取得設定值 (AJAX) - 供測試/預覽使用
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetValue(string key)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.SettingKey == key && !s.IsDeleted && s.IsActive);

                if (setting == null)
                {
                    return Json(new { success = false, message = "找不到該設定或設定未啟用" });
                }

                return Json(new {
                    success = true,
                    key = setting.SettingKey,
                    value = setting.SettingValue,
                    type = setting.SettingType,
                    category = setting.Category
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得設定值時發生錯誤 (Key: {Key})", key);
                return Json(new { success = false, message = "操作失敗，請稍後再試" });
            }
        }
    }
}
