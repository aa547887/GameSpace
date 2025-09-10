// Areas/social_hub/Services/ManagerPermissionService.cs
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameSpace.Data;
using GameSpace.Models; // 要有
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Services
{
	public sealed class ManagerPermissionService : IManagerPermissionService
	{
		private readonly GameSpacedatabaseContext _db;

		public ManagerPermissionService(GameSpacedatabaseContext db) => _db = db;

		public async Task<bool> CanManageMessagePermissionsAsync(int managerId, CancellationToken ct = default)
		{
			if (managerId <= 0) return false;

			// ✅ 直接從 ManagerRolePermission 出發，利用導航屬性 Managers 檢查此 manager 是否屬於任何「有 MessagePermissionManagement==true」的角色
			var can = await _db.Set<ManagerRolePermission>()
				.Where(p => p.MessagePermissionManagement == true)
				.SelectMany(p => p.Managers)           // 導航到 ManagerDatum
				.AnyAsync(m => m.ManagerId == managerId, ct);

			return can;
		}

		public async Task<(bool isManager, int managerId, bool canManage)> GetMuteManagementContextAsync(
			HttpContext http,
			CancellationToken ct = default)
		{
			bool isManager = false;
			int managerId = 0;

			if (http.Request.Cookies.TryGetValue("gs_kind", out var kind) &&
				kind == "manager" &&
				http.Request.Cookies.TryGetValue("gs_id", out var idStr) &&
				int.TryParse(idStr, out managerId) && managerId > 0)
			{
				isManager = true;
			}

			var can = isManager && await CanManageMessagePermissionsAsync(managerId, ct);
			return (isManager, managerId, can);
		}

		public async Task<bool> HasAdministratorPrivilegesAsync(int managerId, CancellationToken ct = default)
		{
			if (managerId <= 0) return false;

			var can = await _db.Set<ManagerRolePermission>()
				.Where(p => p.AdministratorPrivilegesManagement == true)
				.SelectMany(p => p.Managers)              // 導航到 ManagerDatum
				.AnyAsync(m => m.ManagerId == managerId, ct);

			return can;
		}

	}
}
