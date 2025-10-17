using System;
using System.Threading.Tasks;
using GamiPort.Areas.OnlineStore.DTO;

namespace GamiPort.Areas.OnlineStore.Services
{
	public interface ICartService
	{
		Task<Guid> EnsureCartIdAsync(int? userId, Guid? anonymousToken);
		Task AddAsync(Guid cartId, int productId, int quantity);
		Task<CartSummaryDto> GetAsync(Guid cartId);
		Task UpdateQtyAsync(Guid cartId, int productId, int qty);
		Task RemoveAsync(Guid cartId, int productId);
		Task ClearAsync(Guid cartId);
	}
}
