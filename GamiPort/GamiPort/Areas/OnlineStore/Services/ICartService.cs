using System;
using System.Threading.Tasks;
using GamiPort.Areas.OnlineStore.DTO;

namespace GamiPort.Areas.OnlineStore.Services
{
	public interface ICartService
	{
		Task<Guid> EnsureCartIdAsync(int? userId, Guid? anonymousToken);

		Task AddAsync(Guid cartId, int productId, int quantity);

		Task UpdateQtyAsync(Guid cartId, int productId, int qty);

		Task RemoveAsync(Guid cartId, int productId);

		Task ClearAsync(Guid cartId);

		// 🆕 一次兩個結果集（Lines + Summary）
		Task<CartVm> GetFullAsync(Guid cartId, int shipMethodId, string destZip, string? couponCode = null);
	}
}
