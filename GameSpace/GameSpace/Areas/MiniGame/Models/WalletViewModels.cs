using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class WalletViewModels
    {
        public class WalletIndexViewModel
        {
            public List<UserWallet> Wallets { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class WalletDetailsViewModel
        {
            public UserWallet Wallet { get; set; } = new();
            public List<WalletHistory> History { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class WalletCouponsViewModel
        {
            public List<Coupon> Coupons { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class WalletEVouchersViewModel
        {
            public List<Evoucher> EVouchers { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class AvailableCouponsViewModel
        {
            public List<CouponType> CouponTypes { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class AvailableEVouchersViewModel
        {
            public List<EvoucherType> EVoucherTypes { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }
    }
}
