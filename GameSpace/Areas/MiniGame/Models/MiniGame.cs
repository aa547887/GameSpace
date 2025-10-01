using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 小遊戲記錄表
    /// </summary>
    [Table("MiniGame")]
    public class MiniGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameID { get; set; }

        public int UserID { get; set; }

        public int PetID { get; set; }

        [Required]
        [StringLength(30)]
        public string GameType { get; set; } = string.Empty;

        public DateTime StartTime { get; set; } = DateTime.Now;

        public DateTime? EndTime { get; set; }

        [StringLength(10)]
        public string? GameResult { get; set; }

        public int PointsEarned { get; set; } = 0;

        public int PetExpEarned { get; set; } = 0;

        public int? CouponEarned { get; set; }

        [Required]
        [StringLength(50)]
        public string SessionID { get; set; } = string.Empty;

        // 導航屬性
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;

        [ForeignKey("PetID")]
        public virtual Pet Pet { get; set; } = null!;
    }
}