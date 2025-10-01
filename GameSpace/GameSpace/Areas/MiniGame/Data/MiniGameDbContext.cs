using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Data
{
    public class MiniGameDbContext : DbContext
    {
        public MiniGameDbContext(DbContextOptions<MiniGameDbContext> options)
            : base(options) { }

        // 管理員相關表
        public DbSet<ManagerData> ManagerData { get; set; }
        public DbSet<ManagerRolePermission> ManagerRolePermissions { get; set; }
        public DbSet<ManagerRole> ManagerRoles { get; set; }

        // 用戶相關表
        public DbSet<User> Users { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<WalletHistory> WalletHistories { get; set; }

        // 優惠券相關表
        public DbSet<CouponType> CouponTypes { get; set; }
        public DbSet<Coupon> Coupons { get; set; }

        // 電子券相關表
        public DbSet<EVoucherType> EVoucherTypes { get; set; }
        public DbSet<EVoucher> EVouchers { get; set; }
        public DbSet<EVoucherToken> EVoucherTokens { get; set; }
        public DbSet<EVoucherRedeemLog> EVoucherRedeemLogs { get; set; }

        // 簽到相關表
        public DbSet<UserSignInStats> UserSignInStats { get; set; }

        // 寵物相關表
        public DbSet<Pet> Pets { get; set; }
        public DbSet<PetAppearanceChangeLog> PetAppearanceChangeLogs { get; set; }

        // 小遊戲相關表
        public DbSet<MiniGame> MiniGames { get; set; }
        public DbSet<LeaderboardSnapshot> LeaderboardSnapshots { get; set; }

        // 用戶代幣表
        public DbSet<UserToken> UserTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 設定主鍵
            modelBuilder.Entity<ManagerData>()
                .HasKey(m => m.Manager_Id);

            modelBuilder.Entity<ManagerRolePermission>()
                .HasKey(m => m.ManagerRole_Id);

            modelBuilder.Entity<ManagerRole>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<User>()
                .HasKey(u => u.User_Id);

            modelBuilder.Entity<UserWallet>()
                .HasKey(w => w.User_Id);

            modelBuilder.Entity<WalletHistory>()
                .HasKey(w => w.LogID);

            // 設定外鍵關係
            modelBuilder.Entity<ManagerRole>()
                .HasOne(mr => mr.Manager)
                .WithMany(m => m.ManagerRoles)
                .HasForeignKey(mr => mr.Manager_Id);

            modelBuilder.Entity<ManagerRole>()
                .HasOne(mr => mr.ManagerRolePermission)
                .WithMany(mrp => mrp.ManagerRoles)
                .HasForeignKey(mr => mr.ManagerRole_Id);

            modelBuilder.Entity<UserWallet>()
                .HasOne(w => w.User)
                .WithOne()
                .HasForeignKey<UserWallet>(w => w.User_Id);

            modelBuilder.Entity<WalletHistory>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserID);

            // 設定唯一約束
            modelBuilder.Entity<ManagerData>()
                .HasIndex(m => m.Manager_Account)
                .IsUnique();

            modelBuilder.Entity<ManagerData>()
                .HasIndex(m => m.Manager_Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.User_name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.User_Account)
                .IsUnique();
        }
    }
}
