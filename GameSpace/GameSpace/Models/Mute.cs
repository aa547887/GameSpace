using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Mute
{
    public int MuteId { get; set; }

    public string? MuteName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public int? ManagerId { get; set; }

    public virtual ManagerDatum? Manager { get; set; }
}
