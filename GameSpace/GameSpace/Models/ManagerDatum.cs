using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ManagerDatum
{
    public int ManagerId { get; set; }

    public string? ManagerName { get; set; }

    public string? ManagerAccount { get; set; }

    public string? ManagerPassword { get; set; }

    public DateTime? AdministratorRegistrationDate { get; set; }

    public string ManagerEmail { get; set; } = null!;

    public bool ManagerEmailConfirmed { get; set; }

    public int ManagerAccessFailedCount { get; set; }

    public bool ManagerLockoutEnabled { get; set; }

    public DateTime? ManagerLockoutEnd { get; set; }
}
