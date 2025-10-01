using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物顏色選項建立/編輯 ViewModel
    /// </summary>
    public class PetColorOptionViewModel
    {
        /// <summary>
        /// 顏色選項ID
        /// </summary>
        public int ColorOptionId { get; set; }

        /// <summary>
        /// 顏色名稱
        /// </summary>
        [Required(ErrorMessage = \
顏色名稱為必填欄位\)]
        [StringLength(50, ErrorMessage = \顏色名稱長度不能超過50個字元\)]
        [Display(Name = \顏色名稱\)]
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// 顏色代碼（十六進位）
        /// </summary>
        [Required(ErrorMessage = \顏色代碼為必填欄位\)]
        [StringLength(7, MinimumLength = 7, ErrorMessage = \顏色代碼必須為7個字元（包含#）\)]
        [RegularExpression(@^#[0-9A-Fa-f]
6
$, ErrorMessage = \顏色代碼格式不正確，必須為#開頭的6位十六進位數字\)]
        [Display(Name = \顏色代碼\)]
        public string ColorCode { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = \是否啟用\)]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序順序
        /// </summary>
        [Display(Name = \排序順序\)]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 備註
        /// </summary>
        [StringLength(500, ErrorMessage = \備註長度不能超過500個字元\)]
        [Display(Name = \備註\)]
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// 寵物顏色選項列表 ViewModel
    /// </summary>
    public class PetColorOptionListViewModel
    {
        /// <summary>
        /// 顏色選項列表
        /// </summary>
        public List<PetColorOption> ColorOptions { get; set; } = new List<PetColorOption>();

        /// <summary>
        /// 總數量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 啟用數量
        /// </summary>
        public int ActiveCount { get; set; }

        /// <summary>
        /// 停用數量
        /// </summary>
        public int InactiveCount { get; set; }

        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public string? SearchKeyword { get; set; }

        /// <summary>
        /// 是否只顯示啟用的選項
        /// </summary>
        public bool ShowActiveOnly { get; set; } = false;
    }
}
