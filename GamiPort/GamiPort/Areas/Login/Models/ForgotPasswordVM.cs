using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.Login.Models
{
	public class ForgotPasswordVM
	{
		[Required(ErrorMessage = "請輸入帳號或電子郵件")]
		[Display(Name = "帳號或電子郵件")]
		public string AccountOrEmail { get; set; } = null!;
	}

	public class ResetPasswordVM
	{
		public int UserId { get; set; }
		public string Token { get; set; } = null!;

		[Required, DataType(DataType.Password)]
		[StringLength(32, MinimumLength = 8, ErrorMessage = "密碼長度需介於 8~32 字元")]
		[Display(Name = "新密碼")]
		public string NewPassword { get; set; } = null!;

		[Required, DataType(DataType.Password)]
		[Compare(nameof(NewPassword), ErrorMessage = "兩次輸入的密碼不一致")]
		[Display(Name = "確認新密碼")]
		public string ConfirmPassword { get; set; } = null!;
	}
}
