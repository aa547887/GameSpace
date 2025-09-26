using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 錢包視圖模型集合
    /// </summary>
    public class WalletViewModels
    {
        /// <summary>
        /// 錢包首頁視圖模型
        /// </summary>
        public class WalletIndexViewModel
        {
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 錢包歷史記錄
            /// </summary>
            public List<WalletHistory> History { get; set; } = new();
            
            /// <summary>
            /// 錢包統計
            /// </summary>
            public WalletStatistics Statistics { get; set; } = new();
            
            /// <summary>
            /// 最近交易
            /// </summary>
            public List<WalletTransaction> RecentTransactions { get; set; } = new();
        }

        /// <summary>
        /// 錢包歷史視圖模型
        /// </summary>
        public class WalletHistoryViewModel
        {
            /// <summary>
            /// 錢包歷史記錄
            /// </summary>
            public List<WalletHistory> History { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public WalletHistoryQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<WalletHistory> PagedResults { get; set; } = new();
        }

        /// <summary>
        /// 錢包交易視圖模型
        /// </summary>
        public class WalletTransactionViewModel
        {
            /// <summary>
            /// 交易記錄
            /// </summary>
            public WalletHistory Transaction { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 交易詳情
            /// </summary>
            public TransactionDetails Details { get; set; } = new();
        }

        /// <summary>
        /// 錢包充值視圖模型
        /// </summary>
        public class WalletTopUpViewModel
        {
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 充值金額
            /// </summary>
            [Required(ErrorMessage = "請輸入充值金額")]
            [Range(1, 100000, ErrorMessage = "充值金額必須在1-100000之間")]
            public int Amount { get; set; }
            
            /// <summary>
            /// 充值方式
            /// </summary>
            [Required(ErrorMessage = "請選擇充值方式")]
            public string PaymentMethod { get; set; } = string.Empty;
            
            /// <summary>
            /// 充值說明
            /// </summary>
            [StringLength(200, ErrorMessage = "充值說明不能超過200字")]
            public string? Description { get; set; }
        }

        /// <summary>
        /// 錢包轉帳視圖模型
        /// </summary>
        public class WalletTransferViewModel
        {
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 接收者ID
            /// </summary>
            [Required(ErrorMessage = "請輸入接收者ID")]
            public int ReceiverId { get; set; }
            
            /// <summary>
            /// 轉帳金額
            /// </summary>
            [Required(ErrorMessage = "請輸入轉帳金額")]
            [Range(1, 100000, ErrorMessage = "轉帳金額必須在1-100000之間")]
            public int Amount { get; set; }
            
            /// <summary>
            /// 轉帳說明
            /// </summary>
            [StringLength(200, ErrorMessage = "轉帳說明不能超過200字")]
            public string? Description { get; set; }
            
            /// <summary>
            /// 接收者名稱
            /// </summary>
            public string ReceiverName { get; set; } = string.Empty;
        }
    }

    /// <summary>
    /// 管理員錢包管理視圖模型
    /// </summary>
    public class AdminWalletManagementViewModel
    {
        /// <summary>
        /// 用戶列表
        /// </summary>
        public List<User> Users { get; set; } = new();
        
        /// <summary>
        /// 查詢條件
        /// </summary>
        public WalletQueryModel Query { get; set; } = new();
        
        /// <summary>
        /// 統計數據
        /// </summary>
        public WalletStatisticsReadModel Statistics { get; set; } = new();
        
        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 頁碼
        /// </summary>
        public int PageNumber { get; set; }
        
        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// 分頁結果
        /// </summary>
        public PagedResult<UserWallet> PagedResults { get; set; } = new();
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
        public int PageSize { get; set; } = 10;
        
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? SearchTerm { get; set; }
        
        /// <summary>
        /// 最小餘額
        /// </summary>
        public int? MinBalance { get; set; }
        
        /// <summary>
        /// 最大餘額
        /// </summary>
        public int? MaxBalance { get; set; }
        
        /// <summary>
        /// 排序方式
        /// </summary>
        public string SortBy { get; set; } = "Balance";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// 錢包歷史查詢模型
    /// </summary>
    public class WalletHistoryQueryModel
    {
        /// <summary>
        /// 頁碼
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; } = 10;
        
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? SearchTerm { get; set; }
        
        /// <summary>
        /// 交易類型
        /// </summary>
        public string? TransactionType { get; set; }
        
        /// <summary>
        /// 開始日期
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// 結束日期
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// 最小金額
        /// </summary>
        public int? MinAmount { get; set; }
        
        /// <summary>
        /// 最大金額
        /// </summary>
        public int? MaxAmount { get; set; }
        
        /// <summary>
        /// 排序方式
        /// </summary>
        public string SortBy { get; set; } = "TransactionTime";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "desc";
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
        /// 總餘額
        /// </summary>
        public int TotalBalance { get; set; }
        
        /// <summary>
        /// 平均餘額
        /// </summary>
        public double AverageBalance { get; set; }
        
        /// <summary>
        /// 今日交易數
        /// </summary>
        public int TodayTransactions { get; set; }
        
        /// <summary>
        /// 今日交易金額
        /// </summary>
        public int TodayAmount { get; set; }
        
        /// <summary>
        /// 本月交易數
        /// </summary>
        public int MonthlyTransactions { get; set; }
        
        /// <summary>
        /// 本月交易金額
        /// </summary>
        public int MonthlyAmount { get; set; }
        
        /// <summary>
        /// 交易類型統計
        /// </summary>
        public List<TransactionTypeStatistics> TransactionTypeStats { get; set; } = new();
        
        /// <summary>
        /// 每日統計
        /// </summary>
        public List<DailyWalletStatistics> DailyStats { get; set; } = new();
    }

    /// <summary>
    /// 錢包統計
    /// </summary>
    public class WalletStatistics
    {
        /// <summary>
        /// 總餘額
        /// </summary>
        public int TotalBalance { get; set; }
        
        /// <summary>
        /// 總收入
        /// </summary>
        public int TotalIncome { get; set; }
        
        /// <summary>
        /// 總支出
        /// </summary>
        public int TotalExpense { get; set; }
        
        /// <summary>
        /// 淨收入
        /// </summary>
        public int NetIncome { get; set; }
        
        /// <summary>
        /// 交易次數
        /// </summary>
        public int TransactionCount { get; set; }
        
        /// <summary>
        /// 平均交易金額
        /// </summary>
        public double AverageTransactionAmount { get; set; }
        
        /// <summary>
        /// 最大交易金額
        /// </summary>
        public int MaxTransactionAmount { get; set; }
        
        /// <summary>
        /// 最小交易金額
        /// </summary>
        public int MinTransactionAmount { get; set; }
        
        /// <summary>
        /// 最後交易時間
        /// </summary>
        public DateTime? LastTransactionTime { get; set; }
        
        /// <summary>
        /// 淨收入顯示文字
        /// </summary>
        public string NetIncomeDisplay => NetIncome >= 0 ? $"+{NetIncome:N0}" : $"{NetIncome:N0}";
        
        /// <summary>
        /// 平均交易金額顯示文字
        /// </summary>
        public string AverageTransactionAmountDisplay => $"{AverageTransactionAmount:N0}";
    }

    /// <summary>
    /// 錢包交易
    /// </summary>
    public class WalletTransaction
    {
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TransactionId { get; set; }
        
        /// <summary>
        /// 交易類型
        /// </summary>
        public string TransactionType { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易金額
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// 交易時間
        /// </summary>
        public DateTime TransactionTime { get; set; }
        
        /// <summary>
        /// 交易說明
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// 交易狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易後餘額
        /// </summary>
        public int BalanceAfter { get; set; }
        
        /// <summary>
        /// 金額顯示文字
        /// </summary>
        public string AmountDisplay => Amount >= 0 ? $"+{Amount:N0}" : $"{Amount:N0}";
        
        /// <summary>
        /// 交易類型顯示文字
        /// </summary>
        public string TransactionTypeDisplay => TransactionType switch
        {
            "Income" => "收入",
            "Expense" => "支出",
            "Transfer" => "轉帳",
            "Refund" => "退款",
            "Bonus" => "獎勵",
            _ => TransactionType
        };
    }

    /// <summary>
    /// 交易詳情
    /// </summary>
    public class TransactionDetails
    {
        /// <summary>
        /// 交易ID
        /// </summary>
        public int TransactionId { get; set; }
        
        /// <summary>
        /// 交易類型
        /// </summary>
        public string TransactionType { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易金額
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// 交易時間
        /// </summary>
        public DateTime TransactionTime { get; set; }
        
        /// <summary>
        /// 交易說明
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// 交易狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易前餘額
        /// </summary>
        public int BalanceBefore { get; set; }
        
        /// <summary>
        /// 交易後餘額
        /// </summary>
        public int BalanceAfter { get; set; }
        
        /// <summary>
        /// 相關用戶ID
        /// </summary>
        public int? RelatedUserId { get; set; }
        
        /// <summary>
        /// 相關用戶名稱
        /// </summary>
        public string? RelatedUserName { get; set; }
        
        /// <summary>
        /// 交易來源
        /// </summary>
        public string? Source { get; set; }
        
        /// <summary>
        /// 交易備註
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 交易類型統計
    /// </summary>
    public class TransactionTypeStatistics
    {
        /// <summary>
        /// 交易類型
        /// </summary>
        public string TransactionType { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易次數
        /// </summary>
        public int Count { get; set; }
        
        /// <summary>
        /// 總金額
        /// </summary>
        public int TotalAmount { get; set; }
        
        /// <summary>
        /// 平均金額
        /// </summary>
        public double AverageAmount { get; set; }
        
        /// <summary>
        /// 百分比
        /// </summary>
        public double Percentage { get; set; }
        
        /// <summary>
        /// 交易類型顯示文字
        /// </summary>
        public string TransactionTypeDisplay => TransactionType switch
        {
            "Income" => "收入",
            "Expense" => "支出",
            "Transfer" => "轉帳",
            "Refund" => "退款",
            "Bonus" => "獎勵",
            _ => TransactionType
        };
    }

    /// <summary>
    /// 每日錢包統計
    /// </summary>
    public class DailyWalletStatistics
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// 交易次數
        /// </summary>
        public int TransactionCount { get; set; }
        
        /// <summary>
        /// 總金額
        /// </summary>
        public int TotalAmount { get; set; }
        
        /// <summary>
        /// 收入金額
        /// </summary>
        public int IncomeAmount { get; set; }
        
        /// <summary>
        /// 支出金額
        /// </summary>
        public int ExpenseAmount { get; set; }
        
        /// <summary>
        /// 淨收入
        /// </summary>
        public int NetIncome { get; set; }
        
        /// <summary>
        /// 日期顯示文字
        /// </summary>
        public string DateDisplay => Date.ToString("yyyy-MM-dd");
        
        /// <summary>
        /// 淨收入顯示文字
        /// </summary>
        public string NetIncomeDisplay => NetIncome >= 0 ? $"+{NetIncome:N0}" : $"{NetIncome:N0}";
    }

    /// <summary>
    /// 錢包充值記錄
    /// </summary>
    public class WalletTopUpRecord
    {
        /// <summary>
        /// 記錄ID
        /// </summary>
        public int RecordId { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 充值金額
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// 充值方式
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;
        
        /// <summary>
        /// 充值時間
        /// </summary>
        public DateTime TopUpTime { get; set; }
        
        /// <summary>
        /// 充值狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// 充值說明
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// 交易ID
        /// </summary>
        public string? TransactionId { get; set; }
    }

    /// <summary>
    /// 錢包轉帳記錄
    /// </summary>
    public class WalletTransferRecord
    {
        /// <summary>
        /// 記錄ID
        /// </summary>
        public int RecordId { get; set; }
        
        /// <summary>
        /// 發送者ID
        /// </summary>
        public int SenderId { get; set; }
        
        /// <summary>
        /// 接收者ID
        /// </summary>
        public int ReceiverId { get; set; }
        
        /// <summary>
        /// 轉帳金額
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// 轉帳時間
        /// </summary>
        public DateTime TransferTime { get; set; }
        
        /// <summary>
        /// 轉帳狀態
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// 轉帳說明
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// 發送者名稱
        /// </summary>
        public string SenderName { get; set; } = string.Empty;
        
        /// <summary>
        /// 接收者名稱
        /// </summary>
        public string ReceiverName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 錢包餘額警告
    /// </summary>
    public class WalletBalanceWarning
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
        /// 當前餘額
        /// </summary>
        public int CurrentBalance { get; set; }
        
        /// <summary>
        /// 警告閾值
        /// </summary>
        public int WarningThreshold { get; set; }
        
        /// <summary>
        /// 警告類型
        /// </summary>
        public string WarningType { get; set; } = string.Empty;
        
        /// <summary>
        /// 警告時間
        /// </summary>
        public DateTime WarningTime { get; set; }
        
        /// <summary>
        /// 是否已處理
        /// </summary>
        public bool IsProcessed { get; set; }
    }

    /// <summary>
    /// 錢包異常交易
    /// </summary>
    public class WalletAbnormalTransaction
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
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易類型
        /// </summary>
        public string TransactionType { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易金額
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// 交易時間
        /// </summary>
        public DateTime TransactionTime { get; set; }
        
        /// <summary>
        /// 異常類型
        /// </summary>
        public string AbnormalType { get; set; } = string.Empty;
        
        /// <summary>
        /// 異常描述
        /// </summary>
        public string AbnormalDescription { get; set; } = string.Empty;
        
        /// <summary>
        /// 風險等級
        /// </summary>
        public string RiskLevel { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否已處理
        /// </summary>
        public bool IsProcessed { get; set; }
    }
}
