namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 權限操作日誌
    /// </summary>
    public class PermissionOperationLog
    {
        /// <summary>
        /// 日誌ID
        /// </summary>
        public int LogId { get; set; }

        /// <summary>
        /// 操作類型 (Add, Update, Delete, Grant, Revoke)
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// 操作描述
        /// </summary>
        public string OperationDescription { get; set; } = string.Empty;

        /// <summary>
        /// 目標用戶ID
        /// </summary>
        public int? TargetUserId { get; set; }

        /// <summary>
        /// 目標用戶名稱
        /// </summary>
        public string? TargetUserName { get; set; }

        /// <summary>
        /// 權限名稱
        /// </summary>
        public string? RightName { get; set; }

        /// <summary>
        /// 權限類型
        /// </summary>
        public string? RightType { get; set; }

        /// <summary>
        /// 操作者管理員ID
        /// </summary>
        public int OperatorManagerId { get; set; }

        /// <summary>
        /// 操作者管理員名稱
        /// </summary>
        public string OperatorManagerName { get; set; } = string.Empty;

        /// <summary>
        /// 操作時間
        /// </summary>
        public DateTime OperationTime { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 操作前的值（JSON）
        /// </summary>
        public string? BeforeValue { get; set; }

        /// <summary>
        /// 操作後的值（JSON）
        /// </summary>
        public string? AfterValue { get; set; }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息（如有）
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string? Notes { get; set; }

        // Property aliases for compatibility
        public int ManagerId { get => OperatorManagerId; set => OperatorManagerId = value; }
        public string ManagerName { get => OperatorManagerName; set => OperatorManagerName = value; }
        public string Operation { get => OperationType; set => OperationType = value; }
        public string Details { get => OperationDescription; set => OperationDescription = value; }
    }
}
