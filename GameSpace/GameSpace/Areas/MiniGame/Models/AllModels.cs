using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 所有模型集合
    /// </summary>
    public class AllModels
    {
        /// <summary>
        /// 優惠券查詢模型
        /// </summary>
        public class CouponQueryModel
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
            /// 頁碼（別名）
            /// </summary>
            public int PageNumber { get; set; } = 1;
            
            /// <summary>
            /// 用戶ID
            /// </summary>
            public int? UserId { get; set; }
            
            /// <summary>
            /// 優惠券類型ID
            /// </summary>
            public int? CouponTypeId { get; set; }
            
            /// <summary>
            /// 狀態
            /// </summary>
            public string? Status { get; set; }
            
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
        /// 寵物規則讀取模型
        /// </summary>
        public class PetRuleReadModel
        {
            /// <summary>
            /// 規則名稱
            /// </summary>
            public string RuleName { get; set; } = string.Empty;
            
            /// <summary>
            /// 升級所需經驗
            /// </summary>
            public int LevelUpExp { get; set; }
            
            /// <summary>
            /// 最大等級
            /// </summary>
            public int MaxLevel { get; set; }
            
            /// <summary>
            /// 換膚所需點數
            /// </summary>
            public int ColorChangeCost { get; set; }
            
            /// <summary>
            /// 換背景所需點數
            /// </summary>
            public int BackgroundChangeCost { get; set; }
            
            /// <summary>
            /// 互動增益設定
            /// </summary>
            public PetInteractionGains InteractionGains { get; set; } = new();
            
            /// <summary>
            /// 可選膚色選項
            /// </summary>
            public List<PetSkinOption> AvailableSkins { get; set; } = new();
            
            /// <summary>
            /// 可選背景選項
            /// </summary>
            public List<PetBackgroundOption> AvailableBackgrounds { get; set; } = new();
            
            /// <summary>
            /// 規則狀態
            /// </summary>
            public string Status { get; set; } = "Active";
            
            /// <summary>
            /// 創建時間
            /// </summary>
            public DateTime CreatedAt { get; set; }
            
            /// <summary>
            /// 更新時間
            /// </summary>
            public DateTime UpdatedAt { get; set; }
        }

        /// <summary>
        /// 遊戲規則讀取模型
        /// </summary>
        public class GameRuleReadModel
        {
            /// <summary>
            /// 規則名稱
            /// </summary>
            public string RuleName { get; set; } = string.Empty;
            
            /// <summary>
            /// 每日限制次數
            /// </summary>
            public int DailyLimit { get; set; }
            
            /// <summary>
            /// 怪物數量
            /// </summary>
            public int MonsterCount { get; set; }
            
            /// <summary>
            /// 怪物速度
            /// </summary>
            public double MonsterSpeed { get; set; }
            
            /// <summary>
            /// 勝利獲得點數
            /// </summary>
            public int WinPoints { get; set; }
            
            /// <summary>
            /// 勝利獲得經驗
            /// </summary>
            public int WinExp { get; set; }
            
            /// <summary>
            /// 失敗獲得點數
            /// </summary>
            public int LosePoints { get; set; }
            
            /// <summary>
            /// 失敗獲得經驗
            /// </summary>
            public int LoseExp { get; set; }
            
            /// <summary>
            /// 遊戲時間限制（秒）
            /// </summary>
            public int TimeLimit { get; set; }
            
            /// <summary>
            /// 難度設定
            /// </summary>
            public List<GameDifficultySetting> DifficultySettings { get; set; } = new();
            
            /// <summary>
            /// 規則狀態
            /// </summary>
            public string Status { get; set; } = "Active";
            
            /// <summary>
            /// 創建時間
            /// </summary>
            public DateTime CreatedAt { get; set; }
            
            /// <summary>
            /// 更新時間
            /// </summary>
            public DateTime UpdatedAt { get; set; }
        }

        /// <summary>
        /// 簽到規則更新模型
        /// </summary>
        public class SignInRuleUpdateModel
        {
            /// <summary>
            /// 規則名稱
            /// </summary>
            [Required(ErrorMessage = "請輸入規則名稱")]
            [StringLength(100, ErrorMessage = "規則名稱不能超過100字")]
            public string RuleName { get; set; } = string.Empty;
            
            /// <summary>
            /// 每日點數
            /// </summary>
            [Required(ErrorMessage = "請輸入每日點數")]
            [Range(1, 1000, ErrorMessage = "每日點數必須在1-1000之間")]
            public int DailyPoints { get; set; }
            
            /// <summary>
            /// 每週獎勵
            /// </summary>
            [Required(ErrorMessage = "請輸入每週獎勵")]
            [Range(0, 5000, ErrorMessage = "每週獎勵必須在0-5000之間")]
            public int WeeklyBonus { get; set; }
            
            /// <summary>
            /// 每月獎勵
            /// </summary>
            [Required(ErrorMessage = "請輸入每月獎勵")]
            [Range(0, 10000, ErrorMessage = "每月獎勵必須在0-10000之間")]
            public int MonthlyBonus { get; set; }
            
            /// <summary>
            /// 連續簽到獎勵
            /// </summary>
            public List<SignInStreakBonus> StreakBonuses { get; set; } = new();
            
            /// <summary>
            /// 特殊日期獎勵
            /// </summary>
            public List<SignInSpecialDate> SpecialDates { get; set; } = new();
            
            /// <summary>
            /// 規則描述
            /// </summary>
            [StringLength(500, ErrorMessage = "規則描述不能超過500字")]
            public string? Description { get; set; }
        }

        /// <summary>
        /// 寵物規則更新模型
        /// </summary>
        public class PetRuleUpdateModel
        {
            /// <summary>
            /// 規則名稱
            /// </summary>
            [Required(ErrorMessage = "請輸入規則名稱")]
            [StringLength(100, ErrorMessage = "規則名稱不能超過100字")]
            public string RuleName { get; set; } = string.Empty;
            
            /// <summary>
            /// 升級所需經驗
            /// </summary>
            [Required(ErrorMessage = "請輸入升級所需經驗")]
            [Range(100, 10000, ErrorMessage = "升級所需經驗必須在100-10000之間")]
            public int LevelUpExp { get; set; }
            
            /// <summary>
            /// 最大等級
            /// </summary>
            [Required(ErrorMessage = "請輸入最大等級")]
            [Range(1, 100, ErrorMessage = "最大等級必須在1-100之間")]
            public int MaxLevel { get; set; }
            
            /// <summary>
            /// 換膚所需點數
            /// </summary>
            [Required(ErrorMessage = "請輸入換膚所需點數")]
            [Range(0, 10000, ErrorMessage = "換膚所需點數必須在0-10000之間")]
            public int ColorChangeCost { get; set; }
            
            /// <summary>
            /// 換背景所需點數
            /// </summary>
            [Required(ErrorMessage = "請輸入換背景所需點數")]
            [Range(0, 10000, ErrorMessage = "換背景所需點數必須在0-10000之間")]
            public int BackgroundChangeCost { get; set; }
            
            /// <summary>
            /// 互動增益設定
            /// </summary>
            public PetInteractionGains InteractionGains { get; set; } = new();
            
            /// <summary>
            /// 可選膚色選項
            /// </summary>
            public List<PetSkinOption> AvailableSkins { get; set; } = new();
            
            /// <summary>
            /// 可選背景選項
            /// </summary>
            public List<PetBackgroundOption> AvailableBackgrounds { get; set; } = new();
            
            /// <summary>
            /// 規則描述
            /// </summary>
            [StringLength(500, ErrorMessage = "規則描述不能超過500字")]
            public string? Description { get; set; }
        }

        /// <summary>
        /// 遊戲規則更新模型
        /// </summary>
        public class GameRuleUpdateModel
        {
            /// <summary>
            /// 規則名稱
            /// </summary>
            [Required(ErrorMessage = "請輸入規則名稱")]
            [StringLength(100, ErrorMessage = "規則名稱不能超過100字")]
            public string RuleName { get; set; } = string.Empty;
            
            /// <summary>
            /// 每日限制次數
            /// </summary>
            [Required(ErrorMessage = "請輸入每日限制次數")]
            [Range(1, 10, ErrorMessage = "每日限制次數必須在1-10之間")]
            public int DailyLimit { get; set; }
            
            /// <summary>
            /// 怪物數量
            /// </summary>
            [Required(ErrorMessage = "請輸入怪物數量")]
            [Range(1, 50, ErrorMessage = "怪物數量必須在1-50之間")]
            public int MonsterCount { get; set; }
            
            /// <summary>
            /// 怪物速度
            /// </summary>
            [Required(ErrorMessage = "請輸入怪物速度")]
            [Range(0.1, 5.0, ErrorMessage = "怪物速度必須在0.1-5.0之間")]
            public double MonsterSpeed { get; set; }
            
            /// <summary>
            /// 勝利獲得點數
            /// </summary>
            [Required(ErrorMessage = "請輸入勝利獲得點數")]
            [Range(0, 1000, ErrorMessage = "勝利獲得點數必須在0-1000之間")]
            public int WinPoints { get; set; }
            
            /// <summary>
            /// 勝利獲得經驗
            /// </summary>
            [Required(ErrorMessage = "請輸入勝利獲得經驗")]
            [Range(0, 500, ErrorMessage = "勝利獲得經驗必須在0-500之間")]
            public int WinExp { get; set; }
            
            /// <summary>
            /// 失敗獲得點數
            /// </summary>
            [Required(ErrorMessage = "請輸入失敗獲得點數")]
            [Range(0, 500, ErrorMessage = "失敗獲得點數必須在0-500之間")]
            public int LosePoints { get; set; }
            
            /// <summary>
            /// 失敗獲得經驗
            /// </summary>
            [Required(ErrorMessage = "請輸入失敗獲得經驗")]
            [Range(0, 250, ErrorMessage = "失敗獲得經驗必須在0-250之間")]
            public int LoseExp { get; set; }
            
            /// <summary>
            /// 遊戲時間限制（秒）
            /// </summary>
            [Required(ErrorMessage = "請輸入遊戲時間限制")]
            [Range(30, 600, ErrorMessage = "遊戲時間限制必須在30-600秒之間")]
            public int TimeLimit { get; set; }
            
            /// <summary>
            /// 難度設定
            /// </summary>
            public List<GameDifficultySetting> DifficultySettings { get; set; } = new();
            
            /// <summary>
            /// 規則描述
            /// </summary>
            [StringLength(500, ErrorMessage = "規則描述不能超過500字")]
            public string? Description { get; set; }
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
            /// 每日限制次數
            /// </summary>
            [Required(ErrorMessage = "請輸入每日限制次數")]
            [Range(1, 10, ErrorMessage = "每日限制次數必須在1-10之間")]
            public int DailyLimit { get; set; }
            
            /// <summary>
            /// 怪物數量
            /// </summary>
            [Required(ErrorMessage = "請輸入怪物數量")]
            [Range(1, 50, ErrorMessage = "怪物數量必須在1-50之間")]
            public int MonsterCount { get; set; }
            
            /// <summary>
            /// 怪物速度
            /// </summary>
            [Required(ErrorMessage = "請輸入怪物速度")]
            [Range(0.1, 5.0, ErrorMessage = "怪物速度必須在0.1-5.0之間")]
            public double MonsterSpeed { get; set; }
            
            /// <summary>
            /// 勝利獲得點數
            /// </summary>
            [Required(ErrorMessage = "請輸入勝利獲得點數")]
            [Range(0, 1000, ErrorMessage = "勝利獲得點數必須在0-1000之間")]
            public int WinPoints { get; set; }
            
            /// <summary>
            /// 勝利獲得經驗
            /// </summary>
            [Required(ErrorMessage = "請輸入勝利獲得經驗")]
            [Range(0, 500, ErrorMessage = "勝利獲得經驗必須在0-500之間")]
            public int WinExp { get; set; }
            
            /// <summary>
            /// 失敗獲得點數
            /// </summary>
            [Required(ErrorMessage = "請輸入失敗獲得點數")]
            [Range(0, 500, ErrorMessage = "失敗獲得點數必須在0-500之間")]
            public int LosePoints { get; set; }
            
            /// <summary>
            /// 失敗獲得經驗
            /// </summary>
            [Required(ErrorMessage = "請輸入失敗獲得經驗")]
            [Range(0, 250, ErrorMessage = "失敗獲得經驗必須在0-250之間")]
            public int LoseExp { get; set; }
            
            /// <summary>
            /// 遊戲時間限制（秒）
            /// </summary>
            [Required(ErrorMessage = "請輸入遊戲時間限制")]
            [Range(30, 600, ErrorMessage = "遊戲時間限制必須在30-600秒之間")]
            public int TimeLimit { get; set; }
            
            /// <summary>
            /// 難度設定
            /// </summary>
            public List<GameDifficultySetting> DifficultySettings { get; set; } = new();
            
            /// <summary>
            /// 規則描述
            /// </summary>
            [StringLength(500, ErrorMessage = "規則描述不能超過500字")]
            public string? Description { get; set; }
        }

        /// <summary>
        /// 寵物互動增益設定
        /// </summary>
        public class PetInteractionGains
        {
            /// <summary>
            /// 餵食經驗增益
            /// </summary>
            public int FeedExpGain { get; set; }
            
            /// <summary>
            /// 洗澡經驗增益
            /// </summary>
            public int BathExpGain { get; set; }
            
            /// <summary>
            /// 玩耍經驗增益
            /// </summary>
            public int PlayExpGain { get; set; }
            
            /// <summary>
            /// 哄睡經驗增益
            /// </summary>
            public int SleepExpGain { get; set; }
            
            /// <summary>
            /// 餵食點數消耗
            /// </summary>
            public int FeedPointCost { get; set; }
            
            /// <summary>
            /// 洗澡點數消耗
            /// </summary>
            public int BathPointCost { get; set; }
            
            /// <summary>
            /// 玩耍點數消耗
            /// </summary>
            public int PlayPointCost { get; set; }
            
            /// <summary>
            /// 哄睡點數消耗
            /// </summary>
            public int SleepPointCost { get; set; }
        }

        /// <summary>
        /// 寵物膚色選項
        /// </summary>
        public class PetSkinOption
        {
            /// <summary>
            /// 膚色ID
            /// </summary>
            public int Id { get; set; }
            
            /// <summary>
            /// 膚色名稱
            /// </summary>
            public string Name { get; set; } = string.Empty;
            
            /// <summary>
            /// 膚色代碼
            /// </summary>
            public string ColorCode { get; set; } = string.Empty;
            
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
        }

        /// <summary>
        /// 寵物背景選項
        /// </summary>
        public class PetBackgroundOption
        {
            /// <summary>
            /// 背景ID
            /// </summary>
            public int Id { get; set; }
            
            /// <summary>
            /// 背景名稱
            /// </summary>
            public string Name { get; set; } = string.Empty;
            
            /// <summary>
            /// 背景圖片路徑
            /// </summary>
            public string ImagePath { get; set; } = string.Empty;
            
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
        }

        /// <summary>
        /// 遊戲難度設定
        /// </summary>
        public class GameDifficultySetting
        {
            /// <summary>
            /// 難度等級
            /// </summary>
            public int Level { get; set; }
            
            /// <summary>
            /// 難度名稱
            /// </summary>
            public string Name { get; set; } = string.Empty;
            
            /// <summary>
            /// 怪物數量
            /// </summary>
            public int MonsterCount { get; set; }
            
            /// <summary>
            /// 怪物速度
            /// </summary>
            public double MonsterSpeed { get; set; }
            
            /// <summary>
            /// 獎勵倍率
            /// </summary>
            public double RewardMultiplier { get; set; }
            
            /// <summary>
            /// 是否解鎖
            /// </summary>
            public bool IsUnlocked { get; set; }
            
            /// <summary>
            /// 解鎖條件
            /// </summary>
            public string UnlockCondition { get; set; } = string.Empty;
        }

        /// <summary>
        /// 簽到連續獎勵
        /// </summary>
        public class SignInStreakBonus
        {
            /// <summary>
            /// 連續天數
            /// </summary>
            public int StreakDays { get; set; }
            
            /// <summary>
            /// 獎勵點數
            /// </summary>
            public int BonusPoints { get; set; }
            
            /// <summary>
            /// 獎勵經驗
            /// </summary>
            public int BonusExp { get; set; }
            
            /// <summary>
            /// 獎勵描述
            /// </summary>
            public string Description { get; set; } = string.Empty;
        }

        /// <summary>
        /// 簽到特殊日期
        /// </summary>
        public class SignInSpecialDate
        {
            /// <summary>
            /// 特殊日期
            /// </summary>
            public DateTime SpecialDate { get; set; }
            
            /// <summary>
            /// 特殊獎勵點數
            /// </summary>
            public int SpecialPoints { get; set; }
            
            /// <summary>
            /// 特殊獎勵經驗
            /// </summary>
            public int SpecialExp { get; set; }
            
            /// <summary>
            /// 特殊獎勵描述
            /// </summary>
            public string Description { get; set; } = string.Empty;
        }
    }
}
