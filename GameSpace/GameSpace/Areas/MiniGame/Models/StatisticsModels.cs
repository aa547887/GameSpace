using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    public class SignInStatisticsReadModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public decimal SignInRate { get; set; }
        public List<TopUserReadModel> TopUsers { get; set; } = new();
    }

    public class SystemDiagnosticsReadModel
    {
        public bool DatabaseConnection { get; set; }
        public bool EmailService { get; set; }
        public bool FileSystem { get; set; }
        public DateTime LastChecked { get; set; }
        public string SystemStatus { get; set; } = string.Empty;
    }

    // 擴展統計模型
    public class PetStatisticsReadModel
    {
        public int TotalPets { get; set; }
        public int ActivePets { get; set; }
        public double AverageLevel { get; set; }
        public int TotalColorChanges { get; set; }
        public int TotalBackgroundChanges { get; set; }
        public List<PetLevelDistributionModel> LevelDistribution { get; set; } = new();
    }

    public class PetLevelDistributionModel
    {
        public int Level { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class GameStatisticsReadModel
    {
        public int TotalGames { get; set; }
        public int CompletedGames { get; set; }
        public int WinGames { get; set; }
        public int LoseGames { get; set; }
        public int AbortGames { get; set; }
        public double WinRate { get; set; }
        public double AverageScore { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int TotalExperienceAwarded { get; set; }
        public List<GameDailyStatsModel> DailyStats { get; set; } = new();
    }

    public class GameDailyStatsModel
    {
        public DateTime Date { get; set; }
        public int GameCount { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int AbortCount { get; set; }
        public int PointsAwarded { get; set; }
        public int ExperienceAwarded { get; set; }
    }

    public class WalletStatisticsReadModel
    {
        public int TotalUsers { get; set; }
        public long TotalPoints { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalEVouchers { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AveragePointsPerUser { get; set; }
        public List<WalletTransactionTypeModel> TransactionTypes { get; set; } = new();
    }

    public class WalletTransactionTypeModel
    {
        public string TransactionType { get; set; } = string.Empty;
        public int Count { get; set; }
        public long TotalAmount { get; set; }
        public double Percentage { get; set; }
    }

    public class SystemPerformanceReadModel
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public int ActiveConnections { get; set; }
        public int RequestPerSecond { get; set; }
        public double AverageResponseTime { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class UserActivityReadModel
    {
        public int TotalUsers { get; set; }
        public int ActiveToday { get; set; }
        public int ActiveThisWeek { get; set; }
        public int ActiveThisMonth { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }
        public List<UserActivityDailyModel> DailyActivity { get; set; } = new();
    }

    public class UserActivityDailyModel
    {
        public DateTime Date { get; set; }
        public int SignInCount { get; set; }
        public int GamePlayCount { get; set; }
        public int PetInteractionCount { get; set; }
        public int WalletTransactionCount { get; set; }
    }
}
