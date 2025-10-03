using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    /// <summary>
    /// 管理員角色分配表
    /// </summary>
    [Table("ManagerRole")]
    public class ManagerRole
    {
        [Key]
        [Column("Manager_Id")]
        public int ManagerId { get; set; }

        [Key]
        [Column("ManagerRole_Id")]
        public int ManagerRoleId { get; set; }

        // 導航屬性
        [ForeignKey("ManagerId")]
        public virtual ManagerDatum? ManagerData { get; set; }

        [ForeignKey("ManagerRoleId")]
        public virtual ManagerRolePermission? ManagerRolePermission { get; set; }
    }
}
