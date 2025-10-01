using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物換色所需點數設定 ViewModel
    /// </summary>
    public class PetColorCostSettingViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "設定名稱為必填")]
        [StringLength(50, ErrorMessage = "設定名稱長度不能超過50個字元")]
        [Display(Name = "設定名稱")]
        public string SettingName { get; set; } = string.Empty;

        [Required(ErrorMessage = "所需點數為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "點數必須大於等於0")]
        [Display(Name = "所需點數")]
        public int RequiredPoints { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        [StringLength(200, ErrorMessage = "描述長度不能超過200個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        [Display(Name = "排序順序")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物換色所需點數設定列表 ViewModel
    /// </summary>
    public class PetColorCostSettingListViewModel
    {
        public List<PetColorCostSettingViewModel> Settings { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActiveFilter { get; set; }
    }
}
