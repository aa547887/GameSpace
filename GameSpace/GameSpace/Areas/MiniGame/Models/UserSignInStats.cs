using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("UserSignInStats")]
    public class UserSignInStats
    {
        [Key]
        public int StatsID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public DateTime SignTime { get; set; } = DateTime.Now;

        [Required]
        public int PointsEarned { get; set; } = 0;

        [Required]
        public int PetExpEarned { get; set; } = 0;

        public int? CouponEarned { get; set; }

        [Required]
        public int ConsecutiveDays { get; set; } = 1;
    }
}

