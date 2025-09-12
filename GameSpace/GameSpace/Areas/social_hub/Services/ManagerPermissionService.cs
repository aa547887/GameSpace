// Areas/social_hub/Services/ManagerPermissionService.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameSpace.Data;
using GameSpace.Models; // EF 實體（ManagerDatum, ManagerRolePermission 等）
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Services
{
	public sealed class ManagerPermissionService : IManagerPermissionService
	{
		private readonly GameSpacedatabaseContext _db;

		public ManagerPermissionService(GameSpacedatabaseContext db) => _db = db;

		// ------------------------ helpers ------------------------

		/// <summary>
		/// 從 Cookie 解析目前是否為管理員，以及 managerId（測試登入：gs_kind / gs_id；備援 sh_uid）
		/// </summary>
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

		/// <summary>
		/// 此管理員是否具備「訊息權限管理」權限（MessagePermissionManagement）
		/// </summary>
		public async Task<bool> CanManageMessagePermissionsAsync(int managerId, CancellationToken ct = default)
		{
			if (managerId <= 0) return false;

			// 從 ManagerRolePermission 出發，透過多對多導航 Managers 檢查
			var can = await _db.Set<ManagerRolePermission>()
				.AsNoTracking()
				.Where(p => p.MessagePermissionManagement == true) // bool? → 用 == true
				.SelectMany(p => p.Managers)                       // 導航到 ManagerDatum
				.AnyAsync(m => m.ManagerId == managerId, ct);

			return can;
		}

		/// <summary>
		/// 取得禁言管理頁的存取情境（是否管理員、其ID、是否可管理）
		/// </summary>
		public async Task<(bool isManager, int managerId, bool canManage)> GetMuteManagementContextAsync(
			HttpContext http,
			CancellationToken ct = default)
		{
			var (isManager, managerId) = ReadManagerFromCookies(http);
			var can = isManager && await CanManageMessagePermissionsAsync(managerId, ct);
			return (isManager, managerId, can);
		}

		/// <summary>
		/// 是否具備「系統管理（AdministratorPrivilegesManagement）」權限
		/// </summary>
		public async Task<bool> HasAdministratorPrivilegesAsync(int managerId, CancellationToken ct = default)
		{
			if (managerId <= 0) return false;

			var can = await _db.Set<ManagerRolePermission>()
				.AsNoTracking()
				.Where(p => p.AdministratorPrivilegesManagement == true) // bool? → 用 == true
				.SelectMany(p => p.Managers)
				.AnyAsync(m => m.ManagerId == managerId, ct);

			return can;
		}

		/// <summary>
		/// 只有 manager + 具 customer_service 權限才開放。
		/// </summary>
		public async Task<CustomerServiceContext> GetCustomerServiceContextAsync(HttpContext httpContext)
		{
			var (isManager, managerId) = ReadManagerFromCookies(httpContext);

			if (!isManager)
			{
				// 區分兩種原因，便於 Debug
				var kindRaw = httpContext.Request.Cookies.TryGetValue("gs_kind", out var k) ? k : null;
				var kind = (kindRaw ?? "user").Trim().ToLowerInvariant();

				if (kind != "manager")
					return new CustomerServiceContext(managerId == 0 ? null : managerId, false, "not_manager");

				return new CustomerServiceContext(null, false, "not_logged_in");
			}

			// 確認此管理員存在
			var exists = await _db.ManagerData
				.AsNoTracking()
				.AnyAsync(m => m.ManagerId == managerId);
			if (!exists)
				return new CustomerServiceContext(managerId, false, "manager_not_found");

			// ✅ 建議寫法：從角色表出發，完全避免 bool? 造成的委派不相容
			var can = await _db.Set<ManagerRolePermission>()
				.AsNoTracking()
				.Where(rp => rp.CustomerService == true) // bool? → 用 == true
				.SelectMany(rp => rp.Managers)
				.AnyAsync(m => m.ManagerId == managerId);

			if (!can)
				return new CustomerServiceContext(managerId, false, "no_permission");

			return new CustomerServiceContext(managerId, true, null);
		}
	}
}
