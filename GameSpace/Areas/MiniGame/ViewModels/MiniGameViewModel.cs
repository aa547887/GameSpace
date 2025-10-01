using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.ViewModels
{
    /// <summary>
    /// 小遊戲管理 ViewModel
    /// </summary>
    public class MiniGameViewModel
    {
        public int GameID { get; set; }
        public int UserID { get; set; }
        public int PetID { get; set; }
        
        [Display(Name = "會員帳號")]
        public string UserAccount { get; set; } = string.Empty;
        
        [Display(Name = "會員姓名")]
        public string UserName { get; set; } = string.Empty;
        
        [Display(Name = "寵物名稱")]
        public string PetName { get; set; } = string.Empty;
        
        [Display(Name = "遊戲類型")]
        [Required(ErrorMessage = "遊戲類型為必填")]
        [StringLength(30, ErrorMessage = "遊戲類型不能超過30字")]
        public string GameType { get; set; } = string.Empty;
        
        [Display(Name = "開始時間")]
        public DateTime StartTime { get; set; }
        
        [Display(Name = "結束時間")]
        public DateTime? EndTime { get; set; }
        
        [Display(Name = "遊戲結果")]
        [StringLength(10, ErrorMessage = "遊戲結果不能超過10字")]
        public string? GameResult { get; set; }
        
        [Display(Name = "獲得點數")]
        public int PointsEarned { get; set; } = 0;
        
        [Display(Name = "寵物經驗值")]
        public int PetExpEarned { get; set; } = 0;
        
        [Display(Name = "獲得優惠券ID")]
        public int? CouponEarned { get; set; }
        
        [Display(Name = "遊戲會話ID")]
        [Required(ErrorMessage = "遊戲會話ID為必填")]
        [StringLength(50, ErrorMessage = "遊戲會話ID不能超過50字")]
        public string SessionID { get; set; } = string.Empty;
    }

    /// <summary>
    /// 小遊戲規則設定 ViewModel
    /// </summary>
    public class GameRulesViewModel
    {
        [Display(Name = "每日遊戲次數限制")]
        [Range(1, 10, ErrorMessage = "每日遊戲次數範圍為 1-10")]
        public int DailyGameLimit { get; set; } = 3;
        
        [Display(Name = "第1關怪物數量")]
        [Range(1, 20, ErrorMessage = "怪物數量範圍為 1-20")]
        public int Level1MonsterCount { get; set; } = 6;
        
        [Display(Name = "第1關怪物移動速度")]
        [Range(0.1, 5.0, ErrorMessage = "移動速度範圍為 0.1-5.0")]
        public double Level1MonsterSpeed { get; set; } = 1.0;
        
        [Display(Name = "第1關獎勵點數")]
        [Range(0, 1000, ErrorMessage = "獎勵點數範圍為 0-1000")]
        public int Level1PointsReward { get; set; } = 10;
        
        [Display(Name = "第1關獎勵經驗值")]
        [Range(0, 1000, ErrorMessage = "獎勵經驗值範圍為 0-1000")]
        public int Level1PetExpReward { get; set; } = 100;
        
        [Display(Name = "第2關怪物數量")]
        [Range(1, 20, ErrorMessage = "怪物數量範圍為 1-20")]
        public int Level2MonsterCount { get; set; } = 8;
        
        [Display(Name = "第2關怪物移動速度")]
        [Range(0.1, 5.0, ErrorMessage = "移動速度範圍為 0.1-5.0")]
        public double Level2MonsterSpeed { get; set; } = 1.5;
        
        [Display(Name = "第2關獎勵點數")]
        [Range(0, 1000, ErrorMessage = "獎勵點數範圍為 0-1000")]
        public int Level2PointsReward { get; set; } = 20;
        
        [Display(Name = "第2關獎勵經驗值")]
        [Range(0, 1000, ErrorMessage = "獎勵經驗值範圍為 0-1000")]
        public int Level2PetExpReward { get; set; } = 200;
        
        [Display(Name = "第3關怪物數量")]
        [Range(1, 20, ErrorMessage = "怪物數量範圍為 1-20")]
        public int Level3MonsterCount { get; set; } = 10;
        
        [Display(Name = "第3關怪物移動速度")]
        [Range(0.1, 5.0, ErrorMessage = "移動速度範圍為 0.1-5.0")]
        public double Level3MonsterSpeed { get; set; } = 2.0;
        
        [Display(Name = "第3關獎勵點數")]
        [Range(0, 1000, ErrorMessage = "獎勵點數範圍為 0-1000")]
        public int Level3PointsReward { get; set; } = 30;
        
        [Display(Name = "第3關獎勵經驗值")]
        [Range(0, 1000, ErrorMessage = "獎勵經驗值範圍為 0-1000")]
        public int Level3PetExpReward { get; set; } = 300;
        
        [Display(Name = "第3關獎勵優惠券ID")]
        public int? Level3CouponReward { get; set; }
    }
}