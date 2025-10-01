using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物互動狀態增益規則
    /// </summary>
    [Table("PetInteractionBonusRules")]
    public class PetInteractionBonusRule
    {
        /// <summary>
        /// 規則ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 互動類型
        /// </summary>
        [Required(ErrorMessage = "互動類型為必填欄位")]
        [StringLength(50, ErrorMessage = "互動類型長度不能超過50個字元")]
        [Display(Name = "互動類型")]
        public string InteractionType { get; set; } = string.Empty;

        /// <summary>
        /// 互動名稱
        /// </summary>
        [Required(ErrorMessage = "互動名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "互動名稱長度不能超過100個字元")]
        [Display(Name = "互動名稱")]
        public string InteractionName { get; set; } = string.Empty;

        /// <summary>
        /// 所需點數
        /// </summary>
        [Required(ErrorMessage = "所需點數為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "所需點數必須大於等於0")]
        [Display(Name = "所需點數")]
        public int PointsCost { get; set; }

        /// <summary>
        /// 快樂度增益
        /// </summary>
        [Required(ErrorMessage = "快樂度增益為必填欄位")]
        [Range(0, 100, ErrorMessage = "快樂度增益必須在0-100之間")]
        [Display(Name = "快樂度增益")]
        public int HappinessGain { get; set; }

        /// <summary>
        /// 經驗值增益
        /// </summary>
        [Required(ErrorMessage = "經驗值增益為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "經驗值增益必須大於等於0")]
        [Display(Name = "經驗值增益")]
        public int ExpGain { get; set; }

        /// <summary>
        /// 冷卻時間（分鐘）
        /// </summary>
        [Required(ErrorMessage = "冷卻時間為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "冷卻時間必須大於等於0")]
        [Display(Name = "冷卻時間（分鐘）")]
        public int CooldownMinutes { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500, ErrorMessage = "描述長度不能超過500個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 建立者
        /// </summary>
        [StringLength(50)]
        [Display(Name = "建立者")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [StringLength(50)]
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }
    }
}
