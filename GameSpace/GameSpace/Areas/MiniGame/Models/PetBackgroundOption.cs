using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物背景選項模型
    /// </summary>
    public class PetBackgroundOption
    {
        /// <summary>
        /// 背景選項ID
        /// </summary>
        [Key]
        public int BackgroundOptionId { get; set; }

        /// <summary>
        /// 背景名稱
        /// </summary>
        [Required(ErrorMessage = \
背景名稱為必填欄位\)]
        [StringLength(50, ErrorMessage = \背景名稱長度不能超過50個字元\)]
        public string BackgroundName { get; set; } = string.Empty;

        /// <summary>
        /// 背景描述
        /// </summary>
        [StringLength(200, ErrorMessage = \背景描述長度不能超過200個字元\)]
        public string? Description { get; set; }

        /// <summary>
        /// 背景圖片路徑
        /// </summary>
        [StringLength(500, ErrorMessage = \背景圖片路徑長度不能超過500個字元\)]
        public string? ImagePath { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序順序
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 建立者ID
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500, ErrorMessage = \備註長度不能超過500個字元\)]
        public string? Remarks { get; set; }
    }
}
