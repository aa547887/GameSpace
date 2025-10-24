using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.OnlineStore.ViewModels
{
	/// <summary>Step1：收件/配送（伺端驗證版）</summary>
	public class CheckoutStep1Vm
	{
		[Display(Name = "配送方式")]
		[Range(1, int.MaxValue, ErrorMessage = "請選擇有效的配送方式")]
		public int ShipMethodId { get; set; }

		[Display(Name = "收件人")]
		[Required(ErrorMessage = "收件人為必填")]
		[StringLength(100)]
		public string Recipient { get; set; } = null!;

		[Display(Name = "電話")]
		[Required(ErrorMessage = "電話為必填")]
		[Phone(ErrorMessage = "電話格式不正確")]
		[StringLength(30)]
		public string Phone { get; set; } = null!;

		[Display(Name = "郵遞區號")]
		[Required(ErrorMessage = "郵遞區號為必填")]
		[RegularExpression(@"^\d{3}(\d{2})?$", ErrorMessage = "郵遞區號需為 3 或 5 碼數字")]
		public string DestZip { get; set; } = null!;

		[Display(Name = "地址")]
		[Required(ErrorMessage = "地址為必填")]
		[StringLength(200)]
		public string Address1 { get; set; } = null!;

		[StringLength(200)]
		public string? Address2 { get; set; }
	}
}
