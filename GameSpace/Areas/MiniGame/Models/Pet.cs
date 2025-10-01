using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物表
    /// </summary>
    [Table("Pet")]
    public class Pet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PetID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string PetName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string PetType { get; set; } = string.Empty;

        public int PetLevel { get; set; } = 1;

        public int PetExp { get; set; } = 0;

        [Required]
        [StringLength(30)]
        public string PetSkin { get; set; } = "default";

        [Required]
        [StringLength(30)]
        public string PetBackground { get; set; } = "default";

        public int Hunger { get; set; } = 100;

        public int Happiness { get; set; } = 100;

        public int Health { get; set; } = 100;

        public int Energy { get; set; } = 100;

        public int Cleanliness { get; set; } = 100;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastFed { get; set; }

        public DateTime? LastPlayed { get; set; }

        public DateTime? LastBathed { get; set; }

        public DateTime? LastSlept { get; set; }

        // 導航屬性
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }
}