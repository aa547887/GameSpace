using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 使用者簽到統計表
    /// </summary>
    [Table("UserSignInStats")]
    public class UserSignInStats
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StatsID { get; set; }

        public int UserID { get; set; }

        public DateTime SignTime { get; set; } = DateTime.Now;

        public int PointsEarned { get; set; } = 0;

        public int PetExpEarned { get; set; } = 0;

        public int? CouponEarned { get; set; }

        public int ConsecutiveDays { get; set; } = 1;

        // 導航屬性
        [ForeignKey("UserID")]
        public virtual Users Users { get; set; } = null!;
    }
}