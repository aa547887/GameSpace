using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class MetricSource
{
    public int SourceId { get; set; }

    public string? Name { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedAt { get; set; }
}
