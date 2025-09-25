using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
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

    public class WalletQueryModel
    {
        public int PageNumber { get; set; } = 1;
        public int PageNumberSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }

    public class UserWalletReadModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int UserPoint { get; set; }
        public DateTime LastUpdated { get; set; }
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
}
