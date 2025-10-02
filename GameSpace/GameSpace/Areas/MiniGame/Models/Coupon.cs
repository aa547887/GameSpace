using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 優惠券資料模型
    /// 對應資料庫表：Coupon
    /// </summary>
    [Table("Coupon")]
    public class Coupon
    {
        /// <summary>
        /// 優惠券ID（主鍵）
        /// </summary>
        [Key]
        public int CouponID { get; set; }

        /// <summary>
        /// 優惠券代碼（格式：CPN-年月-隨機碼）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        /// <summary>
        /// 優惠券類型ID（外鍵）
        /// </summary>
        [Required]
        public int CouponTypeID { get; set; }

        /// <summary>
        /// 使用者ID（外鍵）
        /// </summary>
        [Required]
        public int UserID { get; set; }

        /// <summary>
        /// 是否已使用（0:未使用, 1:已使用）
        /// </summary>
        [Required]
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// 獲得時間
        /// </summary>
        [Required]
        public DateTime AcquiredTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 使用時間（可為空）
        /// </summary>
        public DateTime? UsedTime { get; set; }

        /// <summary>
        /// 使用於訂單ID（可為空）
        /// </summary>
        public int? UsedInOrderID { get; set; }

        // 導航屬性
        /// <summary>
        /// 優惠券類型導航屬性
        /// </summary>
        [ForeignKey("CouponTypeID")]
        public virtual CouponType? CouponType { get; set; }
    }
}
