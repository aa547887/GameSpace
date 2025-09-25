using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class ShopViewModels
    {
        public class ShopIndexViewModel
        {
            public List<ShopItemViewModel> Items { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class ShopCouponsViewModel
        {
            public List<CouponType> CouponTypes { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class ShopEVouchersViewModel
        {
            public List<EvoucherType> EVoucherTypes { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class ShopOrdersViewModel
        {
            public List<OrderInfo> Orders { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class ShopOrderDetailsViewModel
        {
            public OrderInfo Order { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }
    }

    public class ShopItemViewModel
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PointsCost { get; set; }
        public string ItemType { get; set; } = string.Empty;
    }
}
