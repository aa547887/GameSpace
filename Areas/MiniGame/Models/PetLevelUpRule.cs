using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("PetLevelUpRules")]
    public class PetLevelUpRule
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "等級")]
        public int Level { get; set; }
        
        [Required]
        [Display(Name = "所需經驗值")]
        public int ExperienceRequired { get; set; }
        
        [Required]
        [Display(Name = "點數獎勵")]
        public int PointsReward { get; set; }
        
        [Required]
        [Display(Name = "經驗獎勵")]
        public int ExpReward { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "備註")]
        [StringLength(500)]
        public string? Remarks { get; set; }
        
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "建立者")]
        public int CreatedBy { get; set; }
        
        [Display(Name = "更新者")]
        public int UpdatedBy { get; set; }
    }
}
