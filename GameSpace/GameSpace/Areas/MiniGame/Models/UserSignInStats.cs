using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 使用者簽到統計資料模型
    /// 對應資料庫表：UserSignInStats
    /// </summary>
    [Table("UserSignInStats")]
    public class UserSignInStats
    {
        /// <summary>
        /// 簽到記錄ID（主鍵）
        /// </summary>
        [Key]
        public int LogID { get; set; }

        /// <summary>
        /// 使用者ID（外鍵）
        /// </summary>
        [Required]
        public int UserID { get; set; }

        /// <summary>
        /// 簽到時間
        /// </summary>
        [Required]
        public DateTime SignTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 獲得點數
        /// </summary>
        [Required]
        public int PointsGained { get; set; } = 0;

        /// <summary>
        /// 點數獲得時間
        /// </summary>
        [Required]
        public DateTime PointsGainedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 獲得經驗值
        /// </summary>
        [Required]
        public int ExpGained { get; set; } = 0;

        /// <summary>
        /// 經驗值獲得時間
        /// </summary>
        [Required]
        public DateTime ExpGainedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 獲得優惠券數量
        /// </summary>
        public int? CouponGained { get; set; }

        /// <summary>
        /// 優惠券獲得時間
        /// </summary>
        public DateTime? CouponGainedTime { get; set; }
    }
}
