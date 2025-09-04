using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class VwThreadPostsFlat
{
    public long PostId { get; set; }

    public long? ThreadId { get; set; }

    public int? AuthorUserId { get; set; }

    public long? ParentPostId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ContentMd { get; set; }
}
