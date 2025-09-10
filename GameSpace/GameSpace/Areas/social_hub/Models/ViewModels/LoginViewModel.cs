using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.social_hub.Models
{
	public class LoginViewModel
	{
		[Required, Display(Name = "帳號")]
		public string Account { get; set; } = string.Empty;

		[Required, DataType(DataType.Password), Display(Name = "密碼")]
		public string Password { get; set; } = string.Empty;

		[Display(Name = "記住我")]
		public bool RememberMe { get; set; }

		public string? ReturnUrl { get; set; }
	}
}
