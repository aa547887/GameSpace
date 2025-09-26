using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalGames { get; set; }
        public int TodaySignIns { get; set; }
        public int WalletStats { get; set; }
        public List<PetLevelDistribution> PetStats { get; set; } = new List<PetLevelDistribution>();
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();
    }

    public class PetLevelDistribution
    {
        public int Level { get; set; }
        public int Count { get; set; }
    }

    public class RecentActivity
    {
        public DateTime Time { get; set; }
        public string Module { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class WalletManagementViewModel
    {
        public int TotalPoints { get; set; }
        public int TotalUsers { get; set; }
        public int AvgPoints { get; set; }
        public List<WalletTransaction> RecentTransactions { get; set; } = new List<WalletTransaction>();
    }

    public class WalletTransaction
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public int PointsChanged { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime ChangeTime { get; set; }
    }

    public class PetManagementViewModel
    {
        public int TotalPets { get; set; }
        public double AvgLevel { get; set; }
        public int TotalExperience { get; set; }
        public List<PetLevelDistribution> LevelDistribution { get; set; } = new List<PetLevelDistribution>();
        public List<PetInfo> RecentPetActivities { get; set; } = new List<PetInfo>();
    }

    public class PetInfo
    {
        public int PetID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Stamina { get; set; }
        public int Cleanliness { get; set; }
        public int Health { get; set; }
        public DateTime LevelUpTime { get; set; }
    }

    public class MiniGameManagementViewModel
    {
        public int TotalGames { get; set; }
        public int TodayGames { get; set; }
        public double WinRate { get; set; }
        public List<GameRecord> RecentGames { get; set; } = new List<GameRecord>();
    }

    public class GameRecord
    {
        public int PlayID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int PetID { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int MonsterCount { get; set; }
        public decimal SpeedMultiplier { get; set; }
        public string Result { get; set; } = string.Empty;
        public int ExpGained { get; set; }
        public int PointsGained { get; set; }
        public string CouponGained { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Aborted { get; set; }
    }

    public class SignInManagementViewModel
    {
        public int TotalSignIns { get; set; }
        public int TodaySignIns { get; set; }
        public int ThisWeekSignIns { get; set; }
        public List<SignInRecord> RecentSignIns { get; set; } = new List<SignInRecord>();
    }

    public class SignInRecord
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SignTime { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public string CouponGained { get; set; } = string.Empty;
    }

    // 表單模型
    public class GrantPointsFormModel
    {
        [Required(ErrorMessage = "請選擇發放類型")]
        public string GrantType { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入點數金額")]
        [Range(1, 10000, ErrorMessage = "點數金額必須在1-10000之間")]
        public int PointsAmount { get; set; }

        public int? UserId { get; set; }

        public string UserIds { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入發放原因")]
        [StringLength(200, ErrorMessage = "發放原因不能超過200字")]
        public string Reason { get; set; } = string.Empty;

        public DateTime? ExpiryDate { get; set; }

        public bool NotifyUser { get; set; }
    }

    public class GrantCouponsFormModel
    {
        [Required(ErrorMessage = "請輸入用戶ID")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "請選擇優惠券類型")]
        public int CouponTypeId { get; set; }

        [Required(ErrorMessage = "請輸入數量")]
        [Range(1, 10, ErrorMessage = "數量必須在1-10之間")]
        public int Quantity { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [Required(ErrorMessage = "請輸入發放原因")]
        [StringLength(200, ErrorMessage = "發放原因不能超過200字")]
        public string Reason { get; set; } = string.Empty;

        public decimal? CustomValue { get; set; }
    }

    public class UpdatePetFormModel
    {
        [Required(ErrorMessage = "請輸入寵物ID")]
        public int PetID { get; set; }

        [Required(ErrorMessage = "請輸入寵物名稱")]
        [StringLength(50, ErrorMessage = "寵物名稱不能超過50字")]
        public string PetName { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入等級")]
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        public int Level { get; set; }

        [Required(ErrorMessage = "請輸入經驗值")]
        [Range(0, 10000, ErrorMessage = "經驗值必須在0-10000之間")]
        public int Experience { get; set; }

        [Required(ErrorMessage = "請輸入飢餓值")]
        [Range(0, 100, ErrorMessage = "飢餓值必須在0-100之間")]
        public int Hunger { get; set; }

        [Required(ErrorMessage = "請輸入心情值")]
        [Range(0, 100, ErrorMessage = "心情值必須在0-100之間")]
        public int Mood { get; set; }

        [Required(ErrorMessage = "請輸入體力值")]
        [Range(0, 100, ErrorMessage = "體力值必須在0-100之間")]
        public int Stamina { get; set; }

        [Required(ErrorMessage = "請輸入清潔值")]
        [Range(0, 100, ErrorMessage = "清潔值必須在0-100之間")]
        public int Cleanliness { get; set; }

        [Required(ErrorMessage = "請輸入健康值")]
        [Range(0, 100, ErrorMessage = "健康值必須在0-100之間")]
        public int Health { get; set; }

        [Required(ErrorMessage = "請選擇膚色")]
        public string SkinColor { get; set; } = string.Empty;

        [Required(ErrorMessage = "請選擇背景色")]
        public string BackgroundColor { get; set; } = string.Empty;
    }

    public class UpdateGameResultFormModel
    {
        [Required(ErrorMessage = "請輸入遊戲ID")]
        public int PlayID { get; set; }

        [Required(ErrorMessage = "請選擇結果")]
        public string Result { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入經驗值")]
        public int ExpGained { get; set; }

        [Required(ErrorMessage = "請輸入點數")]
        public int PointsGained { get; set; }

        public string CouponGained { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入飢餓變化")]
        public int HungerDelta { get; set; }

        [Required(ErrorMessage = "請輸入心情變化")]
        public int MoodDelta { get; set; }

        [Required(ErrorMessage = "請輸入體力變化")]
        public int StaminaDelta { get; set; }

        [Required(ErrorMessage = "請輸入清潔變化")]
        public int CleanlinessDelta { get; set; }

        public DateTime? EndTime { get; set; }

        public bool Aborted { get; set; }
    }

    public class UpdateSignInFormModel
    {
        [Required(ErrorMessage = "請輸入記錄ID")]
        public int LogID { get; set; }

        [Required(ErrorMessage = "請輸入點數")]
        public int PointsGained { get; set; }

        [Required(ErrorMessage = "請輸入經驗值")]
        public int ExpGained { get; set; }

        public string CouponGained { get; set; } = string.Empty;
    }

    public class ManualSignInFormModel
    {
        [Required(ErrorMessage = "請輸入用戶ID")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "請輸入點數")]
        [Range(1, 1000, ErrorMessage = "點數必須在1-1000之間")]
        public int PointsGained { get; set; }

        [Required(ErrorMessage = "請輸入經驗值")]
        [Range(1, 500, ErrorMessage = "經驗值必須在1-500之間")]
        public int ExpGained { get; set; }

        public string CouponGained { get; set; } = string.Empty;
    }
}
