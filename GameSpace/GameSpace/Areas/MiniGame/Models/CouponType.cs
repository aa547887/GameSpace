using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 優惠券類型資料模型
    /// 對應資料庫表：CouponType
    /// </summary>
    [Table("CouponType")]
    public class CouponType
    {
        /// <summary>
        /// 優惠券類型ID（主鍵）
        /// </summary>
        [Key]
        public int CouponTypeID { get; set; }

        /// <summary>
        /// 優惠券名稱
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 折扣類型（Amount:金額折扣, Percent:百分比折扣）
        /// </summary>
        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } = string.Empty;

        /// <summary>
        /// 折扣值（金額或百分比）
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// 最低消費金額
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal MinSpend { get; set; }

        /// <summary>
        /// 有效期開始時間
        /// </summary>
        [Required]
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// 有效期結束時間
        /// </summary>
        [Required]
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// 兌換所需點數
        /// </summary>
        [Required]
        public int PointsCost { get; set; } = 0;

        /// <summary>
        /// 優惠券描述
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        // 導航屬性
        /// <summary>
        /// 此類型的優惠券集合
        /// </summary>
        public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
    }
}
