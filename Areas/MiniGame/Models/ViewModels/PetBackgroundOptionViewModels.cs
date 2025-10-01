using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物背景選項列表視圖模型
    /// </summary>
    public class PetBackgroundOptionListViewModel
    {
        /// <summary>
        /// 背景選項列表
        /// </summary>
        public List<PetBackgroundOption> BackgroundOptions { get; set; } = new();

        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        [Display(Name = "搜尋關鍵字")]
        public string? SearchKeyword { get; set; }

        /// <summary>
        /// 狀態篩選
        /// </summary>
        [Display(Name = "狀態篩選")]
        public bool? IsActiveFilter { get; set; }

        /// <summary>
        /// 目前頁碼
        /// </summary>
        public int CurrentPage { get; set; } = 1;

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 寵物背景選項表單視圖模型
    /// </summary>
    public class PetBackgroundOptionFormViewModel
    {
        /// <summary>
        /// 背景選項ID
        /// </summary>
        public int? Id { get; set; }

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
    }
}
