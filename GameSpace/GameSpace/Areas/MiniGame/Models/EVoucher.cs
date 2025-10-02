using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("EVoucher")]
    public class EVoucher
    {
        [Key]
        public int EVoucherID { get; set; }

        [Required]
        [StringLength(50)]
        public string EVoucherCode { get; set; } = string.Empty;

        [Required]
        public int EVoucherTypeID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        [Required]
        public DateTime AcquiredTime { get; set; } = DateTime.Now;

        public DateTime? UsedTime { get; set; }

        // Navigation properties
        [ForeignKey("EVoucherTypeID")]
        public virtual EVoucherType? EVoucherType { get; set; }
    }
}
