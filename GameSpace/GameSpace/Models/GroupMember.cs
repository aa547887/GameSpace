using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class GroupMember
{
    public int GroupId { get; set; }

    public int UserId { get; set; }

    public DateTime? JoinedAt { get; set; }

    public bool IsAdmin { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
