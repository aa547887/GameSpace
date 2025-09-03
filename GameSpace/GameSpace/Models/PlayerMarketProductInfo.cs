using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class PlayerMarketProductInfo
{
    public int PProductId { get; set; }

    public string? PProductType { get; set; }

    public string? PProductTitle { get; set; }

    public string? PProductName { get; set; }

    public string? PProductDescription { get; set; }

    public int? ProductId { get; set; }

    public int? SellerId { get; set; }

    public string? PStatus { get; set; }

    public decimal? Price { get; set; }

    public string? PProductImgId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
