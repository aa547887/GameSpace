using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("ThreadPost")]
    public class ThreadPost
    {
        [Key]
        public int ThreadPostId { get; set; }

        [Required]
        public int ThreadId { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int AuthorUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // 導航屬性
        [ForeignKey("ThreadId")]
        public virtual Thread Thread { get; set; } = null!;

        [ForeignKey("AuthorUserId")]
        public virtual User AuthorUser { get; set; } = null!;
    }
}
