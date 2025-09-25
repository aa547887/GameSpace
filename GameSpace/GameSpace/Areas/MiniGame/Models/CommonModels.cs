using System;
using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    public class SidebarViewModel
    {
        public string CurrentArea { get; set; } = string.Empty;
        public string CurrentController { get; set; } = string.Empty;
        public string CurrentAction { get; set; } = string.Empty;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class EVoucherQueryModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class GameQueryModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class SignInQueryModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public class UserCouponReadModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int CouponTypeId { get; set; }
        public string CouponTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class UserEVoucherReadModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int EVoucherTypeId { get; set; }
        public string EVoucherTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class TopUserReadModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Points { get; set; }
        public int Rank { get; set; }
    }
}
