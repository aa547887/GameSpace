using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理者角色權限表
    /// </summary>
    [Table("ManagerRolePermission")]
    public class ManagerRolePermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ManagerRole_Id { get; set; }

        [Required]
        [StringLength(50)]
        public string role_name { get; set; } = string.Empty;

        public bool AdministratorPrivilegesManagement { get; set; } = false;

        public bool UserStatusManagement { get; set; } = false;

        public bool ShoppingPermissionManagement { get; set; } = false;

        public bool MessagePermissionManagement { get; set; } = false;

        public bool Pet_Rights_Management { get; set; } = false;

        public bool customer_service { get; set; } = false;
    }
}