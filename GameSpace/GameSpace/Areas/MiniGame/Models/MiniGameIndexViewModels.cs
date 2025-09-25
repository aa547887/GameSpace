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
}
