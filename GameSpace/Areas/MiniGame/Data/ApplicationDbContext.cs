using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using System;

namespace GameSpace.Areas.MiniGame.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
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
        public DbSet<PetAppearanceChangeLog> PetAppearanceChangeLogs { get; set; }

        // 小遊戲相關
        public DbSet<MiniGame> MiniGames { get; set; }

        // 錢包歷史
        public DbSet<WalletHistory> WalletHistories { get; set; }

        // 優惠券相關
        public DbSet<Coupon> Coupons { get; set; }

        // 電子禮券相關
        public DbSet<EVoucher> EVouchers { get; set; }
        public DbSet<EVoucherToken> EVoucherTokens { get; set; }
        public DbSet<EVoucherRedeemLog> EVoucherRedeemLogs { get; set; }

        // 系統設定
        public DbSet<SignInRuleSettings> SignInRuleSettings { get; set; }
        public DbSet<PetSystemRuleSettings> PetSystemRuleSettings { get; set; }
        public DbSet<MiniGameRuleSettings> MiniGameRuleSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User 關係設定
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
                .HasForeignKey<UserWallet>(uw => uw.User_ID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserSignInStats)
                .WithOne(usis => usis.User)
                .HasForeignKey(usis => usis.User_ID);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Pet)
                .WithOne(p => p.User)
                .HasForeignKey<Pet>(p => p.User_ID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.MiniGames)
                .WithOne(mg => mg.User)
                .HasForeignKey(mg => mg.User_ID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.WalletHistories)
                .WithOne(wh => wh.User)
                .HasForeignKey(wh => wh.User_ID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Coupons)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.User_ID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.EVouchers)
                .WithOne(ev => ev.User)
                .HasForeignKey(ev => ev.User_ID);

            // Pet 關係設定
            modelBuilder.Entity<Pet>()
                .HasMany(p => p.PetAppearanceChangeLogs)
                .WithOne(pacl => pacl.Pet)
                .HasForeignKey(pacl => pacl.PetID);

            // EVoucher 關係設定
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

            // 設定索引
            modelBuilder.Entity<User>()
                .HasIndex(u => u.User_Account)
                .IsUnique();

            modelBuilder.Entity<EVoucherToken>()
                .HasIndex(evt => evt.Token)
                .IsUnique();

            // 設定預設值
            modelBuilder.Entity<UserWallet>()
                .Property(uw => uw.Points)
                .HasDefaultValue(0);

            modelBuilder.Entity<Pet>()
                .Property(p => p.PetExp)
                .HasDefaultValue(0);

            modelBuilder.Entity<Pet>()
                .Property(p => p.PetLevel)
                .HasDefaultValue(1);

            modelBuilder.Entity<Pet>()
                .Property(p => p.Hunger)
                .HasDefaultValue(50);

            modelBuilder.Entity<Pet>()
                .Property(p => p.Happiness)
                .HasDefaultValue(50);

            modelBuilder.Entity<Pet>()
                .Property(p => p.Health)
                .HasDefaultValue(50);

            modelBuilder.Entity<Pet>()
                .Property(p => p.Energy)
                .HasDefaultValue(50);

            modelBuilder.Entity<Pet>()
                .Property(p => p.Cleanliness)
                .HasDefaultValue(50);
        }
    }
}
