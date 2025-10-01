using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 電子禮券表
    /// </summary>
    [Table("EVoucher")]
    public class EVoucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EVoucherID { get; set; }

        [Required]
        [StringLength(50)]
        public string EVoucherCode { get; set; } = string.Empty;

        public int EVoucherTypeID { get; set; }

        public int UserID { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime AcquiredTime { get; set; } = DateTime.Now;

        public DateTime? UsedTime { get; set; }

        // 導航屬性
        [ForeignKey("EVoucherTypeID")]
        public virtual EVoucherType EVoucherType { get; set; } = null!;

        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }
}