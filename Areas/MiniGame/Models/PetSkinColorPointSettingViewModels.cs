using System.ComponentModel.DataAnnotations;

namespace Areas.MiniGame.Models
{
    /// <summary>
    /// 寵物換色所需點數設定 ViewModel
    /// </summary>
    public class PetSkinColorPointSettingViewModel
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 寵物等級
        /// </summary>
        [Required(ErrorMessage = "寵物等級為必填項目")]
        [Range(1, 100, ErrorMessage = "寵物等級必須在1-100之間")]
        [Display(Name = "寵物等級")]
        public int PetLevel { get; set; }

        /// <summary>
        /// 換色所需點數
        /// </summary>
        [Required(ErrorMessage = "換色所需點數為必填項目")]
        [Range(1, 10000, ErrorMessage = "換色所需點數必須在1-10000之間")]
        [Display(Name = "換色所需點數")]
        public int RequiredPoints { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 建立者名稱
        /// </summary>
        [Display(Name = "建立者")]
        public string CreatedByName { get; set; } = string.Empty;

        /// <summary>
        /// 更新者名稱
        /// </summary>
        [Display(Name = "更新者")]
        public string UpdatedByName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 寵物換色所需點數設定列表 ViewModel
    /// </summary>
    public class PetSkinColorPointSettingListViewModel
    {
        /// <summary>
        /// 設定列表
        /// </summary>
        public List<PetSkinColorPointSettingViewModel> Settings { get; set; } = new();

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
    }
}
