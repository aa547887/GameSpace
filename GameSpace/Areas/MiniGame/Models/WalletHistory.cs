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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HistoryID { get; set; }

        public int UserID { get; set; }

        public int ChangeAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string ChangeType { get; set; } = string.Empty;

        public DateTime ChangeTime { get; set; } = DateTime.Now;

        [StringLength(200)]
        public string? Description { get; set; }

        public int? RelatedID { get; set; }

        // 導航屬性
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }
}