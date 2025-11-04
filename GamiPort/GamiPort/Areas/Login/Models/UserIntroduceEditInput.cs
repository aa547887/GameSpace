using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.Login.Models
{
	public class UserIntroduceEditInput
	{
		[Required, StringLength(30)]
		[Display(Name = "使用者暱稱")]
		public string UserNickName { get; set; } = null!;

		[Required, RegularExpression("^(M|F)$")]
		[Display(Name = "性別")]
		public string Gender { get; set; } = null!;

		[Required, Phone, StringLength(20)]
		[Display(Name = "手機號碼")]
		public string Cellphone { get; set; } = null!;

		[Required, StringLength(200)]
		[Display(Name = "住址")]
		public string Address { get; set; } = null!;
		[Display(Name = "自我介紹")]

		public string? UserIntroduce1 { get; set; }
	}
}