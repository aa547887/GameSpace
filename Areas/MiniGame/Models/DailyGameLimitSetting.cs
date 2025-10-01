using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Areas.MiniGame.Models
{
    /// <summary>
    /// 每日遊戲次數限制設定
    /// </summary>
    [Table("DailyGameLimitSettings")]
    public class DailyGameLimitSetting
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 設定名稱
        /// </summary>
        [Required]
        [StringLength(100)]
        public string SettingName { get; set; } = string.Empty;

        /// <summary>
        /// 每日遊戲次數限制
        /// </summary>
        [Required]
        [Range(1, 100)]
        public int DailyLimit { get; set; } = 3;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 設定描述
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 建立者ID
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public int UpdatedBy { get; set; }
    }
}
