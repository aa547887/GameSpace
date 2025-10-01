using System.ComponentModel.DataAnnotations;

namespace Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物換背景所需點數設定 ViewModel
    /// </summary>
    public class PetBackgroundPointSettingViewModel
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 寵物等級
        /// </summary>
        [Required(ErrorMessage = "寵物等級為必填欄位")]
        [Range(1, 100, ErrorMessage = "寵物等級必須在1-100之間")]
        [Display(Name = "寵物等級")]
        public int PetLevel { get; set; }

        /// <summary>
        /// 所需點數
        /// </summary>
        [Required(ErrorMessage = "所需點數為必填欄位")]
        [Range(0, 10000, ErrorMessage = "所需點數必須在0-10000之間")]
        [Display(Name = "所需點數")]
        public int RequiredPoints { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500, ErrorMessage = "備註長度不能超過500字")]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// 寵物換背景所需點數設定列表 ViewModel
    /// </summary>
    public class PetBackgroundPointSettingListViewModel
    {
        /// <summary>
        /// 設定列表
        /// </summary>
        public List<PetBackgroundPointSettingViewModel> Settings { get; set; } = new();

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 目前頁數
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? SearchKeyword { get; set; }

        /// <summary>
        /// 是否啟用篩選
        /// </summary>
        public bool? IsEnabledFilter { get; set; }
    }
}
