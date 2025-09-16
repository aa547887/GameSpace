using GameSpace.Areas.social_hub.Filters;     // RequireManagerPermissions
using GameSpace.Areas.social_hub.Services;
using GameSpace.Data;
using GameSpace.Infrastructure.Login;          // ILoginIdentity
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class MutesController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly IMuteFilter _muteFilter;
		private readonly ILoginIdentity _login;   // ★ 統一來源

		public MutesController(GameSpacedatabaseContext context, IMuteFilter muteFilter, ILoginIdentity login)
		{
			_context = context;
			_muteFilter = muteFilter;
			_login = login;
		}

		// 取得目前頁面所需身分資訊（唯一來源：ILoginIdentity）
		private async Task<(bool isManager, int managerId, bool canManage, bool canAdminAll)> GetUiContextAsync()
		{
			var me = await _login.GetAsync();
			var isMgr = me.IsAuthenticated && string.Equals(me.Kind, "manager", StringComparison.OrdinalIgnoreCase) && me.ManagerId.HasValue;
			var mid = isMgr ? me.ManagerId!.Value : 0;

			// 可管理頁面：只要是管理員即可（如需更細權限，這裡可以再加布林欄位檢查）
			bool canManage = isMgr;

			// 總管/可管理全部：看 ManagerRoles 是否有 AdministratorPrivilegesManagement
			bool canAdminAll = false;
			if (isMgr)
			{
				canAdminAll = await _context.ManagerData
					.AsNoTracking()
					.Where(m => m.ManagerId == mid)
					.SelectMany(m => m.ManagerRoles)
					.AnyAsync(rp => rp.AdministratorPrivilegesManagement == true);
			}

			return (isMgr, mid, canManage, canAdminAll);
		}

		// GET: social_hub/Mutes
		public async Task<IActionResult> Index(int page = 1, string? q = null)
		{
			const int PageSize = 10;

			var ctx = await GetUiContextAsync();
			ViewBag.CanManageMutes = ctx.canManage;
			ViewBag.IsManager = ctx.isManager;
			ViewBag.CanAdminAll = ctx.canAdminAll;
			ViewBag.ManagerId = ctx.managerId;

			// 一般使用者：僅看啟用
			var baseQuery = _context.Mutes.AsNoTracking();
			if (!ctx.canManage)
				baseQuery = baseQuery.Where(m => m.IsActive == true);

			// 搜尋
			var qTrim = (q ?? string.Empty).Trim();
			ViewBag.Q = qTrim;
			if (!string.IsNullOrEmpty(qTrim))
			{
				var qLower = qTrim.ToLowerInvariant();

				if (qLower.StartsWith("id:") && int.TryParse(qLower[3..], out var idv))
				{
					baseQuery = baseQuery.Where(m => m.MuteId == idv);
				}
				else if ((qLower.StartsWith("mid:") || qLower.StartsWith("mgr:")) &&
						  int.TryParse(qLower.Substring(qLower.IndexOf(':') + 1), out var midv))
				{
					baseQuery = baseQuery.Where(m => m.ManagerId.HasValue && m.ManagerId.Value == midv);
				}
				else
				{
					if (qTrim.All(char.IsDigit) && int.TryParse(qTrim, out var num))
					{
						baseQuery = baseQuery.Where(m =>
							m.MuteId == num ||
							(m.ManagerId.HasValue && m.ManagerId.Value == num));
					}
					else
					{
						bool activeTrue = qLower is "啟用" or "enabled" or "enable" or "on" or "true";
						bool activeFalse = qLower is "停用" or "disabled" or "disable" or "off" or "false";

						baseQuery = baseQuery.Where(m =>
							m.Word.Contains(qTrim) ||
							(m.Replacement != null && m.Replacement.Contains(qTrim)) ||
							(activeTrue && m.IsActive == true) ||
							(activeFalse && m.IsActive == false));
					}
				}
			}

			// 分頁
			var totalItems = await baseQuery.CountAsync();
			var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
			if (page < 1) page = 1;
			if (page > totalPages) page = totalPages;

			var list = await baseQuery
				.OrderByDescending(m => m.MuteId)
				.Skip((page - 1) * PageSize)
				.Take(PageSize)
				.ToListAsync();

			// 管理員姓名對照
			var ids = list.Where(x => x.ManagerId.HasValue).Select(x => x.ManagerId!.Value).Distinct().ToList();
			var nameMap = new Dictionary<int, string>();
			if (ids.Count > 0)
			{
				var managers = await _context.Set<ManagerDatum>()
					.AsNoTracking()
					.Where(m => ids.Contains(m.ManagerId))
					.ToListAsync();

				foreach (var mgr in managers)
				{
					var name = ExtractManagerName(mgr);
					nameMap[mgr.ManagerId] = string.IsNullOrWhiteSpace(name) ? $"管理員#{mgr.ManagerId}" : name;
				}
			}
			ViewBag.ManagerNames = nameMap;

			// 分頁資訊
			ViewBag.Page = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.TotalItems = totalItems;
			ViewBag.PageSize = PageSize;

			return View(list);
		}

		// GET: social_hub/Mutes/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var ctx = await GetUiContextAsync();
			ViewBag.CanManageMutes = ctx.canManage;
			ViewBag.IsManager = ctx.isManager;
			ViewBag.CanAdminAll = ctx.canAdminAll;
			ViewBag.ManagerId = ctx.managerId;

			var mute = await _context.Mutes.AsNoTracking().FirstOrDefaultAsync(m => m.MuteId == id.Value);
			if (mute == null) return NotFound();

			if (!ctx.canManage && mute.IsActive != true) return NotFound();

			ViewBag.ManagerName = mute.ManagerId.HasValue ? await GetManagerNameAsync(mute.ManagerId.Value) : null;
			return View(mute);
		}

		// GET: social_hub/Mutes/Create
		[RequireManagerPermissions] // 由屬性保證：一定是管理員且已登入
		public IActionResult Create()
		{
			ViewBag.CanManageMutes = true;
			ViewBag.IsManager = true;
			ViewBag.CanAdminAll = true; // 只是頁面徽章，不影響權限邏輯
			return View();
		}

		// POST: social_hub/Mutes/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequireManagerPermissions] // 由屬性保證身分
		public async Task<IActionResult> Create([Bind("Word,Replacement,IsActive")] Mute input)
		{
			const int MaxWordLength = 50;
			const int MaxReplacementLength = 50;

			input.Word = (input.Word ?? string.Empty).Trim();
			input.Replacement = (input.Replacement ?? string.Empty).Trim();

			if (string.IsNullOrEmpty(input.Word))
				ModelState.AddModelError("Word", "請輸入詞彙。");
			else if (input.Word.Length > MaxWordLength)
				ModelState.AddModelError("Word", $"詞彙長度不可超過 {MaxWordLength} 個字。");

			if (!string.IsNullOrEmpty(input.Replacement) && input.Replacement.Length > MaxReplacementLength)
				ModelState.AddModelError("Replacement", $"替代字長度不可超過 {MaxReplacementLength} 個字。");

			if (ModelState.IsValid)
			{
				var dup = await _context.Mutes.AsNoTracking().AnyAsync(x => x.Word == input.Word);
				if (dup) ModelState.AddModelError("Word", "此詞已存在。");
			}

			if (!ModelState.IsValid)
			{
				ViewBag.CanManageMutes = true;
				ViewBag.IsManager = true;
				ViewBag.CanAdminAll = true;
				return View(input);
			}

			var me = await _login.GetAsync(); // ★ 唯一來源
			var entity = new Mute
			{
				Word = input.Word,
				Replacement = string.IsNullOrWhiteSpace(input.Replacement) ? null : input.Replacement,
				IsActive = input.IsActive,
				CreatedAt = DateTime.UtcNow,
				ManagerId = (me.ManagerId ?? 0) > 0 ? me.ManagerId : null
			};

			_context.Mutes.Add(entity);
			await _context.SaveChangesAsync();
			await _muteFilter.RefreshAsync();

			TempData["Toast.Success"] = "已新增詞彙並刷新過濾規則。";
			TempData["Toast.AutoHideMs"] = 4000;
			return RedirectToAction(nameof(Index));
		}

		// GET: social_hub/Mutes/Edit/5
		[RequireManagerPermissions]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.Mutes.FindAsync(id.Value);
			if (entity == null) return NotFound();

			var ctx = await GetUiContextAsync();
			ViewBag.ManagerName = entity.ManagerId.HasValue ? await GetManagerNameAsync(entity.ManagerId.Value) : null;

			// 非總管 → 只能編輯自己建立的
			if (!(ctx.canAdminAll || (entity.ManagerId.HasValue && entity.ManagerId.Value == ctx.managerId)))
				return Forbid();

			ViewBag.CanManageMutes = true;
			ViewBag.IsManager = true;
			ViewBag.CanAdminAll = ctx.canAdminAll;
			ViewBag.ManagerId = ctx.managerId;
			return View(entity);
		}

		// POST: social_hub/Mutes/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequireManagerPermissions]
		public async Task<IActionResult> Edit(int id, [Bind("MuteId,Word,Replacement,IsActive")] Mute input)
		{
			const int MaxWordLength = 50;
			const int MaxReplacementLength = 50;

			if (id != input.MuteId) return NotFound();

			var entity = await _context.Mutes.FirstOrDefaultAsync(m => m.MuteId == id);
			if (entity == null) return NotFound();

			var ctx = await GetUiContextAsync();
			if (!(ctx.canAdminAll || (entity.ManagerId.HasValue && entity.ManagerId.Value == ctx.managerId)))
				return Forbid();

			input.Word = (input.Word ?? string.Empty).Trim();
			input.Replacement = (input.Replacement ?? string.Empty).Trim();

			if (string.IsNullOrEmpty(input.Word))
				ModelState.AddModelError("Word", "請輸入詞彙。");
			else if (input.Word.Length > MaxWordLength)
				ModelState.AddModelError("Word", $"詞彙長度不可超過 {MaxWordLength} 個字。");

			if (!string.IsNullOrEmpty(input.Replacement) && input.Replacement.Length > MaxReplacementLength)
				ModelState.AddModelError("Replacement", $"替代字長度不可超過 {MaxReplacementLength} 個字。");

			if (ModelState.IsValid)
			{
				var dup = await _context.Mutes.AnyAsync(x => x.MuteId != id && x.Word == input.Word);
				if (dup) ModelState.AddModelError("Word", "此詞已存在。");
			}

			if (!ModelState.IsValid)
			{
				ViewBag.CanManageMutes = true;
				ViewBag.IsManager = true;
				ViewBag.CanAdminAll = ctx.canAdminAll;
				ViewBag.ManagerId = ctx.managerId;
				ViewBag.ManagerName = entity.ManagerId.HasValue ? await GetManagerNameAsync(entity.ManagerId.Value) : null;

				entity.Word = input.Word;
				entity.Replacement = string.IsNullOrWhiteSpace(input.Replacement) ? null : input.Replacement;
				entity.IsActive = input.IsActive;
				return View(entity);
			}

			// 實際更新
			entity.Word = input.Word;
			entity.Replacement = string.IsNullOrWhiteSpace(input.Replacement) ? null : input.Replacement;
			entity.IsActive = input.IsActive;

			await _context.SaveChangesAsync();
			await _muteFilter.RefreshAsync();

			TempData["Toast.Success"] = "已更新詞彙並刷新過濾規則。";
			TempData["Toast.AutoHideMs"] = 4000;
			return RedirectToAction(nameof(Index));
		}

		// GET: social_hub/Mutes/Delete/5
		[RequireManagerPermissions]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.Mutes.AsNoTracking().FirstOrDefaultAsync(m => m.MuteId == id.Value);
			if (entity == null) return NotFound();

			var ctx = await GetUiContextAsync();
			ViewBag.ManagerName = entity.ManagerId.HasValue ? await GetManagerNameAsync(entity.ManagerId.Value) : null;

			if (!(ctx.canAdminAll || (entity.ManagerId.HasValue && entity.ManagerId.Value == ctx.managerId)))
				return Forbid();

			ViewBag.CanManageMutes = true;
			ViewBag.IsManager = true;
			ViewBag.CanAdminAll = ctx.canAdminAll;
			ViewBag.ManagerId = ctx.managerId;
			return View(entity);
		}

		// POST: social_hub/Mutes/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[RequireManagerPermissions]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var entity = await _context.Mutes.FirstOrDefaultAsync(m => m.MuteId == id);
			if (entity == null) return RedirectToAction(nameof(Index));

			var ctx = await GetUiContextAsync();
			if (!(ctx.canAdminAll || (entity.ManagerId.HasValue && entity.ManagerId.Value == ctx.managerId)))
				return Forbid();

			_context.Mutes.Remove(entity);
			await _context.SaveChangesAsync();
			await _muteFilter.RefreshAsync();

			TempData["Toast.Success"] = "已刪除詞彙並刷新過濾規則。";
			TempData["Toast.AutoHideMs"] = 4000;
			return RedirectToAction(nameof(Index));
		}

		// POST: 重載詞庫
		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequireManagerPermissions]
		public async Task<IActionResult> Reload()
		{
			await _muteFilter.RefreshAsync();
			TempData["Toast.Success"] = "已重載詞庫快取。";
			TempData["Toast.AutoHideMs"] = 4000;
			return RedirectToAction(nameof(Index));
		}

		// ===== Helpers =====

		private static string ExtractManagerName(object mgr)
		{
			if (mgr == null) return string.Empty;
			var t = mgr.GetType();
			foreach (var prop in new[] { "ManagerName", "Name", "DisplayName", "NickName", "Account", "UserName" })
			{
				var pi = t.GetProperty(prop);
				if (pi != null)
				{
					var val = pi.GetValue(mgr) as string;
					if (!string.IsNullOrWhiteSpace(val)) return val;
				}
			}
			return string.Empty;
		}

		private async Task<string?> GetManagerNameAsync(int managerId)
		{
			var mgr = await _context.Set<ManagerDatum>()
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ManagerId == managerId);

			if (mgr == null) return null;
			var name = ExtractManagerName(mgr);
			return string.IsNullOrWhiteSpace(name) ? $"管理員#{managerId}" : name;
		}
	}
}
