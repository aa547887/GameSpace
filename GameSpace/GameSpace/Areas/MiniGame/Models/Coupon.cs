using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("Coupon")]
    public class Coupon
    {
        [Key]
        public int CouponID { get; set; }

        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        [Required]
        public int CouponTypeID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        [Required]
        public DateTime AcquiredTime { get; set; } = DateTime.Now;

        public DateTime? UsedTime { get; set; }

        public int? UsedInOrderID { get; set; }

        // Navigation properties
        [ForeignKey("CouponTypeID")]
        public virtual CouponType? CouponType { get; set; }
    }
}
