using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class GameProductDetail
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductDescription { get; set; }

    public int? SupplierId { get; set; }

    public int? PlatformId { get; set; }

    public int? GameId { get; set; }

    public string? GameName { get; set; }

    public string? DownloadLink { get; set; }
}
