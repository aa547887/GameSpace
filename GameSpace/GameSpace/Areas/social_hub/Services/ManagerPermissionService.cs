// Areas/social_hub/Services/ManagerPermissionService.cs
using GameSpace.Models; // EF 實體（ManagerDatum, ManagerRolePermission 等）
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Services
{
	public sealed class ManagerPermissionService : IManagerPermissionService
	{
		private readonly GameSpacedatabaseContext _db;

		public ManagerPermissionService(GameSpacedatabaseContext db) => _db = db;

		// ------------------------ helpers ------------------------
		private static (bool isManager, int managerId) ReadManagerFromCookies(HttpContext http)
		{
			var kindRaw = http.Request.Cookies.TryGetValue("gs_kind", out var k) ? k : "user";
			var kind = (kindRaw ?? "user").Trim().ToLowerInvariant();

			var idStr = http.Request.Cookies.TryGetValue("gs_id", out var id1) ? id1
					 : http.Request.Cookies.TryGetValue("sh_uid", out var id2) ? id2
					 : "0";

			int managerId = 0;
			_ = int.TryParse(idStr, out managerId);

			var isManager = (kind == "manager") && managerId > 0;
			return (isManager, isManager ? managerId : 0);
		}

		// ------------------------ public APIs ------------------------
		public async Task<bool> CanManageMessagePermissionsAsync(int managerId, CancellationToken ct = default)
		{
			if (managerId <= 0) return false;

			return await _db.Set<ManagerRolePermission>()
				.AsNoTracking()
				.Where(p => p.MessagePermissionManagement == true)
				.SelectMany(p => p.Managers)
				.AnyAsync(m => m.ManagerId == managerId, ct);
		}

		public async Task<(bool isManager, int managerId, bool canManage)> GetMuteManagementContextAsync(
			HttpContext http,
			CancellationToken ct = default)
		{
			var (isManager, managerId) = ReadManagerFromCookies(http);
			var can = isManager && await CanManageMessagePermissionsAsync(managerId, ct);
			return (isManager, managerId, can);
		}

		public async Task<bool> HasAdministratorPrivilegesAsync(int managerId, CancellationToken ct = default)
		{
			if (managerId <= 0) return false;

			return await _db.Set<ManagerRolePermission>()
				.AsNoTracking()
				.Where(p => p.AdministratorPrivilegesManagement == true)
				.SelectMany(p => p.Managers)
				.AnyAsync(m => m.ManagerId == managerId, ct);
		}

		public async Task<CustomerServiceContext> GetCustomerServiceContextAsync(HttpContext httpContext)
		{
			var (isManager, managerId) = ReadManagerFromCookies(httpContext);

			if (!isManager)
			{
				var kindRaw = httpContext.Request.Cookies.TryGetValue("gs_kind", out var k) ? k : null;
				var kind = (kindRaw ?? "user").Trim().ToLowerInvariant();

				if (kind != "manager")
					return new CustomerServiceContext(managerId == 0 ? (int?)null : managerId, false, "not_manager");

				return new CustomerServiceContext(null, false, "not_logged_in");
			}

			var exists = await _db.ManagerData
				.AsNoTracking()
				.AnyAsync(m => m.ManagerId == managerId);

			if (!exists)
				return new CustomerServiceContext(managerId, false, "manager_not_found");

			var canEnter = await _db.Set<ManagerRolePermission>()
				.AsNoTracking()
				.Where(rp => rp.CustomerService == true)
				.SelectMany(rp => rp.Managers)
				.AnyAsync(m => m.ManagerId == managerId);

			if (!canEnter)
				return new CustomerServiceContext(managerId, false, "no_permission");

			return new CustomerServiceContext(managerId, true, null);
		}

		/// <summary>
		/// 僅當同時具有 CustomerService 與 UserStatusManagement 才可「指派」。
		/// </summary>
		public async Task<bool> CanUseSupportAssignmentAsync(int managerId, CancellationToken ct = default)
		{
			if (managerId <= 0) return false;

			return await _db.Set<ManagerRolePermission>()
				.AsNoTracking()
				.Where(rp => rp.CustomerService == true && rp.UserStatusManagement == true)
				.SelectMany(rp => rp.Managers)
				.AnyAsync(m => m.ManagerId == managerId, ct);
		}
	}
}
