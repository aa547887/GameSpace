using System;

namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class ProductQuery
    {
        public string? q { get; set; }
        public string? type { get; set; }
        public int? platformId { get; set; }
        public int? genreId { get; set; }
        public int? merchTypeId { get; set; }
        public int? excludeMerchTypeId { get; set; }
        public int? supplierId { get; set; }
        public decimal? priceMin { get; set; }
        public decimal? priceMax { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 12;
        public string? sort { get; set; } // price_asc / price_desc / newest / random
    }
}
