using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("UserIntroduce")]
    public class UserIntroduce
    {
        [Key]
        public int UserIntroduceId { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(500)]
        public string? Introduction { get; set; }

        [StringLength(200)]
        public string? Hobbies { get; set; }

        [StringLength(200)]
        public string? Interests { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
