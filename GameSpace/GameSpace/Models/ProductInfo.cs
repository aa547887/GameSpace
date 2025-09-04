using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class ProductInfo
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string ProductType { get; set; } = null!;

    public decimal Price { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public int ShipmentQuantity { get; set; }

    public string ProductCreatedBy { get; set; } = null!;

    public DateTime ProductCreatedAt { get; set; }

    public string? ProductUpdatedBy { get; set; }

    public DateTime? ProductUpdatedAt { get; set; }

    public virtual GameProductDetail? GameProductDetail { get; set; }

    public virtual ICollection<OfficialStoreRanking> OfficialStoreRankings { get; set; } = new List<OfficialStoreRanking>();

    public virtual OtherProductDetail? OtherProductDetail { get; set; }

    public virtual ICollection<ProductCode> ProductCodes { get; set; } = new List<ProductCode>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductInfoAuditLog> ProductInfoAuditLogs { get; set; } = new List<ProductInfoAuditLog>();
}
