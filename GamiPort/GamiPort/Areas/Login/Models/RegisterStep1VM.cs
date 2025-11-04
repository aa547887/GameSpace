using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.Login.Models
{
	public class RegisterStep1VM
	{
		[Required, StringLength(10)]
		[Display(Name ="姓名")]
		public string UserName { get; set; } = null!;

		// 你政策要 8~30 就保留，因為 DB 的 User_Account 是 nvarchar(30)
		[Required, StringLength(20, MinimumLength = 8, ErrorMessage = "帳號長度須為 8–20 字元")]
		[Display(Name = "使用者帳號")]
		public string UserAccount { get; set; } = null!;

		// 不設上限或設較寬，並標註為密碼型別
		[Required, StringLength(20,MinimumLength = 8, ErrorMessage = "密碼至少 8 碼")]
		[DataType(DataType.Password)]
		[Display(Name = "使用者密碼")]
		public string UserPassword { get; set; } = null!;

		// 新增確認密碼
		[Required, DataType(DataType.Password)]
		[Compare(nameof(UserPassword), ErrorMessage = "兩次輸入的密碼不一致")]
		[Display(Name = "再次輸入密碼")]
		public string ConfirmPassword { get; set; } = null!;
	}
}

