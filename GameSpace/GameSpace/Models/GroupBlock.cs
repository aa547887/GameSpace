using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class GroupBlock
{
    public int BlockId { get; set; }

    public int? GroupId { get; set; }

    public int? UserId { get; set; }

    public int? BlockedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? BlockedByNavigation { get; set; }

    public virtual Group? Group { get; set; }

    public virtual User? User { get; set; }
}
