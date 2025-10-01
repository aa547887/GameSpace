using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 電子禮券類型表
    /// </summary>
    [Table("EVoucherType")]
    public class EVoucherType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EVoucherTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValueAmount { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public int PointsCost { get; set; } = 0;

        public int TotalAvailable { get; set; } = 0;

        [StringLength(500)]
        public string? Description { get; set; }
    }
}