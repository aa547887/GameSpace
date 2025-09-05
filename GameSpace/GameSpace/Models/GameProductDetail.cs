using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class GameProductDetail
{
    public int? ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? ProductDescription { get; set; }

    public int SupplierId { get; set; }

    public int? PlatformId { get; set; }

    public string? PlatformName { get; set; }

    public string? GameType { get; set; }

    public string? DownloadLink { get; set; }
}
