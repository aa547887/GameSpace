using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
	public class ProductInfoFormVM
	{
	 public int ProductId { get; set; }

	[Display(Name = "商品名稱")]
	[Required(ErrorMessage = "請輸入商品名稱")]
	[StringLength(200, ErrorMessage = "名稱長度不可超過 200 字")]
	public string ProductName { get; set; } = "";

	[Display(Name = "商品類別")]
	[Required(ErrorMessage = "請選擇商品類別")]
	[StringLength(200)]
	public string ProductType { get; set; } = "";

	[Display(Name = "價格")]
	[Range(0, 99999999, ErrorMessage = "價格必須為非負數")]
	[DataType(DataType.Currency)]
	public decimal Price { get; set; }

	[Display(Name = "幣別")]
	[Required]
	[StringLength(10)]
	public string CurrencyCode { get; set; } = "TWD";

	[Display(Name = "現貨量")]
	[Range(0, int.MaxValue, ErrorMessage = "現貨量需為 0 或正整數")]
	public int? ShipmentQuantity { get; set; }

	[Display(Name = "是否上架")]
	public bool IsActive { get; set; } = true;

	// 額外顯示用
	[Display(Name = "建立人")]
	public int? ProductCreatedBy { get; set; }

	[Display(Name = "建立時間")]
	public DateTime? ProductCreatedAt { get; set; }

	[Display(Name = "最後修改人")]
	public int? ProductUpdatedBy { get; set; }

	[Display(Name = "最後修改時間")]
	public DateTime? ProductUpdatedAt { get; set; }
}
}
