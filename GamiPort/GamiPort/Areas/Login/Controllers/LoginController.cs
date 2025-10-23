using GamiPort.Models;
using GamiPort.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // 只用 IPasswordHasher<User>
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

		// GET: /Login/Login
		[HttpGet("Login")]
		[AllowAnonymous]
		public IActionResult Login(string? returnUrl = null)
			=> View(new LoginVM { ReturnUrl = returnUrl });

		// POST: /Login/Login
		[HttpPost("Login")]
		[ValidateAntiForgeryToken]
		[AllowAnonymous]
		public async Task<IActionResult> Login(LoginVM vm)
		{
			if (!ModelState.IsValid)
				return View(vm);

			var inputAccount = (vm.UserAccount ?? string.Empty).Trim();

			// 大小寫敏感查詢
			var user = await _db.Users
				.Where(u => EF.Functions.Collate(u.UserAccount, "Latin1_General_CS_AS") == inputAccount)
				.SingleOrDefaultAsync();

			if (user == null)
			{
				ModelState.AddModelError(nameof(vm.UserAccount), "帳號或密碼錯誤。");
				return View(vm);
			}

			PasswordVerificationResult verify;
			try
			{
				// 嘗試使用 Identity 格式驗證
				verify = _hasher.VerifyHashedPassword(user, user.UserPassword, vm.UserPassword);
			}
			catch (FormatException)
			{
				// ⚠️ 若舊資料是明文，就改用明文比對
				if (user.UserPassword == vm.UserPassword)
				{
					verify = PasswordVerificationResult.Success;

					// 立刻升級為雜湊存回資料庫
					user.UserPassword = _hasher.HashPassword(user, vm.UserPassword);
					await _db.SaveChangesAsync();
				}
				else
				{
					verify = PasswordVerificationResult.Failed;
				}
			}

			if (verify == PasswordVerificationResult.Failed)
			{
				ModelState.AddModelError(nameof(vm.UserAccount), "帳號或密碼錯誤。");
				return View(vm);
			}

			// 信箱未驗證不得登入
			if (!user.UserEmailConfirmed)
			{
				ModelState.AddModelError(string.Empty, "請先完成 Email 驗證後再登入。");
				return View(vm);
			}

			var intro = await _db.UserIntroduces
				.AsNoTracking()
				.FirstOrDefaultAsync(x => x.UserId == user.UserId);

			var nick = intro?.UserNickName ?? user.UserName;
			var email = intro?.Email ?? string.Empty;

			var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            // 直接把暱稱放到 Name，前端顯示更簡單
            new Claim(ClaimTypes.Name, nick),
			new Claim(ClaimTypes.Email, email),
			new Claim("UserNickName", nick),
			new Claim("EmailConfirmed", user.UserEmailConfirmed ? "true" : "false"),
		};

			var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(identity);

			await HttpContext.SignInAsync(
				CookieAuthenticationDefaults.AuthenticationScheme,
				principal,
				new AuthenticationProperties
				{
					IsPersistent = vm.RememberMe,
					ExpiresUtc = vm.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
				});

			if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
				return LocalRedirect(vm.ReturnUrl);

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
