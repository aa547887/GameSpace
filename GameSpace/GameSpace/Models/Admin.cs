using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Admin
{
    public int ManagerId { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual ManagerDatum Manager { get; set; } = null!;
}
