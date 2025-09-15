using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MemberManagement.Models;

namespace GameSpace.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	[Route("MemberManagement/[controller]")] // 固定本控制器的路由前綴：/MemberManagement/ManagerData
	public class ManagerDataController : Controller
	{
		private readonly GameSpacedatabaseContext _context;

		public ManagerDataController(GameSpacedatabaseContext context)
		{
			_context = context;
		}

		// GET /MemberManagement/ManagerData
		// 搜尋：managerId（精準）、managerName（模糊）
		// 排序：sortBy=id|date、sortDir=asc|desc
		[HttpGet("")] // Index 的屬性路由
		public async Task<IActionResult> Index(int? managerId, string? managerName,
											   string? sortBy, string? sortDir,
											   string sidebar = "admin")
		{
			sortBy = (sortBy ?? "id").ToLower();
			sortDir = (sortDir ?? "asc").ToLower();

			ViewBag.Sidebar = sidebar ?? "admin"; // 保留側欄狀態
			ViewBag.ManagerId = managerId;
			ViewBag.ManagerName = managerName;
			ViewBag.SortBy = sortBy;
			ViewBag.SortDir = sortDir;

			var q = _context.ManagerData.AsNoTracking().AsQueryable();

			if (managerId.HasValue)
				q = q.Where(m => m.ManagerId == managerId.Value);

			if (!string.IsNullOrWhiteSpace(managerName))
			{
				var key = managerName.Trim();
				q = q.Where(m => EF.Functions.Like(m.ManagerName!, $"%{key}%"));
			}

			var asc = sortDir != "desc";
			switch (sortBy)
			{
				case "date":
					q = asc
						? q.OrderBy(m => m.AdministratorRegistrationDate).ThenBy(m => m.ManagerId)
						: q.OrderByDescending(m => m.AdministratorRegistrationDate).ThenByDescending(m => m.ManagerId);
					break;
				case "id":
				default:
					q = asc ? q.OrderBy(m => m.ManagerId) : q.OrderByDescending(m => m.ManagerId);
					break;
			}

			var vms = await q
				.Select(m => new ManagerDatumVM
				{
					ManagerId = m.ManagerId,
					ManagerName = m.ManagerName,
					ManagerAccount = m.ManagerAccount,
					ManagerPassword = m.ManagerPassword,
					AdministratorRegistrationDate = m.AdministratorRegistrationDate,
					ManagerEmail = m.ManagerEmail,
					ManagerEmailConfirmed = m.ManagerEmailConfirmed,
					ManagerAccessFailedCount = m.ManagerAccessFailedCount,
					ManagerLockoutEnabled = m.ManagerLockoutEnabled,
					ManagerLockoutEnd = m.ManagerLockoutEnd
				})
				.ToListAsync();

			return View(vms);
		}

		// GET /MemberManagement/ManagerData/Details/5
		[HttpGet("Details/{id:int}")]
		public async Task<IActionResult> Details(int? id, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			if (id == null) return NotFound();

			var e = await _context.ManagerData.FirstOrDefaultAsync(x => x.ManagerId == id);
			if (e == null) return NotFound();

			return View(ToVM(e));
		}

		// GET /MemberManagement/ManagerData/Create
		[HttpGet("Create")]
		public async Task<IActionResult> Create(string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			// 顯示給使用者看（只顯示、不送出）
			ViewBag.NextManagerId = await GenerateNextManagerIdAsync();
			return View(new ManagerDatumVM());
		}

		// POST /MemberManagement/ManagerData/Create
		[HttpPost("Create")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			// 只接前端四個欄位，其餘後端自動
			[Bind("ManagerName,ManagerAccount,ManagerPassword,ManagerEmail")] ManagerDatumVM vm,
			string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";

			if (!ModelState.IsValid)
			{
				ViewBag.NextManagerId = await GenerateNextManagerIdAsync();
				return View(vm);
			}

			// 產生唯一的 ManagerId，若主鍵衝突則重試（避免並發）
			for (int attempt = 0; attempt < 3; attempt++)
			{
				var newId = await GenerateNextManagerIdAsync();

				var e = new ManagerDatum
				{
					ManagerId = newId,
					ManagerName = vm.ManagerName!,
					ManagerAccount = vm.ManagerAccount!,
					ManagerPassword = vm.ManagerPassword!,          //（若要雜湊，這裡處理）
					AdministratorRegistrationDate = DateTime.Now,   // 後端自動填入時間
					ManagerEmail = vm.ManagerEmail!,
					ManagerEmailConfirmed = false,                  // 預設
					ManagerAccessFailedCount = 0,
					ManagerLockoutEnabled = false,
					ManagerLockoutEnd = null
				};

				_context.ManagerData.Add(e);

				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index), new { area = "MemberManagement", sidebar = ViewBag.Sidebar });
				}
				catch (DbUpdateException ex)
				{
					// 避免主鍵衝突（很少見，併發時才會），重試一次
					var msg = ex.InnerException?.Message ?? ex.Message;
					var duplicateKey =
						msg.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) ||
						msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase);

					_context.Entry(e).State = EntityState.Detached;
					if (duplicateKey) continue; // 換下一個號碼再試
					ModelState.AddModelError(string.Empty, msg);
					break;
				}
			}

			// 若重試也失敗，回到畫面
			ViewBag.NextManagerId = await GenerateNextManagerIdAsync();
			return View(vm);
		}

		// ---- 放在同一個 Controller 類別內的私有方法 ----
		private async Task<int> GenerateNextManagerIdAsync()
		{
			const int start = 30000001;

			// 取目前最大值（沒有資料時回傳 0）
			var maxId = await _context.ManagerData.MaxAsync(m => (int?)m.ManagerId) ?? 0;

			// 從 max+1 與起始值中取較大者
			var candidate = Math.Max(start, maxId + 1);

			// 保險起見，確保唯一（通常不會進這個 while）
			while (await _context.ManagerData.AnyAsync(m => m.ManagerId == candidate))
				candidate++;

			return candidate;
		}

		// GET /MemberManagement/ManagerData/Edit/5
		[HttpGet("Edit/{id:int}")]
		public async Task<IActionResult> Edit(int? id, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			if (id == null) return NotFound();
			var e = await _context.ManagerData.FindAsync(id);
			if (e == null) return NotFound();
			return View(ToVM(e));
		}

		// POST /MemberManagement/ManagerData/Edit/5
		[HttpPost("Edit/{id:int}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, ManagerDatumVM vm, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			if (id != vm.ManagerId) return NotFound();
			if (!ModelState.IsValid) return View(vm);

			var e = await _context.ManagerData.FindAsync(id);
			if (e == null) return NotFound();

			e.ManagerName = vm.ManagerName;
			e.ManagerAccount = vm.ManagerAccount;
			e.ManagerPassword = vm.ManagerPassword;
			e.ManagerEmail = vm.ManagerEmail;
			e.AdministratorRegistrationDate = vm.AdministratorRegistrationDate;

			e.ManagerEmailConfirmed = vm.ManagerEmailConfirmed;
			e.ManagerAccessFailedCount = vm.ManagerAccessFailedCount;
			e.ManagerLockoutEnabled = vm.ManagerLockoutEnabled;
			e.ManagerLockoutEnd = vm.ManagerLockoutEnd;

			try { await _context.SaveChangesAsync(); }
			catch (DbUpdateException ex)
			{
				ModelState.AddModelError(string.Empty, ex.InnerException?.Message ?? ex.Message);
				return View(vm);
			}
			return RedirectToAction(nameof(Index), new { area = "MemberManagement", sidebar = ViewBag.Sidebar });
		}

		// GET /MemberManagement/ManagerData/Delete/5
		[HttpGet("Delete/{id:int}")]
		public async Task<IActionResult> Delete(int? id, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			if (id == null) return NotFound();

			var e = await _context.ManagerData.FirstOrDefaultAsync(x => x.ManagerId == id);
			if (e == null) return NotFound();

			return View(ToVM(e));
		}

		// POST /MemberManagement/ManagerData/Delete/5
		[HttpPost("Delete/{ManagerId:int}")]
		[ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int ManagerId, string sidebar = "admin")
		{
			ViewBag.Sidebar = sidebar ?? "admin";
			var entity = await _context.ManagerData.FindAsync(ManagerId);
			if (entity == null) return NotFound();

			_context.ManagerData.Remove(entity);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index), new { area = "MemberManagement", sidebar = ViewBag.Sidebar });
		}

		private static ManagerDatumVM ToVM(ManagerDatum m) => new ManagerDatumVM
		{
			ManagerId = m.ManagerId,
			ManagerName = m.ManagerName,
			ManagerAccount = m.ManagerAccount,
			ManagerPassword = m.ManagerPassword,
			AdministratorRegistrationDate = m.AdministratorRegistrationDate,
			ManagerEmail = m.ManagerEmail,
			ManagerEmailConfirmed = m.ManagerEmailConfirmed,
			ManagerAccessFailedCount = m.ManagerAccessFailedCount,
			ManagerLockoutEnabled = m.ManagerLockoutEnabled,
			ManagerLockoutEnd = m.ManagerLockoutEnd
		};

		private static ManagerDatum ToEntity(ManagerDatumVM vm) => new ManagerDatum
		{
			ManagerId = vm.ManagerId,
			ManagerName = vm.ManagerName,
			ManagerAccount = vm.ManagerAccount,
			ManagerPassword = vm.ManagerPassword,
			AdministratorRegistrationDate = vm.AdministratorRegistrationDate,
			ManagerEmail = vm.ManagerEmail,
			ManagerEmailConfirmed = vm.ManagerEmailConfirmed,
			ManagerAccessFailedCount = vm.ManagerAccessFailedCount,
			ManagerLockoutEnabled = vm.ManagerLockoutEnabled,
			ManagerLockoutEnd = vm.ManagerLockoutEnd
		};
	}
}

