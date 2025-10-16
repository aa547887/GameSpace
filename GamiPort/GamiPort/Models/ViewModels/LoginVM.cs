using System.ComponentModel.DataAnnotations;

namespace GamiPort.Models.ViewModels
{
	public class LoginVM
	{
		[Required, Display(Name = "帳號 / 使用者名稱")]
		public string UserName { get; set; } = null!;

		[Required, DataType(DataType.Password), Display(Name = "密碼")]
		public string UserPassword { get; set; } = null!;

		public string? ReturnUrl { get; set; }
		public bool RememberMe { get; internal set; }
	}
}
