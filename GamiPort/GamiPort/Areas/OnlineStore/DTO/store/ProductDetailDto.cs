using System.Collections.Generic;

namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductType { get; set; } = "";
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public string ProductCode { get; set; } = "";
        public bool IsPreorder { get; set; }
        public string? PlatformName { get; set; }
        public int? PlatformId { get; set; }
        public List<string> Images { get; set; } = new();
        public List<RelatedItem> Related { get; set; } = new();
    }
}

