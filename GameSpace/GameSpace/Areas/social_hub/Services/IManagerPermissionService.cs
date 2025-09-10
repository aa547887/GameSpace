using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GameSpace.Areas.social_hub.Services
{
	public interface IManagerPermissionService
	{
		Task<bool> CanManageMessagePermissionsAsync(int managerId, CancellationToken ct = default);

		Task<(bool isManager, int managerId, bool canManage)> GetMuteManagementContextAsync(
			HttpContext http,
			CancellationToken ct = default);

		// ✅ 新增：是否具有 AdministratorPrivilegesManagement（可編/刪所有人）
		Task<bool> HasAdministratorPrivilegesAsync(int managerId, CancellationToken ct = default);
	}
}
