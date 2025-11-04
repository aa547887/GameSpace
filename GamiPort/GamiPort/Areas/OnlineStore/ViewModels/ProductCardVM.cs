using System;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
    public class ProductCardVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductType { get; set; } = "";        // 你的 S_ProductInfos.ProductType
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public string CoverUrl { get; set; } = "";
        public bool IsPreorder { get; set; }                  // S_ProductInfos.IsPreorderEnabled

        // 額外標籤（遊戲）
        public string? PlatformName { get; set; }             // 例如：NS / PS5 / PC 等（從 S_GameProductDetails or 關聯表）
        public string? GenreName { get; set; }                // 例如：RPG / ACT
        // 周邊
        public string? PeripheralTypeName { get; set; }       // 例如：公仔 / 服飾 / 週邊分類
    }

    public class ProductDetailVM
    {
        public int ProductId { get; set; }
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

        // 額外資訊
        public string? PlatformName { get; set; }
        public string? GenreName { get; set; }
        public string? PeripheralTypeName { get; set; }

        // （可選）如果你未來要顯示官方影片
        public string? YoutubeUrl { get; set; }
    }

    public class PagedResult<T>
    {
        public int TotalCount { get; set; }
        public T[] Items { get; set; } = Array.Empty<T>();
    }
}
