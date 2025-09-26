using System.ComponentModel.DataAnnotations;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 優化的 MiniGame Area 模型集合
    /// 確保所有 Admin 後台功能完整實作
    /// </summary>

    #region 會員錢包系統模型

    /// <summary>
    /// 會員點數查詢模型
    /// </summary>
    public class UserPointsQueryModel
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "UserName";
        public bool SortDescending { get; set; } = false;
    }

    /// <summary>
    /// 發放點數模型
    /// </summary>
    public class GrantPointsModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "請輸入點數")]
        [Range(1, 10000, ErrorMessage = "點數必須在 1-10000 之間")]
        public int Points { get; set; }

        [Required(ErrorMessage = "請輸入發放原因")]
        [StringLength(200, ErrorMessage = "原因不能超過 200 字")]
        public string Reason { get; set; } = string.Empty;

        public List<dynamic> Users { get; set; } = new();
    }

    /// <summary>
    /// 發放優惠券模型
    /// </summary>
    public class GrantCouponsModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "請選擇優惠券類型")]
        public int CouponTypeId { get; set; }

        [Required(ErrorMessage = "請輸入數量")]
        [Range(1, 100, ErrorMessage = "數量必須在 1-100 之間")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "請輸入發放原因")]
        [StringLength(200, ErrorMessage = "原因不能超過 200 字")]
        public string Reason { get; set; } = string.Empty;

        public List<dynamic> Users { get; set; } = new();
        public List<dynamic> CouponTypes { get; set; } = new();
    }

    /// <summary>
    /// 發放電子禮券模型
    /// </summary>
    public class GrantEVouchersModel
    {
        [Required(ErrorMessage = "請選擇用戶")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "請選擇電子禮券類型")]
        public int EVoucherTypeId { get; set; }

        [Required(ErrorMessage = "請輸入數量")]
        [Range(1, 100, ErrorMessage = "數量必須在 1-100 之間")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "請輸入發放原因")]
        [StringLength(200, ErrorMessage = "原因不能超過 200 字")]
        public string Reason { get; set; } = string.Empty;

        public List<dynamic> Users { get; set; } = new();
        public List<dynamic> EVoucherTypes { get; set; } = new();
    }

    /// <summary>
    /// 錢包歷史查詢模型
    /// </summary>
    public class WalletHistoryQueryModel
    {
        public int? UserId { get; set; }
        public string? TransactionType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 錢包歷史詳情模型
    /// </summary>
    public class WalletHistoryDetailModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int PointsChange { get; set; }
        public int PointsBefore { get; set; }
        public int PointsAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionTime { get; set; }
        public string? RelatedItemType { get; set; }
        public int? RelatedItemId { get; set; }
    }

    #endregion

    #region 會員簽到系統模型

    /// <summary>
    /// 簽到規則模型
    /// </summary>
    public class SignInRuleModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "請輸入連續簽到天數")]
        [Range(1, 30, ErrorMessage = "連續簽到天數必須在 1-30 之間")]
        public int ConsecutiveDays { get; set; }

        [Required(ErrorMessage = "請輸入獎勵類型")]
        public string RewardType { get; set; } = string.Empty; // Points, Coupon, EVoucher

        [Required(ErrorMessage = "請輸入獎勵值")]
        [Range(1, 10000, ErrorMessage = "獎勵值必須在 1-10000 之間")]
        public int RewardValue { get; set; }

        [StringLength(200, ErrorMessage = "描述不能超過 200 字")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 簽到規則視圖模型
    /// </summary>
    public class SignInRulesViewModel
    {
        public List<SignInRuleModel> Rules { get; set; } = new();
        public SignInRuleModel NewRule { get; set; } = new();
    }

    /// <summary>
    /// 簽到紀錄查詢模型
    /// </summary>
    public class SignInRecordQueryModel
    {
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? RewardType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 簽到紀錄詳情模型
    /// </summary>
    public class SignInRecordDetailModel
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SignTime { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public string CouponGained { get; set; } = string.Empty;
        public DateTime PointsGainedTime { get; set; }
        public DateTime ExpGainedTime { get; set; }
        public DateTime CouponGainedTime { get; set; }
        public int ConsecutiveDays { get; set; }
        public string RewardDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// 簽到統計模型
    /// </summary>
    public class SignInStatisticsModel
    {
        public int TotalSignIns { get; set; }
        public int TodaySignIns { get; set; }
        public int ThisWeekSignIns { get; set; }
        public int ThisMonthSignIns { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int TotalCouponsAwarded { get; set; }
        public double AverageSignInsPerDay { get; set; }
        public List<DailySignInCount> DailyCounts { get; set; } = new();
    }

    /// <summary>
    /// 每日簽到統計
    /// </summary>
    public class DailySignInCount
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    #endregion

    #region 寵物系統模型

    /// <summary>
    /// 寵物系統規則模型
    /// </summary>
    public class PetSystemRuleModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "請輸入規則名稱")]
        [StringLength(100, ErrorMessage = "規則名稱不能超過 100 字")]
        public string RuleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入規則類型")]
        public string RuleType { get; set; } = string.Empty; // LevelUp, Interaction, Skin, Background

        [Required(ErrorMessage = "請輸入規則值")]
        public string RuleValue { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述不能超過 500 字")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 寵物升級規則模型
    /// </summary>
    public class PetLevelUpRuleModel
    {
        public int Level { get; set; }
        public int RequiredExp { get; set; }
        public int PointsReward { get; set; }
        public string? SpecialReward { get; set; }
        public bool UnlockNewSkin { get; set; }
        public bool UnlockNewBackground { get; set; }
    }

    /// <summary>
    /// 寵物互動增益模型
    /// </summary>
    public class PetInteractionGainModel
    {
        public string InteractionType { get; set; } = string.Empty; // Feed, Play, Clean, Sleep
        public int HungerChange { get; set; }
        public int MoodChange { get; set; }
        public int StaminaChange { get; set; }
        public int CleanlinessChange { get; set; }
        public int ExpGain { get; set; }
        public int PointsCost { get; set; }
        public int CooldownMinutes { get; set; }
    }

    /// <summary>
    /// 寵物膚色選項模型
    /// </summary>
    public class PetSkinOptionModel
    {
        public string SkinColor { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int PointsCost { get; set; }
        public int RequiredLevel { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? PreviewImage { get; set; }
    }

    /// <summary>
    /// 寵物背景選項模型
    /// </summary>
    public class PetBackgroundOptionModel
    {
        public string BackgroundColor { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int PointsCost { get; set; }
        public int RequiredLevel { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? PreviewImage { get; set; }
    }

    /// <summary>
    /// 寵物系統規則視圖模型
    /// </summary>
    public class PetSystemRulesViewModel
    {
        public List<PetSystemRuleModel> Rules { get; set; } = new();
        public List<PetLevelUpRuleModel> LevelUpRules { get; set; } = new();
        public List<PetInteractionGainModel> InteractionGains { get; set; } = new();
        public List<PetSkinOptionModel> SkinOptions { get; set; } = new();
        public List<PetBackgroundOptionModel> BackgroundOptions { get; set; } = new();
        public PetSystemRuleModel NewRule { get; set; } = new();
    }

    /// <summary>
    /// 寵物設定模型
    /// </summary>
    public class PetSettingModel
    {
        public int PetId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入寵物名稱")]
        [StringLength(50, ErrorMessage = "寵物名稱不能超過 50 字")]
        public string PetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請選擇膚色")]
        public string SkinColor { get; set; } = string.Empty;

        [Required(ErrorMessage = "請選擇背景")]
        public string BackgroundColor { get; set; } = string.Empty;

        public int Level { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Stamina { get; set; }
        public int Cleanliness { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// 寵物設定視圖模型
    /// </summary>
    public class PetSettingsViewModel
    {
        public List<PetSettingModel> PetSettings { get; set; } = new();
        public List<dynamic> Users { get; set; } = new();
        public int? SelectedUserId { get; set; }
        public PetSettingModel EditPet { get; set; } = new();
    }

    /// <summary>
    /// 寵物清單查詢模型
    /// </summary>
    public class PetListQueryModel
    {
        public int? UserId { get; set; }
        public string? PetName { get; set; }
        public string? SkinColor { get; set; }
        public string? BackgroundColor { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public string? StatusFilter { get; set; } // Healthy, Sick, Hungry, etc.
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 寵物換膚/換背景紀錄模型
    /// </summary>
    public class PetAppearanceChangeModel
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // Skin, Background
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public int PointsCost { get; set; }
        public DateTime ChangeTime { get; set; }
        public string? Reason { get; set; }
    }

    #endregion

    #region 小遊戲系統模型

    /// <summary>
    /// 遊戲規則模型
    /// </summary>
    public class GameRuleModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "請輸入規則名稱")]
        [StringLength(100, ErrorMessage = "規則名稱不能超過 100 字")]
        public string RuleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入每日限制次數")]
        [Range(1, 10, ErrorMessage = "每日限制次數必須在 1-10 之間")]
        public int DailyLimit { get; set; } = 3;

        [Required(ErrorMessage = "請輸入怪物數量")]
        [Range(1, 50, ErrorMessage = "怪物數量必須在 1-50 之間")]
        public int MonsterCount { get; set; }

        [Required(ErrorMessage = "請輸入怪物速度")]
        [Range(0.1, 5.0, ErrorMessage = "怪物速度必須在 0.1-5.0 之間")]
        public double MonsterSpeed { get; set; }

        [Required(ErrorMessage = "請輸入勝利獲得點數")]
        [Range(0, 1000, ErrorMessage = "勝利獲得點數必須在 0-1000 之間")]
        public int WinPoints { get; set; }

        [Required(ErrorMessage = "請輸入勝利獲得經驗")]
        [Range(0, 1000, ErrorMessage = "勝利獲得經驗必須在 0-1000 之間")]
        public int WinExp { get; set; }

        [Required(ErrorMessage = "請輸入失敗獲得點數")]
        [Range(0, 100, ErrorMessage = "失敗獲得點數必須在 0-100 之間")]
        public int LosePoints { get; set; }

        [Required(ErrorMessage = "請輸入失敗獲得經驗")]
        [Range(0, 100, ErrorMessage = "失敗獲得經驗必須在 0-100 之間")]
        public int LoseExp { get; set; }

        [Required(ErrorMessage = "請輸入遊戲時間限制")]
        [Range(30, 600, ErrorMessage = "遊戲時間限制必須在 30-600 秒之間")]
        public int TimeLimit { get; set; }

        public List<GameDifficultySettingModel> DifficultySettings { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 遊戲難度設定模型
    /// </summary>
    public class GameDifficultySettingModel
    {
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public double MonsterSpeed { get; set; }
        public int TimeLimit { get; set; }
        public int WinPoints { get; set; }
        public int WinExp { get; set; }
        public int LosePoints { get; set; }
        public int LoseExp { get; set; }
    }

    /// <summary>
    /// 遊戲規則視圖模型
    /// </summary>
    public class GameRulesViewModel
    {
        public List<GameRuleModel> Rules { get; set; } = new();
        public GameRuleModel NewRule { get; set; } = new();
    }

    /// <summary>
    /// 遊戲紀錄查詢模型
    /// </summary>
    public class GameRecordQueryModel
    {
        public int? UserId { get; set; }
        public int? PetId { get; set; }
        public string? Result { get; set; } // Win, Lose, Abort
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinLevel { get; set; }
        public int? MaxLevel { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 遊戲紀錄詳情模型
    /// </summary>
    public class GameRecordDetailModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public string Result { get; set; } = string.Empty;
        public int ExpGained { get; set; }
        public int PointsGained { get; set; }
        public string CouponGained { get; set; } = string.Empty;
        public int HungerDelta { get; set; }
        public int MoodDelta { get; set; }
        public int StaminaDelta { get; set; }
        public int CleanlinessDelta { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Aborted { get; set; }
        public int Duration => EndTime.HasValue ? (int)(EndTime.Value - StartTime).TotalSeconds : 0;
        public string StatusDisplay => Result switch
        {
            "Win" => "勝利",
            "Lose" => "失敗",
            "Abort" => "中止",
            _ => "進行中"
        };
    }

    /// <summary>
    /// 遊戲統計模型
    /// </summary>
    public class GameStatisticsModel
    {
        public int TotalGames { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public int TotalAborts { get; set; }
        public double WinRate { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int TotalExpAwarded { get; set; }
        public int TotalCouponsAwarded { get; set; }
        public double AverageGameDuration { get; set; }
        public int TodayGames { get; set; }
        public int ThisWeekGames { get; set; }
        public int ThisMonthGames { get; set; }
        public List<DailyGameCount> DailyCounts { get; set; } = new();
    }

    /// <summary>
    /// 每日遊戲統計
    /// </summary>
    public class DailyGameCount
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Aborts { get; set; }
    }

    #endregion

    #region 通用模型

    /// <summary>
    /// 分頁結果模型
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    /// <summary>
    /// 操作結果模型
    /// </summary>
    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// 統計概覽模型
    /// </summary>
    public class StatisticsOverviewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalGames { get; set; }
        public int TotalSignIns { get; set; }
        public int TotalPointsInCirculation { get; set; }
        public int TotalCouponsIssued { get; set; }
        public int TotalEVouchersIssued { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    #endregion
}
