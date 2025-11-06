using System;

namespace GamiPort.Areas.OnlineStore.ViewModels
{
    public class ProductCardVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductType { get; set; } = "";        // S_ProductInfos.ProductType
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public string CoverUrl { get; set; } = "";
        public bool IsPreorder { get; set; }                  // S_ProductInfos.IsPreorderEnabled

        // 額外資訊（卡片用）
        public string? PlatformName { get; set; }             // 例如：NS / PS5 / PC（S_GameProductDetails or 關聯表）
        public string? GenreName { get; set; }                // 例如：RPG / ACT
        public string? PeripheralTypeName { get; set; }       // 例如：公仔 / 服飾 / 週邊分類
    }

    public class ProductDetailVM
    {
        public int ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductType { get; set; } = "";
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public string? ProductDescription { get; set; }
        public bool IsPreorderEnabled { get; set; }
        public DateTime? PublishAt { get; set; }

        public string CoverUrl { get; set; } = "";
        public string[] Gallery { get; set; } = Array.Empty<string>();

        public decimal RatingAvg { get; set; }
        public int RatingCount { get; set; }

        // 商品資訊
        public string? PlatformName { get; set; }
        public string? GenreName { get; set; }
        public string? PeripheralTypeName { get; set; }

        // 供應商 / 下載連結
        public string? SupplierName { get; set; }
        public string? DownloadLink { get; set; }

        // 影片（若有）
        public string? YoutubeUrl { get; set; }

        // 週邊規格（若為週邊）
        public string? DigitalCode { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? Weight { get; set; }
        public string? Dimensions { get; set; }
        public string? Material { get; set; }
    }

    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public T[] Items { get; set; } = Array.Empty<T>();
    }

}

