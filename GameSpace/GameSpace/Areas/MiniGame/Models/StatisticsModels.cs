using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 統計模型集合
    /// </summary>
    public class StatisticsModels
    {
        /// <summary>
        /// 簽到統計讀取模型
        /// </summary>
        public class SignInStatisticsReadModel
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
            /// 簽到率
            /// </summary>
            public decimal SignInRate { get; set; }
            
            /// <summary>
            /// 今日簽到數
            /// </summary>
            public int TodaySignIns { get; set; }
            
            /// <summary>
            /// 本週簽到數
            /// </summary>
            public int ThisWeekSignIns { get; set; }
            
            /// <summary>
            /// 本月簽到數
            /// </summary>
            public int ThisMonthSignIns { get; set; }
            
            /// <summary>
            /// 平均連續簽到天數
            /// </summary>
            public double AverageConsecutiveDays { get; set; }
            
            /// <summary>
            /// 最高連續簽到天數
            /// </summary>
            public int MaxConsecutiveDays { get; set; }
            
            /// <summary>
            /// 簽到率顯示文字
            /// </summary>
            public string SignInRateDisplay => $"{SignInRate:P1}";
        }

        /// <summary>
        /// 寵物統計讀取模型
        /// </summary>
        public class PetStatisticsReadModel
        {
            /// <summary>
            /// 總寵物數
            /// </summary>
            public int TotalPets { get; set; }
            
            /// <summary>
            /// 平均等級
            /// </summary>
            public double AverageLevel { get; set; }
            
            /// <summary>
            /// 平均經驗
            /// </summary>
            public double AverageExperience { get; set; }
            
            /// <summary>
            /// 最高等級
            /// </summary>
            public int HighestLevel { get; set; }
            
            /// <summary>
            /// 最高經驗
            /// </summary>
            public int HighestExperience { get; set; }
            
            /// <summary>
            /// 膚色統計
            /// </summary>
            public List<PetSkinStatistics> SkinStatistics { get; set; } = new();
            
            /// <summary>
            /// 背景統計
            /// </summary>
            public List<PetBackgroundStatistics> BackgroundStatistics { get; set; } = new();
            
            /// <summary>
            /// 等級分布
            /// </summary>
            public List<PetLevelDistribution> LevelDistribution { get; set; } = new();
        }

        /// <summary>
        /// 小遊戲統計讀取模型
        /// </summary>
        public class MiniGameStatisticsReadModel
        {
            /// <summary>
            /// 總遊戲次數
            /// </summary>
            public int TotalGames { get; set; }
            
            /// <summary>
            /// 勝利次數
            /// </summary>
            public int WinCount { get; set; }
            
            /// <summary>
            /// 失敗次數
            /// </summary>
            public int LoseCount { get; set; }
            
            /// <summary>
            /// 中止次數
            /// </summary>
            public int AbortCount { get; set; }
            
            /// <summary>
            /// 勝率
            /// </summary>
            public double WinRate { get; set; }
            
            /// <summary>
            /// 平均分數
            /// </summary>
            public double AverageScore { get; set; }
            
            /// <summary>
            /// 最高分數
            /// </summary>
            public int HighestScore { get; set; }
            
            /// <summary>
            /// 總獲得點數
            /// </summary>
            public int TotalPointsEarned { get; set; }
            
            /// <summary>
            /// 總獲得經驗
            /// </summary>
            public int TotalExperienceEarned { get; set; }
            
            /// <summary>
            /// 平均遊戲時間（秒）
            /// </summary>
            public double AverageGameDuration { get; set; }
            
            /// <summary>
            /// 勝率顯示文字
            /// </summary>
            public string WinRateDisplay => $"{WinRate:P1}";
            
            /// <summary>
            /// 平均分數顯示文字
            /// </summary>
            public string AverageScoreDisplay => $"{AverageScore:F1}";
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
        /// 商城統計讀取模型
        /// </summary>
        public class ShopStatisticsReadModel
        {
            /// <summary>
            /// 總優惠券類型數
            /// </summary>
            public int TotalCouponTypes { get; set; }
            
            /// <summary>
            /// 總電子禮券類型數
            /// </summary>
            public int TotalEVoucherTypes { get; set; }
            
            /// <summary>
            /// 總發行優惠券數
            /// </summary>
            public int TotalCouponsIssued { get; set; }
            
            /// <summary>
            /// 總發行電子禮券數
            /// </summary>
            public int TotalEVouchersIssued { get; set; }
            
            /// <summary>
            /// 總消耗點數
            /// </summary>
            public int TotalPointsSpent { get; set; }
            
            /// <summary>
            /// 總折扣金額
            /// </summary>
            public int TotalDiscountGiven { get; set; }
            
            /// <summary>
            /// 熱門商品
            /// </summary>
            public List<ShopPopularItemModel> PopularItems { get; set; } = new();
            
            /// <summary>
            /// 今日統計
            /// </summary>
            public DailyShopStatistics TodayStats { get; set; } = new();
        }

        /// <summary>
        /// 遊戲規則統計讀取模型
        /// </summary>
        public class GameRuleStatisticsReadModel
        {
            /// <summary>
            /// 總規則數
            /// </summary>
            public int TotalRules { get; set; }
            
            /// <summary>
            /// 啟用規則數
            /// </summary>
            public int ActiveRules { get; set; }
            
            /// <summary>
            /// 規則使用次數
            /// </summary>
            public int RuleUsageCount { get; set; }
            
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
        /// 寵物規則統計讀取模型
        /// </summary>
        public class PetRuleStatisticsReadModel
        {
            /// <summary>
            /// 總規則數
            /// </summary>
            public int TotalRules { get; set; }
            
            /// <summary>
            /// 啟用規則數
            /// </summary>
            public int ActiveRules { get; set; }
            
            /// <summary>
            /// 規則使用次數
            /// </summary>
            public int RuleUsageCount { get; set; }
            
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
        /// 簽到規則統計讀取模型
        /// </summary>
        public class SignInRuleStatisticsReadModel
        {
            /// <summary>
            /// 總規則數
            /// </summary>
            public int TotalRules { get; set; }
            
            /// <summary>
            /// 啟用規則數
            /// </summary>
            public int ActiveRules { get; set; }
            
            /// <summary>
            /// 規則使用次數
            /// </summary>
            public int RuleUsageCount { get; set; }
            
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
        /// 整體統計讀取模型
        /// </summary>
        public class OverallStatisticsReadModel
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
            /// 總寵物數
            /// </summary>
            public int TotalPets { get; set; }
            
            /// <summary>
            /// 總遊戲次數
            /// </summary>
            public int TotalGames { get; set; }
            
            /// <summary>
            /// 總簽到次數
            /// </summary>
            public int TotalSignIns { get; set; }
            
            /// <summary>
            /// 總交易次數
            /// </summary>
            public int TotalTransactions { get; set; }
            
            /// <summary>
            /// 總餘額
            /// </summary>
            public int TotalBalance { get; set; }
            
            /// <summary>
            /// 今日統計
            /// </summary>
            public DailyOverallStatistics TodayStats { get; set; } = new();
            
            /// <summary>
            /// 本週統計
            /// </summary>
            public WeeklyOverallStatistics ThisWeekStats { get; set; } = new();
            
            /// <summary>
            /// 本月統計
            /// </summary>
            public MonthlyOverallStatistics ThisMonthStats { get; set; } = new();
        }

        /// <summary>
        /// 每日整體統計
        /// </summary>
        public class DailyOverallStatistics
        {
            /// <summary>
            /// 日期
            /// </summary>
            public DateTime Date { get; set; }
            
            /// <summary>
            /// 新增用戶數
            /// </summary>
            public int NewUsers { get; set; }
            
            /// <summary>
            /// 新增寵物數
            /// </summary>
            public int NewPets { get; set; }
            
            /// <summary>
            /// 遊戲次數
            /// </summary>
            public int GamesPlayed { get; set; }
            
            /// <summary>
            /// 簽到次數
            /// </summary>
            public int SignIns { get; set; }
            
            /// <summary>
            /// 交易次數
            /// </summary>
            public int Transactions { get; set; }
            
            /// <summary>
            /// 交易金額
            /// </summary>
            public int TransactionAmount { get; set; }
            
            /// <summary>
            /// 日期顯示文字
            /// </summary>
            public string DateDisplay => Date.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 每週整體統計
        /// </summary>
        public class WeeklyOverallStatistics
        {
            /// <summary>
            /// 週開始日期
            /// </summary>
            public DateTime WeekStart { get; set; }
            
            /// <summary>
            /// 週結束日期
            /// </summary>
            public DateTime WeekEnd { get; set; }
            
            /// <summary>
            /// 新增用戶數
            /// </summary>
            public int NewUsers { get; set; }
            
            /// <summary>
            /// 新增寵物數
            /// </summary>
            public int NewPets { get; set; }
            
            /// <summary>
            /// 遊戲次數
            /// </summary>
            public int GamesPlayed { get; set; }
            
            /// <summary>
            /// 簽到次數
            /// </summary>
            public int SignIns { get; set; }
            
            /// <summary>
            /// 交易次數
            /// </summary>
            public int Transactions { get; set; }
            
            /// <summary>
            /// 交易金額
            /// </summary>
            public int TransactionAmount { get; set; }
            
            /// <summary>
            /// 週顯示文字
            /// </summary>
            public string WeekDisplay => $"{WeekStart:yyyy-MM-dd} ~ {WeekEnd:yyyy-MM-dd}";
        }

        /// <summary>
        /// 每月整體統計
        /// </summary>
        public class MonthlyOverallStatistics
        {
            /// <summary>
            /// 月份
            /// </summary>
            public DateTime Month { get; set; }
            
            /// <summary>
            /// 新增用戶數
            /// </summary>
            public int NewUsers { get; set; }
            
            /// <summary>
            /// 新增寵物數
            /// </summary>
            public int NewPets { get; set; }
            
            /// <summary>
            /// 遊戲次數
            /// </summary>
            public int GamesPlayed { get; set; }
            
            /// <summary>
            /// 簽到次數
            /// </summary>
            public int SignIns { get; set; }
            
            /// <summary>
            /// 交易次數
            /// </summary>
            public int Transactions { get; set; }
            
            /// <summary>
            /// 交易金額
            /// </summary>
            public int TransactionAmount { get; set; }
            
            /// <summary>
            /// 月份顯示文字
            /// </summary>
            public string MonthDisplay => Month.ToString("yyyy-MM");
        }

        /// <summary>
        /// 統計圖表數據
        /// </summary>
        public class ChartData
        {
            /// <summary>
            /// 圖表標題
            /// </summary>
            public string Title { get; set; } = string.Empty;
            
            /// <summary>
            /// 圖表類型
            /// </summary>
            public string ChartType { get; set; } = string.Empty;
            
            /// <summary>
            /// 數據標籤
            /// </summary>
            public List<string> Labels { get; set; } = new();
            
            /// <summary>
            /// 數據值
            /// </summary>
            public List<decimal> Values { get; set; } = new();
            
            /// <summary>
            /// 數據顏色
            /// </summary>
            public List<string> Colors { get; set; } = new();
            
            /// <summary>
            /// 圖表選項
            /// </summary>
            public Dictionary<string, object> Options { get; set; } = new();
        }

        /// <summary>
        /// 統計報表
        /// </summary>
        public class StatisticsReport
        {
            /// <summary>
            /// 報表ID
            /// </summary>
            public int ReportId { get; set; }
            
            /// <summary>
            /// 報表名稱
            /// </summary>
            public string ReportName { get; set; } = string.Empty;
            
            /// <summary>
            /// 報表類型
            /// </summary>
            public string ReportType { get; set; } = string.Empty;
            
            /// <summary>
            /// 報表描述
            /// </summary>
            public string Description { get; set; } = string.Empty;
            
            /// <summary>
            /// 生成時間
            /// </summary>
            public DateTime GeneratedTime { get; set; }
            
            /// <summary>
            /// 報表數據
            /// </summary>
            public Dictionary<string, object> ReportData { get; set; } = new();
            
            /// <summary>
            /// 報表狀態
            /// </summary>
            public string Status { get; set; } = string.Empty;
        }
    }

    /// <summary>
    /// 寵物膚色統計
    /// </summary>
    public class PetSkinStatistics
    {
        /// <summary>
        /// 膚色名稱
        /// </summary>
        public string SkinName { get; set; } = string.Empty;
        
        /// <summary>
        /// 使用次數
        /// </summary>
        public int UsageCount { get; set; }
        
        /// <summary>
        /// 百分比
        /// </summary>
        public double Percentage { get; set; }
    }

    /// <summary>
    /// 寵物背景統計
    /// </summary>
    public class PetBackgroundStatistics
    {
        /// <summary>
        /// 背景名稱
        /// </summary>
        public string BackgroundName { get; set; } = string.Empty;
        
        /// <summary>
        /// 使用次數
        /// </summary>
        public int UsageCount { get; set; }
        
        /// <summary>
        /// 百分比
        /// </summary>
        public double Percentage { get; set; }
    }

    /// <summary>
    /// 寵物等級分布
    /// </summary>
    public class PetLevelDistribution
    {
        /// <summary>
        /// 等級範圍
        /// </summary>
        public string LevelRange { get; set; } = string.Empty;
        
        /// <summary>
        /// 寵物數量
        /// </summary>
        public int PetCount { get; set; }
        
        /// <summary>
        /// 百分比
        /// </summary>
        public double Percentage { get; set; }
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
    }

    /// <summary>
    /// 商城熱門商品模型
    /// </summary>
    public class ShopPopularItemModel
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public int ItemId { get; set; }
        
        /// <summary>
        /// 商品名稱
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
        
        /// <summary>
        /// 商品類型
        /// </summary>
        public string ItemType { get; set; } = string.Empty;
        
        /// <summary>
        /// 銷售數量
        /// </summary>
        public int SalesCount { get; set; }
        
        /// <summary>
        /// 總獲得點數
        /// </summary>
        public int TotalPointsEarned { get; set; }
        
        /// <summary>
        /// 熱門度分數
        /// </summary>
        public double PopularityScore { get; set; }
        
        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }
    }

    /// <summary>
    /// 每日商城統計
    /// </summary>
    public class DailyShopStatistics
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// 新增優惠券數
        /// </summary>
        public int NewCoupons { get; set; }
        
        /// <summary>
        /// 新增電子禮券數
        /// </summary>
        public int NewEVouchers { get; set; }
        
        /// <summary>
        /// 消耗點數
        /// </summary>
        public int PointsSpent { get; set; }
        
        /// <summary>
        /// 折扣金額
        /// </summary>
        public int DiscountGiven { get; set; }
    }
}
