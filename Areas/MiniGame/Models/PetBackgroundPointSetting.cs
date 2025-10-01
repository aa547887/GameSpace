using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物換背景所需點數設定
    /// </summary>
    [Table("PetBackgroundPointSettings")]
    public class PetBackgroundPointSetting
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 寵物等級
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "寵物等級必須在1-100之間")]
        public int PetLevel { get; set; }

        /// <summary>
        /// 所需點數
        /// </summary>
        [Required]
        [Range(0, 10000, ErrorMessage = "所需點數必須在0-10000之間")]
        public int RequiredPoints { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 建立者ID
        /// </summary>
        [Required]
        public int CreatedBy { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新者ID
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500, ErrorMessage = "備註長度不能超過500字")]
        public string? Remarks { get; set; }
    }
}
