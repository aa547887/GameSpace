namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// SystemSettings 更新服務介面
    /// 提供 SystemSettings 表的新增、更新、刪除功能
    /// </summary>
    public interface ISystemSettingsMutationService
    {
        /// <summary>
        /// 更新或新增設定值（字串）
        /// </summary>
        /// <param name="settingKey">設定Key</param>
        /// <param name="settingValue">設定Value</param>
        /// <param name="description">設定說明</param>
        /// <param name="updatedBy">更新者ID</param>
        Task<(bool success, string message)> UpsertSettingAsync(string settingKey, string settingValue, string? description = null, int? updatedBy = null);

        /// <summary>
        /// 更新或新增整數設定值
        /// </summary>
        Task<(bool success, string message)> UpsertSettingIntAsync(string settingKey, int value, string? description = null, int? updatedBy = null);

        /// <summary>
        /// 更新或新增小數設定值
        /// </summary>
        Task<(bool success, string message)> UpsertSettingDecimalAsync(string settingKey, decimal value, string? description = null, int? updatedBy = null);

        /// <summary>
        /// 更新或新增布林設定值
        /// </summary>
        Task<(bool success, string message)> UpsertSettingBoolAsync(string settingKey, bool value, string? description = null, int? updatedBy = null);

        /// <summary>
        /// 批次更新多個設定值
        /// </summary>
        /// <param name="settings">設定字典（Key-Value pairs）</param>
        /// <param name="updatedBy">更新者ID</param>
        Task<(bool success, string message, int updatedCount)> BatchUpsertSettingsAsync(Dictionary<string, string> settings, int? updatedBy = null);

        /// <summary>
        /// 刪除設定值（軟刪除）
        /// </summary>
        Task<(bool success, string message)> DeleteSettingAsync(string settingKey, int? deletedBy = null, string? deleteReason = null);

        /// <summary>
        /// 啟用設定
        /// </summary>
        Task<(bool success, string message)> ActivateSettingAsync(string settingKey, int? updatedBy = null);

        /// <summary>
        /// 停用設定
        /// </summary>
        Task<(bool success, string message)> DeactivateSettingAsync(string settingKey, int? updatedBy = null);
    }
}
