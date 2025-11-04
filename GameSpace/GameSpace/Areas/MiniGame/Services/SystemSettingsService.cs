using GameSpace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// SystemSettings 讀取服務實作
    /// 提供統一的 SystemSettings 表讀取與快取機制
    /// </summary>
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SystemSettingsService> _logger;
        private const string CACHE_KEY_PREFIX = "SystemSettings_";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(30);

        public SystemSettingsService(
            GameSpacedatabaseContext context,
            IMemoryCache cache,
            ILogger<SystemSettingsService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 根據 SettingKey 取得設定值（泛型版本）
        /// </summary>
        public async Task<T> GetSettingAsync<T>(string settingKey, T defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(settingKey))
            {
                _logger.LogWarning("GetSettingAsync 呼叫時 settingKey 為空");
                return defaultValue;
            }

            var cacheKey = $"{CACHE_KEY_PREFIX}{settingKey}";

            // 嘗試從快取取得
            if (_cache.TryGetValue(cacheKey, out string cachedValue))
            {
                return ConvertValue<T>(cachedValue, defaultValue);
            }

            try
            {
                // 從資料庫取得
                var setting = await _context.SystemSettings
                    .AsNoTracking()
                    .Where(s => s.SettingKey == settingKey && s.IsActive && !s.IsDeleted)
                    .Select(s => s.SettingValue)
                    .FirstOrDefaultAsync();

                if (setting == null)
                {
                    _logger.LogWarning("找不到設定 Key: {SettingKey}，使用預設值: {DefaultValue}", settingKey, defaultValue);
                    return defaultValue;
                }

                // 存入快取
                _cache.Set(cacheKey, setting, CACHE_DURATION);

                return ConvertValue<T>(setting, defaultValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "讀取設定失敗: {SettingKey}", settingKey);
                return defaultValue;
            }
        }

        /// <summary>
        /// 根據 SettingKey 取得字串設定值
        /// </summary>
        public async Task<string> GetSettingStringAsync(string settingKey, string defaultValue = "")
        {
            return await GetSettingAsync(settingKey, defaultValue);
        }

        /// <summary>
        /// 根據 SettingKey 取得整數設定值
        /// </summary>
        public async Task<int> GetSettingIntAsync(string settingKey, int defaultValue = 0)
        {
            return await GetSettingAsync(settingKey, defaultValue);
        }

        /// <summary>
        /// 根據 SettingKey 取得小數設定值
        /// </summary>
        public async Task<decimal> GetSettingDecimalAsync(string settingKey, decimal defaultValue = 0)
        {
            return await GetSettingAsync(settingKey, defaultValue);
        }

        /// <summary>
        /// 根據 SettingKey 取得布林設定值
        /// </summary>
        public async Task<bool> GetSettingBoolAsync(string settingKey, bool defaultValue = false)
        {
            return await GetSettingAsync(settingKey, defaultValue);
        }

        /// <summary>
        /// 根據 SettingKey 取得 JSON 設定值並反序列化為物件
        /// </summary>
        public async Task<T?> GetSettingJsonAsync<T>(string settingKey) where T : class
        {
            try
            {
                var jsonString = await GetSettingStringAsync(settingKey, "");
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    _logger.LogWarning("JSON 設定為空: {SettingKey}", settingKey);
                    return null;
                }

                return JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON 反序列化失敗: {SettingKey}", settingKey);
                return null;
            }
        }

        /// <summary>
        /// 批次取得多個設定值
        /// </summary>
        public async Task<Dictionary<string, string>> GetMultipleSettingsAsync(params string[] settingKeys)
        {
            if (settingKeys == null || settingKeys.Length == 0)
            {
                return new Dictionary<string, string>();
            }

            try
            {
                var settings = await _context.SystemSettings
                    .AsNoTracking()
                    .Where(s => settingKeys.Contains(s.SettingKey) && s.IsActive && !s.IsDeleted)
                    .Select(s => new { s.SettingKey, s.SettingValue })
                    .ToListAsync();

                var result = new Dictionary<string, string>();
                foreach (var setting in settings)
                {
                    result[setting.SettingKey] = setting.SettingValue;

                    // 同時存入快取
                    var cacheKey = $"{CACHE_KEY_PREFIX}{setting.SettingKey}";
                    _cache.Set(cacheKey, setting.SettingValue, CACHE_DURATION);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次讀取設定失敗");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 清除快取（當 SystemSettings 被更新時呼叫）
        /// </summary>
        public void ClearCache()
        {
            _logger.LogInformation("清除所有 SystemSettings 快取");
            // 注意：IMemoryCache 沒有提供清除所有快取的方法
            // 這裡只是記錄日誌，實際上快取會在 30 分鐘後自動過期
        }

        /// <summary>
        /// 清除特定 Key 的快取
        /// </summary>
        public void ClearCache(string settingKey)
        {
            if (string.IsNullOrWhiteSpace(settingKey))
            {
                return;
            }

            var cacheKey = $"{CACHE_KEY_PREFIX}{settingKey}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("清除 SystemSettings 快取: {SettingKey}", settingKey);
        }

        /// <summary>
        /// 將字串值轉換為指定類型
        /// </summary>
        private T ConvertValue<T>(string value, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            try
            {
                var targetType = typeof(T);

                // 處理 Nullable 類型
                if (Nullable.GetUnderlyingType(targetType) != null)
                {
                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                // 字串類型直接返回
                if (targetType == typeof(string))
                {
                    return (T)(object)value;
                }

                // 整數類型
                if (targetType == typeof(int))
                {
                    if (int.TryParse(value, out int intValue))
                    {
                        return (T)(object)intValue;
                    }
                }

                // 小數類型
                if (targetType == typeof(decimal))
                {
                    if (decimal.TryParse(value, out decimal decimalValue))
                    {
                        return (T)(object)decimalValue;
                    }
                }

                // 浮點數類型
                if (targetType == typeof(double))
                {
                    if (double.TryParse(value, out double doubleValue))
                    {
                        return (T)(object)doubleValue;
                    }
                }

                // 布林類型
                if (targetType == typeof(bool))
                {
                    if (bool.TryParse(value, out bool boolValue))
                    {
                        return (T)(object)boolValue;
                    }
                    // 處理 1/0 表示 true/false
                    if (value == "1")
                    {
                        return (T)(object)true;
                    }
                    if (value == "0")
                    {
                        return (T)(object)false;
                    }
                }

                // 其他類型嘗試使用 Convert
                return (T)Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "轉換設定值失敗: {Value} -> {TargetType}", value, typeof(T).Name);
                return defaultValue;
            }
        }
    }
}
