using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    /// <summary>
    /// 寵物互動狀態增益規則 ViewModel
    /// </summary>
    public class PetInteractionBonusRuleViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "互動類型為必填欄位")]
        [StringLength(50, ErrorMessage = "互動類型長度不能超過50個字元")]
        [Display(Name = "互動類型")]
        public string InteractionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "互動名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "互動名稱長度不能超過100個字元")]
        [Display(Name = "互動名稱")]
        public string InteractionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "所需點數為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "所需點數必須大於等於0")]
        [Display(Name = "所需點數")]
        public int PointsCost { get; set; }

        [Required(ErrorMessage = "快樂度增益為必填欄位")]
        [Range(0, 100, ErrorMessage = "快樂度增益必須在0-100之間")]
        [Display(Name = "快樂度增益")]
        public int HappinessGain { get; set; }

        [Required(ErrorMessage = "經驗值增益為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "經驗值增益必須大於等於0")]
        [Display(Name = "經驗值增益")]
        public int ExpGain { get; set; }

        [Required(ErrorMessage = "冷卻時間為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "冷卻時間必須大於等於0")]
        [Display(Name = "冷卻時間（分鐘）")]
        public int CooldownMinutes { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "描述長度不能超過500個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 寵物互動狀態增益規則建立 ViewModel
    /// </summary>
    public class PetInteractionBonusRuleCreateViewModel
    {
        [Required(ErrorMessage = "互動類型為必填欄位")]
        [StringLength(50, ErrorMessage = "互動類型長度不能超過50個字元")]
        [Display(Name = "互動類型")]
        public string InteractionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "互動名稱為必填欄位")]
        [StringLength(100, ErrorMessage = "互動名稱長度不能超過100個字元")]
        [Display(Name = "互動名稱")]
        public string InteractionName { get; set; } = string.Empty;

        [Required(ErrorMessage = "所需點數為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "所需點數必須大於等於0")]
        [Display(Name = "所需點數")]
        public int PointsCost { get; set; }

        [Required(ErrorMessage = "快樂度增益為必填欄位")]
        [Range(0, 100, ErrorMessage = "快樂度增益必須在0-100之間")]
        [Display(Name = "快樂度增益")]
        public int HappinessGain { get; set; }

        [Required(ErrorMessage = "經驗值增益為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "經驗值增益必須大於等於0")]
        [Display(Name = "經驗值增益")]
        public int ExpGain { get; set; }

        [Required(ErrorMessage = "冷卻時間為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "冷卻時間必須大於等於0")]
        [Display(Name = "冷卻時間（分鐘）")]
        public int CooldownMinutes { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        [StringLength(500, ErrorMessage = "描述長度不能超過500個字元")]
        [Display(Name = "描述")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 寵物互動狀態增益規則列表 ViewModel
    /// </summary>
    public class PetInteractionBonusRuleListViewModel
    {
        public int Id { get; set; }
        public string InteractionType { get; set; } = string.Empty;
        public string InteractionName { get; set; } = string.Empty;
        public int PointsCost { get; set; }
        public int HappinessGain { get; set; }
        public int ExpGain { get; set; }
        public int CooldownMinutes { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 寵物互動狀態增益規則搜尋 ViewModel
    /// </summary>
    public class PetInteractionBonusRuleSearchViewModel
    {
        public string? InteractionType { get; set; }
        public string? InteractionName { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "InteractionType";
        public bool SortDescending { get; set; } = false;
    }
}
