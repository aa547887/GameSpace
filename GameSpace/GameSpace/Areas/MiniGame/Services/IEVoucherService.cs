using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IEVoucherService
    {
        Task<IEnumerable<EVoucher>> GetAllEVouchersAsync();
        Task<IEnumerable<EVoucher>> GetEVouchersByUserIdAsync(int userId);
        Task<IEnumerable<EVoucher>> GetUnusedEVouchersByUserIdAsync(int userId);
        Task<EVoucher?> GetEVoucherByIdAsync(int eVoucherId);
        Task<EVoucher?> GetEVoucherByCodeAsync(string eVoucherCode);
        Task<bool> CreateEVoucherAsync(EVoucher eVoucher);
        Task<bool> UpdateEVoucherAsync(EVoucher eVoucher);
        Task<bool> DeleteEVoucherAsync(int eVoucherId);
        Task<bool> UseEVoucherAsync(int eVoucherId);
        Task<bool> GrantEVoucherToUserAsync(int userId, int eVoucherTypeId);
        Task<int> GetEVoucherCountByUserIdAsync(int userId);
        Task<IEnumerable<EVoucherType>> GetAllEVoucherTypesAsync();
        Task<EVoucherType?> GetEVoucherTypeByIdAsync(int eVoucherTypeId);
    }
}
