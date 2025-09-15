// Areas/social_hub/Services/IManagerPermissionService.cs
namespace GameSpace.Areas.social_hub.Services
{
	/// <summary>
	/// 客服／禁言／管理權限相關的服務介面
	/// </summary>
	public record CustomerServiceContext(
		int? ManagerId,
		bool CanEnter,
		string? DenyReason // "not_logged_in" | "not_manager" | "manager_not_found" | "no_permission"
	);

	public interface IManagerPermissionService
	{
		/// <summary>此管理員是否具備「訊息權限管理」權限（MessagePermissionManagement）</summary>
		Task<bool> CanManageMessagePermissionsAsync(int managerId, CancellationToken ct = default);

		/// <summary>取得禁言管理頁的存取情境（是否管理員、其ID、是否可管理）</summary>
		Task<(bool isManager, int managerId, bool canManage)> GetMuteManagementContextAsync(
			HttpContext http,
			CancellationToken ct = default);

		/// <summary>是否具備「系統管理（AdministratorPrivilegesManagement）」權限</summary>
		Task<bool> HasAdministratorPrivilegesAsync(int managerId, CancellationToken ct = default);

		/// <summary>只有 manager + 具 customer_service 權限才開放客服工作台</summary>
		Task<CustomerServiceContext> GetCustomerServiceContextAsync(HttpContext httpContext);

		/// <summary>
		/// 具備「客服指派權」：需同時擁有 CustomerService 與 UserStatusManagement 兩個旗標。
		/// 用於判定是否可「指派給我 / 轉單」。
		/// </summary>
		Task<bool> CanUseSupportAssignmentAsync(int managerId, CancellationToken ct = default);
	}
}
