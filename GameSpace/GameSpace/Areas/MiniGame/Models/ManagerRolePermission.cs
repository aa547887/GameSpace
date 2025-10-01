using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理員角色權限表
    /// </summary>
    [Table("ManagerRolePermission")]
    public class ManagerRolePermission
    {
        [Key]
        [Column("ManagerRole_Id")]
        public int ManagerRoleId { get; set; }

        [Column("role_name")]
        [MaxLength(50)]
        [Required]
        public string RoleName { get; set; } = string.Empty;

        [Column("AdministratorPrivilegesManagement")]
        public bool AdministratorPrivilegesManagement { get; set; } = false;

        [Column("UserStatusManagement")]
        public bool UserStatusManagement { get; set; } = false;

        [Column("ShoppingPermissionManagement")]
        public bool ShoppingPermissionManagement { get; set; } = false;

        [Column("MessagePermissionManagement")]
        public bool MessagePermissionManagement { get; set; } = false;

        [Column("Pet_Rights_Management")]
        public bool PetRightsManagement { get; set; } = false;

        [Column("customer_service")]
        public bool CustomerService { get; set; } = false;

        // 導航屬性
        public virtual ICollection<ManagerRole> ManagerRoles { get; set; } = new List<ManagerRole>();
    }
}
