using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物換背景點數設定 ViewModel
    /// </summary>
    public class PetBackgroundChangeSettingsViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "背景名稱為必填")]
        [StringLength(50, ErrorMessage = "背景名稱長度不能超過50字")]
        [Display(Name = "背景名稱")]
        public string BackgroundName { get; set; } = "";

        [Required(ErrorMessage = "所需點數為必填")]
        [Range(1, 10000, ErrorMessage = "所需點數必須在1-10000之間")]
        [Display(Name = "所需點數")]
        public int RequiredPoints { get; set; }

        [StringLength(7, ErrorMessage = "背景代碼長度不能超過7字")]
        [Display(Name = "背景代碼")]
        public string BackgroundCode { get; set; } = "";

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
