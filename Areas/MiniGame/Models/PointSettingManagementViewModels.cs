using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 點數設定管理主頁面 ViewModel
    /// </summary>
    public class PointSettingManagementIndexViewModel
    {
        public List<PetSkinColorPointSetting> SkinColorSettings { get; set; } = new();
        public List<PetBackgroundPointSetting> BackgroundSettings { get; set; } = new();
        public int TotalSkinColorSettings { get; set; }
        public int TotalBackgroundSettings { get; set; }
        public string SearchTerm { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// 點數設定統計 ViewModel
    /// </summary>
    public class PointSettingStatisticsViewModel
    {
        public int TotalSkinColorSettings { get; set; }
        public int TotalBackgroundSettings { get; set; }
        public int ActiveSkinColorSettings { get; set; }
        public int ActiveBackgroundSettings { get; set; }
        public int InactiveSkinColorSettings { get; set; }
        public int InactiveBackgroundSettings { get; set; }
        public decimal AverageSkinColorPoints { get; set; }
        public decimal AverageBackgroundPoints { get; set; }
        public int MaxSkinColorPoints { get; set; }
        public int MaxBackgroundPoints { get; set; }
        public int MinSkinColorPoints { get; set; }
        public int MinBackgroundPoints { get; set; }
    }

    /// <summary>
    /// 點數設定匯出 ViewModel
    /// </summary>
    public class PointSettingExportViewModel
    {
        public string ExportType { get; set; } = string.Empty; // "SkinColor", "Background", "All"
        public string FileFormat { get; set; } = "Excel"; // "Excel", "CSV", "JSON"
        public bool IncludeInactive { get; set; } = false;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// 點數設定批量操作 ViewModel
    /// </summary>
    public class PointSettingBatchOperationViewModel
    {
        public string OperationType { get; set; } = string.Empty; // "Enable", "Disable", "Delete", "UpdatePoints"
        public List<int> SelectedIds { get; set; } = new();
        public int? NewPointsValue { get; set; }
        public string ConfirmationMessage { get; set; } = string.Empty;
    }
}
