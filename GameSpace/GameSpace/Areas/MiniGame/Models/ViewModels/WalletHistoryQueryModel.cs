using System;

namespace GameSpace.Areas.MiniGame.Models.ViewModels
{
    public class WalletHistoryQueryModel
    {
        public int? UserId { get; set; }
        public string? ChangeType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "ChangeTime";
        public bool Descending { get; set; } = true;
    }
}


