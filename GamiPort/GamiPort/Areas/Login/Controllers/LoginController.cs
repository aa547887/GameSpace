// Areas/Login/Controllers/LoginController.cs
using System.Security.Claims;
using GamiPort.Models;
using GamiPort.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity; // 只用 IPasswordHasher<User>
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.Login.Controllers
{
	[Area("Login")]
	[Route("[area]/[controller]/[action]")]
	public class LoginController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IPasswordHasher<User> _hasher;

		public LoginController(GameSpacedatabaseContext db, IPasswordHasher<User> hasher)
		{
			_db = db;
			_hasher = hasher;
		}

		// GET: /Login/Login/Login 以及 /Login/Login
		[HttpGet]
		[Route("/Login/Login")]
		[Route("/Login/Login/Login")]
		public IActionResult Login(string? returnUrl = null)
			=> View(new LoginVM { ReturnUrl = returnUrl });

		// POST: /Login/Login/Login 以及 /Login/Login
		[ValidateAntiForgeryToken]
		[HttpPost]
		[Route("/Login/Login")]
		[Route("/Login/Login/Login")]
		public async Task<IActionResult> Login(LoginVM vm)
		{
			if (!ModelState.IsValid) return View(vm);

			var inputAccount = vm.UserAccount?.Trim() ?? string.Empty;
			// ★ 大小寫敏感查詢（Latin1_General_CS_AS 只是常見的 CS 排序規則，支援 NVARCHAR）
			var user = await _db.Users
				.Where(u => EF.Functions.Collate(u.UserAccount, "Latin1_General_CS_AS") == inputAccount)
				.FirstOrDefaultAsync();


			if (user == null)
			{
				ModelState.AddModelError(nameof(vm.UserAccount), "帳號或密碼錯誤。");
				return View(vm);
			}

			var verify = PasswordVerificationResult.Failed;
			try { verify = _hasher.VerifyHashedPassword(user, user.UserPassword, vm.UserPassword); }
			catch { if (user.UserPassword == vm.UserPassword) verify = PasswordVerificationResult.Success; }

			if (verify == PasswordVerificationResult.Failed)
			{
				ModelState.AddModelError(nameof(vm.UserAccount), "帳號或密碼錯誤。");
				return View(vm);
			}

			var intro = await _db.UserIntroduces.FirstOrDefaultAsync(x => x.UserId == user.UserId);
			var nick = intro?.UserNickName ?? user.UserName;
			var email = intro?.Email ?? string.Empty;

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.Email, email),
				new Claim("UserNickName", nick),
				new Claim("EmailConfirmed", user.UserEmailConfirmed ? "true" : "false")
			};

			var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);

			await HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				principal,
				new AuthenticationProperties { IsPersistent = vm.RememberMe, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });

			if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
				return Redirect(vm.ReturnUrl);

			return RedirectToAction("Index", "Home", new { area = "" });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("/Login/Login/Logout")]
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index", "Home", new { area = "" });
		}

		[HttpGet]
		public IActionResult Denied() => View();
	}
}
