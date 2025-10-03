using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 權限日誌查詢模型
    /// </summary>
    public class PermissionLogQueryModel
    {
        /// <summary>
        /// 操作類型 (Add, Update, Delete, Grant, Revoke)
        /// </summary>
        [StringLength(20)]
        public string? OperationType { get; set; }

        /// <summary>
        /// 目標用戶ID
        /// </summary>
        public int? TargetUserId { get; set; }

        /// <summary>
        /// 權限名稱
        /// </summary>
        [StringLength(100)]
        public string? RightName { get; set; }

        /// <summary>
        /// 權限類型
        /// </summary>
        [StringLength(50)]
        public string? RightType { get; set; }

        /// <summary>
        /// 操作者管理員ID
        /// </summary>
        public int? OperatorManagerId { get; set; }

        /// <summary>
        /// 開始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 結束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 是否僅顯示失敗操作
        /// </summary>
        public bool? OnlyFailures { get; set; }

        /// <summary>
        /// 搜尋關鍵字（用於描述、用戶名等）
        /// </summary>
        [StringLength(200)]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// 排序欄位
        /// </summary>
        [StringLength(50)]
        public string SortBy { get; set; } = "OperationTime";

        /// <summary>
        /// 是否降序排列
        /// </summary>
        public bool Descending { get; set; } = true;

        /// <summary>
        /// 頁碼
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }
}
