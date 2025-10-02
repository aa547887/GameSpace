using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 電子禮券資料模型
    /// 對應資料庫表：EVoucher
    /// </summary>
    [Table("EVoucher")]
    public class EVoucher
    {
        /// <summary>
        /// 電子禮券ID（主鍵）
        /// </summary>
        [Key]
        public int EVoucherID { get; set; }

        /// <summary>
        /// 電子禮券代碼（格式：EV-類型-隨機碼-數字）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string EVoucherCode { get; set; } = string.Empty;

        /// <summary>
        /// 電子禮券類型ID（外鍵）
        /// </summary>
        [Required]
        public int EVoucherTypeID { get; set; }

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

        // 導航屬性
        /// <summary>
        /// 電子禮券類型導航屬性
        /// </summary>
        [ForeignKey("EVoucherTypeID")]
        public virtual EVoucherType? EVoucherType { get; set; }
    }
}
