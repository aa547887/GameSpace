using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("ManagerData")]
    public class ManagerData
    {
        [Key]
        [Column("Manager_Id")]
        public int Manager_Id { get; set; }

        [Column("Manager_Name")]
        [StringLength(30)]
        public string? Manager_Name { get; set; }

        [Column("Manager_Account")]
        [StringLength(30)]
        public string? Manager_Account { get; set; }

        [Column("Manager_Password")]
        [StringLength(200)]
        public string? Manager_Password { get; set; }

        [Column("Administrator_registration_date")]
        public DateTime? Administrator_registration_date { get; set; }

        [Column("Manager_Email")]
        [Required]
        [StringLength(255)]
        public string Manager_Email { get; set; } = string.Empty;

        [Column("Manager_EmailConfirmed")]
        public bool Manager_EmailConfirmed { get; set; } = false;

        [Column("Manager_AccessFailedCount")]
        public int Manager_AccessFailedCount { get; set; } = 0;

        [Column("Manager_LockoutEnabled")]
        public bool Manager_LockoutEnabled { get; set; } = true;

        [Column("Manager_LockoutEnd")]
        public DateTime? Manager_LockoutEnd { get; set; }

        public virtual ICollection<ManagerRole> ManagerRoles { get; set; } = new List<ManagerRole>();
    }
}
