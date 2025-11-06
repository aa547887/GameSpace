using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.OnlineStore.ViewModels
{
	public sealed class CheckoutOnePageVm
	{
		// ───── 收件人資料 ─────
		[Required(ErrorMessage = "請輸入收件人姓名")]
		[Display(Name = "收件人")]
		public string Recipient { get; set; }

		[Required(ErrorMessage = "請輸入手機號碼")]
		[RegularExpression("^09\\d{8}$", ErrorMessage = "請輸入正確手機號碼格式")]
		[Display(Name = "手機號碼")]
		public string Phone { get; set; }

		// ───── 地址（分段式）─────
		[Required(ErrorMessage = "請選擇縣市")]
		[Display(Name = "縣市")]
		public string City { get; set; }

		[Required(ErrorMessage = "請選擇區")]
		[Display(Name = "區")]
		public string District { get; set; }

		[Required(ErrorMessage = "請輸入路名門牌")]
		[Display(Name = "路名/門牌")]
		public string Address1 { get; set; }

		//[Display(Name = "地址補充")]
		//public string Address2 { get; set; }

		[Required(ErrorMessage = "請輸入郵遞區號")]
		[RegularExpression("^\\d{3,5}$", ErrorMessage = "郵遞區號格式錯誤")]
		[Display(Name = "郵遞區號")]
		public string Zipcode { get; set; }

		// ───── 配送/付款（本版固定單一選項）─────
		[Required]
		[Display(Name = "配送方式")]
		public int ShipMethodId { get; set; } = 1; // 宅配

		[Required]
		[Display(Name = "付款方式")]
		public int PayMethodId { get; set; } = 1; // 信用卡

		// 改成可選：
		[Display(Name = "優惠碼")]
		[StringLength(50)]
		public string? CouponCode { get; set; }
	}
}
