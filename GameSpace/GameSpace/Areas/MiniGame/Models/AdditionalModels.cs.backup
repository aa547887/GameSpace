using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    // 遊戲相關模型
    public class GameQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageNumberSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }

    public class GameRecordReadModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime PlayDate { get; set; }
    }

    public class GameSummaryReadModel
    {
        public int TotalPlays { get; set; }
        public int TotalUsers { get; set; }
        public int AverageScore { get; set; }
    }

    public class GameDetailReadModel
    {
        public int PlayId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime PlayDate { get; set; }
        public string GameData { get; set; } = string.Empty;
    }

    // 寵物相關模型
    public class PetQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageNumberSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }

    public class PetReadModel
    {
        public int PetId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
    }

    public class PetSummaryReadModel
    {
        public int TotalPets { get; set; }
        public int ActivePets { get; set; }
        public int AverageLevel { get; set; }
    }

    public class PetDetailReadModel
    {
        public int PetId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public string SkinColor { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
    }

    public class PetUpdateModel
    {
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
    }

    // 錢包相關模型
    public class WalletQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageNumberSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }

    public class WalletReadModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int UserPoint { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class WalletSummaryReadModel
    {
        public int TotalUsers { get; set; }
        public int TotalPoints { get; set; }
        public int AveragePoints { get; set; }
    }

    public class WalletDetailReadModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int UserPoint { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<WalletTransactionReadModel> Transactions { get; set; } = new();
    }

    public class WalletTransactionReadModel
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public int Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }

    // 統計相關模型
    public class TopUserReadModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Score { get; set; }
    }
}
