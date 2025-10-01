using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    public class PetLevelUpRuleViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 999, ErrorMessage = "等級必須在 1-999 之間")]
        [Display(Name = "等級")]
        public int Level { get; set; }
        
        [Required(ErrorMessage = "所需經驗值為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "所需經驗值必須大於等於 0")]
        [Display(Name = "所需經驗值")]
        public int ExperienceRequired { get; set; }
        
        [Required(ErrorMessage = "點數獎勵為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "點數獎勵必須大於等於 0")]
        [Display(Name = "點數獎勵")]
        public int PointsReward { get; set; }
        
        [Required(ErrorMessage = "經驗獎勵為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "經驗獎勵必須大於等於 0")]
        [Display(Name = "經驗獎勵")]
        public int ExpReward { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;
        
        [StringLength(500, ErrorMessage = "備註長度不能超過 500 字元")]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }
    }
    
    public class PetLevelUpRuleCreateViewModel
    {
        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 999, ErrorMessage = "等級必須在 1-999 之間")]
        [Display(Name = "等級")]
        public int Level { get; set; }
        
        [Required(ErrorMessage = "所需經驗值為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "所需經驗值必須大於等於 0")]
        [Display(Name = "所需經驗值")]
        public int ExperienceRequired { get; set; }
        
        [Required(ErrorMessage = "點數獎勵為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "點數獎勵必須大於等於 0")]
        [Display(Name = "點數獎勵")]
        public int PointsReward { get; set; }
        
        [Required(ErrorMessage = "經驗獎勵為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "經驗獎勵必須大於等於 0")]
        [Display(Name = "經驗獎勵")]
        public int ExpReward { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;
        
        [StringLength(500, ErrorMessage = "備註長度不能超過 500 字元")]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }
    }
    
    public class PetLevelUpRuleEditViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "等級為必填欄位")]
        [Range(1, 999, ErrorMessage = "等級必須在 1-999 之間")]
        [Display(Name = "等級")]
        public int Level { get; set; }
        
        [Required(ErrorMessage = "所需經驗值為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "所需經驗值必須大於等於 0")]
        [Display(Name = "所需經驗值")]
        public int ExperienceRequired { get; set; }
        
        [Required(ErrorMessage = "點數獎勵為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "點數獎勵必須大於等於 0")]
        [Display(Name = "點數獎勵")]
        public int PointsReward { get; set; }
        
        [Required(ErrorMessage = "經驗獎勵為必填欄位")]
        [Range(0, int.MaxValue, ErrorMessage = "經驗獎勵必須大於等於 0")]
        [Display(Name = "經驗獎勵")]
        public int ExpReward { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;
        
        [StringLength(500, ErrorMessage = "備註長度不能超過 500 字元")]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }
    }
}
