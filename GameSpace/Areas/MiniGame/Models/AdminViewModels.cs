using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int NewSignInsToday { get; set; }
        public List<ActivityLogViewModel> RecentActivity { get; set; } = new List<ActivityLogViewModel>();
    }

    public class ActivityLogViewModel
    {
        public DateTime Timestamp { get; set; }
        public string Module { get; set; }
        public string Operation { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
    }

    public class AdminWalletViewModel
    {
        public int TotalPointsDistributed { get; set; }
        public int TodayPointChanges { get; set; }
        public int TotalCouponsIssued { get; set; }
        public int TotalEVouchersIssued { get; set; }
    }

    public class AdminPetViewModel
    {
        public int TotalPets { get; set; }
        public int ActivePets { get; set; }
        public int HighLevelPets { get; set; }
        public int PetsNeedingCare { get; set; }
    }

    public class AdminMiniGameViewModel
    {
        public int TotalGamesPlayed { get; set; }
        public int TodayGamesPlayed { get; set; }
        public double WinRate { get; set; }
        public double AverageGameDuration { get; set; }
    }

    public class AdminSignInViewModel
    {
        public int TotalSignIns { get; set; }
        public int TodaySignIns { get; set; }
        public int UniqueUsersToday { get; set; }
        public double AveragePointsPerSignIn { get; set; }
    }

    public class UserPointAdjustmentViewModel
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int PointsChange { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Reason { get; set; }
    }

    public class PetAttributeUpdateViewModel
    {
        [Required]
        public int PetId { get; set; }
        
        [Range(0, 100)]
        public int Hunger { get; set; }
        
        [Range(0, 100)]
        public int Mood { get; set; }
        
        [Range(0, 100)]
        public int Stamina { get; set; }
        
        [Range(0, 100)]
        public int Cleanliness { get; set; }
        
        [Range(0, 100)]
        public int Health { get; set; }
    }

    public class GameRewardUpdateViewModel
    {
        [Required]
        public int PlayId { get; set; }
        
        [Range(0, int.MaxValue)]
        public int ExpGained { get; set; }
        
        [Range(0, int.MaxValue)]
        public int PointsGained { get; set; }
    }

    public class ManualSignInViewModel
    {
        [Required]
        public int UserId { get; set; }
        
        [Range(0, int.MaxValue)]
        public int PointsGained { get; set; }
        
        [Range(0, int.MaxValue)]
        public int ExpGained { get; set; }
        
        [StringLength(50)]
        public string CouponCode { get; set; }
    }
}
