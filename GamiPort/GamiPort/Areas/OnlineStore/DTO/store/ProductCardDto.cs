using System;
using System.Text.Json.Serialization;

namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class ProductCardDto
    {
        [JsonPropertyName("productId")] public int ProductId { get; set; }
        [JsonPropertyName("productName")] public string ProductName { get; set; } = "";
        [JsonPropertyName("productType")] public string ProductType { get; set; } = "";
        [JsonPropertyName("price")] public decimal Price { get; set; }
        [JsonPropertyName("currencyCode")] public string CurrencyCode { get; set; } = "TWD";
        [JsonPropertyName("productCode")] public string ProductCode { get; set; } = "";
        [JsonPropertyName("coverUrl")] public string CoverUrl { get; set; } = "";
        [JsonPropertyName("platformName")] public string? PlatformName { get; set; }
        [JsonPropertyName("isPreorder")] public bool IsPreorder { get; set; }

        [JsonPropertyName("peripheralTypeName")] public string? PeripheralTypeName { get; set; }

        [JsonPropertyName("merchTypeName")] public string? MerchTypeName { get; set; }
    }
}

