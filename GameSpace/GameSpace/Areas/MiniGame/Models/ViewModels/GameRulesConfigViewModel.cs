using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 遊戲規則完整設定 ViewModel - 包含關卡設定、冒險結果影響、健康檢查設定
    /// </summary>
    public class GameRulesConfigViewModel
    {
        /// <summary>
        /// 遊戲名稱
        /// </summary>
        public string GameName { get; set; } = "Adventure Game - 冒險遊戲";

        /// <summary>
        /// 遊戲描述
        /// </summary>
        public string? Description { get; set; } = "經典冒險遊戲，通過擊敗怪物來獲得經驗值和點數獎勵";

        /// <summary>
        /// 每日遊戲次數限制
        /// </summary>
        public int DailyPlayLimit { get; set; } = 3;

        /// <summary>
        /// 遊戲是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 總遊戲次數
        /// </summary>
        public int TotalGamesPlayed { get; set; }

        /// <summary>
        /// 今日遊戲次數
        /// </summary>
        public int TodayGamesPlayed { get; set; }

        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// 關卡設定列表（3關）
        /// </summary>
        public List<LevelConfigViewModel> LevelConfigs { get; set; } = new List<LevelConfigViewModel>();

        /// <summary>
        /// 冒險勝利時的寵物狀態變化
        /// </summary>
        public AdventureResultImpact WinImpact { get; set; } = new AdventureResultImpact();

        /// <summary>
        /// 冒險失敗時的寵物狀態變化
        /// </summary>
        public AdventureResultImpact LoseImpact { get; set; } = new AdventureResultImpact();

        /// <summary>
        /// 健康檢查門檻設定
        /// </summary>
        public HealthCheckThreshold HealthCheckThreshold { get; set; } = new HealthCheckThreshold();
    }

    /// <summary>
    /// 關卡設定 ViewModel
    /// </summary>
    public class LevelConfigViewModel
    {
        /// <summary>
        /// 關卡等級
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 怪物數量
        /// </summary>
        public int MonsterCount { get; set; }

        /// <summary>
        /// 移動速度倍率
        /// </summary>
        public decimal SpeedMultiplier { get; set; }

        /// <summary>
        /// 勝利經驗值獎勵
        /// </summary>
        public int ExperienceReward { get; set; }

        /// <summary>
        /// 勝利點數獎勵
        /// </summary>
        public int PointsReward { get; set; }

        /// <summary>
        /// 是否獎勵優惠券（僅第3關）
        /// </summary>
        public bool HasCoupon { get; set; }

        /// <summary>
        /// 優惠券類型代碼（僅第3關）
        /// </summary>
        public string? CouponType { get; set; }

        /// <summary>
        /// 關卡描述
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// 冒險結果影響 - 寵物狀態變化
    /// </summary>
    public class AdventureResultImpact
    {
        /// <summary>
        /// 飽食度變化（負數表示減少）
        /// </summary>
        public int HungerDelta { get; set; }

        /// <summary>
        /// 心情值變化（正數增加，負數減少）
        /// </summary>
        public int MoodDelta { get; set; }

        /// <summary>
        /// 體力值變化（負數表示消耗）
        /// </summary>
        public int StaminaDelta { get; set; }

        /// <summary>
        /// 清潔度變化（負數表示減少）
        /// </summary>
        public int CleanlinessDelta { get; set; }
    }

    /// <summary>
    /// 健康檢查門檻設定 - 用於判斷是否可以開始冒險
    /// </summary>
    public class HealthCheckThreshold
    {
        /// <summary>
        /// 最低飽食度要求
        /// </summary>
        public int MinHunger { get; set; } = 1;

        /// <summary>
        /// 最低心情值要求
        /// </summary>
        public int MinMood { get; set; } = 1;

        /// <summary>
        /// 最低體力值要求
        /// </summary>
        public int MinStamina { get; set; } = 1;

        /// <summary>
        /// 最低清潔度要求
        /// </summary>
        public int MinCleanliness { get; set; } = 1;

        /// <summary>
        /// 最低健康值要求
        /// </summary>
        public int MinHealth { get; set; } = 1;

        /// <summary>
        /// 提示訊息
        /// </summary>
        public string Message { get; set; } = "寵物任一屬性值為 0 時無法開始冒險，請先透過互動恢復寵物狀態";
    }

    /// <summary>
    /// 遊戲規則更新輸入模型
    /// </summary>
    public class GameRulesConfigInputModel
    {
        /// <summary>
        /// 每日遊戲次數限制
        /// </summary>
        [Required(ErrorMessage = "每日遊戲次數為必填")]
        [Range(1, 100, ErrorMessage = "每日次數必須在1-100之間")]
        public int DailyPlayLimit { get; set; } = 3;

        /// <summary>
        /// 遊戲是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 關卡設定更新輸入模型
    /// </summary>
    public class LevelConfigInputModel
    {
        /// <summary>
        /// 關卡等級
        /// </summary>
        [Required(ErrorMessage = "關卡等級為必填")]
        [Range(1, 3, ErrorMessage = "關卡等級必須在1-3之間")]
        public int Level { get; set; }

        /// <summary>
        /// 怪物數量
        /// </summary>
        [Required(ErrorMessage = "怪物數量為必填")]
        [Range(1, 50, ErrorMessage = "怪物數量必須在1-50之間")]
        public int MonsterCount { get; set; }

        /// <summary>
        /// 移動速度倍率
        /// </summary>
        [Required(ErrorMessage = "速度倍率為必填")]
        [Range(0.1, 10.0, ErrorMessage = "速度倍率必須在0.1-10.0之間")]
        public decimal SpeedMultiplier { get; set; }

        /// <summary>
        /// 勝利經驗值獎勵
        /// </summary>
        [Required(ErrorMessage = "經驗值為必填")]
        [Range(0, 10000, ErrorMessage = "經驗值必須在0-10000之間")]
        public int ExperienceReward { get; set; }

        /// <summary>
        /// 勝利點數獎勵
        /// </summary>
        [Required(ErrorMessage = "點數為必填")]
        [Range(0, 10000, ErrorMessage = "點數必須在0-10000之間")]
        public int PointsReward { get; set; }

        /// <summary>
        /// 是否獎勵優惠券（僅第3關有效）
        /// </summary>
        public bool HasCoupon { get; set; }

        /// <summary>
        /// 優惠券類型代碼
        /// </summary>
        [StringLength(50, ErrorMessage = "優惠券類型不可超過50字元")]
        public string? CouponType { get; set; }
    }

    /// <summary>
    /// 冒險結果影響更新輸入模型
    /// </summary>
    public class AdventureResultImpactInputModel
    {
        /// <summary>
        /// 結果類型（Win/Lose）
        /// </summary>
        [Required(ErrorMessage = "結果類型為必填")]
        public string ResultType { get; set; } = "Win";

        /// <summary>
        /// 飽食度變化
        /// </summary>
        [Required(ErrorMessage = "飽食度變化為必填")]
        [Range(-100, 100, ErrorMessage = "飽食度變化必須在-100到100之間")]
        public int HungerDelta { get; set; }

        /// <summary>
        /// 心情值變化
        /// </summary>
        [Required(ErrorMessage = "心情值變化為必填")]
        [Range(-100, 100, ErrorMessage = "心情值變化必須在-100到100之間")]
        public int MoodDelta { get; set; }

        /// <summary>
        /// 體力值變化
        /// </summary>
        [Required(ErrorMessage = "體力值變化為必填")]
        [Range(-100, 100, ErrorMessage = "體力值變化必須在-100到100之間")]
        public int StaminaDelta { get; set; }

        /// <summary>
        /// 清潔度變化
        /// </summary>
        [Required(ErrorMessage = "清潔度變化為必填")]
        [Range(-100, 100, ErrorMessage = "清潔度變化必須在-100到100之間")]
        public int CleanlinessDelta { get; set; }
    }
}
