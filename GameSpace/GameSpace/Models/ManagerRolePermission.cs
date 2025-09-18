using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ManagerRolePermission
{
    public int ManagerRoleId { get; set; }

    public string RoleName { get; set; } = null!;
    public string PermissionName { get; set; } = string.Empty;

    public bool? AdministratorPrivilegesManagement { get; set; }

    public bool? UserStatusManagement { get; set; }

    public bool? ShoppingPermissionManagement { get; set; }

    public bool? MessagePermissionManagement { get; set; }

    public bool? PetRightsManagement { get; set; }

    public bool? CustomerService { get; set; }

    // 添加缺少的屬性
    public bool? Pet_Rights_Management { get; set; }
    public bool? customer_service { get; set; }

    public virtual ICollection<ManagerDatum> Managers { get; set; } = new List<ManagerDatum>();
}
