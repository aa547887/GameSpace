using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class OtherProductDetail
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductDescription { get; set; }

    public int? SupplierId { get; set; }

    public int? PlatformId { get; set; }

    public string? DigitalCode { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    public string? Weight { get; set; }

    public string? Dimensions { get; set; }

    public string? Material { get; set; }

    public string? StockQuantity { get; set; }
}
