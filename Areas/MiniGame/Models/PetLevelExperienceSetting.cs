using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物等級經驗值對應設定
    /// </summary>
    [Table("PetLevelExperienceSettings")]
    public class PetLevelExperienceSetting
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 等級
        /// </summary>
        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        public int Level { get; set; }

        /// <summary>
        /// 所需經驗值
        /// </summary>
        [Required(ErrorMessage = "所需經驗值為必填欄位")]
        [Range(1, int.MaxValue, ErrorMessage = "所需經驗值必須大於0")]
        public int RequiredExperience { get; set; }

        /// <summary>
        /// 等級名稱
        /// </summary>
        [Required(ErrorMessage = "等級名稱為必填欄位")]
        [StringLength(50, ErrorMessage = "等級名稱長度不能超過50個字元")]
        public string LevelName { get; set; } = string.Empty;

        /// <summary>
        /// 等級描述
        /// </summary>
        [StringLength(200, ErrorMessage = "等級描述長度不能超過200個字元")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 建立者
        /// </summary>
        [StringLength(50)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [StringLength(50)]
        public string? UpdatedBy { get; set; }
    }
}
