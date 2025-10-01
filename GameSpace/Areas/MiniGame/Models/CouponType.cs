using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 優惠券類型表
    /// </summary>
    [Table("CouponType")]
    public class CouponType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CouponTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MinSpend { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public int PointsCost { get; set; } = 0;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}