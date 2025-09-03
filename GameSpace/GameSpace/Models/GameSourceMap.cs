using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class GameSourceMap
{
    public int Id { get; set; }

    public int? GameId { get; set; }

    public int? SourceId { get; set; }

    public string? ExternalKey { get; set; }
}
