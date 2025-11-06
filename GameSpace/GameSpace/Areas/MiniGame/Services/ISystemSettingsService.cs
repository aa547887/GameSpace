using System.Text.Json;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// SystemSettings 讀取服務介面
    /// 提供統一的 SystemSettings 表讀取與快取機制
    /// </summary>
    public interface ISystemSettingsService
    {
        /// <summary>
        /// 根據 SettingKey 取得設定值（泛型版本）
        /// </summary>
        Task<T> GetSettingAsync<T>(string settingKey, T defaultValue = default);

        /// <summary>
        /// 根據 SettingKey 取得字串設定值
        /// </summary>
        Task<string> GetSettingStringAsync(string settingKey, string defaultValue = "");

        /// <summary>
        /// 根據 SettingKey 取得整數設定值
        /// </summary>
        Task<int> GetSettingIntAsync(string settingKey, int defaultValue = 0);

        /// <summary>
        /// 根據 SettingKey 取得小數設定值
        /// </summary>
        Task<decimal> GetSettingDecimalAsync(string settingKey, decimal defaultValue = 0);

        /// <summary>
        /// 根據 SettingKey 取得布林設定值
        /// </summary>
        Task<bool> GetSettingBoolAsync(string settingKey, bool defaultValue = false);

        /// <summary>
        /// 根據 SettingKey 取得 JSON 設定值並反序列化為物件
        /// </summary>
        Task<T?> GetSettingJsonAsync<T>(string settingKey) where T : class;

        /// <summary>
        /// 批次取得多個設定值
        /// </summary>
        Task<Dictionary<string, string>> GetMultipleSettingsAsync(params string[] settingKeys);

        /// <summary>
        /// 清除快取（當 SystemSettings 被更新時呼叫）
        /// </summary>
        void ClearCache();

        /// <summary>
        /// 清除特定 Key 的快取
        /// </summary>
        void ClearCache(string settingKey);
    }
}
