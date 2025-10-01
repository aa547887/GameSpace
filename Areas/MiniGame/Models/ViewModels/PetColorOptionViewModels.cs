using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物顏色選項列表視圖模型
    /// </summary>
    public class PetColorOptionListViewModel
    {
        /// <summary>
        /// 顏色選項列表
        /// </summary>
        public List<PetColorOption> ColorOptions { get; set; } = new();

        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// 啟用狀態篩選
        /// </summary>
        public string? IsActiveFilter { get; set; }

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 目前頁碼
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 寵物顏色選項建立/編輯視圖模型
    /// </summary>
    public class PetColorOptionViewModel
    {
        /// <summary>
        /// 顏色選項ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 顏色名稱
        /// </summary>
        [Required(ErrorMessage = "顏色名稱為必填項目")]
        [StringLength(50, ErrorMessage = "顏色名稱長度不能超過50個字元")]
        [Display(Name = "顏色名稱")]
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// 顏色代碼
        /// </summary>
        [Required(ErrorMessage = "顏色代碼為必填項目")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "顏色代碼格式不正確，請使用HEX格式 (例: #FF0000)")]
        [Display(Name = "顏色代碼")]
        public string ColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 顏色描述
        /// </summary>
        [StringLength(200, ErrorMessage = "顏色描述長度不能超過200個字元")]
        [Display(Name = "顏色描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "啟用狀態")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序順序
        /// </summary>
        [Display(Name = "排序順序")]
        public int SortOrder { get; set; } = 0;
    }
}
