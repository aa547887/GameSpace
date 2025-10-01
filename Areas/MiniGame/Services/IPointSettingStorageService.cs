using Areas.MiniGame.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Areas.MiniGame.Services
{
    /// <summary>
    /// 點數設定儲存邏輯服務
    /// 處理點數設定的儲存、驗證和業務邏輯
    /// </summary>
    public interface IPointSettingStorageService
    {
        /// <summary>
        /// 儲存點數設定
        /// </summary>
        Task<PointSettingStorageResult> SavePointSettingAsync(PointSettingStorageModel model);

        /// <summary>
        /// 批量儲存點數設定
        /// </summary>
        Task<PointSettingBatchStorageResult> BatchSavePointSettingsAsync(List<PointSettingStorageModel> models);

        /// <summary>
        /// 驗證點數設定
        /// </summary>
        Task<PointSettingValidationResult> ValidatePointSettingAsync(PointSettingStorageModel model);

        /// <summary>
        /// 取得點數設定統計
        /// </summary>
        Task<PointSettingStatisticsResult> GetPointSettingStatisticsAsync();
    }

    /// <summary>
    /// 點數設定儲存模型
    /// </summary>
    public class PointSettingStorageModel
    {
        public int Id { get; set; }
        public string SettingType { get; set; } = ""; // "SkinColor" or "Background"
        public int PetLevel { get; set; }
        public int RequiredPoints { get; set; }
        public bool IsEnabled { get; set; }
        public string? Remarks { get; set; }
        public int UserId { get; set; }
        public string Operation { get; set; } = ""; // "Create", "Update", "Delete", "Toggle"
    }

    /// <summary>
    /// 點數設定儲存結果
    /// </summary>
    public class PointSettingStorageResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int AffectedRows { get; set; }
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// 點數設定批量儲存結果
    /// </summary>
    public class PointSettingBatchStorageResult
    {
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }
        public List<string> ErrorMessages { get; set; } = new();
        public List<PointSettingStorageResult> Results { get; set; } = new();
    }

    /// <summary>
    /// 點數設定驗證結果
    /// </summary>
    public class PointSettingValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// 點數設定統計結果
    /// </summary>
    public class PointSettingStatisticsResult
    {
        public int TotalSkinColorSettings { get; set; }
        public int TotalBackgroundSettings { get; set; }
        public int ActiveSkinColorSettings { get; set; }
        public int ActiveBackgroundSettings { get; set; }
        public decimal AverageSkinColorPoints { get; set; }
        public decimal AverageBackgroundPoints { get; set; }
        public int MaxSkinColorPoints { get; set; }
        public int MaxBackgroundPoints { get; set; }
        public int MinSkinColorPoints { get; set; }
        public int MinBackgroundPoints { get; set; }
    }
}
