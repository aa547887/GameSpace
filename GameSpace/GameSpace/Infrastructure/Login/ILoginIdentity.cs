using System.Threading;
using System.Threading.Tasks;

namespace GameSpace.Infrastructure.Login
{
	public sealed class LoginIdentityResult
	{
		public int? UserId { get; init; }
		public int? ManagerId { get; init; }
		public string Kind { get; init; } = "guest"; // "manager" | "user" | "guest"
		public int EffectiveId => ManagerId ?? UserId ?? 0;
		public bool IsAuthenticated => ManagerId.HasValue || UserId.HasValue;
		public string? DisplayName { get; init; }
	}

	public interface ILoginIdentity
	{
		Task<LoginIdentityResult> GetAsync(CancellationToken ct = default);
	}
}
