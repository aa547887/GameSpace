using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物顏色選項視圖模型
    /// </summary>
    public class PetColorOptions
    {
        /// <summary>
        /// 顏色選項ID
        /// </summary>
        public int ColorOptionId { get; set; }

        /// <summary>
        /// 顏色名稱
        /// </summary>
        [Required(ErrorMessage = "顏色名稱為必填欄位")]
        [StringLength(50, ErrorMessage = "顏色名稱長度不能超過50個字元")]
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// 顏色代碼（十六進位）
        /// </summary>
        [Required(ErrorMessage = "顏色代碼為必填欄位")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "顏色代碼必須為7個字元（包含#）")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "顏色代碼格式不正確")]
        public string ColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 更換所需點數
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "更換所需點數必須大於等於0")]
        public int PointsCost { get; set; } = 2000;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序順序
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 顏色類型 (Skin/Background)
        /// </summary>
        [StringLength(20)]
        public string ColorType { get; set; } = "Skin";

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物互動獎勵規則
    /// </summary>
    public class PetInteractionBonusRules
    {
        /// <summary>
        /// 規則ID
        /// </summary>
        public int RuleId { get; set; }

        /// <summary>
        /// 互動類型 (Feed/Play/Clean)
        /// </summary>
        [Required(ErrorMessage = "互動類型為必填欄位")]
        [StringLength(20, ErrorMessage = "互動類型長度不能超過20個字元")]
        public string InteractionType { get; set; } = string.Empty;

        /// <summary>
        /// 最小獎勵值
        /// </summary>
        [Range(0, 100, ErrorMessage = "最小獎勵值必須在0-100之間")]
        public int MinBonus { get; set; } = 5;

        /// <summary>
        /// 最大獎勵值
        /// </summary>
        [Range(0, 100, ErrorMessage = "最大獎勵值必須在0-100之間")]
        public int MaxBonus { get; set; } = 15;

        /// <summary>
        /// 所需點數
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "所需點數必須大於等於0")]
        public int PointsCost { get; set; } = 10;

        /// <summary>
        /// 冷卻時間（分鐘）
        /// </summary>
        [Range(0, 1440, ErrorMessage = "冷卻時間必須在0-1440分鐘之間")]
        public int CooldownMinutes { get; set; } = 60;

        /// <summary>
        /// 經驗值獎勵
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "經驗值獎勵必須大於等於0")]
        public int ExpReward { get; set; } = 5;

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物規則讀取模型
    /// </summary>
    public class PetRuleReadModel
    {
        public int RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int LevelUpExpRequired { get; set; }
        public decimal ExpMultiplier { get; set; }
        public int MaxLevel { get; set; }
        public int ColorChangePointCost { get; set; }
        public int BackgroundChangePointCost { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物摘要模型
    /// </summary>
    public class PetSummary
    {
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int NextLevelExp { get; set; }
        public string CurrentSkinColor { get; set; } = string.Empty;
        public string CurrentBackground { get; set; } = string.Empty;
        public int Health { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Cleanliness { get; set; }
        public int Loyalty { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastInteractionTime { get; set; }
        public int TotalColorChanges { get; set; }
        public int TotalBackgroundChanges { get; set; }
    }

    /// <summary>
    /// 寵物膚色變更記錄
    /// </summary>
    public class PetSkinColorChangeLog
    {
        public int LogId { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string OldColor { get; set; } = string.Empty;
        public string NewColor { get; set; } = string.Empty;
        public int PointCost { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 寵物背景變更記錄
    /// </summary>
    public class PetBackgroundColorChangeLog
    {
        public int LogId { get; set; }
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string OldBackground { get; set; } = string.Empty;
        public string NewBackground { get; set; } = string.Empty;
        public int PointCost { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 寵物背景選項
    /// </summary>
    public class PetBackgroundOption
    {
        public int OptionId { get; set; }
        public string BackgroundName { get; set; } = string.Empty;
        public string BackgroundCode { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int PointCost { get; set; }
        public int UnlockLevel { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// 寵物顏色歷史查詢 ViewModel
    /// </summary>
    public class PetColorChangeHistoryViewModel
    {
        public int? PetId { get; set; }
        public string? ChangeType { get; set; } // "Skin" or "Background"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageSize { get; set; } = 20;
        public int PageNumber { get; set; } = 1;
        public List<PetSkinColorChangeLog> SkinChanges { get; set; } = new();
        public List<PetBackgroundColorChangeLog> BackgroundChanges { get; set; } = new();
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }

    /// <summary>
    /// 寵物詳細資料 ViewModel
    /// </summary>
    public class PetDetailsViewModel
    {
        public PetSummary Pet { get; set; } = new();
        public List<PetSkinColorChangeLog> RecentColorChanges { get; set; } = new();
        public List<PetBackgroundColorChangeLog> RecentBackgroundChanges { get; set; } = new();
        public PetRuleReadModel CurrentRules { get; set; } = new();
        public List<PetBackgroundOption> AvailableBackgrounds { get; set; } = new();
    }

    /// <summary>
    /// 寵物更新模型
    /// </summary>
    public class PetUpdateModel
    {
        [Required(ErrorMessage = "寵物ID為必填")]
        public int PetId { get; set; }

        [StringLength(50, ErrorMessage = "寵物名稱長度不可超過 50 字元")]
        public string? PetName { get; set; }

        [Range(1, 100, ErrorMessage = "等級必須在 1-100 之間")]
        public int? Level { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "經驗值必須大於等於 0")]
        public int? Experience { get; set; }

        [Range(0, 100, ErrorMessage = "飽食度必須在 0-100 之間")]
        public int? Hunger { get; set; }

        [Range(0, 100, ErrorMessage = "心情必須在 0-100 之間")]
        public int? Mood { get; set; }

        [Range(0, 100, ErrorMessage = "體力必須在 0-100 之間")]
        public int? Stamina { get; set; }

        [Range(0, 100, ErrorMessage = "乾淨度必須在 0-100 之間")]
        public int? Cleanliness { get; set; }

        [Range(0, 100, ErrorMessage = "健康度必須在 0-100 之間")]
        public int? Health { get; set; }

        [StringLength(10, ErrorMessage = "膚色代碼長度不可超過 10 字元")]
        public string? SkinColor { get; set; }

        [StringLength(20, ErrorMessage = "背景顏色代碼長度不可超過 20 字元")]
        public string? BackgroundColor { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物規則更新模型
    /// </summary>
    public class PetRuleUpdateModel
    {
        [Required(ErrorMessage = "規則ID為必填")]
        public int RuleId { get; set; }

        [Required(ErrorMessage = "規則名稱為必填")]
        [StringLength(100, ErrorMessage = "規則名稱長度不可超過 100 字元")]
        public string RuleName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述長度不可超過 500 字元")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "升級所需經驗值為必填")]
        [Range(1, 100000, ErrorMessage = "升級所需經驗值必須在 1-100000 之間")]
        public int LevelUpExpRequired { get; set; }

        [Range(0.1, 10.0, ErrorMessage = "經驗值倍率必須在 0.1-10.0 之間")]
        public decimal ExpMultiplier { get; set; } = 1.0m;

        [Range(1, 100, ErrorMessage = "最大等級必須在 1-100 之間")]
        public int MaxLevel { get; set; } = 100;

        [Range(0, 100000, ErrorMessage = "膚色變更點數成本必須在 0-100000 之間")]
        public int ColorChangePointCost { get; set; } = 2000;

        [Range(0, 100000, ErrorMessage = "背景變更點數成本必須在 0-100000 之間")]
        public int BackgroundChangePointCost { get; set; } = 3000;

        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物升級規則創建 ViewModel
    /// </summary>
    public class PetLevelUpRuleCreateViewModel
    {
        [Required(ErrorMessage = "等級為必填")]
        [Range(1, 100, ErrorMessage = "等級必須在 1-100 之間")]
        public int Level { get; set; }

        [Required(ErrorMessage = "所需經驗值為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "所需經驗值必須大於等於 0")]
        public int RequiredExp { get; set; }

        [Range(0, 100000, ErrorMessage = "點數獎勵必須在 0-100000 之間")]
        public int PointsReward { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int HealthBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int HungerBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int MoodBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int StaminaBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int CleanlinessBonus { get; set; } = 0;

        [StringLength(200, ErrorMessage = "描述長度不可超過 200 字元")]
        public string? Description { get; set; }

        public bool IsSpecialLevel { get; set; } = false;

        [StringLength(100, ErrorMessage = "特殊獎勵描述長度不可超過 100 字元")]
        public string? SpecialReward { get; set; }
    }

    /// <summary>
    /// 寵物升級規則編輯 ViewModel
    /// </summary>
    public class PetLevelUpRuleEditViewModel
    {
        [Required(ErrorMessage = "規則ID為必填")]
        public int Id { get; set; }

        [Required(ErrorMessage = "等級為必填")]
        [Range(1, 100, ErrorMessage = "等級必須在 1-100 之間")]
        public int Level { get; set; }

        [Required(ErrorMessage = "所需經驗值為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "所需經驗值必須大於等於 0")]
        public int RequiredExp { get; set; }

        [Range(0, 100000, ErrorMessage = "點數獎勵必須在 0-100000 之間")]
        public int PointsReward { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int HealthBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int HungerBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int MoodBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int StaminaBonus { get; set; } = 0;

        [Range(0, 10, ErrorMessage = "屬性加成必須在 0-10 之間")]
        public int CleanlinessBonus { get; set; } = 0;

        [StringLength(200, ErrorMessage = "描述長度不可超過 200 字元")]
        public string? Description { get; set; }

        public bool IsSpecialLevel { get; set; } = false;

        [StringLength(100, ErrorMessage = "特殊獎勵描述長度不可超過 100 字元")]
        public string? SpecialReward { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }
    }
}

