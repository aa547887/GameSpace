using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models;

public partial class UserRight
{
    public int UserId { get; set; }

    public bool? UserStatus { get; set; }

    public bool? ShoppingPermission { get; set; }

    public bool? MessagePermission { get; set; }

    public bool? SalesAuthority { get; set; }

    public virtual User User { get; set; } = null!;

    // Additional properties for permission management system
    [NotMapped]
    public string RightName { get; set; } = string.Empty;

    [NotMapped]
    public string Description { get; set; } = string.Empty;

    [NotMapped]
    public string RightType { get; set; } = string.Empty;

    [NotMapped]
    public int RightLevel { get; set; }

    [NotMapped]
    public DateTime? ExpiresAt { get; set; }

    [NotMapped]
    public bool IsActive { get; set; } = true;

    [NotMapped]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public string? RightScope { get; set; }
}
