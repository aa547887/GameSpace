using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class ProductFullDto
    {
        [JsonPropertyName("productId")] public int ProductId { get; set; }
        [JsonPropertyName("productName")] public string ProductName { get; set; } = string.Empty;
        [JsonPropertyName("productType")] public string ProductType { get; set; } = string.Empty;
        [JsonPropertyName("price")] public decimal Price { get; set; }
        [JsonPropertyName("currencyCode")] public string CurrencyCode { get; set; } = "TWD";
        [JsonPropertyName("productCode")] public string ProductCode { get; set; } = string.Empty;

        [JsonPropertyName("isPreorder")] public bool IsPreorder { get; set; }
        [JsonPropertyName("isPhysical")] public bool IsPhysical { get; set; }

        [JsonPropertyName("platformId")] public int? PlatformId { get; set; }
        [JsonPropertyName("platformName")] public string? PlatformName { get; set; }
        [JsonPropertyName("peripheralTypeName")] public string? PeripheralTypeName { get; set; }

        [JsonPropertyName("productDescription")] public string? ProductDescription { get; set; }

        [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")] public DateTime? UpdatedAt { get; set; }
        [JsonPropertyName("publishAt")] public DateTime? PublishAt { get; set; }
        [JsonPropertyName("unpublishAt")] public DateTime? UnpublishAt { get; set; }

        [JsonPropertyName("safetyStock")] public int? SafetyStock { get; set; }

        [JsonPropertyName("ratingAvg")] public decimal? RatingAvg { get; set; }
        [JsonPropertyName("ratingCount")] public int? RatingCount { get; set; }

        [JsonPropertyName("images")] public List<string> Images { get; set; } = new();
        [JsonPropertyName("genres")] public List<string> Genres { get; set; } = new();
    }
}
