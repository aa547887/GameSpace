using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class VwForumOverview
{
    public int ForumId { get; set; }

    public int? GameId { get; set; }

    public string? ForumName { get; set; }

    public int? ThreadCount { get; set; }

    public int? ReplyCount { get; set; }

    public DateTime? LastActivityAt { get; set; }
}
