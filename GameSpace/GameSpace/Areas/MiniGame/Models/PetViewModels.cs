using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物視圖模型集合
    /// </summary>
    public class PetViewModels
    {
        /// <summary>
        /// 寵物首頁視圖模型
        /// </summary>
        public class PetIndexViewModel
        {
            /// <summary>
            /// 寵物列表
            /// </summary>
            public List<GameSpace.Models.Pet> Pets { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 用戶寵物
            /// </summary>
            public GameSpace.Models.Pet? UserPet { get; set; }
            
            /// <summary>
            /// 可用互動選項
            /// </summary>
            public List<string> AvailableInteractions { get; set; } = new();
            
            /// <summary>
            /// 寵物狀態摘要
            /// </summary>
            public PetStatusSummary StatusSummary { get; set; } = new();
            
            /// <summary>
            /// 寵物統計
            /// </summary>
            public PetStatistics Statistics { get; set; } = new();
        }

        /// <summary>
        /// 寵物詳情視圖模型
        /// </summary>
        public class PetDetailsViewModel
        {
            /// <summary>
            /// 寵物資料
            /// </summary>
            public GameSpace.Models.Pet Pet { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 寵物詳情
            /// </summary>
            public PetDetails Details { get; set; } = new();
            
            /// <summary>
            /// 可用互動
            /// </summary>
            public List<PetInteraction> AvailableInteractions { get; set; } = new();
            
            /// <summary>
            /// 互動歷史
            /// </summary>
            public List<PetInteractionHistory> InteractionHistory { get; set; } = new();
        }

        /// <summary>
        /// 寵物互動視圖模型
        /// </summary>
        public class PetInteractionViewModel
        {
            /// <summary>
            /// 寵物資料
            /// </summary>
            public GameSpace.Models.Pet Pet { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 互動類型
            /// </summary>
            [Required(ErrorMessage = "請選擇互動類型")]
            public string InteractionType { get; set; } = string.Empty;
            
            /// <summary>
            /// 互動結果
            /// </summary>
            public PetInteractionResult Result { get; set; } = new();
            
            /// <summary>
            /// 可用互動選項
            /// </summary>
            public List<PetInteractionOption> AvailableOptions { get; set; } = new();
        }

        /// <summary>
        /// 寵物換膚視圖模型
        /// </summary>
        public class PetSkinChangeViewModel
        {
            /// <summary>
            /// 寵物資料
            /// </summary>
            public GameSpace.Models.Pet Pet { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 可用膚色
            /// </summary>
            public List<PetSkinOption> AvailableSkins { get; set; } = new();
            
            /// <summary>
            /// 當前膚色
            /// </summary>
            public string CurrentSkin { get; set; } = string.Empty;
            
            /// <summary>
            /// 選中膚色
            /// </summary>
            [Required(ErrorMessage = "請選擇膚色")]
            public string SelectedSkin { get; set; } = string.Empty;
        }

        /// <summary>
        /// 寵物換背景視圖模型
        /// </summary>
        public class PetBackgroundChangeViewModel
        {
            /// <summary>
            /// 寵物資料
            /// </summary>
            public GameSpace.Models.Pet Pet { get; set; } = new();
            
            /// <summary>
            /// 用戶錢包
            /// </summary>
            public UserWallet Wallet { get; set; } = new();
            
            /// <summary>
            /// 發送者ID
            /// </summary>
            public int SenderID { get; set; }
            
            /// <summary>
            /// 可用背景
            /// </summary>
            public List<PetBackgroundOption> AvailableBackgrounds { get; set; } = new();
            
            /// <summary>
            /// 當前背景
            /// </summary>
            public string CurrentBackground { get; set; } = string.Empty;
            
            /// <summary>
            /// 選中背景
            /// </summary>
            [Required(ErrorMessage = "請選擇背景")]
            public string SelectedBackground { get; set; } = string.Empty;
        }
    }

    /// <summary>
    /// 管理員寵物管理視圖模型
    /// </summary>
    public class AdminPetManagementViewModel
    {
        /// <summary>
        /// 寵物列表
        /// </summary>
        public List<Pet> Pets { get; set; } = new();
        
        /// <summary>
        /// 用戶列表
        /// </summary>
        public List<User> Users { get; set; } = new();
        
        /// <summary>
        /// 查詢條件
        /// </summary>
        public PetQueryModel Query { get; set; } = new();
        
        /// <summary>
        /// 統計數據
        /// </summary>
        public PetStatisticsReadModel Statistics { get; set; } = new();
        
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
        public PagedResult<Pet> PagedResults { get; set; } = new();
    }

    /// <summary>
    /// 寵物規則管理視圖模型
    /// </summary>
    public class PetRuleManagementViewModel
    {
        /// <summary>
        /// 當前規則
        /// </summary>
        public PetRuleReadModel CurrentRule { get; set; } = new();
        
        /// <summary>
        /// 更新模型
        /// </summary>
        public PetRulesUpdateModel UpdateModel { get; set; } = new();
        
        /// <summary>
        /// 規則歷史
        /// </summary>
        public List<PetRule> RuleHistory { get; set; } = new();
        
        /// <summary>
        /// 規則統計
        /// </summary>
        public PetRuleStatistics RuleStatistics { get; set; } = new();
    }

    /// <summary>
    /// 寵物查詢模型
    /// </summary>
    public class PetQueryModel
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
        /// 寵物名稱
        /// </summary>
        public string? PetName { get; set; }
        
        /// <summary>
        /// 膚色
        /// </summary>
        public string? SkinColor { get; set; }
        
        /// <summary>
        /// 背景
        /// </summary>
        public string? Background { get; set; }
        
        /// <summary>
        /// 最小等級
        /// </summary>
        public int? MinLevel { get; set; }
        
        /// <summary>
        /// 最大等級
        /// </summary>
        public int? MaxLevel { get; set; }
        
        /// <summary>
        /// 最小經驗
        /// </summary>
        public int? MinExperience { get; set; }
        
        /// <summary>
        /// 最大經驗
        /// </summary>
        public int? MaxExperience { get; set; }
        
        /// <summary>
        /// 排序方式
        /// </summary>
        public string SortBy { get; set; } = "Name";
        
        /// <summary>
        /// 排序方向
        /// </summary>
        public string SortDirection { get; set; } = "asc";
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
    /// 寵物狀態摘要
    /// </summary>
    public class PetStatusSummary
    {
        /// <summary>
        /// 寵物ID
        /// </summary>
        public int PetId { get; set; }
        
        /// <summary>
        /// 寵物名稱
        /// </summary>
        public string PetName { get; set; } = string.Empty;
        
        /// <summary>
        /// 等級
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// 經驗值
        /// </summary>
        public int Experience { get; set; }
        
        /// <summary>
        /// 生命值
        /// </summary>
        public int Health { get; set; }
        
        /// <summary>
        /// 能量值
        /// </summary>
        public int Energy { get; set; }
        
        /// <summary>
        /// 快樂值
        /// </summary>
        public int Happiness { get; set; }
        
        /// <summary>
        /// 飽食度
        /// </summary>
        public int Hunger { get; set; }
        
        /// <summary>
        /// 清潔度
        /// </summary>
        public int Cleanliness { get; set; }
        
        /// <summary>
        /// 狀態評級
        /// </summary>
        public string StatusRating { get; set; } = string.Empty;
        
        /// <summary>
        /// 狀態顏色
        /// </summary>
        public string StatusColor { get; set; } = string.Empty;
        
        /// <summary>
        /// 生命值百分比
        /// </summary>
        public double HealthPercentage => Health / 100.0 * 100;
        
        /// <summary>
        /// 能量值百分比
        /// </summary>
        public double EnergyPercentage => Energy / 100.0 * 100;
        
        /// <summary>
        /// 快樂值百分比
        /// </summary>
        public double HappinessPercentage => Happiness / 100.0 * 100;
        
        /// <summary>
        /// 飽食度百分比
        /// </summary>
        public double HungerPercentage => Hunger / 100.0 * 100;
        
        /// <summary>
        /// 清潔度百分比
        /// </summary>
        public double CleanlinessPercentage => Cleanliness / 100.0 * 100;
    }

    /// <summary>
    /// 寵物詳情
    /// </summary>
    public class PetDetails
    {
        /// <summary>
        /// 寵物ID
        /// </summary>
        public int PetId { get; set; }
        
        /// <summary>
        /// 寵物名稱
        /// </summary>
        public string PetName { get; set; } = string.Empty;
        
        /// <summary>
        /// 膚色
        /// </summary>
        public string SkinColor { get; set; } = string.Empty;
        
        /// <summary>
        /// 背景
        /// </summary>
        public string Background { get; set; } = string.Empty;
        
        /// <summary>
        /// 等級
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// 經驗值
        /// </summary>
        public int Experience { get; set; }
        
        /// <summary>
        /// 生命值
        /// </summary>
        public int Health { get; set; }
        
        /// <summary>
        /// 能量值
        /// </summary>
        public int Energy { get; set; }
        
        /// <summary>
        /// 快樂值
        /// </summary>
        public int Happiness { get; set; }
        
        /// <summary>
        /// 飽食度
        /// </summary>
        public int Hunger { get; set; }
        
        /// <summary>
        /// 清潔度
        /// </summary>
        public int Cleanliness { get; set; }
        
        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime LastUpdated { get; set; }
        
        /// <summary>
        /// 總互動次數
        /// </summary>
        public int TotalInteractions { get; set; }
        
        /// <summary>
        /// 總換膚次數
        /// </summary>
        public int TotalSkinChanges { get; set; }
        
        /// <summary>
        /// 總換背景次數
        /// </summary>
        public int TotalBackgroundChanges { get; set; }
    }

    /// <summary>
    /// 寵物互動
    /// </summary>
    public class PetInteraction
    {
        /// <summary>
        /// 互動ID
        /// </summary>
        public int InteractionId { get; set; }
        
        /// <summary>
        /// 互動類型
        /// </summary>
        public string InteractionType { get; set; } = string.Empty;
        
        /// <summary>
        /// 互動名稱
        /// </summary>
        public string InteractionName { get; set; } = string.Empty;
        
        /// <summary>
        /// 互動描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 所需能量
        /// </summary>
        public int RequiredEnergy { get; set; }
        
        /// <summary>
        /// 所需點數
        /// </summary>
        public int RequiredPoints { get; set; }
        
        /// <summary>
        /// 經驗獎勵
        /// </summary>
        public int ExperienceReward { get; set; }
        
        /// <summary>
        /// 快樂值獎勵
        /// </summary>
        public int HappinessReward { get; set; }
        
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsAvailable { get; set; }
        
        /// <summary>
        /// 冷卻時間（分鐘）
        /// </summary>
        public int CooldownMinutes { get; set; }
        
        /// <summary>
        /// 互動圖標
        /// </summary>
        public string Icon { get; set; } = string.Empty;
    }

    /// <summary>
    /// 寵物互動選項
    /// </summary>
    public class PetInteractionOption
    {
        /// <summary>
        /// 選項ID
        /// </summary>
        public int OptionId { get; set; }
        
        /// <summary>
        /// 選項名稱
        /// </summary>
        public string OptionName { get; set; } = string.Empty;
        
        /// <summary>
        /// 選項描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 所需能量
        /// </summary>
        public int RequiredEnergy { get; set; }
        
        /// <summary>
        /// 所需點數
        /// </summary>
        public int RequiredPoints { get; set; }
        
        /// <summary>
        /// 經驗獎勵
        /// </summary>
        public int ExperienceReward { get; set; }
        
        /// <summary>
        /// 快樂值獎勵
        /// </summary>
        public int HappinessReward { get; set; }
        
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsAvailable { get; set; }
        
        /// <summary>
        /// 冷卻時間（分鐘）
        /// </summary>
        public int CooldownMinutes { get; set; }
    }

    /// <summary>
    /// 寵物互動結果
    /// </summary>
    public class PetInteractionResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// 互動類型
        /// </summary>
        public string InteractionType { get; set; } = string.Empty;
        
        /// <summary>
        /// 經驗獎勵
        /// </summary>
        public int ExperienceReward { get; set; }
        
        /// <summary>
        /// 快樂值獎勵
        /// </summary>
        public int HappinessReward { get; set; }
        
        /// <summary>
        /// 消耗能量
        /// </summary>
        public int EnergyConsumed { get; set; }
        
        /// <summary>
        /// 消耗點數
        /// </summary>
        public int PointsConsumed { get; set; }
        
        /// <summary>
        /// 結果訊息
        /// </summary>
        public string ResultMessage { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否升級
        /// </summary>
        public bool IsLevelUp { get; set; }
        
        /// <summary>
        /// 新等級
        /// </summary>
        public int NewLevel { get; set; }
        
        /// <summary>
        /// 新經驗值
        /// </summary>
        public int NewExperience { get; set; }
    }

    /// <summary>
    /// 寵物互動歷史
    /// </summary>
    public class PetInteractionHistory
    {
        /// <summary>
        /// 歷史ID
        /// </summary>
        public int HistoryId { get; set; }
        
        /// <summary>
        /// 寵物ID
        /// </summary>
        public int PetId { get; set; }
        
        /// <summary>
        /// 互動類型
        /// </summary>
        public string InteractionType { get; set; } = string.Empty;
        
        /// <summary>
        /// 互動時間
        /// </summary>
        public DateTime InteractionTime { get; set; }
        
        /// <summary>
        /// 經驗獎勵
        /// </summary>
        public int ExperienceReward { get; set; }
        
        /// <summary>
        /// 快樂值獎勵
        /// </summary>
        public int HappinessReward { get; set; }
        
        /// <summary>
        /// 消耗能量
        /// </summary>
        public int EnergyConsumed { get; set; }
        
        /// <summary>
        /// 消耗點數
        /// </summary>
        public int PointsConsumed { get; set; }
        
        /// <summary>
        /// 互動結果
        /// </summary>
        public string Result { get; set; } = string.Empty;
    }

    /// <summary>
    /// 寵物膚色選項
    /// </summary>
    public class PetSkinOption
    {
        /// <summary>
        /// 膚色ID
        /// </summary>
        public int SkinId { get; set; }
        
        /// <summary>
        /// 膚色名稱
        /// </summary>
        public string SkinName { get; set; } = string.Empty;
        
        /// <summary>
        /// 膚色代碼
        /// </summary>
        public string SkinCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 所需點數
        /// </summary>
        public int RequiredPoints { get; set; }
        
        /// <summary>
        /// 是否解鎖
        /// </summary>
        public bool IsUnlocked { get; set; }
        
        /// <summary>
        /// 解鎖條件
        /// </summary>
        public string UnlockCondition { get; set; } = string.Empty;
        
        /// <summary>
        /// 膚色預覽圖
        /// </summary>
        public string PreviewImage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 寵物背景選項
    /// </summary>
    public class PetBackgroundOption
    {
        /// <summary>
        /// 背景ID
        /// </summary>
        public int BackgroundId { get; set; }
        
        /// <summary>
        /// 背景名稱
        /// </summary>
        public string BackgroundName { get; set; } = string.Empty;
        
        /// <summary>
        /// 背景代碼
        /// </summary>
        public string BackgroundCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 所需點數
        /// </summary>
        public int RequiredPoints { get; set; }
        
        /// <summary>
        /// 是否解鎖
        /// </summary>
        public bool IsUnlocked { get; set; }
        
        /// <summary>
        /// 解鎖條件
        /// </summary>
        public string UnlockCondition { get; set; } = string.Empty;
        
        /// <summary>
        /// 背景預覽圖
        /// </summary>
        public string PreviewImage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 寵物統計
    /// </summary>
    public class PetStatistics
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
        /// 總互動次數
        /// </summary>
        public int TotalInteractions { get; set; }
        
        /// <summary>
        /// 總換膚次數
        /// </summary>
        public int TotalSkinChanges { get; set; }
        
        /// <summary>
        /// 總換背景次數
        /// </summary>
        public int TotalBackgroundChanges { get; set; }
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
    /// 寵物規則統計
    /// </summary>
    public class PetRuleStatistics
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
}
