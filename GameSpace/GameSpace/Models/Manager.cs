using GameSpace.Models;

namespace GameSpace.Models
{
    /// <summary>
    /// 管理員模型
    /// </summary>
    public class Manager
    {
        public int ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerAccount { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime? AdministratorRegistrationDate { get; set; }
        public bool ManagerEmailConfirmed { get; set; }
        public bool ManagerLockoutEnabled { get; set; }
        public DateTime? ManagerLockoutEnd { get; set; }
        public int ManagerAccessFailedCount { get; set; }
        
        // 導航屬性
        public virtual ICollection<ManagerRole> ManagerRoles { get; set; } = new List<ManagerRole>();
    }

    /// <summary>
    /// 管理員角色
    /// </summary>
    public class ManagerRole
    {
        public int ManagerRoleId { get; set; }
        public int ManagerId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedDate { get; set; }
        
        // 導航屬性
        public virtual Manager Manager { get; set; } = null!;
        public virtual ManagerRolePermission ManagerRole { get; set; } = null!;
    }
}
