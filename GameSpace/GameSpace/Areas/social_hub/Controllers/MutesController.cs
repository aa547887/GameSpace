using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Services;
using GameSpace.Areas.social_hub.Filters;
using System.Collections.Generic;



namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class MutesController : Controller
	{
		private readonly GameSpacedatabaseContext _context;
		private readonly IMuteFilter _muteFilter;
		private readonly IManagerPermissionService _perm;

		public MutesController(GameSpacedatabaseContext context, IMuteFilter muteFilter, IManagerPermissionService perm)
		{
			_context = context;
			_muteFilter = muteFilter;
			_perm = perm;
		}

		// GET: social_hub/Mutes
		// using System.Linq; // ← 確保有
		public async Task<IActionResult> Index(int page = 1, string? q = null)
		{
			const int PageSize = 10;

			// 權限上下文
			var ctx = await _perm.GetMuteManagementContextAsync(HttpContext);
			ViewBag.CanManageMutes = ctx.canManage;
			ViewBag.IsManager = ctx.isManager;

			var canAdminAll = ctx.isManager && await _perm.HasAdministratorPrivilegesAsync(ctx.managerId);
			ViewBag.CanAdminAll = canAdminAll;
			ViewBag.ManagerId = ctx.managerId;

			// 基礎查詢（一般使用者僅看啟用）
			var baseQuery = _context.Mutes.AsNoTracking();
			if (!ctx.canManage)
				baseQuery = baseQuery.Where(m => m.IsActive == true);

			// ★ 伺服器端搜尋（跨頁）
			var qTrim = (q ?? string.Empty).Trim();
			ViewBag.Q = qTrim; // 回填搜尋框
			if (!string.IsNullOrEmpty(qTrim))
			{
				var qLower = qTrim.ToLowerInvariant();

				// 1) 明確欄位前綴：id: / mid: / mgr:
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
					// 2) 純數字 → 視為 ID 或 ManagerId（不把 "1"/"0" 當狀態）
					if (qTrim.All(char.IsDigit) && int.TryParse(qTrim, out var num))
					{
						baseQuery = baseQuery.Where(m =>
							m.MuteId == num ||
							(m.ManagerId.HasValue && m.ManagerId.Value == num));
					}
					else
					{
						// 3) 文字：詞彙包含 或 狀態關鍵字
						bool activeTrue = qLower is "啟用" or "enabled" or "enable" or "on" or "true";
						bool activeFalse = qLower is "停用" or "disabled" or "disable" or "off" or "false";

						baseQuery = baseQuery.Where(m =>
							m.MuteName.Contains(qTrim) ||
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

			// 管理員姓名對照表（當頁）
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

			var ctx = await _perm.GetMuteManagementContextAsync(HttpContext);
			var canAdminAll = ctx.isManager && await _perm.HasAdministratorPrivilegesAsync(ctx.managerId);

			ViewBag.CanManageMutes = ctx.canManage;
			ViewBag.IsManager = ctx.isManager;
			ViewBag.CanAdminAll = canAdminAll;
			ViewBag.ManagerId = ctx.managerId;

			var mute = await _context.Mutes.AsNoTracking().FirstOrDefaultAsync(m => m.MuteId == id.Value);
			if (mute == null) return NotFound();

			if (!ctx.canManage && mute.IsActive != true) return NotFound();

			if (mute.ManagerId.HasValue)
				ViewBag.ManagerName = await GetManagerNameAsync(mute.ManagerId.Value);
			else
				ViewBag.ManagerName = null;

			return View(mute);


		}

		// GET: social_hub/Mutes/Create
		[RequireMuteManagePermission]
		public IActionResult Create()
		{
			ViewBag.CanManageMutes = true;
			ViewBag.IsManager = true;
			ViewBag.CanAdminAll = true; // 只有頁面標示用途
			return View();
		}

		// POST: social_hub/Mutes/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequireMuteManagePermission]
		public async Task<IActionResult> Create([Bind("MuteName,IsActive")] Mute input)
		{
			const int MaxMuteNameLength = 50; // ← 依你的資料庫欄位長度調整（例如 nvarchar(50)）

			input.MuteName = (input.MuteName ?? string.Empty).Trim();
			if (string.IsNullOrEmpty(input.MuteName))
				ModelState.AddModelError("MuteName", "請輸入詞彙。");
			else if (input.MuteName.Length > MaxMuteNameLength)
				ModelState.AddModelError("MuteName", $"詞彙長度不可超過 {MaxMuteNameLength} 個字。");

			if (ModelState.IsValid)
			{
				bool dup = await _context.Mutes
					.AsNoTracking()
					.AnyAsync(x => x.IsActive == true && x.MuteName == input.MuteName);

				if (dup)
					ModelState.AddModelError("MuteName", "此詞已存在（啟用中）。");
			}

			if (!ModelState.IsValid)
			{
				ViewBag.CanManageMutes = true;
				ViewBag.IsManager = true;
				ViewBag.CanAdminAll = true;
				return View(input);
			}

			var entity = new Mute
			{
				MuteName = input.MuteName,
				IsActive = input.IsActive,
				CreatedAt = DateTime.Now
			};

			if (Request.Cookies.TryGetValue("gs_kind", out var k) == true && k == "manager" &&
				Request.Cookies.TryGetValue("gs_id", out var idStr) == true &&
				int.TryParse(idStr, out var mid) && mid > 0)
			{
				entity.ManagerId = mid; // 建立者
			}

			_context.Mutes.Add(entity);
			await _context.SaveChangesAsync();
			await _muteFilter.RefreshAsync();

			TempData["Msg"] = "已新增詞彙並刷新過濾規則。";
			return RedirectToAction(nameof(Index));
		}



		// GET: social_hub/Mutes/Edit/5
		[RequireMuteManagePermission]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var mute = await _context.Mutes.FindAsync(id.Value);
			if (mute == null) return NotFound();

			var ctx = await _perm.GetMuteManagementContextAsync(HttpContext);
			var canAdminAll = ctx.isManager && await _perm.HasAdministratorPrivilegesAsync(ctx.managerId);


			if (mute.ManagerId.HasValue)
				ViewBag.ManagerName = await GetManagerNameAsync(mute.ManagerId.Value);
			else
				ViewBag.ManagerName = null;

			// ✅ 非超管 → 只能編輯自己建立的
			if (!canAdminAll && mute.ManagerId != ctx.managerId) return Forbid();

			ViewBag.CanManageMutes = true;
			ViewBag.IsManager = true;
			ViewBag.CanAdminAll = canAdminAll;
			ViewBag.ManagerId = ctx.managerId;
			return View(mute);
		}

		// POST: social_hub/Mutes/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		[RequireMuteManagePermission]
		public async Task<IActionResult> Edit(int id, [Bind("MuteId,MuteName,IsActive")] Mute input)
		{
			const int MaxMuteNameLength = 50; // ← 依資料庫欄位長度調整

			if (id != input.MuteId) return NotFound();

			var entity = await _context.Mutes.FirstOrDefaultAsync(m => m.MuteId == id);
			if (entity == null) return NotFound();

			var ctx = await _perm.GetMuteManagementContextAsync(HttpContext);
			var canAdminAll = ctx.isManager && await _perm.HasAdministratorPrivilegesAsync(ctx.managerId);

			if (!canAdminAll && entity.ManagerId != ctx.managerId) return Forbid();

			input.MuteName = (input.MuteName ?? string.Empty).Trim();
			if (string.IsNullOrEmpty(input.MuteName))
				ModelState.AddModelError("MuteName", "請輸入詞彙。");
			else if (input.MuteName.Length > MaxMuteNameLength)
				ModelState.AddModelError("MuteName", $"詞彙長度不可超過 {MaxMuteNameLength} 個字。");

			bool dup = false;
			if (ModelState.IsValid)
			{
				dup = await _context.Mutes
					.AnyAsync(x => x.MuteId != id && x.IsActive == true && x.MuteName == input.MuteName);
				if (dup) ModelState.AddModelError("MuteName", "此詞已存在（啟用中）。");
			}

			if (!ModelState.IsValid)
			{
				ViewBag.CanManageMutes = true;
				ViewBag.IsManager = true;
				ViewBag.CanAdminAll = canAdminAll;
				ViewBag.ManagerId = ctx.managerId;
				ViewBag.ManagerName = entity.ManagerId.HasValue ? await GetManagerNameAsync(entity.ManagerId.Value) : null;

				entity.MuteName = input.MuteName;
				entity.IsActive = input.IsActive;
				return View(entity);
			}

			entity.MuteName = input.MuteName;
			entity.IsActive = input.IsActive;

			await _context.SaveChangesAsync();
			await _muteFilter.RefreshAsync();

			TempData["Msg"] = "已更新詞彙並刷新過濾規則。";
			return RedirectToAction(nameof(Index));
		}



		// GET: social_hub/Mutes/Delete/5
		[RequireMuteManagePermission]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var mute = await _context.Mutes.AsNoTracking().FirstOrDefaultAsync(m => m.MuteId == id.Value);
			if (mute == null) return NotFound();

			var ctx = await _perm.GetMuteManagementContextAsync(HttpContext);
			var canAdminAll = ctx.isManager && await _perm.HasAdministratorPrivilegesAsync(ctx.managerId);


			if (mute.ManagerId.HasValue)
				ViewBag.ManagerName = await GetManagerNameAsync(mute.ManagerId.Value);
			else
				ViewBag.ManagerName = null;

			// ✅ 非超管 → 只能刪除自己建立的
			if (!canAdminAll && mute.ManagerId != ctx.managerId) return Forbid();

			ViewBag.CanManageMutes = true;
			ViewBag.IsManager = true;
			ViewBag.CanAdminAll = canAdminAll;
			ViewBag.ManagerId = ctx.managerId;
			return View(mute);
		}

		// POST: social_hub/Mutes/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[RequireMuteManagePermission]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var entity = await _context.Mutes.FirstOrDefaultAsync(m => m.MuteId == id);
			if (entity == null) return RedirectToAction(nameof(Index));

			var ctx = await _perm.GetMuteManagementContextAsync(HttpContext);
			var canAdminAll = ctx.isManager && await _perm.HasAdministratorPrivilegesAsync(ctx.managerId);

			// ✅ 非超管 → 只能刪除自己建立的
			if (!canAdminAll && entity.ManagerId != ctx.managerId) return Forbid();

			_context.Mutes.Remove(entity);
			await _context.SaveChangesAsync();
			await _muteFilter.RefreshAsync();

			TempData["Msg"] = "已刪除詞彙並刷新過濾規則。";
			return RedirectToAction(nameof(Index));
		}

		// 由 ManagerDatum 物件抽出可讀姓名（兼容多種欄位名）
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
