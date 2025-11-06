using System;

namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class ProductCardDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductType { get; set; } = "";
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public string ProductCode { get; set; } = "";
        public string CoverUrl { get; set; } = "";
        public string? PlatformName { get; set; }
        public string? PeripheralTypeName { get; set; }
        public bool IsPreorder { get; set; }

        public string? MerchTypeName { get; set; }
    }
}

