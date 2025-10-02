using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IMiniGameService
    {
        Task<IEnumerable<MiniGame>> GetAllMiniGamesAsync();
        Task<IEnumerable<MiniGame>> GetMiniGamesByUserIdAsync(int userId);
        Task<IEnumerable<MiniGame>> GetMiniGamesByPetIdAsync(int petId);
        Task<MiniGame?> GetMiniGameByIdAsync(int gameId);
        Task<bool> CreateMiniGameAsync(MiniGame miniGame);
        Task<bool> UpdateMiniGameAsync(MiniGame miniGame);
        Task<bool> DeleteMiniGameAsync(int gameId);
        Task<bool> StartGameAsync(int userId, int petId, string gameType);
        Task<bool> EndGameAsync(int gameId, string gameResult, int pointsEarned, int petExpEarned, int? couponEarned = null);
        Task<bool> AbortGameAsync(int gameId);
        Task<IEnumerable<MiniGame>> GetGamesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetGameStatisticsAsync();
        Task<int> GetTodayGameCountByUserIdAsync(int userId);
        Task<bool> CanUserPlayTodayAsync(int userId);
    }
}
