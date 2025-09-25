using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IMiniGameService
    {
        Task<UserWallet?> GetUserWalletAsync(int userId);
        Task<bool> UpdateUserPointsAsync(int userId, int points);
        Task<bool> AddUserPointsAsync(int userId, int points);
    }

    public class MiniGameService : IMiniGameService
    {
        private readonly GameSpacedatabaseContext _context;

        public MiniGameService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<UserWallet?> GetUserWalletAsync(int userId)
        {
            return await _context.UserWallets.FindAsync(userId);
        }

        public async Task<bool> UpdateUserPointsAsync(int userId, int points)
        {
            var wallet = await _context.UserWallets.FindAsync(userId);
            if (wallet != null)
            {
                wallet.UserPoint = points;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> AddUserPointsAsync(int userId, int points)
        {
            var wallet = await _context.UserWallets.FindAsync(userId);
            if (wallet != null)
            {
                wallet.UserPoint += points;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
