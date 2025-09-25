using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("Relation")]
    public class Relation
    {
        [Key]
        public int RelationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int TargetUserId { get; set; }

        [Required]
        [StringLength(50)]
        public string RelationType { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("TargetUserId")]
        public virtual User TargetUser { get; set; } = null!;
    }
}
