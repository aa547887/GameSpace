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

    public virtual Admin? Admin { get; set; }

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<Mute> Mutes { get; set; } = new List<Mute>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Style> Styles { get; set; } = new List<Style>();

    public virtual ICollection<ManagerRolePermission> ManagerRoles { get; set; } = new List<ManagerRolePermission>();
}
