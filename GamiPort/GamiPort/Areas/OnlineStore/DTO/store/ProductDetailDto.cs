using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class ProductDetailDto
    {
        [JsonPropertyName("productId")] public int ProductId { get; set; }
        [JsonPropertyName("productName")] public string ProductName { get; set; } = "";
        [JsonPropertyName("productType")] public string ProductType { get; set; } = "";
        [JsonPropertyName("price")] public decimal Price { get; set; }
        [JsonPropertyName("currencyCode")] public string CurrencyCode { get; set; } = "TWD";
        [JsonPropertyName("productCode")] public string ProductCode { get; set; } = "";
        [JsonPropertyName("isPreorder")] public bool IsPreorder { get; set; }
        [JsonPropertyName("platformName")] public string? PlatformName { get; set; }
        [JsonPropertyName("platformId")] public int? PlatformId { get; set; }
        [JsonPropertyName("images")] public List<string> Images { get; set; } = new();
        [JsonPropertyName("related")] public List<RelatedItem> Related { get; set; } = new();
    }
}

