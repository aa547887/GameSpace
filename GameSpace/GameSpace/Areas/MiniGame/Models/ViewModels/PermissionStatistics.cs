namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 權限統計資訊
    /// </summary>
    public class PermissionStatistics
    {
        /// <summary>
        /// 總權限數
        /// </summary>
        public int TotalPermissions { get; set; }

        /// <summary>
        /// 啟用中的權限數
        /// </summary>
        public int ActivePermissions { get; set; }

        /// <summary>
        /// 已過期的權限數
        /// </summary>
        public int ExpiredPermissions { get; set; }

        /// <summary>
        /// 已停用的權限數
        /// </summary>
        public int InactivePermissions { get; set; }

        /// <summary>
        /// 擁有權限的用戶總數
        /// </summary>
        public int TotalUsersWithRights { get; set; }

        /// <summary>
        /// 權限類型總數
        /// </summary>
        public int TotalRightTypes { get; set; }

        /// <summary>
        /// 今日新增的權限數
        /// </summary>
        public int TodayNewPermissions { get; set; }

        /// <summary>
        /// 本週新增的權限數
        /// </summary>
        public int ThisWeekNewPermissions { get; set; }

        /// <summary>
        /// 本月新增的權限數
        /// </summary>
        public int ThisMonthNewPermissions { get; set; }

        /// <summary>
        /// 即將過期的權限數（30天內）
        /// </summary>
        public int ExpiringPermissions { get; set; }

        /// <summary>
        /// 權限類型分佈統計
        /// </summary>
        public Dictionary<string, int> RightTypeDistribution { get; set; } = new();

        /// <summary>
        /// 權限等級分佈統計
        /// </summary>
        public Dictionary<int, int> RightLevelDistribution { get; set; } = new();

        /// <summary>
        /// 每月權限變化趨勢
        /// </summary>
        public Dictionary<string, int> MonthlyTrend { get; set; } = new();

        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
