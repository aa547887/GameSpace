using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("MiniGame")]
    public class MiniGame
    {
        [Key]
        public int GameID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int PetID { get; set; }

        [Required]
        [StringLength(30)]
        public string GameType { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; } = DateTime.Now;

        public DateTime? EndTime { get; set; }

        [StringLength(10)]
        public string? GameResult { get; set; }

        [Required]
        public int PointsEarned { get; set; } = 0;

        [Required]
        public int PetExpEarned { get; set; } = 0;

        public int? CouponEarned { get; set; }

        [Required]
        [StringLength(50)]
        public string SessionID { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("PetID")]
        public virtual Pet? Pet { get; set; }
    }
}
