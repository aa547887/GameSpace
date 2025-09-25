using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class AdminWalletIndexViewModel
    {
        public PagedResult<WalletReadModel> Wallets { get; set; } = new();
        public WalletSummaryReadModel WalletSummary { get; set; } = new();
        public WalletQueryModel Query { get; set; } = new();
        public string Sidebar { get; set; } = "admin";
        public List<UserWalletReadModel> UserPoints { get; set; } = new();
        public List<GameSpace.Models.User> Users { get; set; } = new();
        public string SearchTerm { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminWalletDetailsViewModel
    {
        public WalletDetailReadModel WalletDetail { get; set; } = new();
        public string Sidebar { get; set; } = "admin";
    }

    public class AdminWalletStatisticsViewModel
    {
        public WalletSummaryReadModel Summary { get; set; } = new();
        public List<WalletReadModel> TopWallets { get; set; } = new();
        public string Sidebar { get; set; } = "admin";
    }
}
