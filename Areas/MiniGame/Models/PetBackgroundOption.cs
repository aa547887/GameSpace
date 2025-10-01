using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public int Id { get; set; }

        /// <summary>
        /// 背景名稱
        /// </summary>
        [Required(ErrorMessage = "背景名稱為必填欄位")]
        [StringLength(50, ErrorMessage = "背景名稱長度不能超過50個字元")]
        [Display(Name = "背景名稱")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 背景描述
        /// </summary>
        [StringLength(200, ErrorMessage = "背景描述長度不能超過200個字元")]
        [Display(Name = "背景描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 背景顏色代碼
        /// </summary>
        [Required(ErrorMessage = "背景顏色代碼為必填欄位")]
        [StringLength(7, MinimumLength = 7, ErrorMessage = "背景顏色代碼必須為7個字元")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "背景顏色代碼格式不正確，必須為#開頭的6位十六進位數字")]
        [Display(Name = "背景顏色代碼")]
        public string BackgroundColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序順序
        /// </summary>
        [Display(Name = "排序順序")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        [Display(Name = "更新時間")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 建立者ID
        /// </summary>
        [Display(Name = "建立者ID")]
        public int? CreatedBy { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        [Display(Name = "更新者ID")]
        public int? UpdatedBy { get; set; }
    }
}
