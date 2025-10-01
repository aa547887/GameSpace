using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物等級經驗值設定建立模型
    /// </summary>
    public class PetLevelExperienceSettingCreateViewModel
    {
        /// <summary>
        /// 等級
        /// </summary>
        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        [Display(Name = "等級")]
        public int Level { get; set; }

        /// <summary>
        /// 所需經驗值
        /// </summary>
        [Required(ErrorMessage = "所需經驗值為必填欄位")]
        [Range(1, int.MaxValue, ErrorMessage = "所需經驗值必須大於0")]
        [Display(Name = "所需經驗值")]
        public int RequiredExperience { get; set; }

        /// <summary>
        /// 等級名稱
        /// </summary>
        [Required(ErrorMessage = "等級名稱為必填欄位")]
        [StringLength(50, ErrorMessage = "等級名稱長度不能超過50個字元")]
        [Display(Name = "等級名稱")]
        public string LevelName { get; set; } = string.Empty;

        /// <summary>
        /// 等級描述
        /// </summary>
        [StringLength(200, ErrorMessage = "等級描述長度不能超過200個字元")]
        [Display(Name = "等級描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 寵物等級經驗值設定編輯模型
    /// </summary>
    public class PetLevelExperienceSettingEditViewModel
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 等級
        /// </summary>
        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 100, ErrorMessage = "等級必須在1-100之間")]
        [Display(Name = "等級")]
        public int Level { get; set; }

        /// <summary>
        /// 所需經驗值
        /// </summary>
        [Required(ErrorMessage = "所需經驗值為必填欄位")]
        [Range(1, int.MaxValue, ErrorMessage = "所需經驗值必須大於0")]
        [Display(Name = "所需經驗值")]
        public int RequiredExperience { get; set; }

        /// <summary>
        /// 等級名稱
        /// </summary>
        [Required(ErrorMessage = "等級名稱為必填欄位")]
        [StringLength(50, ErrorMessage = "等級名稱長度不能超過50個字元")]
        [Display(Name = "等級名稱")]
        public string LevelName { get; set; } = string.Empty;

        /// <summary>
        /// 等級描述
        /// </summary>
        [StringLength(200, ErrorMessage = "等級描述長度不能超過200個字元")]
        [Display(Name = "等級描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// 寵物等級經驗值設定列表模型
    /// </summary>
    public class PetLevelExperienceSettingListViewModel
    {
        /// <summary>
        /// 設定ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 等級
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 所需經驗值
        /// </summary>
        public int RequiredExperience { get; set; }

        /// <summary>
        /// 等級名稱
        /// </summary>
        public string LevelName { get; set; } = string.Empty;

        /// <summary>
        /// 等級描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 建立者
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// 寵物等級經驗值設定搜尋模型
    /// </summary>
    public class PetLevelExperienceSettingSearchViewModel
    {
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        [Display(Name = "搜尋關鍵字")]
        public string? Keyword { get; set; }

        /// <summary>
        /// 等級範圍 - 最小值
        /// </summary>
        [Display(Name = "等級範圍 - 最小值")]
        public int? MinLevel { get; set; }

        /// <summary>
        /// 等級範圍 - 最大值
        /// </summary>
        [Display(Name = "等級範圍 - 最大值")]
        public int? MaxLevel { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// 頁碼
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
