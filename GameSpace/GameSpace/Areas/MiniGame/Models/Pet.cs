using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("Pet")]
    public class Pet
    {
        [Key]
        public int PetID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string PetName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string PetType { get; set; } = string.Empty;

        [Required]
        public int PetLevel { get; set; } = 1;

        [Required]
        public int PetExp { get; set; } = 0;

        [Required]
        [StringLength(30)]
        public string PetSkin { get; set; } = "default";

        [Required]
        [StringLength(30)]
        public string PetBackground { get; set; } = "default";

        [Required]
        public int Hunger { get; set; } = 100;

        [Required]
        public int Happiness { get; set; } = 100;

        [Required]
        public int Health { get; set; } = 100;

        [Required]
        public int Energy { get; set; } = 100;

        [Required]
        public int Cleanliness { get; set; } = 100;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastFed { get; set; }

        public DateTime? LastPlayed { get; set; }

        public DateTime? LastBathed { get; set; }

        public DateTime? LastSlept { get; set; }

        // Navigation properties
        public virtual ICollection<MiniGame> MiniGames { get; set; } = new List<MiniGame>();
    }
}
