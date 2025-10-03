using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
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
        public virtual ManagerData ManagerData { get; set; } = null!;

        [ForeignKey("ManagerRoleId")]
        public virtual ManagerRolePermission ManagerRolePermission { get; set; } = null!;
    }
}

