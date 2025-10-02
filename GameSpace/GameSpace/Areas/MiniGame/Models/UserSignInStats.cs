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
        public int StatsID { get; set; }

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
        public int PointsEarned { get; set; } = 0;

        /// <summary>
        /// 寵物獲得經驗值
        /// </summary>
        [Required]
        public int PetExpEarned { get; set; } = 0;

        /// <summary>
        /// 獲得優惠券數量
        /// </summary>
        public int? CouponEarned { get; set; }

        /// <summary>
        /// 連續簽到天數
        /// </summary>
        [Required]
        public int ConsecutiveDays { get; set; } = 1;
    }
}
