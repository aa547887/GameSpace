using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IWalletService
    {
        Task<User_Wallet?> GetUserWalletAsync(int userId);
        Task<bool> UpdateUserPointsAsync(int userId, int points);
        Task<bool> AddPointsAsync(int userId, int points, string description);
        Task<bool> DeductPointsAsync(int userId, int points, string description);
        Task<IEnumerable<WalletHistory>> GetWalletHistoryAsync(int userId, int page = 1, int pageSize = 20);
        Task<bool> CreateWalletHistoryAsync(WalletHistory history);
        Task<int> GetUserPointsAsync(int userId);
    }
}
