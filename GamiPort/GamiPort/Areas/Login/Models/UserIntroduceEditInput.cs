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

		[Required(ErrorMessage = "請輸入手機號碼")]
		[RegularExpression(@"^09\d{8}$", ErrorMessage = "請輸入正確的手機號碼格式（09 開頭，共 10 碼）")]
		[StringLength(10, MinimumLength = 10, ErrorMessage = "手機號碼必須為 10 碼")]
		[Display(Name = "手機號碼")]
		[DataType(DataType.PhoneNumber)]
		public string Cellphone { get; set; } = null!;

		[Required, StringLength(200)]
		[Display(Name = "住址")]
		public string Address { get; set; } = null!;
		[Display(Name = "自我介紹")]

		public string? UserIntroduce1 { get; set; }
	}
}