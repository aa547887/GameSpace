using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    /// <summary>
    /// 寵物換色所需點數設定
    /// </summary>
    [Table("PetColorCostSettings")]
    public class PetColorCostSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "設定名稱")]
        public string SettingName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "所需點數")]
        [Range(0, int.MaxValue, ErrorMessage = "點數必須大於等於0")]
        public int RequiredPoints { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        [StringLength(200)]
        [Display(Name = "描述")]
        public string? Description { get; set; }

        [Display(Name = "排序順序")]
        public int SortOrder { get; set; } = 0;

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
