using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 優惠券表
    /// </summary>
    [Table("Coupon")]
    public class Coupon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CouponID { get; set; }

        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        public int CouponTypeID { get; set; }

        public int UserID { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime AcquiredTime { get; set; } = DateTime.Now;

        public DateTime? UsedTime { get; set; }

        public int? UsedInOrderID { get; set; }

        // 導航屬性
        [ForeignKey("CouponTypeID")]
        public virtual CouponType CouponType { get; set; } = null!;

        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }
}