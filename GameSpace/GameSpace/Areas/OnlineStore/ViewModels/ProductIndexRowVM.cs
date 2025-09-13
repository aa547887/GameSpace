namespace GameSpace.Areas.OnlineStore.ViewModels
{
	public class ProductIndexRowVM
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; } = "";
		public string ProductType { get; set; } = "";
		public decimal Price { get; set; }
		public string CurrencyCode { get; set; } = "TWD";
		public int? ShipmentQuantity { get; set; }
		public bool IsActive { get; set; } // true=上架, false=下架
		public DateTime ProductCreatedAt { get; set; }
		public int? CreatedByManagerId { get; set; }

		public DateTime? ProductUpdatedAt { get; set; }

		public int? UpdatedByManagerId { get; set; }

		public LastLogDto? LastLog { get; set; }
	}

	public class LastLogDto // 最後異動
	{
		public long LogId { get; set; }
		public int? ManagerId { get; set; }
		public DateTime ChangedAt { get; set; } //異動時間
	}
}
