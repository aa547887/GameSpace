using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    // 管理員儀表板相關 Models
    public class AdminDashboardViewModel
    {
        public SystemStatsModel SystemStats { get; set; } = new();
        public List<RecentSignInModel> RecentSignIns { get; set; } = new();
        public List<RecentGameRecordModel> RecentGameRecords { get; set; } = new();
        public List<RecentWalletTransactionModel> RecentWalletTransactions { get; set; } = new();
        public ChartDataModel ChartData { get; set; } = new();
    }

    public class SystemStatsModel
    {
        public int TotalUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalGamePlays { get; set; }
        public int TotalPoints { get; set; }
        public int ActiveUsers { get; set; }
        public int TodaySignIns { get; set; }
        public int TodayGamePlays { get; set; }
        public int TodayPointsEarned { get; set; }
    }

    public class RecentSignInModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SignInDate { get; set; }
        public int ConsecutiveDays { get; set; }
        public int PointsEarned { get; set; }
    }

    public class RecentGameRecordModel
    {
        public int GameId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Result { get; set; } = string.Empty;
        public int PointsEarned { get; set; }
        public int ExperienceEarned { get; set; }
    }

    public class RecentWalletTransactionModel
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }

    public class ChartDataModel
    {
        public List<string> Labels { get; set; } = new();
        public List<int> SignInData { get; set; } = new();
        public List<int> GameData { get; set; } = new();
        public List<int> PointsData { get; set; } = new();
    }

    // 寵物系統規則設定 Models
    public class PetSystemRulesUpdateModel
    {
        [Required(ErrorMessage = "請輸入規則名稱")]
        [StringLength(100, ErrorMessage = "規則名稱不能超過100字")]
        public string RuleName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入最大等級")]
        [Range(1, 100, ErrorMessage = "最大等級必須在1-100之間")]
        public int MaxLevel { get; set; }
        
        [Required(ErrorMessage = "請輸入每級所需經驗")]
        [Range(100, 10000, ErrorMessage = "每級所需經驗必須在100-10000之間")]
        public int ExperiencePerLevel { get; set; }
        
        [Required(ErrorMessage = "請輸入最大飢餓值")]
        [Range(50, 200, ErrorMessage = "最大飢餓值必須在50-200之間")]
        public int MaxHunger { get; set; }
        
        [Required(ErrorMessage = "請輸入最大快樂值")]
        [Range(50, 200, ErrorMessage = "最大快樂值必須在50-200之間")]
        public int MaxHappiness { get; set; }
        
        [Required(ErrorMessage = "請輸入最大健康值")]
        [Range(50, 200, ErrorMessage = "最大健康值必須在50-200之間")]
        public int MaxHealth { get; set; }
        
        [Required(ErrorMessage = "請輸入最大能量值")]
        [Range(50, 200, ErrorMessage = "最大能量值必須在50-200之間")]
        public int MaxEnergy { get; set; }
        
        [Required(ErrorMessage = "請輸入最大清潔值")]
        [Range(50, 200, ErrorMessage = "最大清潔值必須在50-200之間")]
        public int MaxCleanliness { get; set; }
        
        [Required(ErrorMessage = "請輸入每日衰減率")]
        [Range(1, 50, ErrorMessage = "每日衰減率必須在1-50之間")]
        public int DailyDecayRate { get; set; }
        
        [Required(ErrorMessage = "請輸入互動獎勵")]
        [Range(1, 50, ErrorMessage = "互動獎勵必須在1-50之間")]
        public int InteractionBonus { get; set; }
        
        [Required(ErrorMessage = "請輸入換膚所需點數")]
        [Range(0, 1000, ErrorMessage = "換膚所需點數必須在0-1000之間")]
        public int ColorChangeCost { get; set; }
        
        [Required(ErrorMessage = "請輸入換背景所需點數")]
        [Range(0, 1000, ErrorMessage = "換背景所需點數必須在0-1000之間")]
        public int BackgroundChangeCost { get; set; }
    }

    // 寵物個別設定 Models
    public class PetIndividualSettingsModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "請輸入寵物名稱")]
        [StringLength(50, ErrorMessage = "寵物名稱不能超過50字")]
        public string PetName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請選擇膚色")]
        [StringLength(20, ErrorMessage = "膚色不能超過20字")]
        public string SkinColor { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請選擇背景")]
        [StringLength(20, ErrorMessage = "背景不能超過20字")]
        public string Background { get; set; } = string.Empty;
        
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        public int Level { get; set; } = 1;
        
        [Range(0, 10000, ErrorMessage = "經驗值必須在0-10000之間")]
        public int Experience { get; set; } = 0;
        
        [Range(0, 200, ErrorMessage = "飢餓值必須在0-200之間")]
        public int Hunger { get; set; } = 100;
        
        [Range(0, 200, ErrorMessage = "快樂值必須在0-200之間")]
        public int Happiness { get; set; } = 100;
        
        [Range(0, 200, ErrorMessage = "健康值必須在0-200之間")]
        public int Health { get; set; } = 100;
        
        [Range(0, 200, ErrorMessage = "能量值必須在0-200之間")]
        public int Energy { get; set; } = 100;
        
        [Range(0, 200, ErrorMessage = "清潔值必須在0-200之間")]
        public int Cleanliness { get; set; } = 100;
        
        public List<User> Users { get; set; } = new();
        public List<string> AvailableSkinColors { get; set; } = new();
        public List<string> AvailableBackgrounds { get; set; } = new();
    }

    // 小遊戲規則設定 Models
    public class MiniGameRulesUpdateModel
    {
        [Required(ErrorMessage = "請輸入規則名稱")]
        [StringLength(100, ErrorMessage = "規則名稱不能超過100字")]
        public string RuleName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入每日遊戲次數限制")]
        [Range(1, 10, ErrorMessage = "每日遊戲次數限制必須在1-10之間")]
        public int MaxDailyPlays { get; set; } = 3;
        
        [Required(ErrorMessage = "請輸入最大遊戲時間(秒)")]
        [Range(30, 600, ErrorMessage = "最大遊戲時間必須在30-600秒之間")]
        public int MaxPlayTime { get; set; } = 300;
        
        [Required(ErrorMessage = "請輸入勝利獎勵點數")]
        [Range(1, 1000, ErrorMessage = "勝利獎勵點數必須在1-1000之間")]
        public int PointsPerWin { get; set; } = 100;
        
        [Required(ErrorMessage = "請輸入失敗獎勵點數")]
        [Range(0, 500, ErrorMessage = "失敗獎勵點數必須在0-500之間")]
        public int PointsPerLose { get; set; } = 10;
        
        [Required(ErrorMessage = "請輸入勝利獎勵經驗")]
        [Range(1, 500, ErrorMessage = "勝利獎勵經驗必須在1-500之間")]
        public int ExperiencePerWin { get; set; } = 50;
        
        [Required(ErrorMessage = "請輸入失敗獎勵經驗")]
        [Range(0, 250, ErrorMessage = "失敗獎勵經驗必須在0-250之間")]
        public int ExperiencePerLose { get; set; } = 5;
        
        [Required(ErrorMessage = "請輸入怪物數量")]
        [Range(1, 20, ErrorMessage = "怪物數量必須在1-20之間")]
        public int MonsterCount { get; set; } = 5;
        
        [Required(ErrorMessage = "請輸入怪物速度")]
        [Range(0.1, 5.0, ErrorMessage = "怪物速度必須在0.1-5.0之間")]
        public double MonsterSpeed { get; set; } = 1.0;
    }

    // 簽到規則設定 Models
    public class SignInRulesUpdateModel
    {
        [Required(ErrorMessage = "請輸入規則名稱")]
        [StringLength(100, ErrorMessage = "規則名稱不能超過100字")]
        public string RuleName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入每日簽到獎勵點數")]
        [Range(1, 1000, ErrorMessage = "每日簽到獎勵點數必須在1-1000之間")]
        public int DailyPoints { get; set; } = 10;
        
        [Required(ErrorMessage = "請輸入連續簽到7天額外獎勵")]
        [Range(0, 2000, ErrorMessage = "連續簽到7天額外獎勵必須在0-2000之間")]
        public int WeeklyBonus { get; set; } = 50;
        
        [Required(ErrorMessage = "請輸入連續簽到30天額外獎勵")]
        [Range(0, 5000, ErrorMessage = "連續簽到30天額外獎勵必須在0-5000之間")]
        public int MonthlyBonus { get; set; } = 200;
        
        [Required(ErrorMessage = "請輸入最大連續簽到天數")]
        [Range(7, 365, ErrorMessage = "最大連續簽到天數必須在7-365之間")]
        public int MaxConsecutiveDays { get; set; } = 30;
    }

    // 錯誤日誌查詢 Models
    public class ErrorLogQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? ErrorLevel { get; set; }
        public string? Source { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class ErrorLogModel
    {
        public int Id { get; set; }
        public string ErrorLevel { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string RequestPath { get; set; } = string.Empty;
    }

    // 系統設定 Models
    public class SystemSettingsModel
    {
        public string SiteName { get; set; } = string.Empty;
        public string SiteDescription { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public bool MaintenanceMode { get; set; }
        public string MaintenanceMessage { get; set; } = string.Empty;
        public int SessionTimeout { get; set; } = 30;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LoginLockoutDuration { get; set; } = 15;
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnableSMSNotifications { get; set; } = false;
    }

    // 快速搜尋 Models
    public class QuickSearchModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public string SearchType { get; set; } = "all"; // all, user, pet, game, wallet
    }

    public class QuickSearchResultModel
    {
        public string Type { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
