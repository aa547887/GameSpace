using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物換色點數設定 ViewModel
    /// </summary>
    public class PetColorChangeSettingsViewModel
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        public int SettingId { get; set; }

        /// <summary>
        /// 設定名稱
        /// </summary>
        [Required(ErrorMessage = "設定名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "設定名稱長度不能超過100個字元")]
        [Display(Name = "設定名稱")]
        public string SettingName { get; set; } = string.Empty;

        /// <summary>
        /// 換色所需點數
        /// </summary>
        [Required(ErrorMessage = "換色所需點數為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "換色所需點數必須大於等於0")]
        [Display(Name = "換色所需點數")]
        public int PointsRequired { get; set; } = 2000;

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        [Display(Name = "更新時間")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 建立者ID
        /// </summary>
        [Display(Name = "建立者ID")]
        public int CreatedBy { get; set; }

        /// <summary>
        /// 更新者ID
        /// </summary>
        [Display(Name = "更新者ID")]
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500, ErrorMessage = "備註長度不能超過500個字元")]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }
    }
}
