using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 錢包歷史記錄表
    /// </summary>
    [Table("WalletHistory")]
    public class WalletHistory
    {
        [Key]
        [Column("HistoryID")]
        public int HistoryId { get; set; }

        [Column("UserID")]
        public int UserId { get; set; }

        [Column("ChangeAmount")]
        public int ChangeAmount { get; set; }

        [Column("ChangeType")]
        [MaxLength(50)]
        [Required]
        public string ChangeType { get; set; } = string.Empty;

        [Column("ChangeTime")]
        public DateTime ChangeTime { get; set; } = DateTime.Now;

        [Column("Description")]
        [MaxLength(200)]
        public string? Description { get; set; }

        [Column("RelatedID")]
        public int? RelatedId { get; set; }

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual UserWallet UserWallet { get; set; } = null!;
    }
}
