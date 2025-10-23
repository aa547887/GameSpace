using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class ManagerRolePermission
{
    public int ManagerRoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public bool? AdministratorPrivilegesManagement { get; set; }

    public bool? UserStatusManagement { get; set; }

    public bool? ShoppingPermissionManagement { get; set; }

    public bool? MessagePermissionManagement { get; set; }

    public bool? PetRightsManagement { get; set; }

    public bool? CustomerService { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public string? DeleteReason { get; set; }

    public virtual ICollection<ManagerRole> ManagerRoles { get; set; } = new List<ManagerRole>();
}
