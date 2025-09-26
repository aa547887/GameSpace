using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 用戶權限模型
    /// </summary>
    [Table("UserRight")]
    public class UserRight
    {
        /// <summary>
        /// 用戶權限ID
        /// </summary>
        [Key]
        public int UserRightId { get; set; }

        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required(ErrorMessage = "用戶ID為必填欄位")]
        public int UserId { get; set; }

        /// <summary>
        /// 權限名稱
        /// </summary>
        [Required(ErrorMessage = "權限名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "權限名稱不能超過100字")]
        public string RightName { get; set; } = string.Empty;

        /// <summary>
        /// 權限描述
        /// </summary>
        [StringLength(200, ErrorMessage = "權限描述不能超過200字")]
        public string? Description { get; set; }

        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 過期時間
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 權限類型
        /// </summary>
        [StringLength(50, ErrorMessage = "權限類型不能超過50字")]
        public string RightType { get; set; } = "General";

        /// <summary>
        /// 權限等級
        /// </summary>
        public int RightLevel { get; set; } = 1;

        /// <summary>
        /// 權限範圍
        /// </summary>
        [StringLength(100, ErrorMessage = "權限範圍不能超過100字")]
        public string? RightScope { get; set; }

        /// <summary>
        /// 權限參數
        /// </summary>
        [StringLength(500, ErrorMessage = "權限參數不能超過500字")]
        public string? RightParameters { get; set; }

        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新者ID
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// 是否過期
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

        /// <summary>
        /// 權限狀態
        /// </summary>
        public string Status => IsActive ? (IsExpired ? "已過期" : "啟用中") : "已停用";

        /// <summary>
        /// 權限狀態顏色類別
        /// </summary>
        public string StatusColorClass => Status switch
        {
            "啟用中" => "text-success",
            "已過期" => "text-warning",
            "已停用" => "text-danger",
            _ => "text-muted"
        };

        /// <summary>
        /// 創建時間顯示文字
        /// </summary>
        public string CreatedAtDisplay => CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// 過期時間顯示文字
        /// </summary>
        public string ExpiresAtDisplay => ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "永不過期";

        /// <summary>
        /// 權限類型顯示文字
        /// </summary>
        public string RightTypeDisplay => RightType switch
        {
            "General" => "一般權限",
            "Admin" => "管理員權限",
            "System" => "系統權限",
            "Special" => "特殊權限",
            _ => RightType
        };

        /// <summary>
        /// 權限等級顯示文字
        /// </summary>
        public string RightLevelDisplay => $"等級 {RightLevel}";

        // 導航屬性
        /// <summary>
        /// 關聯的用戶
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// 關聯的更新者
        /// </summary>
        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedByUser { get; set; }
    }

    /// <summary>
    /// 用戶權限查詢模型
    /// </summary>
    public class UserRightQueryModel
    {
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string SearchTerm { get; set; } = string.Empty;

        /// <summary>
        /// 頁碼
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 權限類型
        /// </summary>
        public string? RightType { get; set; }

        /// <summary>
        /// 權限狀態
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 開始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 結束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 排序方式
        /// </summary>
        public string SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// 用戶權限統計模型
    /// </summary>
    public class UserRightStatistics
    {
        /// <summary>
        /// 總權限數
        /// </summary>
        public int TotalRights { get; set; }

        /// <summary>
        /// 啟用權限數
        /// </summary>
        public int ActiveRights { get; set; }

        /// <summary>
        /// 過期權限數
        /// </summary>
        public int ExpiredRights { get; set; }

        /// <summary>
        /// 停用權限數
        /// </summary>
        public int InactiveRights { get; set; }

        /// <summary>
        /// 各類型權限統計
        /// </summary>
        public Dictionary<string, int> RightsByType { get; set; } = new();

        /// <summary>
        /// 各等級權限統計
        /// </summary>
        public Dictionary<int, int> RightsByLevel { get; set; } = new();

        /// <summary>
        /// 啟用權限比例
        /// </summary>
        public double ActiveRatio => TotalRights > 0 ? (double)ActiveRights / TotalRights * 100 : 0;

        /// <summary>
        /// 啟用權限比例顯示文字
        /// </summary>
        public string ActiveRatioDisplay => $"{ActiveRatio:F1}%";
    }
}
