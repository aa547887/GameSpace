using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// SystemSettings 更新服務實作
    /// </summary>
    public class SystemSettingsMutationService : ISystemSettingsMutationService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly ISystemSettingsService _settingsService;
        private readonly ILogger<SystemSettingsMutationService> _logger;

        public SystemSettingsMutationService(
            GameSpacedatabaseContext context,
            ISystemSettingsService settingsService,
            ILogger<SystemSettingsMutationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 更新或新增設定值（字串）
        /// </summary>
        public async Task<(bool success, string message)> UpsertSettingAsync(
            string settingKey,
            string settingValue,
            string? description = null,
            int? updatedBy = null)
        {
            if (string.IsNullOrWhiteSpace(settingKey))
            {
                return (false, "設定Key不可為空");
            }

            try
            {
                var existing = await _context.SystemSettings
                    .Where(s => s.SettingKey == settingKey && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    // 更新現有設定
                    existing.SettingValue = settingValue ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        existing.Description = description;
                    }
                    existing.UpdatedAt = DateTime.UtcNow;
                    existing.UpdatedBy = updatedBy;

                    _context.SystemSettings.Update(existing);
                }
                else
                {
                    // 新增設定
                    var newSetting = new SystemSetting
                    {
                        SettingKey = settingKey,
                        SettingValue = settingValue ?? string.Empty,
                        Description = description ?? settingKey,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = updatedBy,
                        IsDeleted = false
                    };

                    _context.SystemSettings.Add(newSetting);
                }

                await _context.SaveChangesAsync();

                // 清除快取
                _settingsService.ClearCache(settingKey);

                _logger.LogInformation("設定 {SettingKey} 已更新為 {SettingValue}", settingKey, settingValue);
                return (true, "設定更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新設定失敗: {SettingKey}", settingKey);
                return (false, $"更新設定失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新或新增整數設定值
        /// </summary>
        public async Task<(bool success, string message)> UpsertSettingIntAsync(
            string settingKey,
            int value,
            string? description = null,
            int? updatedBy = null)
        {
            return await UpsertSettingAsync(settingKey, value.ToString(), description, updatedBy);
        }

        /// <summary>
        /// 更新或新增小數設定值
        /// </summary>
        public async Task<(bool success, string message)> UpsertSettingDecimalAsync(
            string settingKey,
            decimal value,
            string? description = null,
            int? updatedBy = null)
        {
            return await UpsertSettingAsync(settingKey, value.ToString("F1"), description, updatedBy);
        }

        /// <summary>
        /// 更新或新增布林設定值
        /// </summary>
        public async Task<(bool success, string message)> UpsertSettingBoolAsync(
            string settingKey,
            bool value,
            string? description = null,
            int? updatedBy = null)
        {
            return await UpsertSettingAsync(settingKey, value.ToString().ToLower(), description, updatedBy);
        }

        /// <summary>
        /// 批次更新多個設定值
        /// </summary>
        public async Task<(bool success, string message, int updatedCount)> BatchUpsertSettingsAsync(
            Dictionary<string, string> settings,
            int? updatedBy = null)
        {
            if (settings == null || settings.Count == 0)
            {
                return (false, "沒有要更新的設定", 0);
            }

            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int updatedCount = 0;

                foreach (var kvp in settings)
                {
                    var result = await UpsertSettingAsync(kvp.Key, kvp.Value, null, updatedBy);
                    if (result.success)
                    {
                        updatedCount++;
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation("批次更新 {Count} 個設定成功", updatedCount);
                return (true, $"成功更新 {updatedCount} 個設定", updatedCount);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "批次更新設定失敗");
                return (false, $"批次更新失敗: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// 刪除設定值（軟刪除）
        /// </summary>
        public async Task<(bool success, string message)> DeleteSettingAsync(
            string settingKey,
            int? deletedBy = null,
            string? deleteReason = null)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .Where(s => s.SettingKey == settingKey && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (setting == null)
                {
                    return (false, "找不到該設定");
                }

                setting.IsDeleted = true;
                setting.DeletedAt = DateTime.UtcNow;
                setting.DeletedBy = deletedBy;
                setting.DeleteReason = deleteReason ?? "管理員刪除";

                _context.SystemSettings.Update(setting);
                await _context.SaveChangesAsync();

                // 清除快取
                _settingsService.ClearCache(settingKey);

                _logger.LogInformation("設定 {SettingKey} 已刪除", settingKey);
                return (true, "設定刪除成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除設定失敗: {SettingKey}", settingKey);
                return (false, $"刪除設定失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 啟用設定
        /// </summary>
        public async Task<(bool success, string message)> ActivateSettingAsync(string settingKey, int? updatedBy = null)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .Where(s => s.SettingKey == settingKey && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (setting == null)
                {
                    return (false, "找不到該設定");
                }

                setting.IsActive = true;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedBy = updatedBy;

                _context.SystemSettings.Update(setting);
                await _context.SaveChangesAsync();

                // 清除快取
                _settingsService.ClearCache(settingKey);

                _logger.LogInformation("設定 {SettingKey} 已啟用", settingKey);
                return (true, "設定已啟用");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "啟用設定失敗: {SettingKey}", settingKey);
                return (false, $"啟用設定失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 停用設定
        /// </summary>
        public async Task<(bool success, string message)> DeactivateSettingAsync(string settingKey, int? updatedBy = null)
        {
            try
            {
                var setting = await _context.SystemSettings
                    .Where(s => s.SettingKey == settingKey && !s.IsDeleted)
                    .FirstOrDefaultAsync();

                if (setting == null)
                {
                    return (false, "找不到該設定");
                }

                setting.IsActive = false;
                setting.UpdatedAt = DateTime.UtcNow;
                setting.UpdatedBy = updatedBy;

                _context.SystemSettings.Update(setting);
                await _context.SaveChangesAsync();

                // 清除快取
                _settingsService.ClearCache(settingKey);

                _logger.LogInformation("設定 {SettingKey} 已停用", settingKey);
                return (true, "設定已停用");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停用設定失敗: {SettingKey}", settingKey);
                return (false, $"停用設定失敗: {ex.Message}");
            }
        }
    }
}
