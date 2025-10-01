using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物背景選項建立/編輯 ViewModel
    /// </summary>
    public class PetBackgroundOptionViewModel
    {
        /// <summary>
        /// 背景選項ID
        /// </summary>
        public int BackgroundOptionId { get; set; }

        /// <summary>
        /// 背景名稱
        /// </summary>
        [Required(ErrorMessage = \
背景名稱為必填欄位\)]
        [StringLength(50, ErrorMessage = \背景名稱長度不能超過50個字元\)]
        [Display(Name = \背景名稱\)]
        public string BackgroundName { get; set; } = string.Empty;

        /// <summary>
        /// 背景描述
        /// </summary>
        [StringLength(200, ErrorMessage = \背景描述長度不能超過200個字元\)]
        [Display(Name = \背景描述\)]
        public string? Description { get; set; }

        /// <summary>
        /// 背景圖片路徑
        /// </summary>
        [StringLength(500, ErrorMessage = \背景圖片路徑長度不能超過500個字元\)]
        [Display(Name = \背景圖片路徑\)]
        public string? ImagePath { get; set; }

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
    /// 寵物背景選項列表 ViewModel
    /// </summary>
    public class PetBackgroundOptionListViewModel
    {
        /// <summary>
        /// 背景選項列表
        /// </summary>
        public List<PetBackgroundOption> BackgroundOptions { get; set; } = new List<PetBackgroundOption>();

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
