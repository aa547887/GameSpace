using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ManagerRole
{
    public int ManagerId { get; set; }
    public int ManagerRoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ManagerDatum Manager { get; set; } = null!;
    public virtual ManagerRolePermission ManagerRolePermission { get; set; } = null!;

    public virtual ICollection<ManagerRolePermission> ManagerRolePermissions { get; set; } = new List<ManagerRolePermission>();
}
