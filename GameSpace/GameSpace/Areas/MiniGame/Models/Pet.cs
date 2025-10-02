using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("Pet")]
    public class Pet
    {
        [Key]
        [Column("PetID")]
        public int PetID { get; set; }

        [Required]
        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("PetName")]
        public string PetName { get; set; } = string.Empty;

        [Required]
        [Column("Level")]
        public int Level { get; set; } = 1;

        [Required]
        [Column("LevelUpTime")]
        public DateTime LevelUpTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("Experience")]
        public int Experience { get; set; } = 0;

        [Required]
        [Column("Hunger")]
        public int Hunger { get; set; } = 100;

        [Required]
        [Column("Mood")]
        public int Mood { get; set; } = 100;

        [Required]
        [Column("Stamina")]
        public int Stamina { get; set; } = 100;

        [Required]
        [Column("Cleanliness")]
        public int Cleanliness { get; set; } = 100;

        [Required]
        [Column("Health")]
        public int Health { get; set; } = 100;

        [Required]
        [StringLength(10)]
        [Column("SkinColor", TypeName = "varchar(10)")]
        public string SkinColor { get; set; } = "#FFFFFF";

        [Required]
        [Column("SkinColorChangedTime")]
        public DateTime SkinColorChangedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        [Column("BackgroundColor")]
        public string BackgroundColor { get; set; } = "#F0F0F0";

        [Required]
        [Column("BackgroundColorChangedTime")]
        public DateTime BackgroundColorChangedTime { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("PointsChanged_SkinColor")]
        public int PointsChanged_SkinColor { get; set; } = 0;

        [Required]
        [Column("PointsChanged_BackgroundColor")]
        public int PointsChanged_BackgroundColor { get; set; } = 0;

        [Required]
        [Column("PointsGained_LevelUp")]
        public int PointsGained_LevelUp { get; set; } = 0;

        [Required]
        [Column("PointsGainedTime_LevelUp")]
        public DateTime PointsGainedTime_LevelUp { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User? Users { get; set; }

        public virtual ICollection<MiniGame>? MiniGames { get; set; }
    }
}
