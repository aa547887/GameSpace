using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // 使用者相關
        public DbSet<User> Users { get; set; }
        public DbSet<UserIntroduce> UserIntroduces { get; set; }
        public DbSet<UserRights> UserRights { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<UserSignInStats> UserSignInStats { get; set; }

        // 寵物相關
        public DbSet<Pet> Pets { get; set; }

        // 小遊戲相關
        public DbSet<MiniGame> MiniGames { get; set; }

        // 錢包相關
        public DbSet<WalletHistory> WalletHistories { get; set; }

        // 優惠券相關
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponType> CouponTypes { get; set; }

        // 電子禮券相關
        public DbSet<EVoucher> EVouchers { get; set; }
        public DbSet<EVoucherType> EVoucherTypes { get; set; }
        public DbSet<EVoucherToken> EVoucherTokens { get; set; }
        public DbSet<EVoucherRedeemLog> EVoucherRedeemLogs { get; set; }

        // 管理者相關
        public DbSet<ManagerData> ManagerData { get; set; }
        public DbSet<ManagerRole> ManagerRoles { get; set; }
        public DbSet<ManagerRolePermission> ManagerRolePermissions { get; set; }

        // 規則設定相關
        public DbSet<SignInRuleSettings> SignInRuleSettings { get; set; }
        public DbSet<PetSystemRuleSettings> PetSystemRuleSettings { get; set; }
        public DbSet<MiniGameRuleSettings> MiniGameRuleSettings { get; set; }

        // 寵物外觀變更記錄
        public DbSet<PetAppearanceChangeLog> PetAppearanceChangeLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 使用者相關關聯
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserIntroduce)
                .WithOne(ui => ui.User)
                .HasForeignKey<UserIntroduce>(ui => ui.User_ID);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserRights)
                .WithOne(ur => ur.User)
                .HasForeignKey<UserRights>(ur => ur.User_ID);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserWallet)
                .WithOne(uw => uw.User)
                .HasForeignKey<UserWallet>(uw => uw.User_Id);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserSignInStats)
                .WithOne(us => us.User)
                .HasForeignKey(us => us.UserID);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Pet)
                .WithOne(p => p.User)
                .HasForeignKey<Pet>(p => p.UserID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.MiniGames)
                .WithOne(mg => mg.User)
                .HasForeignKey(mg => mg.UserID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.WalletHistories)
                .WithOne(wh => wh.User)
                .HasForeignKey(wh => wh.UserID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Coupons)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.EVouchers)
                .WithOne(ev => ev.User)
                .HasForeignKey(ev => ev.UserID);

            // 寵物相關關聯
            modelBuilder.Entity<Pet>()
                .HasMany(p => p.MiniGames)
                .WithOne(mg => mg.Pet)
                .HasForeignKey(mg => mg.PetID);

            modelBuilder.Entity<Pet>()
                .HasMany(p => p.PetAppearanceChangeLogs)
                .WithOne(pacl => pacl.Pet)
                .HasForeignKey(pacl => pacl.PetID);

            // 優惠券相關關聯
            modelBuilder.Entity<CouponType>()
                .HasMany(ct => ct.Coupons)
                .WithOne(c => c.CouponType)
                .HasForeignKey(c => c.CouponTypeID);

            // 電子禮券相關關聯
            modelBuilder.Entity<EVoucherType>()
                .HasMany(evt => evt.EVouchers)
                .WithOne(ev => ev.EVoucherType)
                .HasForeignKey(ev => ev.EVoucherTypeID);

            modelBuilder.Entity<EVoucher>()
                .HasMany(ev => ev.EVoucherTokens)
                .WithOne(evt => evt.EVoucher)
                .HasForeignKey(evt => evt.EVoucherID);

            modelBuilder.Entity<EVoucher>()
                .HasMany(ev => ev.EVoucherRedeemLogs)
                .WithOne(evrl => evrl.EVoucher)
                .HasForeignKey(evrl => evrl.EVoucherID);

            modelBuilder.Entity<EVoucherToken>()
                .HasMany(evt => evt.EVoucherRedeemLogs)
                .WithOne(evrl => evrl.EVoucherToken)
                .HasForeignKey(evrl => evrl.TokenID);

            // 管理者相關關聯
            modelBuilder.Entity<ManagerData>()
                .HasMany(md => md.ManagerRoles)
                .WithOne(mr => mr.ManagerData)
                .HasForeignKey(mr => mr.Manager_Id);

            modelBuilder.Entity<ManagerRolePermission>()
                .HasMany(mrp => mrp.ManagerRoles)
                .WithOne(mr => mr.ManagerRolePermission)
                .HasForeignKey(mr => mr.ManagerRole_Id);

            // 複合主鍵設定
            modelBuilder.Entity<ManagerRole>()
                .HasKey(mr => new { mr.Manager_Id, mr.ManagerRole_Id });

            // 設定欄位名稱對應
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserIntroduce>().ToTable("User_Introduce");
            modelBuilder.Entity<UserRights>().ToTable("User_Rights");
            modelBuilder.Entity<UserWallet>().ToTable("User_Wallet");
            modelBuilder.Entity<UserSignInStats>().ToTable("UserSignInStats");
            modelBuilder.Entity<Pet>().ToTable("Pet");
            modelBuilder.Entity<MiniGame>().ToTable("MiniGame");
            modelBuilder.Entity<WalletHistory>().ToTable("WalletHistory");
            modelBuilder.Entity<Coupon>().ToTable("Coupon");
            modelBuilder.Entity<CouponType>().ToTable("CouponType");
            modelBuilder.Entity<EVoucher>().ToTable("EVoucher");
            modelBuilder.Entity<EVoucherType>().ToTable("EVoucherType");
            modelBuilder.Entity<EVoucherToken>().ToTable("EVoucherToken");
            modelBuilder.Entity<EVoucherRedeemLog>().ToTable("EVoucherRedeemLog");
            modelBuilder.Entity<ManagerData>().ToTable("ManagerData");
            modelBuilder.Entity<ManagerRole>().ToTable("ManagerRole");
            modelBuilder.Entity<ManagerRolePermission>().ToTable("ManagerRolePermission");
        }
    }
}
