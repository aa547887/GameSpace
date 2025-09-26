using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 用戶令牌模型
    /// </summary>
    [Table("UserToken")]
    public class UserToken
    {
        /// <summary>
        /// 用戶令牌ID
        /// </summary>
        [Key]
        public int UserTokenId { get; set; }

        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required(ErrorMessage = "用戶ID為必填欄位")]
        public int UserId { get; set; }

        /// <summary>
        /// 令牌
        /// </summary>
        [Required(ErrorMessage = "令牌為必填欄位")]
        [StringLength(500, ErrorMessage = "令牌不能超過500字")]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// 令牌類型
        /// </summary>
        [Required(ErrorMessage = "令牌類型為必填欄位")]
        [StringLength(50, ErrorMessage = "令牌類型不能超過50字")]
        public string TokenType { get; set; } = string.Empty; // "Access", "Refresh", "ResetPassword", etc.

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
        /// 描述
        /// </summary>
        [StringLength(200, ErrorMessage = "描述不能超過200字")]
        public string? Description { get; set; }

        /// <summary>
        /// 令牌用途
        /// </summary>
        [StringLength(100, ErrorMessage = "令牌用途不能超過100字")]
        public string? Purpose { get; set; }

        /// <summary>
        /// 令牌來源
        /// </summary>
        [StringLength(100, ErrorMessage = "令牌來源不能超過100字")]
        public string? Source { get; set; }

        /// <summary>
        /// 最後使用時間
        /// </summary>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// 使用次數
        /// </summary>
        public int UsageCount { get; set; } = 0;

        /// <summary>
        /// 最大使用次數
        /// </summary>
        public int? MaxUsageCount { get; set; }

        /// <summary>
        /// 是否單次使用
        /// </summary>
        public bool IsSingleUse { get; set; } = false;

        /// <summary>
        /// 是否已使用
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// 令牌狀態
        /// </summary>
        public string TokenStatus { get; set; } = "Active";

        /// <summary>
        /// 創建者ID
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// 是否過期
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

        /// <summary>
        /// 是否達到最大使用次數
        /// </summary>
        public bool IsMaxUsageReached => MaxUsageCount.HasValue && UsageCount >= MaxUsageCount.Value;

        /// <summary>
        /// 令牌狀態
        /// </summary>
        public string Status => IsActive ? (IsExpired ? "已過期" : (IsUsed ? "已使用" : (IsMaxUsageReached ? "已達上限" : "啟用中"))) : "已停用";

        /// <summary>
        /// 令牌狀態顏色類別
        /// </summary>
        public string StatusColorClass => Status switch
        {
            "啟用中" => "text-success",
            "已過期" => "text-warning",
            "已使用" => "text-info",
            "已達上限" => "text-warning",
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
        /// 最後使用時間顯示文字
        /// </summary>
        public string LastUsedAtDisplay => LastUsedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "未使用";

        /// <summary>
        /// 令牌類型顯示文字
        /// </summary>
        public string TokenTypeDisplay => TokenType switch
        {
            "Access" => "存取令牌",
            "Refresh" => "刷新令牌",
            "ResetPassword" => "重設密碼令牌",
            "EmailVerification" => "電子郵件驗證令牌",
            "TwoFactor" => "雙因素驗證令牌",
            "ApiKey" => "API金鑰",
            "Session" => "會話令牌",
            _ => TokenType
        };

        /// <summary>
        /// 令牌用途顯示文字
        /// </summary>
        public string PurposeDisplay => Purpose ?? "未設定";

        /// <summary>
        /// 令牌來源顯示文字
        /// </summary>
        public string SourceDisplay => Source ?? "未設定";

        /// <summary>
        /// 使用次數顯示文字
        /// </summary>
        public string UsageCountDisplay => $"{UsageCount} 次";

        /// <summary>
        /// 最大使用次數顯示文字
        /// </summary>
        public string MaxUsageCountDisplay => MaxUsageCount?.ToString() ?? "無限制";

        /// <summary>
        /// 令牌長度顯示文字
        /// </summary>
        public string TokenLengthDisplay => $"{Token.Length} 字元";

        /// <summary>
        /// 令牌前綴顯示文字
        /// </summary>
        public string TokenPrefixDisplay => Token.Length > 8 ? Token.Substring(0, 8) + "..." : Token;

        /// <summary>
        /// 是否可重複使用
        /// </summary>
        public bool CanReuse => !IsSingleUse && !IsMaxUsageReached && !IsExpired && IsActive;

        /// <summary>
        /// 剩餘使用次數
        /// </summary>
        public int? RemainingUsageCount => MaxUsageCount.HasValue ? MaxUsageCount.Value - UsageCount : null;

        /// <summary>
        /// 剩餘使用次數顯示文字
        /// </summary>
        public string RemainingUsageCountDisplay => RemainingUsageCount?.ToString() ?? "無限制";

        /// <summary>
        /// 令牌生命週期（天）
        /// </summary>
        public int? TokenLifetimeDays => ExpiresAt.HasValue ? (int)(ExpiresAt.Value - CreatedAt).TotalDays : null;

        /// <summary>
        /// 令牌生命週期顯示文字
        /// </summary>
        public string TokenLifetimeDaysDisplay => TokenLifetimeDays?.ToString() ?? "永不過期";

        // 導航屬性
        /// <summary>
        /// 關聯的用戶
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// 關聯的創建者
        /// </summary>
        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        /// <summary>
        /// 關聯的更新者
        /// </summary>
        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedByUser { get; set; }
    }

    /// <summary>
    /// 用戶令牌查詢模型
    /// </summary>
    public class UserTokenQueryModel
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
        /// 令牌類型
        /// </summary>
        public string? TokenType { get; set; }

        /// <summary>
        /// 令牌狀態
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 是否過期
        /// </summary>
        public bool? IsExpired { get; set; }

        /// <summary>
        /// 是否已使用
        /// </summary>
        public bool? IsUsed { get; set; }

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
    /// 用戶令牌統計模型
    /// </summary>
    public class UserTokenStatistics
    {
        /// <summary>
        /// 總令牌數
        /// </summary>
        public int TotalTokens { get; set; }

        /// <summary>
        /// 啟用令牌數
        /// </summary>
        public int ActiveTokens { get; set; }

        /// <summary>
        /// 過期令牌數
        /// </summary>
        public int ExpiredTokens { get; set; }

        /// <summary>
        /// 已使用令牌數
        /// </summary>
        public int UsedTokens { get; set; }

        /// <summary>
        /// 停用令牌數
        /// </summary>
        public int InactiveTokens { get; set; }

        /// <summary>
        /// 各類型令牌統計
        /// </summary>
        public Dictionary<string, int> TokensByType { get; set; } = new();

        /// <summary>
        /// 各狀態令牌統計
        /// </summary>
        public Dictionary<string, int> TokensByStatus { get; set; } = new();

        /// <summary>
        /// 啟用令牌比例
        /// </summary>
        public double ActiveRatio => TotalTokens > 0 ? (double)ActiveTokens / TotalTokens * 100 : 0;

        /// <summary>
        /// 啟用令牌比例顯示文字
        /// </summary>
        public string ActiveRatioDisplay => $"{ActiveRatio:F1}%";

        /// <summary>
        /// 平均使用次數
        /// </summary>
        public double AverageUsageCount { get; set; }

        /// <summary>
        /// 平均使用次數顯示文字
        /// </summary>
        public string AverageUsageCountDisplay => $"{AverageUsageCount:F1} 次";
    }
}
