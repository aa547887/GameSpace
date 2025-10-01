using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("PetInteractionHistories")]
    public class PetInteractionHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PetID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string InteractionType { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string InteractionName { get; set; } = string.Empty;

        [Required]
        public int PointsCost { get; set; }

        [Required]
        public int ExpGained { get; set; }

        [Required]
        public int HappinessGained { get; set; }

        [Required]
        public DateTime InteractionTime { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Description { get; set; }
    }
}
