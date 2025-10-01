using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Areas.MiniGame.Models
{
    /// <summary>
    /// 管理者角色分配表
    /// </summary>
    [Table("ManagerRole")]
    public class ManagerRole
    {
        [Key, Column(Order = 0)]
        public int Manager_Id { get; set; }

        [Key, Column(Order = 1)]
        public int ManagerRole_Id { get; set; }

        // 導航屬性
        [ForeignKey("Manager_Id")]
        public virtual ManagerData ManagerData { get; set; } = null!;

        [ForeignKey("ManagerRole_Id")]
        public virtual ManagerRolePermission ManagerRolePermission { get; set; } = null!;
    }
}