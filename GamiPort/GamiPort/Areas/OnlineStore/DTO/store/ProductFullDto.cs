using System;
using System.Collections.Generic;

namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class ProductFullDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public string ProductCode { get; set; } = string.Empty;

        public bool IsPreorder { get; set; }
        public bool IsPhysical { get; set; }

        public int? PlatformId { get; set; }
        public string? PlatformName { get; set; }
        public string? PeripheralTypeName { get; set; }

        public string? ProductDescription { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishAt { get; set; }
        public DateTime? UnpublishAt { get; set; }

        public int? SafetyStock { get; set; }

        public decimal? RatingAvg { get; set; }
        public int? RatingCount { get; set; }

        public List<string> Images { get; set; } = new();
        public List<string> Genres { get; set; } = new();
    }
}
