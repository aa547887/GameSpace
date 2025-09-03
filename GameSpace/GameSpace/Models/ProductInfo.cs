using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ProductInfo
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductType { get; set; }

    public decimal? Price { get; set; }

    public string? CurrencyCode { get; set; }

    public int? ShipmentQuantity { get; set; }

    public string? ProductCreatedBy { get; set; }

    public DateTime? ProductCreatedAt { get; set; }

    public string? ProductUpdatedBy { get; set; }

    public DateTime? ProductUpdatedAt { get; set; }

    public int? UserId { get; set; }
}
