using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    [Table("ManagerRolePermission")]
    public class ManagerRolePermission
    {
        [Key]
        [Column("ManagerRole_Id")]
        public int ManagerRole_Id { get; set; }

        [Column("role_name")]
        [Required]
        [StringLength(50)]
        public string role_name { get; set; } = string.Empty;

        [Column("AdministratorPrivilegesManagement")]
        public bool AdministratorPrivilegesManagement { get; set; } = false;

        [Column("UserStatusManagement")]
        public bool UserStatusManagement { get; set; } = false;

        [Column("ShoppingPermissionManagement")]
        public bool ShoppingPermissionManagement { get; set; } = false;

        [Column("MessagePermissionManagement")]
        public bool MessagePermissionManagement { get; set; } = false;

        [Column("Pet_Rights_Management")]
        public bool Pet_Rights_Management { get; set; } = false;

        [Column("customer_service")]
        public bool customer_service { get; set; } = false;

        public virtual ICollection<ManagerRole> ManagerRoles { get; set; } = new List<ManagerRole>();
    }
}
