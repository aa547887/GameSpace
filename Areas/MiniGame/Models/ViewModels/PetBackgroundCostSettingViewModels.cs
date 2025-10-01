using System.ComponentModel.DataAnnotations;

namespace Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物換背景所需點數設定 ViewModel
    /// </summary>
    public class PetBackgroundCostSettingViewModels
    {
        /// <summary>
        /// 建立寵物換背景所需點數設定 ViewModel
        /// </summary>
        public class CreateViewModel
        {
            [Required(ErrorMessage = "設定名稱為必填欄位")]
            [StringLength(50, ErrorMessage = "設定名稱長度不能超過50個字元")]
            [Display(Name = "設定名稱")]
            public string SettingName { get; set; } = string.Empty;

            [Required(ErrorMessage = "所需點數為必填欄位")]
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
        }

        /// <summary>
        /// 編輯寵物換背景所需點數設定 ViewModel
        /// </summary>
        public class EditViewModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "設定名稱為必填欄位")]
            [StringLength(50, ErrorMessage = "設定名稱長度不能超過50個字元")]
            [Display(Name = "設定名稱")]
            public string SettingName { get; set; } = string.Empty;

            [Required(ErrorMessage = "所需點數為必填欄位")]
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
        }

        /// <summary>
        /// 寵物換背景所需點數設定列表 ViewModel
        /// </summary>
        public class IndexViewModel
        {
            public List<PetBackgroundCostSettingItem> Items { get; set; } = new();
            public string? SearchTerm { get; set; }
            public bool? IsActiveFilter { get; set; }
            public int CurrentPage { get; set; } = 1;
            public int TotalPages { get; set; }
            public int PageSize { get; set; } = 10;
            public int TotalItems { get; set; }
        }

        /// <summary>
        /// 寵物換背景所需點數設定項目
        /// </summary>
        public class PetBackgroundCostSettingItem
        {
            public int Id { get; set; }
            public string SettingName { get; set; } = string.Empty;
            public int RequiredPoints { get; set; }
            public bool IsActive { get; set; }
            public string? Description { get; set; }
            public int SortOrder { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}
