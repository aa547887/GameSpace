using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理員儀表板主要視圖模型
    /// </summary>
    public class AdminDashboardViewModel
    {
        /// <summary>
        /// 系統統計數據
        /// </summary>
        public SystemStatsModel SystemStats { get; set; } = new();
        
        /// <summary>
        /// 最近簽到記錄
        /// </summary>
        public List<RecentSignInModel> RecentSignIns { get; set; } = new();
        
        /// <summary>
        /// 最近遊戲記錄
        /// </summary>
        public List<RecentGameRecordModel> RecentGameRecords { get; set; } = new();
        
        /// <summary>
        /// 最近錢包交易記錄
        /// </summary>
        public List<RecentWalletTransactionModel> RecentWalletTransactions { get; set; } = new();
        
        /// <summary>
        /// 圖表數據
        /// </summary>
        public ChartDataModel ChartData { get; set; } = new();
        
        /// <summary>
        /// 系統狀態指示器
        /// </summary>
        public SystemStatusModel SystemStatus { get; set; } = new();
    }

    /// <summary>
    /// 系統統計數據模型
    /// </summary>
    public class SystemStatsModel
    {
        /// <summary>
        /// 總用戶數
        /// </summary>
        public int TotalUsers { get; set; }
        
        /// <summary>
        /// 總寵物數
        /// </summary>
        public int TotalPets { get; set; }
        
        /// <summary>
        /// 總遊戲次數
        /// </summary>
        public int TotalGamePlays { get; set; }
        
        /// <summary>
        /// 總點數
        /// </summary>
        public int TotalPoints { get; set; }
        
        /// <summary>
        /// 活躍用戶數
        /// </summary>
        public int ActiveUsers { get; set; }
        
        /// <summary>
        /// 今日簽到數
        /// </summary>
        public int TodaySignIns { get; set; }
        
        /// <summary>
        /// 今日遊戲次數
        /// </summary>
        public int TodayGamePlays { get; set; }
        
        /// <summary>
        /// 今日獲得點數
        /// </summary>
        public int TodayPointsEarned { get; set; }
        
        /// <summary>
        /// 系統健康度百分比
        /// </summary>
        public double SystemHealthPercentage { get; set; }
        
        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// 最近簽到記錄模型
    /// </summary>
    public class RecentSignInModel
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
        /// 簽到日期
        /// </summary>
        public DateTime SignInDate { get; set; }
        
        /// <summary>
        /// 連續簽到天數
        /// </summary>
        public int ConsecutiveDays { get; set; }
        
        /// <summary>
        /// 獲得點數
        /// </summary>
        public int PointsEarned { get; set; }
        
        /// <summary>
        /// 是否為今日簽到
        /// </summary>
        public bool IsToday { get; set; }
        
        /// <summary>
        /// 簽到時間（格式化顯示）
        /// </summary>
        public string SignInTimeDisplay => SignInDate.ToString("HH:mm");
    }

    /// <summary>
    /// 最近遊戲記錄模型
    /// </summary>
    public class RecentGameRecordModel
    {
        /// <summary>
        /// 遊戲ID
        /// </summary>
        public int GameId { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// 用戶名稱
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        
        /// <summary>
        /// 開始時間
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// 結束時間
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// 遊戲結果
        /// </summary>
        public string Result { get; set; } = string.Empty;
        
        /// <summary>
        /// 獲得點數
        /// </summary>
        public int PointsEarned { get; set; }
        
        /// <summary>
        /// 獲得經驗
        /// </summary>
        public int ExperienceEarned { get; set; }
        
        /// <summary>
        /// 遊戲持續時間（秒）
        /// </summary>
        public int Duration => EndTime.HasValue ? (int)(EndTime.Value - StartTime).TotalSeconds : 0;
        
        /// <summary>
        /// 結果顯示文字
        /// </summary>
        public string ResultDisplay => Result switch
        {
            "Win" => "勝利",
            "Lose" => "失敗",
            "Abort" => "中止",
            _ => "未知"
        };
    }

    /// <summary>
    /// 最近錢包交易記錄模型
    /// </summary>
    public class RecentWalletTransactionModel
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
        /// 交易描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 交易日期
        /// </summary>
        public DateTime TransactionDate { get; set; }
        
        /// <summary>
        /// 是否為收入
        /// </summary>
        public bool IsIncome => Amount > 0;
        
        /// <summary>
        /// 金額顯示（帶正負號）
        /// </summary>
        public string AmountDisplay => IsIncome ? $"+{Amount:N0}" : $"{Amount:N0}";
    }

    /// <summary>
    /// 圖表數據模型
    /// </summary>
    public class ChartDataModel
    {
        /// <summary>
        /// 圖表標籤
        /// </summary>
        public List<string> Labels { get; set; } = new();
        
        /// <summary>
        /// 簽到數據
        /// </summary>
        public List<int> SignInData { get; set; } = new();
        
        /// <summary>
        /// 遊戲數據
        /// </summary>
        public List<int> GameData { get; set; } = new();
        
        /// <summary>
        /// 點數數據
        /// </summary>
        public List<int> PointsData { get; set; } = new();
        
        /// <summary>
        /// 用戶活動數據
        /// </summary>
        public List<int> UserActivityData { get; set; } = new();
    }

    /// <summary>
    /// 系統狀態模型
    /// </summary>
    public class SystemStatusModel
    {
        /// <summary>
        /// 數據庫連接狀態
        /// </summary>
        public bool DatabaseConnected { get; set; }
        
        /// <summary>
        /// 郵件服務狀態
        /// </summary>
        public bool EmailServiceActive { get; set; }
        
        /// <summary>
        /// 文件系統狀態
        /// </summary>
        public bool FileSystemAccessible { get; set; }
        
        /// <summary>
        /// 系統負載狀態
        /// </summary>
        public string LoadStatus { get; set; } = "正常";
        
        /// <summary>
        /// 最後檢查時間
        /// </summary>
        public DateTime LastChecked { get; set; }
        
        /// <summary>
        /// 系統狀態文字
        /// </summary>
        public string StatusText => DatabaseConnected && EmailServiceActive && FileSystemAccessible ? "正常" : "異常";
    }

    /// <summary>
    /// 寵物系統規則更新模型
    /// </summary>
    public class PetSystemRulesUpdateModel
    {
        /// <summary>
        /// 規則名稱
        /// </summary>
        [Required(ErrorMessage = "請輸入規則名稱")]
        [StringLength(100, ErrorMessage = "規則名稱不能超過100字")]
        public string RuleName { get; set; } = string.Empty;
        
        /// <summary>
        /// 最大等級
        /// </summary>
        [Required(ErrorMessage = "請輸入最大等級")]
        [Range(1, 100, ErrorMessage = "最大等級必須在1-100之間")]
        public int MaxLevel { get; set; }
        
        /// <summary>
        /// 每級所需經驗
        /// </summary>
        [Required(ErrorMessage = "請輸入每級所需經驗")]
        [Range(100, 10000, ErrorMessage = "每級所需經驗必須在100-10000之間")]
        public int ExperiencePerLevel { get; set; }
        
        /// <summary>
        /// 最大飢餓值
        /// </summary>
        [Required(ErrorMessage = "請輸入最大飢餓值")]
        [Range(50, 200, ErrorMessage = "最大飢餓值必須在50-200之間")]
        public int MaxHunger { get; set; }
        
        /// <summary>
        /// 最大快樂值
        /// </summary>
        [Required(ErrorMessage = "請輸入最大快樂值")]
        [Range(50, 200, ErrorMessage = "最大快樂值必須在50-200之間")]
        public int MaxHappiness { get; set; }
        
        /// <summary>
        /// 最大健康值
        /// </summary>
        [Required(ErrorMessage = "請輸入最大健康值")]
        [Range(50, 200, ErrorMessage = "最大健康值必須在50-200之間")]
        public int MaxHealth { get; set; }
        
        /// <summary>
        /// 最大能量值
        /// </summary>
        [Required(ErrorMessage = "請輸入最大能量值")]
        [Range(50, 200, ErrorMessage = "最大能量值必須在50-200之間")]
        public int MaxEnergy { get; set; }
        
        /// <summary>
        /// 最大清潔值
        /// </summary>
        [Required(ErrorMessage = "請輸入最大清潔值")]
        [Range(50, 200, ErrorMessage = "最大清潔值必須在50-200之間")]
        public int MaxCleanliness { get; set; }
        
        /// <summary>
        /// 每日衰減率
        /// </summary>
        [Required(ErrorMessage = "請輸入每日衰減率")]
        [Range(1, 50, ErrorMessage = "每日衰減率必須在1-50之間")]
        public int DailyDecayRate { get; set; }
        
        /// <summary>
        /// 互動獎勵
        /// </summary>
        [Required(ErrorMessage = "請輸入互動獎勵")]
        [Range(1, 50, ErrorMessage = "互動獎勵必須在1-50之間")]
        public int InteractionBonus { get; set; }
        
        /// <summary>
        /// 換膚所需點數
        /// </summary>
        [Required(ErrorMessage = "請輸入換膚所需點數")]
        [Range(0, 1000, ErrorMessage = "換膚所需點數必須在0-1000之間")]
        public int ColorChangeCost { get; set; }
        
        /// <summary>
        /// 換背景所需點數
        /// </summary>
        [Required(ErrorMessage = "請輸入換背景所需點數")]
        [Range(0, 1000, ErrorMessage = "換背景所需點數必須在0-1000之間")]
        public int BackgroundChangeCost { get; set; }
        
        /// <summary>
        /// 是否啟用自動衰減
        /// </summary>
        public bool EnableAutoDecay { get; set; } = true;
        
        /// <summary>
        /// 衰減檢查間隔（小時）
        /// </summary>
        [Range(1, 24, ErrorMessage = "衰減檢查間隔必須在1-24小時之間")]
        public int DecayCheckInterval { get; set; } = 24;
    }

    /// <summary>
    /// 寵物個別設定模型
    /// </summary>
    public class PetIndividualSettingsModel
    {
        /// <summary>
        /// 用戶ID
        /// </summary>
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }
        
        /// <summary>
        /// 寵物名稱
        /// </summary>
        [Required(ErrorMessage = "請輸入寵物名稱")]
        [StringLength(50, ErrorMessage = "寵物名稱不能超過50字")]
        public string PetName { get; set; } = string.Empty;
        
        /// <summary>
        /// 膚色
        /// </summary>
        [Required(ErrorMessage = "請選擇膚色")]
        [StringLength(20, ErrorMessage = "膚色不能超過20字")]
        public string SkinColor { get; set; } = string.Empty;
        
        /// <summary>
        /// 背景
        /// </summary>
        [Required(ErrorMessage = "請選擇背景")]
        [StringLength(20, ErrorMessage = "背景不能超過20字")]
        public string Background { get; set; } = string.Empty;
        
        /// <summary>
        /// 等級
        /// </summary>
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        public int Level { get; set; } = 1;
        
        /// <summary>
        /// 經驗值
        /// </summary>
        [Range(0, 10000, ErrorMessage = "經驗值必須在0-10000之間")]
        public int Experience { get; set; } = 0;
        
        /// <summary>
        /// 飢餓值
        /// </summary>
        [Range(0, 200, ErrorMessage = "飢餓值必須在0-200之間")]
        public int Hunger { get; set; } = 100;
        
        /// <summary>
        /// 快樂值
        /// </summary>
        [Range(0, 200, ErrorMessage = "快樂值必須在0-200之間")]
        public int Happiness { get; set; } = 100;
        
        /// <summary>
        /// 健康值
        /// </summary>
        [Range(0, 200, ErrorMessage = "健康值必須在0-200之間")]
        public int Health { get; set; } = 100;
        
        /// <summary>
        /// 能量值
        /// </summary>
        [Range(0, 200, ErrorMessage = "能量值必須在0-200之間")]
        public int Energy { get; set; } = 100;
        
        /// <summary>
        /// 清潔值
        /// </summary>
        [Range(0, 200, ErrorMessage = "清潔值必須在0-200之間")]
        public int Cleanliness { get; set; } = 100;
        
        /// <summary>
        /// 用戶列表
        /// </summary>
        public List<User> Users { get; set; } = new();
        
        /// <summary>
        /// 可用膚色列表
        /// </summary>
        public List<string> AvailableSkinColors { get; set; } = new();
        
        /// <summary>
        /// 可用背景列表
        /// </summary>
        public List<string> AvailableBackgrounds { get; set; } = new();
        
        /// <summary>
        /// 調整原因
        /// </summary>
        [StringLength(500, ErrorMessage = "調整原因不能超過500字")]
        public string? AdjustmentReason { get; set; }
    }

    /// <summary>
    /// 小遊戲規則更新模型
    /// </summary>
    public class MiniGameRulesUpdateModel
    {
        /// <summary>
        /// 規則名稱
        /// </summary>
        [Required(ErrorMessage = "請輸入規則名稱")]
        [StringLength(100, ErrorMessage = "規則名稱不能超過100字")]
        public string RuleName { get; set; } = string.Empty;
        
        /// <summary>
        /// 每日遊戲次數限制
        /// </summary>
        [Required(ErrorMessage = "請輸入每日遊戲次數限制")]
        [Range(1, 10, ErrorMessage = "每日遊戲次數限制必須在1-10之間")]
        public int MaxDailyPlays { get; set; } = 3;
        
        /// <summary>
        /// 最大遊戲時間（秒）
        /// </summary>
        [Required(ErrorMessage = "請輸入最大遊戲時間(秒)")]
        [Range(30, 600, ErrorMessage = "最大遊戲時間必須在30-600秒之間")]
        public int MaxPlayTime { get; set; } = 300;
        
        /// <summary>
        /// 勝利獎勵點數
        /// </summary>
        [Required(ErrorMessage = "請輸入勝利獎勵點數")]
        [Range(1, 1000, ErrorMessage = "勝利獎勵點數必須在1-1000之間")]
        public int PointsPerWin { get; set; } = 100;
        
        /// <summary>
        /// 失敗獎勵點數
        /// </summary>
        [Required(ErrorMessage = "請輸入失敗獎勵點數")]
        [Range(0, 500, ErrorMessage = "失敗獎勵點數必須在0-500之間")]
        public int PointsPerLose { get; set; } = 10;
        
        /// <summary>
        /// 勝利獎勵經驗
        /// </summary>
        [Required(ErrorMessage = "請輸入勝利獎勵經驗")]
        [Range(1, 500, ErrorMessage = "勝利獎勵經驗必須在1-500之間")]
        public int ExperiencePerWin { get; set; } = 50;
        
        /// <summary>
        /// 失敗獎勵經驗
        /// </summary>
        [Required(ErrorMessage = "請輸入失敗獎勵經驗")]
        [Range(0, 250, ErrorMessage = "失敗獎勵經驗必須在0-250之間")]
        public int ExperiencePerLose { get; set; } = 5;
        
        /// <summary>
        /// 怪物數量
        /// </summary>
        [Required(ErrorMessage = "請輸入怪物數量")]
        [Range(1, 20, ErrorMessage = "怪物數量必須在1-20之間")]
        public int MonsterCount { get; set; } = 5;
        
        /// <summary>
        /// 怪物速度
        /// </summary>
        [Required(ErrorMessage = "請輸入怪物速度")]
        [Range(0.1, 5.0, ErrorMessage = "怪物速度必須在0.1-5.0之間")]
        public double MonsterSpeed { get; set; } = 1.0;
        
        /// <summary>
        /// 是否啟用特殊獎勵
        /// </summary>
        public bool EnableSpecialRewards { get; set; } = true;
        
        /// <summary>
        /// 特殊獎勵觸發機率（百分比）
        /// </summary>
        [Range(0, 100, ErrorMessage = "特殊獎勵觸發機率必須在0-100之間")]
        public int SpecialRewardChance { get; set; } = 10;
    }

    /// <summary>
    /// 簽到規則更新模型
    /// </summary>
    public class SignInRulesUpdateModel
    {
        /// <summary>
        /// 規則名稱
        /// </summary>
        [Required(ErrorMessage = "請輸入規則名稱")]
        [StringLength(100, ErrorMessage = "規則名稱不能超過100字")]
        public string RuleName { get; set; } = string.Empty;
        
        /// <summary>
        /// 每日簽到獎勵點數
        /// </summary>
        [Required(ErrorMessage = "請輸入每日簽到獎勵點數")]
        [Range(1, 1000, ErrorMessage = "每日簽到獎勵點數必須在1-1000之間")]
        public int DailyPoints { get; set; } = 10;
        
        /// <summary>
        /// 連續簽到7天額外獎勵
        /// </summary>
        [Required(ErrorMessage = "請輸入連續簽到7天額外獎勵")]
        [Range(0, 2000, ErrorMessage = "連續簽到7天額外獎勵必須在0-2000之間")]
        public int WeeklyBonus { get; set; } = 50;
        
        /// <summary>
        /// 連續簽到30天額外獎勵
        /// </summary>
        [Required(ErrorMessage = "請輸入連續簽到30天額外獎勵")]
        [Range(0, 5000, ErrorMessage = "連續簽到30天額外獎勵必須在0-5000之間")]
        public int MonthlyBonus { get; set; } = 200;
        
        /// <summary>
        /// 最大連續簽到天數
        /// </summary>
        [Required(ErrorMessage = "請輸入最大連續簽到天數")]
        [Range(7, 365, ErrorMessage = "最大連續簽到天數必須在7-365之間")]
        public int MaxConsecutiveDays { get; set; } = 30;
        
        /// <summary>
        /// 是否啟用補簽功能
        /// </summary>
        public bool EnableMakeupSignIn { get; set; } = false;
        
        /// <summary>
        /// 補簽所需點數
        /// </summary>
        [Range(0, 1000, ErrorMessage = "補簽所需點數必須在0-1000之間")]
        public int MakeupSignInCost { get; set; } = 100;
    }

    /// <summary>
    /// 錯誤日誌查詢模型
    /// </summary>
    public class ErrorLogQueryModel
    {
        /// <summary>
        /// 頁碼
        /// </summary>
        public int PageNumber { get; set; } = 1;
        
        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; } = 20;
        
        /// <summary>
        /// 錯誤級別
        /// </summary>
        public string? ErrorLevel { get; set; }
        
        /// <summary>
        /// 來源
        /// </summary>
        public string? Source { get; set; }
        
        /// <summary>
        /// 開始日期
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// 結束日期
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? SearchTerm { get; set; }
    }

    /// <summary>
    /// 錯誤日誌模型
    /// </summary>
    public class ErrorLogModel
    {
        /// <summary>
        /// 錯誤ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 錯誤級別
        /// </summary>
        public string ErrorLevel { get; set; } = string.Empty;
        
        /// <summary>
        /// 來源
        /// </summary>
        public string Source { get; set; } = string.Empty;
        
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// 堆疊追蹤
        /// </summary>
        public string StackTrace { get; set; } = string.Empty;
        
        /// <summary>
        /// 建立日期
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// 請求路徑
        /// </summary>
        public string RequestPath { get; set; } = string.Empty;
        
        /// <summary>
        /// 錯誤級別顯示文字
        /// </summary>
        public string ErrorLevelDisplay => ErrorLevel switch
        {
            "Error" => "錯誤",
            "Warning" => "警告",
            "Info" => "資訊",
            "Debug" => "除錯",
            _ => "未知"
        };
    }

    /// <summary>
    /// 系統設定模型
    /// </summary>
    public class SystemSettingsModel
    {
        /// <summary>
        /// 網站名稱
        /// </summary>
        [Required(ErrorMessage = "請輸入網站名稱")]
        [StringLength(100, ErrorMessage = "網站名稱不能超過100字")]
        public string SiteName { get; set; } = string.Empty;
        
        /// <summary>
        /// 網站描述
        /// </summary>
        [StringLength(500, ErrorMessage = "網站描述不能超過500字")]
        public string SiteDescription { get; set; } = string.Empty;
        
        /// <summary>
        /// 管理員信箱
        /// </summary>
        [Required(ErrorMessage = "請輸入管理員信箱")]
        [EmailAddress(ErrorMessage = "請輸入有效的信箱格式")]
        public string AdminEmail { get; set; } = string.Empty;
        
        /// <summary>
        /// 維護模式
        /// </summary>
        public bool MaintenanceMode { get; set; }
        
        /// <summary>
        /// 維護訊息
        /// </summary>
        [StringLength(1000, ErrorMessage = "維護訊息不能超過1000字")]
        public string MaintenanceMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// 會話超時時間（分鐘）
        /// </summary>
        [Range(5, 1440, ErrorMessage = "會話超時時間必須在5-1440分鐘之間")]
        public int SessionTimeout { get; set; } = 30;
        
        /// <summary>
        /// 最大登入嘗試次數
        /// </summary>
        [Range(3, 10, ErrorMessage = "最大登入嘗試次數必須在3-10之間")]
        public int MaxLoginAttempts { get; set; } = 5;
        
        /// <summary>
        /// 登入鎖定持續時間（分鐘）
        /// </summary>
        [Range(5, 60, ErrorMessage = "登入鎖定持續時間必須在5-60分鐘之間")]
        public int LoginLockoutDuration { get; set; } = 15;
        
        /// <summary>
        /// 啟用郵件通知
        /// </summary>
        public bool EnableEmailNotifications { get; set; } = true;
        
        /// <summary>
        /// 啟用簡訊通知
        /// </summary>
        public bool EnableSMSNotifications { get; set; } = false;
    }

    /// <summary>
    /// 快速搜尋模型
    /// </summary>
    public class QuickSearchModel
    {
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        [Required(ErrorMessage = "請輸入搜尋關鍵字")]
        [StringLength(100, ErrorMessage = "搜尋關鍵字不能超過100字")]
        public string SearchTerm { get; set; } = string.Empty;
        
        /// <summary>
        /// 搜尋類型
        /// </summary>
        public string SearchType { get; set; } = "all"; // all, user, pet, game, wallet
    }

    /// <summary>
    /// 快速搜尋結果模型
    /// </summary>
    public class QuickSearchResultModel
    {
        /// <summary>
        /// 結果類型
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// 結果ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 標題
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 連結URL
        /// </summary>
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// 相關性分數
        /// </summary>
        public double RelevanceScore { get; set; }
        
        /// <summary>
        /// 類型顯示文字
        /// </summary>
        public string TypeDisplay => Type switch
        {
            "user" => "用戶",
            "pet" => "寵物",
            "game" => "遊戲",
            "wallet" => "錢包",
            "coupon" => "優惠券",
            "evoucher" => "電子禮券",
            _ => "其他"
        };
    }
}
