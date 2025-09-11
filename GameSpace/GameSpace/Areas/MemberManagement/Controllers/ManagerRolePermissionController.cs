using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MemberManagement.Models;

namespace GameSpace.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	public class ManagerRolePermissionController : Controller
	{
		private readonly GameSpacedatabaseContext _context;

		public ManagerRolePermissionController(GameSpacedatabaseContext context)
		{
			_context = context;
		}

		// GET: MemberManagement/ManagerRolePermission
		public async Task<IActionResult> Index()
		{
			var list = await _context.ManagerRolePermissions
				.Select(u => new ManagerRolePermissionVM
				{
					ManagerRoleId = u.ManagerRoleId,
					RoleName = u.RoleName,
					AdministratorPrivilegesManagement = u.AdministratorPrivilegesManagement,
					UserStatusManagement = u.UserStatusManagement,
					ShoppingPermissionManagement = u.ShoppingPermissionManagement,
					MessagePermissionManagement = u.MessagePermissionManagement,
					PetRightsManagement = u.PetRightsManagement,
					CustomerService = u.CustomerService
				}).ToListAsync();

			return View(list);
		}

		// GET: MemberManagement/ManagerRolePermission/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.ManagerRolePermissions
				.FirstOrDefaultAsync(m => m.ManagerRoleId == id);
			if (entity == null) return NotFound();

			var vm = MapToVM(entity);
			return View(vm);
		}

		// GET: MemberManagement/ManagerRolePermission/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: MemberManagement/ManagerRolePermission/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ManagerRolePermissionVM vm)
		{
			if (ModelState.IsValid)
			{
				var entity = MapToEntity(vm);
				_context.ManagerRolePermissions.Add(entity);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(vm);
		}

		// GET: MemberManagement/ManagerRolePermission/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.ManagerRolePermissions.FindAsync(id);
			if (entity == null) return NotFound();

			var vm = MapToVM(entity);
			return View(vm);
		}

		// POST: MemberManagement/ManagerRolePermission/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, ManagerRolePermissionVM vm)
		{
			if (id != vm.ManagerRoleId) return NotFound();

			if (ModelState.IsValid)
			{
				var entity = await _context.ManagerRolePermissions.FindAsync(id);
				if (entity == null) return NotFound();

				// 更新欄位
				entity.RoleName = vm.RoleName;
				entity.AdministratorPrivilegesManagement = vm.AdministratorPrivilegesManagement;
				entity.UserStatusManagement = vm.UserStatusManagement;
				entity.ShoppingPermissionManagement = vm.ShoppingPermissionManagement;
				entity.MessagePermissionManagement = vm.MessagePermissionManagement;
				entity.PetRightsManagement = vm.PetRightsManagement;
				entity.CustomerService = vm.CustomerService;

				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(vm);
		}

		// GET: MemberManagement/ManagerRolePermission/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var entity = await _context.ManagerRolePermissions
				.FirstOrDefaultAsync(m => m.ManagerRoleId == id);
			if (entity == null) return NotFound();

			var vm = MapToVM(entity);
			return View(vm);
		}

		// POST: MemberManagement/ManagerRolePermission/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var entity = await _context.ManagerRolePermissions.FindAsync(id);
			if (entity != null)
			{
				_context.ManagerRolePermissions.Remove(entity);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}

		private bool ManagerRolePermissionExists(int id)
		{
			return _context.ManagerRolePermissions.Any(e => e.ManagerRoleId == id);
		}

		// 🔹 共用轉換方法
		private ManagerRolePermissionVM MapToVM(ManagerRolePermission entity)
		{
			return new ManagerRolePermissionVM
			{
				ManagerRoleId = entity.ManagerRoleId,
				RoleName = entity.RoleName,
				AdministratorPrivilegesManagement = entity.AdministratorPrivilegesManagement,
				UserStatusManagement = entity.UserStatusManagement,
				ShoppingPermissionManagement = entity.ShoppingPermissionManagement,
				MessagePermissionManagement = entity.MessagePermissionManagement,
				PetRightsManagement = entity.PetRightsManagement,
				CustomerService = entity.CustomerService
			};
		}

		private ManagerRolePermission MapToEntity(ManagerRolePermissionVM vm)
		{
			return new ManagerRolePermission
			{
				ManagerRoleId = vm.ManagerRoleId,
				RoleName = vm.RoleName,
				AdministratorPrivilegesManagement = vm.AdministratorPrivilegesManagement,
				UserStatusManagement = vm.UserStatusManagement,
				ShoppingPermissionManagement = vm.ShoppingPermissionManagement,
				MessagePermissionManagement = vm.MessagePermissionManagement,
				PetRightsManagement = vm.PetRightsManagement,
				CustomerService = vm.CustomerService
			};
		}
	}
}
