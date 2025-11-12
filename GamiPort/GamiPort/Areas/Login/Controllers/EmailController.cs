using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.Login.Controllers
{
	[Area("Login")]
	[Route("[area]/[controller]/[action]")]
	public class EmailController : Controller
	{
		private readonly GameSpacedatabaseContext _db;

		public EmailController(GameSpacedatabaseContext db)
		{
			_db = db;
		}

		[HttpGet]
		public async Task<IActionResult> Confirm(int uid, string token)
		{
			if (uid <= 0 || string.IsNullOrWhiteSpace(token))
				return View("Invalid");

			var record = await _db.UserTokens
				.FirstOrDefaultAsync(t => t.UserId == uid && t.Provider == "Email" && t.Name == "Confirm");

			if (record == null || record.ExpireAt < DateTime.UtcNow)
				return View("Expired");

			if (record.Value != token)
				return View("Invalid");

			// 驗證成功
			var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == uid);
			if (user == null) return View("Invalid");

			user.UserEmailConfirmed = true;
			_db.UserTokens.Remove(record);
			await _db.SaveChangesAsync();

			return View("Success", model: user.UserAccount);
		}

		public IActionResult Success(string id) => View(model: id);
		public IActionResult Invalid() => View();
		public IActionResult Expired() => View();
	}
}
