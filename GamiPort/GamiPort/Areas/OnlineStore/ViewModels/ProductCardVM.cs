using System;

// Areas/OnlineStore/ViewModels/ProductCardVM.cs
namespace GamiPort.Areas.OnlineStore.ViewModels
{
    public class ProductCardVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductType { get; set; } = ""; // "Game" / "Other"
        public string? PlatformName { get; set; }     // 遊戲平台
        public string? PeripheralTypeName { get; set; } // 周邊類別
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public string? CoverUrl { get; set; }
        public string? ProductCode { get; set; }
        public DateTime? PublishAt { get; set; }
    }

    public class ProductQuery // [FromQuery]用這個吃
    {
        public string? Q { get; set; } = "";
        public string Sort { get; set; } = "newest"; // newest | price_asc | price_desc | random
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 40;

        // 類別篩選（預留）
        public int? PlatformId { get; set; }
        public int? GenreId { get; set; }
        public int? MerchTypeId { get; set; }
        public bool? NewOnly { get; set; } // 新品（最近 N 天）
        public bool? DiscountOnly { get; set; } // 折扣（未來你加上價格/折扣表即可）
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
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
