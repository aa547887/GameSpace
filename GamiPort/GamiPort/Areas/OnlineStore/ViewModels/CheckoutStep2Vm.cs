using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.OnlineStore.ViewModels
{
	/// <summary>Step2：付款/優惠（伺端驗證版）</summary>
	public class CheckoutStep2Vm
	{
		[Display(Name = "付款方式")]
		[Range(1, int.MaxValue, ErrorMessage = "請選擇有效的付款方式")]
		public int PayMethodId { get; set; }

		[Display(Name = "優惠碼")]
		[StringLength(50)]
		public string? CouponCode { get; set; }
	}
}
