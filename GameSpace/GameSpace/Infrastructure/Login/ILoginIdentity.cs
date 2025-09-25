using System.Threading;
using System.Threading.Tasks;

namespace GameSpace.Infrastructure.Login
{
	/// <summary>
	/// 代表目前請求的登入身分（統一對外介面）。
	/// </summary>
	public sealed class LoginIdentityResult
	{
		public int? UserId { get; init; }
		public int? ManagerId { get; init; }

		/// <summary>"manager" | "user" | "guest"</summary>
		public string Kind { get; init; } = "guest";

		/// <summary>若為 manager 則回 ManagerId，否則回 UserId，預設 0。</summary>
		public int EffectiveId => ManagerId ?? UserId ?? 0;

		/// <summary>是否已有有效身分（manager 或 user）。</summary>
		public bool IsAuthenticated => ManagerId.HasValue || UserId.HasValue;

		public string? DisplayName { get; init; }
	}

	public interface ILoginIdentity
	{
		/// <summary>
		/// 取得目前請求的登入身分。
		/// </summary>
		/// <param name="ct">取消權杖（可選）</param>
		Task<LoginIdentityResult> GetAsync(CancellationToken ct = default);
	}
}
