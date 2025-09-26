using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class MiniGameIndexViewModel
    {
        public List<GameSpace.Models.MiniGame> MiniGames { get; set; } = new();
        public GameSpace.Models.Pet Pet { get; set; } = new();
        public UserWallet Wallet { get; set; } = new();
        public bool CanPlay { get; set; }
        public int TodayGames { get; set; }
        public int RemainingGames { get; set; }
        public int SenderID { get; set; }
    }

    public class MiniGameDetailsViewModel
    {
        public GameSpace.Models.MiniGame MiniGame { get; set; } = new();
        public GameSpace.Models.Pet Pet { get; set; } = new();
        public UserWallet Wallet { get; set; } = new();
        public int SenderID { get; set; }
    }

    public class MiniGamePlayViewModel
    {
        public GameSpace.Models.Pet Pet { get; set; } = new();
        public UserWallet Wallet { get; set; } = new();
        public int SenderID { get; set; }
        public int Difficulty { get; set; } = 1;
    }

    public class MiniGameResultViewModel
    {
        public GameSpace.Models.MiniGame MiniGame { get; set; } = new();
        public GameSpace.Models.Pet Pet { get; set; } = new();
        public UserWallet Wallet { get; set; } = new();
        public int SenderID { get; set; }
        public bool IsWin { get; set; }
        public int PointsGained { get; set; }
        public int ExpGained { get; set; }
    }

    public class MiniGameStatsViewModel
    {
        public List<GameSpace.Models.MiniGame> MiniGames { get; set; } = new();
        public int TotalGames { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int AbortCount { get; set; }
        public double WinRate { get; set; }
        public int TotalPoints { get; set; }
        public int TotalExp { get; set; }
        public int SenderID { get; set; }
    }

    // 管理員小遊戲管理相關 ViewModels
    public class AdminMiniGameManagementViewModel
    {
        public List<MiniGame> GameRecords { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public MiniGameRecordQueryModel Query { get; set; } = new();
        public GameStatisticsReadModel Statistics { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class MiniGameRuleManagementViewModel
    {
        public GameRuleReadModel CurrentRule { get; set; } = new();
        public MiniGameRulesUpdateModel UpdateModel { get; set; } = new();
        public List<GameRule> RuleHistory { get; set; } = new();
    }

    public class MiniGameSessionViewModel
    {
        [Required(ErrorMessage = "請選擇難度")]
        [Range(1, 5, ErrorMessage = "難度必須在1-5之間")]
        public int Difficulty { get; set; } = 1;
        
        public string SessionId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int MaxPlayTime { get; set; }
        public int RemainingTime { get; set; }
        public bool IsActive { get; set; }
    }

    public class MiniGameLeaderboardViewModel
    {
        public List<MiniGameLeaderboardEntry> TopPlayers { get; set; } = new();
        public List<MiniGameLeaderboardEntry> RecentGames { get; set; } = new();
        public MiniGameLeaderboardEntry? UserRank { get; set; }
        public int TotalPlayers { get; set; }
        public int UserRank { get; set; }
    }

    public class MiniGameLeaderboardEntry
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalGames { get; set; }
        public int WinCount { get; set; }
        public double WinRate { get; set; }
        public int TotalPoints { get; set; }
        public int TotalExperience { get; set; }
        public int Rank { get; set; }
        public DateTime LastPlayed { get; set; }
    }

    public class MiniGameRewardViewModel
    {
        public int GameId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string GameResult { get; set; } = string.Empty;
        public int PointsEarned { get; set; }
        public int ExperienceEarned { get; set; }
        public List<string> BonusRewards { get; set; } = new();
        public DateTime AwardedDate { get; set; }
    }
}
