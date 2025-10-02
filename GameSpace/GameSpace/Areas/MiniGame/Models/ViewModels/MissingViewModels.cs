using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    // System Statistics Model for Dashboard
    public class SystemStatsModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalPets { get; set; }
        public int TotalGames { get; set; }
        public int TotalPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int NewUsersToday { get; set; }
        public int GamesToday { get; set; }
        public int SignInsToday { get; set; }
        public Dictionary<string, int> UserStatusDistribution { get; set; } = new();
        public Dictionary<string, int> PetLevelDistribution { get; set; } = new();
    }

    // Chart Data Model for Dashboard Charts
    public class ChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Data { get; set; } = new();
        public string Label { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public string BorderColor { get; set; } = string.Empty;
    }

    // Pet Interaction Bonus Rule
    public class PetInteractionBonusRule
    {
        public int Id { get; set; }
        public string InteractionType { get; set; } = string.Empty;
        public int MinBonus { get; set; }
        public int MaxBonus { get; set; }
        public int PointsCost { get; set; }
        public int CooldownMinutes { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // Pet Level Up Rule ViewModels
    public class PetLevelUpRuleCreateViewModel
    {
        [Required]
        [Range(1, 1000)]
        public int Level { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ExperienceRequired { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int PointsReward { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ExpReward { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool HasCouponReward { get; set; } = false;

        [StringLength(50)]
        public string? CouponCode { get; set; }
    }

    public class PetLevelUpRuleEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Level { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ExperienceRequired { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int PointsReward { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ExpReward { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool HasCouponReward { get; set; } = false;

        [StringLength(50)]
        public string? CouponCode { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // Read Models for Services
    public class SignInRuleReadModel
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public int ConsecutiveDays { get; set; }
        public int PointsReward { get; set; }
        public int ExpReward { get; set; }
        public string? CouponTypeId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SignInRecordReadModel
    {
        public int LogID { get; set; }
        public int UserID { get; set; }
        public string UserAccount { get; set; } = string.Empty;
        public DateTime SignInTime { get; set; }
        public int ConsecutiveDays { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
        public string? CouponGained { get; set; }
    }

    public class PetRuleReadModel
    {
        public int MaxHunger { get; set; } = 100;
        public int MaxMood { get; set; } = 100;
        public int MaxStamina { get; set; } = 100;
        public int MaxCleanliness { get; set; } = 100;
        public int MaxHealth { get; set; } = 100;
        public int HungerDecayRate { get; set; } = 1;
        public int MoodDecayRate { get; set; } = 1;
        public int StaminaDecayRate { get; set; } = 1;
        public int CleanlinessDecayRate { get; set; } = 1;
        public int FeedCost { get; set; } = 10;
        public int PlayCost { get; set; } = 5;
        public int CleanCost { get; set; } = 5;
    }

    public class PetSummary
    {
        public int PetID { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Health { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public string OwnerAccount { get; set; } = string.Empty;
    }

    // Game Rule Read Model
    public class GameRuleReadModel
    {
        public string GameType { get; set; } = string.Empty;
        public int BasePointsReward { get; set; }
        public int BaseExpReward { get; set; }
        public int MaxDailyPlays { get; set; }
        public int PointsPerLevel { get; set; }
        public int ExpPerLevel { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
