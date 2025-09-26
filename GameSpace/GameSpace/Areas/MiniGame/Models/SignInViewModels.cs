using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 簽到視圖模型集合
    /// </summary>
    public class SignInViewModels
    {
        /// <summary>
        /// 簽到首頁視圖模型
        /// </summary>
        public class SignInIndexViewModel
        {
            /// <summary>
            /// 今日是否已簽到
            /// </summary>
            public bool HasSignedInToday { get; set; }
            
            /// <summary>
            /// 連續簽到天數
            /// </summary>
            public int ConsecutiveDays { get; set; }
            
            /// <summary>
            /// 本月簽到次數
            /// </summary>
            public int MonthSignInCount { get; set; }
            
            /// <summary>
            /// 今日簽到記錄
            /// </summary>
            public UserSignInStat? TodaySignIn { get; set; }
            
            /// <summary>
            /// 簽到獎勵列表
            /// </summary>
            public List<SignInReward> SignInRewards { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 簽到統計
            /// </summary>
            public SignInStatistics Statistics { get; set; } = new();
            
            /// <summary>
            /// 月曆數據
            /// </summary>
            public List<CalendarDay> CalendarData { get; set; } = new();
        }

        /// <summary>
        /// 簽到歷史視圖模型
        /// </summary>
        public class SignInHistoryViewModel
        {
            /// <summary>
            /// 簽到記錄列表
            /// </summary>
            public List<UserSignInStat> SignInRecords { get; set; } = new();
            
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
            public SignInHistoryQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<UserSignInStat> PagedResults { get; set; } = new();
            
            /// <summary>
            /// 簽到統計
            /// </summary>
            public SignInStatistics Statistics { get; set; } = new();
        }

        /// <summary>
        /// 簽到獎勵視圖模型
        /// </summary>
        public class SignInRewardViewModel
        {
            /// <summary>
            /// 簽到獎勵列表
            /// </summary>
            public List<SignInReward> Rewards { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 當前連續簽到天數
            /// </summary>
            public int CurrentConsecutiveDays { get; set; }
            
            /// <summary>
            /// 下次獎勵天數
            /// </summary>
            public int NextRewardDays { get; set; }
            
            /// <summary>
            /// 獎勵統計
            /// </summary>
            public SignInRewardStatistics RewardStatistics { get; set; } = new();
        }
    }

    /// <summary>
    /// 管理員簽到管理視圖模型
    /// </summary>
    public class AdminSignInManagementViewModel
    {
        /// <summary>
        /// 簽到記錄列表
        /// </summary>
        public List<UserSignInStat> SignInRecords { get; set; } = new();
        
        /// <summary>
        /// 用戶列表
        /// </summary>
        public List<User> Users { get; set; } = new();
        
        /// <summary>
        /// 查詢條件
        /// </summary>
        public SignInQueryModel Query { get; set; } = new();
        
        /// <summary>
        /// 統計數據
        /// </summary>
        public SignInStatisticsReadModel Statistics { get; set; } = new();
        
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
        public PagedResult<UserSignInStat> PagedResults { get; set; } = new();
    }

    /// <summary>
    /// 簽到規則管理視圖模型
    /// </summary>
    public class SignInRuleManagementViewModel
    {
        /// <summary>
        /// 當前規則
        /// </summary>
        public SignInRuleReadModel CurrentRule { get; set; } = new();
        
        /// <summary>
        /// 更新模型
        /// </summary>
        public SignInRulesUpdateModel UpdateModel { get; set; } = new();
        
        /// <summary>
        /// 規則歷史
        /// </summary>
        public List<SignInRule> RuleHistory { get; set; } = new();
        
        /// <summary>
        /// 規則統計
        /// </summary>
        public SignInRuleStatistics RuleStatistics { get; set; } = new();
    }

    /// <summary>
    /// 簽到查詢模型
    /// </summary>
    public class SignInQueryModel
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
        /// 用戶ID
        /// </summary>
        public int? UserId { get; set; }
        
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
        public string SortBy { get; set; } = "SignInDate";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// 簽到歷史查詢模型
    /// </summary>
    public class SignInHistoryQueryModel
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
        public string SortBy { get; set; } = "SignInDate";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// 簽到統計讀取模型
    /// </summary>
    public class SignInStatisticsReadModel
    {
        /// <summary>
        /// 總簽到次數
        /// </summary>
        public int TotalSignIns { get; set; }
        
        /// <summary>
        /// 今日簽到次數
        /// </summary>
        public int TodaySignIns { get; set; }
        
        /// <summary>
        /// 本月簽到次數
        /// </summary>
        public int MonthlySignIns { get; set; }
        
        /// <summary>
        /// 平均每日簽到率
        /// </summary>
        public double AverageDailySignInRate { get; set; }
        
        /// <summary>
        /// 連續簽到統計
        /// </summary>
        public List<ConsecutiveSignInStatistics> ConsecutiveStats { get; set; } = new();
        
        /// <summary>
        /// 每日簽到統計
        /// </summary>
        public List<DailySignInStatistics> DailyStats { get; set; } = new();
    }

    /// <summary>
    /// 簽到統計
    /// </summary>
    public class SignInStatistics
    {
        /// <summary>
        /// 總簽到次數
        /// </summary>
        public int TotalSignIns { get; set; }
        
        /// <summary>
        /// 連續簽到天數
        /// </summary>
        public int ConsecutiveDays { get; set; }
        
        /// <summary>
        /// 本月簽到次數
        /// </summary>
        public int MonthlySignIns { get; set; }
        
        /// <summary>
        /// 平均每日簽到率
        /// </summary>
        public double AverageDailySignInRate { get; set; }
        
        /// <summary>
        /// 總獲得點數
        /// </summary>
        public int TotalPointsEarned { get; set; }
        
        /// <summary>
        /// 總獲得經驗
        /// </summary>
        public int TotalExperienceEarned { get; set; }
        
        /// <summary>
        /// 平均每日簽到率顯示文字
        /// </summary>
        public string AverageDailySignInRateDisplay => $"{AverageDailySignInRate:P1}";
    }

    /// <summary>
    /// 簽到獎勵
    /// </summary>
    public class SignInReward
    {
        /// <summary>
        /// 獎勵ID
        /// </summary>
        public int RewardId { get; set; }
        
        /// <summary>
        /// 連續天數
        /// </summary>
        public int ConsecutiveDays { get; set; }
        
        /// <summary>
        /// 獎勵類型
        /// </summary>
        public string RewardType { get; set; } = string.Empty;
        
        /// <summary>
        /// 獎勵數量
        /// </summary>
        public int RewardAmount { get; set; }
        
        /// <summary>
        /// 獎勵描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否已領取
        /// </summary>
        public bool IsClaimed { get; set; }
        
        /// <summary>
        /// 領取時間
        /// </summary>
        public DateTime? ClaimedTime { get; set; }
        
        /// <summary>
        /// 獎勵圖標
        /// </summary>
        public string Icon { get; set; } = string.Empty;
        
        /// <summary>
        /// 獎勵顏色
        /// </summary>
        public string Color { get; set; } = string.Empty;
    }

    /// <summary>
    /// 簽到獎勵統計
    /// </summary>
    public class SignInRewardStatistics
    {
        /// <summary>
        /// 總獎勵數
        /// </summary>
        public int TotalRewards { get; set; }
        
        /// <summary>
        /// 已領取獎勵數
        /// </summary>
        public int ClaimedRewards { get; set; }
        
        /// <summary>
        /// 未領取獎勵數
        /// </summary>
        public int UnclaimedRewards { get; set; }
        
        /// <summary>
        /// 領取率
        /// </summary>
        public double ClaimRate { get; set; }
        
        /// <summary>
        /// 總獲得點數
        /// </summary>
        public int TotalPointsEarned { get; set; }
        
        /// <summary>
        /// 總獲得經驗
        /// </summary>
        public int TotalExperienceEarned { get; set; }
        
        /// <summary>
        /// 領取率顯示文字
        /// </summary>
        public string ClaimRateDisplay => $"{ClaimRate:P1}";
    }

    /// <summary>
    /// 連續簽到統計
    /// </summary>
    public class ConsecutiveSignInStatistics
    {
        /// <summary>
        /// 連續天數
        /// </summary>
        public int ConsecutiveDays { get; set; }
        
        /// <summary>
        /// 用戶數量
        /// </summary>
        public int UserCount { get; set; }
        
        /// <summary>
        /// 百分比
        /// </summary>
        public double Percentage { get; set; }
    }

    /// <summary>
    /// 每日簽到統計
    /// </summary>
    public class DailySignInStatistics
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// 簽到次數
        /// </summary>
        public int SignInCount { get; set; }
        
        /// <summary>
        /// 簽到率
        /// </summary>
        public double SignInRate { get; set; }
        
        /// <summary>
        /// 日期顯示文字
        /// </summary>
        public string DateDisplay => Date.ToString("yyyy-MM-dd");
        
        /// <summary>
        /// 簽到率顯示文字
        /// </summary>
        public string SignInRateDisplay => $"{SignInRate:P1}";
    }

    /// <summary>
    /// 月曆天數
    /// </summary>
    public class CalendarDay
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// 是否為今天
        /// </summary>
        public bool IsToday { get; set; }
        
        /// <summary>
        /// 是否已簽到
        /// </summary>
        public bool IsSignedIn { get; set; }
        
        /// <summary>
        /// 是否為本月
        /// </summary>
        public bool IsCurrentMonth { get; set; }
        
        /// <summary>
        /// 簽到獎勵
        /// </summary>
        public SignInReward? Reward { get; set; }
        
        /// <summary>
        /// 日期顯示文字
        /// </summary>
        public string DateDisplay => Date.Day.ToString();
        
        /// <summary>
        /// 日期完整顯示文字
        /// </summary>
        public string FullDateDisplay => Date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 簽到規則統計
    /// </summary>
    public class SignInRuleStatistics
    {
        /// <summary>
        /// 規則使用次數
        /// </summary>
        public int RuleUsageCount { get; set; }
        
        /// <summary>
        /// 規則生效時間
        /// </summary>
        public DateTime RuleEffectiveTime { get; set; }
        
        /// <summary>
        /// 規則修改次數
        /// </summary>
        public int RuleModificationCount { get; set; }
        
        /// <summary>
        /// 最後修改時間
        /// </summary>
        public DateTime LastModifiedTime { get; set; }
        
        /// <summary>
        /// 規則狀態
        /// </summary>
        public string RuleStatus { get; set; } = string.Empty;
    }

    /// <summary>
    /// 簽到提醒
    /// </summary>
    public class SignInReminder
    {
        /// <summary>
        /// 提醒ID
        /// </summary>
        public int ReminderId { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 提醒時間
        /// </summary>
        public DateTime ReminderTime { get; set; }
        
        /// <summary>
        /// 提醒類型
        /// </summary>
        public string ReminderType { get; set; } = string.Empty;
        
        /// <summary>
        /// 提醒內容
        /// </summary>
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否已發送
        /// </summary>
        public bool IsSent { get; set; }
        
        /// <summary>
        /// 發送時間
        /// </summary>
        public DateTime? SentTime { get; set; }
    }

    /// <summary>
    /// 簽到成就
    /// </summary>
    public class SignInAchievement
    {
        /// <summary>
        /// 成就ID
        /// </summary>
        public int AchievementId { get; set; }
        
        /// <summary>
        /// 成就名稱
        /// </summary>
        public string AchievementName { get; set; } = string.Empty;
        
        /// <summary>
        /// 成就描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 成就條件
        /// </summary>
        public string Condition { get; set; } = string.Empty;
        
        /// <summary>
        /// 成就獎勵
        /// </summary>
        public string Reward { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否已達成
        /// </summary>
        public bool IsAchieved { get; set; }
        
        /// <summary>
        /// 達成時間
        /// </summary>
        public DateTime? AchievedTime { get; set; }
        
        /// <summary>
        /// 成就圖標
        /// </summary>
        public string Icon { get; set; } = string.Empty;
        
        /// <summary>
        /// 成就顏色
        /// </summary>
        public string Color { get; set; } = string.Empty;
    }
}
