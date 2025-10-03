using System;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class PetBackgroundOptionEntity
    {
        [Key]
        public int BackgroundOptionId { get; set; }
        [Required]
        [StringLength(50)]
        public string BackgroundName { get; set; } = string.Empty;
        [Required]
        [StringLength(20)]
        public string BackgroundCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}


