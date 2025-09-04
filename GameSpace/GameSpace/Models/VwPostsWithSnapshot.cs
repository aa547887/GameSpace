using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class VwPostsWithSnapshot
{
    public int PostId { get; set; }

    public int? GameId { get; set; }

    public string? Title { get; set; }

    public string? Tldr { get; set; }

    public string? Status { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateOnly? SnapshotDate { get; set; }

    public decimal? IndexValue { get; set; }

    public int? SourceCount { get; set; }
}
