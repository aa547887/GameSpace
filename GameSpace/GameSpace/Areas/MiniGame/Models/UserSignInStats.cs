using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("UserSignInStats")]
    public class UserSignInStats
    {
        [Key]
        public int LogID { get; set; }

        // Alias for compatibility
        [NotMapped]
        public int StatsID
        {
            get => LogID;
            set => LogID = value;
        }

        [Required]
        public int UserID { get; set; }

        [Required]
        public DateTime SignTime { get; set; } = DateTime.Now;

        [Required]
        public int PointsGained { get; set; } = 0;

        // Alias for compatibility
        [NotMapped]
        public int PointsEarned
        {
            get => PointsGained;
            set => PointsGained = value;
        }

        public DateTime PointsGainedTime { get; set; }

        [Required]
        public int ExpGained { get; set; } = 0;

        // Alias for compatibility
        [NotMapped]
        public int PetExpEarned
        {
            get => ExpGained;
            set => ExpGained = value;
        }

        public DateTime ExpGainedTime { get; set; }

        public string? CouponGained { get; set; }

        // Alias for compatibility (int? instead of string?)
        [NotMapped]
        public int? CouponEarned
        {
            get => string.IsNullOrEmpty(CouponGained) ? null : int.TryParse(CouponGained, out int val) ? val : null;
            set => CouponGained = value?.ToString();
        }

        public DateTime? CouponGainedTime { get; set; }

        [Required]
        public int ConsecutiveDays { get; set; } = 1;

        // Navigation property
        [ForeignKey("UserID")]
        public virtual User? Users { get; set; }
    }
}
