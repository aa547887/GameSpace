using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("EVoucherType")]
    public class EVoucherType
    {
        [Key]
        public int EVoucherTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValueAmount { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; }

        [Required]
        public DateTime ValidTo { get; set; }

        [Required]
        public int PointsCost { get; set; } = 0;

        [Required]
        public int TotalAvailable { get; set; } = 0;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<EVoucher> EVouchers { get; set; } = new List<EVoucher>();
    }
}
