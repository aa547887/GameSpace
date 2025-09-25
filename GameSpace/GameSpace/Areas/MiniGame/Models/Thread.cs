using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("Thread")]
    public class Thread
    {
        [Key]
        public int ThreadId { get; set; }

        [Required]
        public int ForumId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int AuthorUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // 導航屬性
        [ForeignKey("ForumId")]
        public virtual Forum Forum { get; set; } = null!;

        [ForeignKey("AuthorUserId")]
        public virtual User AuthorUser { get; set; } = null!;

        public virtual ICollection<ThreadPost> ThreadPosts { get; set; } = new List<ThreadPost>();

        // 為了向後兼容，添加別名屬性
        public ICollection<ThreadPost> OrderInfoBy => ThreadPosts;
    }
}
