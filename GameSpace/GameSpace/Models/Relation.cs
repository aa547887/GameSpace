using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class Relation
{
    public int RelationId { get; set; }

    public int UserId { get; set; }

    public int FriendId { get; set; }

    public int StatusId { get; set; }

    public string? FriendNickname { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Friend { get; set; } = null!;

    public virtual RelationStatus Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
