namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class RelatedItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = "TWD";
        public string CoverUrl { get; set; } = "";
    }
}

