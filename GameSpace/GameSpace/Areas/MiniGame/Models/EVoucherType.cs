using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 電子禮券類型資料模型
    /// 對應資料庫表：EVoucherType
    /// </summary>
    [Table("EVoucherType")]
    public class EVoucherType
    {
        /// <summary>
        /// 電子禮券類型ID（主鍵）
        /// </summary>
        [Key]
        public int EVoucherTypeID { get; set; }

        /// <summary>
        /// 電子禮券名稱
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 禮券價值金額
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValueAmount { get; set; }

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
        /// 總可用數量
        /// </summary>
        [Required]
        public int TotalAvailable { get; set; } = 0;

        /// <summary>
        /// 電子禮券描述
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        // 導航屬性
        /// <summary>
        /// 此類型的電子禮券集合
        /// </summary>
        public virtual ICollection<EVoucher> EVouchers { get; set; } = new List<EVoucher>();
    }
}
