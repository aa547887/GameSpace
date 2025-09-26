using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 錢包讀取模型
    /// </summary>
    public class WalletReadModel
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// 用戶點數
        /// </summary>
        public int UserPoint { get; set; }
        
        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// 點數餘額顯示文字
        /// </summary>
        public string PointBalanceDisplay => $"{UserPoint:N0} 點";
        
        /// <summary>
        /// 最後更新時間顯示文字
        /// </summary>
        public string LastUpdatedDisplay => LastUpdated.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 錢包摘要讀取模型
    /// </summary>
    public class WalletSummaryReadModel
    {
        /// <summary>
        /// 總用戶數
        /// </summary>
        public int TotalUsers { get; set; }
        
        /// <summary>
        /// 總點數
        /// </summary>
        public int TotalPoints { get; set; }
        
        /// <summary>
        /// 平均點數
        /// </summary>
        public int AveragePoints { get; set; }
        
        /// <summary>
        /// 最高點數
        /// </summary>
        public int MaxPoints { get; set; }
        
        /// <summary>
        /// 最低點數
        /// </summary>
        public int MinPoints { get; set; }
        
        /// <summary>
        /// 活躍用戶數
        /// </summary>
        public int ActiveUsers { get; set; }
        
        /// <summary>
        /// 總點數顯示文字
        /// </summary>
        public string TotalPointsDisplay => $"{TotalPoints:N0} 點";
        
        /// <summary>
        /// 平均點數顯示文字
        /// </summary>
        public string AveragePointsDisplay => $"{AveragePoints:N0} 點";
        
        /// <summary>
        /// 活躍用戶比例
        /// </summary>
        public double ActiveUserRatio => TotalUsers > 0 ? (double)ActiveUsers / TotalUsers * 100 : 0;
        
        /// <summary>
        /// 活躍用戶比例顯示文字
        /// </summary>
        public string ActiveUserRatioDisplay => $"{ActiveUserRatio:F1}%";
    }

    /// <summary>
    /// 錢包查詢模型
    /// </summary>
    public class WalletQueryModel
    {
        /// <summary>
        /// 頁碼
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageNumberSize { get; set; } = 10;
        
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? SearchTerm { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// 最小點數
        /// </summary>
        public int? MinPoints { get; set; }
        
        /// <summary>
        /// 最大點數
        /// </summary>
        public int? MaxPoints { get; set; }
        
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
        public string SortBy { get; set; } = "UserPoint";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// 用戶錢包讀取模型
    /// </summary>
    public class UserWalletReadModel
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// 用戶點數
        /// </summary>
        public int UserPoint { get; set; }
        
        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// 用戶等級
        /// </summary>
        public int UserLevel { get; set; }
        
        /// <summary>
        /// 用戶狀態
        /// </summary>
        public string UserStatus { get; set; } = "Active";
        
        /// <summary>
        /// 註冊時間
        /// </summary>
        public DateTime RegistrationDate { get; set; }
        
        /// <summary>
        /// 最後登入時間
        /// </summary>
        public DateTime? LastLoginDate { get; set; }
        
        /// <summary>
        /// 點數餘額顯示文字
        /// </summary>
        public string PointBalanceDisplay => $"{UserPoint:N0} 點";
        
        /// <summary>
        /// 最後更新時間顯示文字
        /// </summary>
        public string LastUpdatedDisplay => LastUpdated.ToString("yyyy-MM-dd HH:mm:ss");
        
        /// <summary>
        /// 用戶狀態顯示文字
        /// </summary>
        public string UserStatusDisplay => UserStatus switch
        {
            "Active" => "活躍",
            "Inactive" => "非活躍",
            "Suspended" => "已停權",
            "Banned" => "已封鎖",
            _ => "未知"
        };
    }

    /// <summary>
    /// 錢包詳情讀取模型
    /// </summary>
    public class WalletDetailReadModel
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// 用戶點數
        /// </summary>
        public int UserPoint { get; set; }
        
        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// 交易記錄列表
        /// </summary>
        public List<WalletTransactionReadModel> Transactions { get; set; } = new();
        
        /// <summary>
        /// 總交易次數
        /// </summary>
        public int TotalTransactions { get; set; }
        
        /// <summary>
        /// 總收入點數
        /// </summary>
        public int TotalIncome { get; set; }
        
        /// <summary>
        /// 總支出點數
        /// </summary>
        public int TotalExpense { get; set; }
        
        /// <summary>
        /// 淨收入點數
        /// </summary>
        public int NetIncome => TotalIncome - TotalExpense;
        
        /// <summary>
        /// 點數餘額顯示文字
        /// </summary>
        public string PointBalanceDisplay => $"{UserPoint:N0} 點";
        
        /// <summary>
        /// 最後更新時間顯示文字
        /// </summary>
        public string LastUpdatedDisplay => LastUpdated.ToString("yyyy-MM-dd HH:mm:ss");
        
        /// <summary>
        /// 總收入顯示文字
        /// </summary>
        public string TotalIncomeDisplay => $"{TotalIncome:N0} 點";
        
        /// <summary>
        /// 總支出顯示文字
        /// </summary>
        public string TotalExpenseDisplay => $"{TotalExpense:N0} 點";
        
        /// <summary>
        /// 淨收入顯示文字
        /// </summary>
        public string NetIncomeDisplay => $"{NetIncome:N0} 點";
    }

    /// <summary>
    /// 錢包交易讀取模型
    /// </summary>
    public class WalletTransactionReadModel
    {
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TransactionId { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 交易金額
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// 交易類型
        /// </summary>
        public string TransactionType { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易日期
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// 交易狀態
        /// </summary>
        public string TransactionStatus { get; set; } = "Completed";
        
        /// <summary>
        /// 相關訂單ID
        /// </summary>
        public int? RelatedOrderId { get; set; }
        
        /// <summary>
        /// 相關優惠券ID
        /// </summary>
        public int? RelatedCouponId { get; set; }
        
        /// <summary>
        /// 相關電子禮券ID
        /// </summary>
        public int? RelatedEVoucherId { get; set; }
        
        /// <summary>
        /// 交易金額顯示文字
        /// </summary>
        public string AmountDisplay => Amount > 0 ? $"+{Amount:N0} 點" : $"{Amount:N0} 點";
        
        /// <summary>
        /// 交易日期顯示文字
        /// </summary>
        public string TransactionDateDisplay => TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
        
        /// <summary>
        /// 交易類型顯示文字
        /// </summary>
        public string TransactionTypeDisplay => TransactionType switch
        {
            "Deposit" => "儲值",
            "Withdraw" => "提領",
            "Purchase" => "購買",
            "Refund" => "退款",
            "Bonus" => "獎勵",
            "Penalty" => "罰款",
            "Transfer" => "轉帳",
            "Exchange" => "兌換",
            _ => TransactionType
        };
        
        /// <summary>
        /// 交易狀態顯示文字
        /// </summary>
        public string TransactionStatusDisplay => TransactionStatus switch
        {
            "Completed" => "已完成",
            "Pending" => "處理中",
            "Failed" => "失敗",
            "Cancelled" => "已取消",
            _ => TransactionStatus
        };
        
        /// <summary>
        /// 交易金額顏色類別
        /// </summary>
        public string AmountColorClass => Amount > 0 ? "text-success" : "text-danger";
        
        /// <summary>
        /// 交易狀態顏色類別
        /// </summary>
        public string StatusColorClass => TransactionStatus switch
        {
            "Completed" => "text-success",
            "Pending" => "text-warning",
            "Failed" => "text-danger",
            "Cancelled" => "text-secondary",
            _ => "text-muted"
        };
    }

    /// <summary>
    /// 錢包統計讀取模型
    /// </summary>
    public class WalletStatisticsReadModel
    {
        /// <summary>
        /// 總用戶數
        /// </summary>
        public int TotalUsers { get; set; }
        
        /// <summary>
        /// 活躍用戶數
        /// </summary>
        public int ActiveUsers { get; set; }
        
        /// <summary>
        /// 總點數
        /// </summary>
        public int TotalPoints { get; set; }
        
        /// <summary>
        /// 平均點數
        /// </summary>
        public int AveragePoints { get; set; }
        
        /// <summary>
        /// 最高點數
        /// </summary>
        public int MaxPoints { get; set; }
        
        /// <summary>
        /// 最低點數
        /// </summary>
        public int MinPoints { get; set; }
        
        /// <summary>
        /// 今日新增點數
        /// </summary>
        public int TodayAddedPoints { get; set; }
        
        /// <summary>
        /// 今日消耗點數
        /// </summary>
        public int TodayConsumedPoints { get; set; }
        
        /// <summary>
        /// 本月新增點數
        /// </summary>
        public int ThisMonthAddedPoints { get; set; }
        
        /// <summary>
        /// 本月消耗點數
        /// </summary>
        public int ThisMonthConsumedPoints { get; set; }
        
        /// <summary>
        /// 總交易次數
        /// </summary>
        public int TotalTransactions { get; set; }
        
        /// <summary>
        /// 今日交易次數
        /// </summary>
        public int TodayTransactions { get; set; }
        
        /// <summary>
        /// 本月交易次數
        /// </summary>
        public int ThisMonthTransactions { get; set; }
        
        /// <summary>
        /// 總點數顯示文字
        /// </summary>
        public string TotalPointsDisplay => $"{TotalPoints:N0} 點";
        
        /// <summary>
        /// 平均點數顯示文字
        /// </summary>
        public string AveragePointsDisplay => $"{AveragePoints:N0} 點";
        
        /// <summary>
        /// 活躍用戶比例
        /// </summary>
        public double ActiveUserRatio => TotalUsers > 0 ? (double)ActiveUsers / TotalUsers * 100 : 0;
        
        /// <summary>
        /// 活躍用戶比例顯示文字
        /// </summary>
        public string ActiveUserRatioDisplay => $"{ActiveUserRatio:F1}%";
        
        /// <summary>
        /// 今日淨點數
        /// </summary>
        public int TodayNetPoints => TodayAddedPoints - TodayConsumedPoints;
        
        /// <summary>
        /// 今日淨點數顯示文字
        /// </summary>
        public string TodayNetPointsDisplay => $"{TodayNetPoints:N0} 點";
        
        /// <summary>
        /// 本月淨點數
        /// </summary>
        public int ThisMonthNetPoints => ThisMonthAddedPoints - ThisMonthConsumedPoints;
        
        /// <summary>
        /// 本月淨點數顯示文字
        /// </summary>
        public string ThisMonthNetPointsDisplay => $"{ThisMonthNetPoints:N0} 點";
    }
}
