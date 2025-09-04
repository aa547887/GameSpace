using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class VwThreadActivity
{
    public long ThreadId { get; set; }

    public int? ForumId { get; set; }

    public string? Title { get; set; }

    public int? AuthorUserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? ReplyCount { get; set; }

    public int? LikeCount { get; set; }

    public int? BookmarkCount { get; set; }
}
