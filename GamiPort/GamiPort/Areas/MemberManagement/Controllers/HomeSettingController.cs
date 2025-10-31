using GamiPort.Models;
using GamiPort.Services; // ICurrentUserService
using GamiPort.Areas.MemberManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	[Authorize]
	public class HomeSettingController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly ICurrentUserService _current;
		private readonly ILogger<HomeSettingController> _logger;

		public HomeSettingController(
			GameSpacedatabaseContext db,
			ICurrentUserService current,
			ILogger<HomeSettingController> logger)
		{
			_db = db;
			_current = current;
			_logger = logger;
		}

		// GET: /MemberManagement/HomeSetting/Edit
		[HttpGet]
		public async Task<IActionResult> Edit()
		{
			if (!_current.IsAuthenticated)
				return RedirectToAction("Login", "Login", new { area = "Login" });

			var uid = _current.UserId.Value;

			var home = await _db.UserHomes
				.AsNoTracking()
				.FirstOrDefaultAsync(x => x.UserId == uid);

			if (home == null)
			{
				// 尚未有記錄 → 建立一筆預設
				home = new UserHome
				{
					UserId = uid,
					Title = null,
					Theme = null,
					IsPublic = false,
					HomeCode = null,
					VisitCount = 0,
					CreatedAt = DateTime.Now,
					UpdatedAt = DateTime.Now
				};
				_db.UserHomes.Add(home);
				await _db.SaveChangesAsync();
			}

			var vm = new HomeSettingEditVM
			{
				Title = home.Title,
				IsPublic = home.IsPublic,
				HasTheme = home.Theme != null && home.Theme.Length > 0,
				ThemeSizeBytes = home.Theme?.Length,
				UpdatedAtText = home.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
			};

			return View(vm);
		}

		// POST: /MemberManagement/HomeSetting/Edit
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(HomeSettingEditVM vm)
		{
			if (!_current.IsAuthenticated)
				return RedirectToAction("Login", "Login", new { area = "Login" });

			var uid = _current.UserId.Value;

			var home = await _db.UserHomes.FirstOrDefaultAsync(x => x.UserId == uid);
			if (home == null)
			{
				home = new UserHome
				{
					UserId = uid,
					CreatedAt = DateTime.Now
				};
				_db.UserHomes.Add(home);
			}

			// 更新欄位
			home.Title = string.IsNullOrWhiteSpace(vm.Title) ? null : vm.Title.Trim();
			home.IsPublic = vm.IsPublic;

			if (vm.RemoveTheme)
			{
				home.Theme = null;
			}
			else if (vm.ThemeFile is not null && vm.ThemeFile.Length > 0)
			{
				using var ms = new MemoryStream();
				await vm.ThemeFile.CopyToAsync(ms);
				home.Theme = ms.ToArray();
			}

			home.UpdatedAt = DateTime.Now;

			try
			{
				await _db.SaveChangesAsync();
				TempData["Success"] = "小屋設定已更新成功！";
				return RedirectToAction(nameof(Edit));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "更新 HomeSetting 失敗 (UserId={UserId})", uid);
				ModelState.AddModelError(string.Empty, "儲存失敗，請稍後再試。");
			}

			vm.HasTheme = home.Theme != null && home.Theme.Length > 0;
			vm.ThemeSizeBytes = home.Theme?.Length;
			vm.UpdatedAtText = home.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");

			return View(vm);
		}
	}
}
