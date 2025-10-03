using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IMiniGameService
    {
        Task<IEnumerable<Models.MiniGame>> GetAllMiniGamesAsync();
        Task<IEnumerable<Models.MiniGame>> GetMiniGamesByUserIdAsync(int userId);
        Task<IEnumerable<Models.MiniGame>> GetMiniGamesByPetIdAsync(int petId);
        Task<Models.MiniGame?> GetMiniGameByIdAsync(int gameId);
        Task<bool> CreateMiniGameAsync(Models.MiniGame miniGame);
        Task<bool> UpdateMiniGameAsync(Models.MiniGame miniGame);
        Task<bool> DeleteMiniGameAsync(int gameId);
        Task<bool> StartGameAsync(int userId, int petId, string gameType);
        Task<bool> EndGameAsync(int gameId, string gameResult, int pointsEarned, int petExpEarned, int? couponEarned = null);
        Task<bool> AbortGameAsync(int gameId);
        Task<IEnumerable<Models.MiniGame>> GetGamesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetGameStatisticsAsync();
        Task<int> GetTodayGameCountByUserIdAsync(int userId);
        Task<bool> CanUserPlayTodayAsync(int userId);
    }
}

