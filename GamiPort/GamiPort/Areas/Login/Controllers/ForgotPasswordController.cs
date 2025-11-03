using GamiPort.Areas.Login.Models;
using GamiPort.Areas.Login.Services; // TokenUtility
using GamiPort.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace GamiPort.Areas.Login.Controllers
{
	[Area("Login")]
	[Route("[area]/[controller]/[action]")]
	public class ForgotPasswordController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IEmailSender _email;
		private readonly IPasswordHasher<User> _hasher;

		public ForgotPasswordController(GameSpacedatabaseContext db, IEmailSender email, IPasswordHasher<User> hasher)
		{
			_db = db;
			_email = email;
			_hasher = hasher;
		}

		// Step 1：請求重設密碼
		[HttpGet]
		public IActionResult Start() => View(new ForgotPasswordVM());

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Start(ForgotPasswordVM vm)
		{
			if (!ModelState.IsValid) return View(vm);

			var input = vm.AccountOrEmail.Trim();
			var user = await _db.Users
				.FirstOrDefaultAsync(u =>
					EF.Functions.Collate(u.UserAccount, "Latin1_General_CS_AS") == input ||
					_db.UserIntroduces.Any(i => i.UserId == u.UserId && i.Email == input));

			if (user == null)
			{
				ViewBag.Message = "若該帳號存在，重設密碼信已寄出。";
				return View("StartSent");
			}

			// 建立 Token
			var token = TokenUtility.NewUrlSafeToken();
			var provider = "Password";
			var name = "Reset";

			// 清理舊 token
			var olds = _db.UserTokens.Where(t => t.UserId == user.UserId && t.Provider == provider && t.Name == name);
			_db.UserTokens.RemoveRange(olds);

			_db.UserTokens.Add(new UserToken
			{
				UserId = user.UserId,
				Provider = provider,
				Name = name,
				Value = token,
				ExpireAt = DateTime.UtcNow.AddHours(1)
			});
			await _db.SaveChangesAsync();

			// 組信件
			var resetUrl = Url.Action("Reset", "ForgotPassword",
				new { area = "Login", uid = user.UserId, token }, Request.Scheme);

			await _email.SendAsync(
				to: _db.UserIntroduces.FirstOrDefault(x => x.UserId == user.UserId)?.Email ?? "",
				"重設密碼通知",
				$"請點擊以下連結重設您的密碼（1 小時內有效）：\n{resetUrl}"
			);

			ViewBag.Message = "若該帳號存在，重設密碼信已寄出。";
			return View("StartSent");
		}

		// Step 2：開啟重設密碼頁面
		[HttpGet]
		public async Task<IActionResult> Reset(int uid, string token)
		{
			var record = await _db.UserTokens
				.FirstOrDefaultAsync(t => t.UserId == uid && t.Provider == "Password" && t.Name == "Reset");

			if (record == null || record.ExpireAt < DateTime.UtcNow || record.Value != token)
				return View("InvalidToken");

			return View(new ResetPasswordVM { UserId = uid, Token = token });
		}

		// Step 3：提交新密碼
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Reset(ResetPasswordVM vm)
		{
			if (!ModelState.IsValid) return View(vm);

			var record = await _db.UserTokens
				.FirstOrDefaultAsync(t => t.UserId == vm.UserId && t.Provider == "Password" && t.Name == "Reset");

			if (record == null || record.ExpireAt < DateTime.UtcNow || record.Value != vm.Token)
				return View("InvalidToken");

			var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == vm.UserId);
			if (user == null) return View("InvalidToken");

			user.UserEmailConfirmed = true;            
			user.UserAccessFailedCount = 0;           
			user.UserPassword = _hasher.HashPassword(user, vm.NewPassword);
			_db.UserTokens.Remove(record);
			await _db.SaveChangesAsync();

			return View("ResetSuccess");
		}

		// Views
		public IActionResult RequestSent() => View();
		public IActionResult InvalidToken() => View();
		public IActionResult ResetSuccess() => View();
	}
}
