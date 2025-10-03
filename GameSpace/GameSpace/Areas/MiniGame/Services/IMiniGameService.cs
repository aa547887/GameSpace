﻿using MiniGameEntity = GameSpace.Models.MiniGame;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IMiniGameService
    {
        Task<IEnumerable<MiniGameEntity>> GetAllMiniGamesAsync();
        Task<IEnumerable<MiniGameEntity>> GetMiniGamesByUserIdAsync(int userId);
        Task<IEnumerable<MiniGameEntity>> GetMiniGamesByPetIdAsync(int petId);
        Task<MiniGameEntity?> GetMiniGameByIdAsync(int gameId);
        Task<bool> CreateMiniGameAsync(MiniGameEntity miniGame);
        Task<bool> UpdateMiniGameAsync(MiniGameEntity miniGame);
        Task<bool> DeleteMiniGameAsync(int gameId);
        Task<bool> StartGameAsync(int userId, int petId, string gameType);
        Task<bool> EndGameAsync(int gameId, string gameResult, int pointsEarned, int petExpEarned, int? couponEarned = null);
        Task<bool> AbortGameAsync(int gameId);
        Task<IEnumerable<MiniGameEntity>> GetGamesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetGameStatisticsAsync();
        Task<int> GetTodayGameCountByUserIdAsync(int userId);
        Task<bool> CanUserPlayTodayAsync(int userId);
    }
}

