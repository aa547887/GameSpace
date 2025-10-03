using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("MiniGame")]
    public class MiniGame
    {
        [Key]
        [Column("PlayID")]
        public int PlayID { get; set; }

        [Required]
        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [Column("PetID")]
        public int PetID { get; set; }

        [Required]
        [Column("Level")]
        public int Level { get; set; } = 1;

        [Required]
        [Column("MonsterCount")]
        public int MonsterCount { get; set; } = 0;

        [Required]
        [Column("SpeedMultiplier", TypeName = "decimal(18,2)")]
        public decimal SpeedMultiplier { get; set; } = 1.0m;

        [Required]
        [StringLength(20)]
        [Column("Result")]
        public string Result { get; set; } = string.Empty;

        [Required]
        [Column("ExpGained")]
        public int ExpGained { get; set; } = 0;

        [Required]
        [Column("ExpGainedTime")]
        public DateTime ExpGainedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("PointsGained")]
        public int PointsGained { get; set; } = 0;

        [Required]
        [Column("PointsGainedTime")]
        public DateTime PointsGainedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        [Column("CouponGained")]
        public string CouponGained { get; set; } = string.Empty;

        [Required]
        [Column("CouponGainedTime")]
        public DateTime CouponGainedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("HungerDelta")]
        public int HungerDelta { get; set; } = 0;

        [Required]
        [Column("MoodDelta")]
        public int MoodDelta { get; set; } = 0;

        [Required]
        [Column("StaminaDelta")]
        public int StaminaDelta { get; set; } = 0;

        [Required]
        [Column("CleanlinessDelta")]
        public int CleanlinessDelta { get; set; } = 0;

        [Required]
        [Column("StartTime")]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;

        [Column("EndTime")]
        public DateTime? EndTime { get; set; }

        [Required]
        [Column("Aborted")]
        public bool Aborted { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User? Users { get; set; }

        [ForeignKey("PetID")]
        public virtual Pet? Pet { get; set; }
    }
}

