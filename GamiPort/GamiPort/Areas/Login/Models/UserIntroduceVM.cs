using GamiPort.Models;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GamiPort.Areas.Login.Models;
public partial class UserIntroduceVM
{
	public int UserId { get; set; }

	[Required, StringLength(30)]
	[Display(Name = "使用者暱稱")]
	public string UserNickName { get; set; } = null!;

	[Required, RegularExpression("^(M|F)$")]
	[Display(Name = "性別")]
	public string Gender { get; set; } = null!;

	[Required, StringLength(20)]
	[Display(Name = "身分證字號")]
	public string IdNumber { get; set; } = null!;

	[Required, Phone]
	[Display(Name = "手機號碼")]
	public string Cellphone { get; set; } = null!;

	[Required, EmailAddress]
	[Display(Name = "電子信箱")]
	public string Email { get; set; } = null!;

	[Required, StringLength(200)]
	[Display(Name = "住址")]
	public string Address { get; set; } = null!;
	[Display(Name = "出生年月日")]
	public DateOnly DateOfBirth { get; set; } = DateOnly.FromDateTime(DateTime.Today);

	public DateTime CreateAccount { get; set; }

	public byte[]? UserPicture { get; set; }
	[Display(Name = "自我介紹")]
	public string? UserIntroduce1 { get; set; }

	public virtual User User { get; set; } = null!;
}
