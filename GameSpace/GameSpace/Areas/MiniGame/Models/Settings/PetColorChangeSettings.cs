using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物換色點數設定
    /// </summary>
    [Table("PetColorChangeSettings")]
    public class PetColorChangeSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ColorName { get; set; } = "";

        [Required]
        public int RequiredPoints { get; set; }

        [StringLength(7)]
        public string ColorCode { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
