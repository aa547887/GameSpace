using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理者資料表
    /// </summary>
    [Table("ManagerData")]
    public class ManagerData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Manager_Id { get; set; }

        [StringLength(30)]
        public string? Manager_Name { get; set; }

        [StringLength(30)]
        public string? Manager_Account { get; set; }

        [StringLength(200)]
        public string? Manager_Password { get; set; }

        public DateTime? Administrator_registration_date { get; set; }

        [Required]
        [StringLength(255)]
        public string Manager_Email { get; set; } = string.Empty;

        public bool Manager_EmailConfirmed { get; set; } = false;

        public int Manager_AccessFailedCount { get; set; } = 0;

        public bool Manager_LockoutEnabled { get; set; } = true;

        public DateTime? Manager_LockoutEnd { get; set; }
    }
}