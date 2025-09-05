using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Style
{
    public int StyleId { get; set; }

    public string? StyleName { get; set; }

    public string? EffectDesc { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ManagerId { get; set; }

    public virtual ManagerDatum? Manager { get; set; }
}
