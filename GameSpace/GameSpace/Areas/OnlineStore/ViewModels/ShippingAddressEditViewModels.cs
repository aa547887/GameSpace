using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.OnlineStore.ViewModels
{
	public class ShippingAddressEditViewModels
	{
		public int OrderId { get; set; }

		[Required, StringLength(100)]
		public string Recipient { get; set; } = null!;

		[Required, StringLength(30)]
		public string Phone { get; set; } = null!;

		[Required, StringLength(10)]
		public string Zipcode { get; set; } = null!;

		[Required, StringLength(200)]
		public string Address1 { get; set; } = null!;

		[StringLength(200)]
		public string? Address2 { get; set; }

		[Required, StringLength(50)]
		public string City { get; set; } = null!;

		[Required, StringLength(30)]
		public string Country { get; set; } = null!;

		public string OrderStatus { get; set; } = null!;

	}
}
