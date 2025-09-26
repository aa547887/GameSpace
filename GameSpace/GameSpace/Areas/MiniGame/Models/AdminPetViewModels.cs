using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理員寵物視圖模型集合
    /// </summary>
    public class AdminPetViewModels
    {
        /// <summary>
        /// 管理員寵物首頁視圖模型
        /// </summary>
        public class AdminPetIndexViewModel
        {
            /// <summary>
            /// 寵物列表
            /// </summary>
            public List<GameSpace.Models.Pet> Pets { get; set; } = new();
            
            /// <summary>
            /// 側邊欄視圖模型
            /// </summary>
            public SidebarViewModel Sidebar { get; set; } = new();
            
            /// <summary>
            /// 寵物摘要
            /// </summary>
            public PetSummary PetSummary { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public PetQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<GameSpace.Models.Pet> PagedResults { get; set; } = new();
            
            /// <summary>
            /// 統計數據
            /// </summary>
            public PetStatisticsReadModel Statistics { get; set; } = new();
        }

        /// <summary>
        /// 管理員寵物規則視圖模型
        /// </summary>
        public class AdminPetRulesViewModel
        {
            /// <summary>
            /// 寵物規則
            /// </summary>
            public PetRuleReadModel PetRule { get; set; } = new();
            
            /// <summary>
            /// 側邊欄視圖模型
            /// </summary>
            public SidebarViewModel Sidebar { get; set; } = new();
            
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
        /// 管理員寵物詳情視圖模型
        /// </summary>
        public class AdminPetDetailsViewModel
        {
            /// <summary>
            /// 寵物資料
            /// </summary>
            public GameSpace.Models.Pet Pet { get; set; } = new();
            
            /// <summary>
            /// 側邊欄視圖模型
            /// </summary>
            public SidebarViewModel Sidebar { get; set; } = new();
            
            /// <summary>
            /// 寵物詳情
            /// </summary>
            public PetDetails Details { get; set; } = new();
            
            /// <summary>
            /// 互動歷史
            /// </summary>
            public List<PetInteractionHistory> InteractionHistory { get; set; } = new();
            
            /// <summary>
            /// 換膚歷史
            /// </summary>
            public List<PetSkinColorChangeLog> SkinChangeHistory { get; set; } = new();
            
            /// <summary>
            /// 換背景歷史
            /// </summary>
            public List<PetBackgroundColorChangeLog> BackgroundChangeHistory { get; set; } = new();
        }

        /// <summary>
        /// 管理員寵物編輯視圖模型
        /// </summary>
        public class AdminPetEditViewModel
        {
            /// <summary>
            /// 寵物資料
            /// </summary>
            public GameSpace.Models.Pet Pet { get; set; } = new();
            
            /// <summary>
            /// 側邊欄視圖模型
            /// </summary>
            public SidebarViewModel Sidebar { get; set; } = new();
            
            /// <summary>
            /// 編輯模型
            /// </summary>
            public PetEditModel EditModel { get; set; } = new();
            
            /// <summary>
            /// 可用膚色選項
            /// </summary>
            public List<PetSkinOption> AvailableSkins { get; set; } = new();
            
            /// <summary>
            /// 可用背景選項
            /// </summary>
            public List<PetBackgroundOption> AvailableBackgrounds { get; set; } = new();
        }

        /// <summary>
        /// 管理員寵物膚色變更記錄視圖模型
        /// </summary>
        public class AdminPetSkinColorChangeLogViewModel
        {
            /// <summary>
            /// 膚色變更記錄列表
            /// </summary>
            public List<PetSkinColorChangeLog> SkinColorChangeLogs { get; set; } = new();
            
            /// <summary>
            /// 側邊欄視圖模型
            /// </summary>
            public SidebarViewModel Sidebar { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public PetQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<PetSkinColorChangeLog> PagedResults { get; set; } = new();
            
            /// <summary>
            /// 統計數據
            /// </summary>
            public PetSkinChangeStatistics Statistics { get; set; } = new();
        }

        /// <summary>
        /// 管理員寵物背景變更記錄視圖模型
        /// </summary>
        public class AdminPetBackgroundColorChangeLogViewModel
        {
            /// <summary>
            /// 背景變更記錄列表
            /// </summary>
            public List<PetBackgroundColorChangeLog> BackgroundColorChangeLogs { get; set; } = new();
            
            /// <summary>
            /// 側邊欄視圖模型
            /// </summary>
            public SidebarViewModel Sidebar { get; set; } = new();
            
            /// <summary>
            /// 查詢條件
            /// </summary>
            public PetQueryModel Query { get; set; } = new();
            
            /// <summary>
            /// 分頁結果
            /// </summary>
            public PagedResult<PetBackgroundColorChangeLog> PagedResults { get; set; } = new();
            
            /// <summary>
            /// 統計數據
            /// </summary>
            public PetBackgroundChangeStatistics Statistics { get; set; } = new();
        }

        /// <summary>
        /// 管理員寵物統計視圖模型
        /// </summary>
        public class AdminPetStatisticsViewModel
        {
            /// <summary>
            /// 側邊欄視圖模型
            /// </summary>
            public SidebarViewModel Sidebar { get; set; } = new();
            
            /// <summary>
            /// 統計數據
            /// </summary>
            public PetStatisticsReadModel Statistics { get; set; } = new();
            
            /// <summary>
            /// 圖表數據
            /// </summary>
            public List<ChartData> ChartData { get; set; } = new();
            
            /// <summary>
            /// 時間範圍
            /// </summary>
            public string TimeRange { get; set; } = "30days";
        }
    }

    /// <summary>
    /// 寵物摘要
    /// </summary>
    public class PetSummary
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
    /// 寵物編輯模型
    /// </summary>
    public class PetEditModel
    {
        /// <summary>
        /// 寵物ID
        /// </summary>
        public int PetId { get; set; }
        
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
        public string SkinColor { get; set; } = string.Empty;
        
        /// <summary>
        /// 背景
        /// </summary>
        [Required(ErrorMessage = "請選擇背景")]
        public string Background { get; set; } = string.Empty;
        
        /// <summary>
        /// 等級
        /// </summary>
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        public int Level { get; set; } = 1;
        
        /// <summary>
        /// 經驗值
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "經驗值不能為負數")]
        public int Experience { get; set; } = 0;
        
        /// <summary>
        /// 生命值
        /// </summary>
        [Range(0, 100, ErrorMessage = "生命值必須在0-100之間")]
        public int Health { get; set; } = 100;
        
        /// <summary>
        /// 能量值
        /// </summary>
        [Range(0, 100, ErrorMessage = "能量值必須在0-100之間")]
        public int Energy { get; set; } = 100;
        
        /// <summary>
        /// 快樂值
        /// </summary>
        [Range(0, 100, ErrorMessage = "快樂值必須在0-100之間")]
        public int Happiness { get; set; } = 100;
        
        /// <summary>
        /// 飽食度
        /// </summary>
        [Range(0, 100, ErrorMessage = "飽食度必須在0-100之間")]
        public int Hunger { get; set; } = 100;
        
        /// <summary>
        /// 清潔度
        /// </summary>
        [Range(0, 100, ErrorMessage = "清潔度必須在0-100之間")]
        public int Cleanliness { get; set; } = 100;
    }

    /// <summary>
    /// 寵物膚色變更統計
    /// </summary>
    public class PetSkinChangeStatistics
    {
        /// <summary>
        /// 總變更次數
        /// </summary>
        public int TotalChanges { get; set; }
        
        /// <summary>
        /// 今日變更次數
        /// </summary>
        public int TodayChanges { get; set; }
        
        /// <summary>
        /// 本月變更次數
        /// </summary>
        public int MonthlyChanges { get; set; }
        
        /// <summary>
        /// 最受歡迎膚色
        /// </summary>
        public string MostPopularSkin { get; set; } = string.Empty;
        
        /// <summary>
        /// 膚色使用統計
        /// </summary>
        public List<PetSkinUsageStatistics> SkinUsageStats { get; set; } = new();
    }

    /// <summary>
    /// 寵物背景變更統計
    /// </summary>
    public class PetBackgroundChangeStatistics
    {
        /// <summary>
        /// 總變更次數
        /// </summary>
        public int TotalChanges { get; set; }
        
        /// <summary>
        /// 今日變更次數
        /// </summary>
        public int TodayChanges { get; set; }
        
        /// <summary>
        /// 本月變更次數
        /// </summary>
        public int MonthlyChanges { get; set; }
        
        /// <summary>
        /// 最受歡迎背景
        /// </summary>
        public string MostPopularBackground { get; set; } = string.Empty;
        
        /// <summary>
        /// 背景使用統計
        /// </summary>
        public List<PetBackgroundUsageStatistics> BackgroundUsageStats { get; set; } = new();
    }

    /// <summary>
    /// 寵物膚色使用統計
    /// </summary>
    public class PetSkinUsageStatistics
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
    /// 寵物背景使用統計
    /// </summary>
    public class PetBackgroundUsageStatistics
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
}
