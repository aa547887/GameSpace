using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Data
{
    /// <summary>
    /// MiniGame Area 專用資料庫上下文
    /// 統一管理所有MiniGame相關的資料表
    /// </summary>
    public class MiniGameDbContext : DbContext
    {
        public MiniGameDbContext(DbContextOptions<MiniGameDbContext> options)
            : base(options) { }

        // 管理員相關表
        public DbSet<ManagerData> ManagerData { get; set; } = null!;
        public DbSet<ManagerRolePermission> ManagerRolePermissions { get; set; } = null!;
        public DbSet<ManagerRole> ManagerRoles { get; set; } = null!;

        // 用戶相關表
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserWallet> UserWallets { get; set; } = null!;
        public DbSet<WalletHistory> WalletHistories { get; set; } = null!;

        // 優惠券相關表
        public DbSet<CouponType> CouponTypes { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;

        // 電子券相關表
        public DbSet<EVoucherType> EVoucherTypes { get; set; } = null!;
        public DbSet<EVoucher> EVouchers { get; set; } = null!;
        public DbSet<EVoucherToken> EVoucherTokens { get; set; } = null!;
        public DbSet<EVoucherRedeemLog> EVoucherRedeemLogs { get; set; } = null!;

        // 簽到相關表
        public DbSet<UserSignInStats> UserSignInStats { get; set; } = null!;

        // 寵物相關表
        public DbSet<Pet> Pets { get; set; } = null!;
        public DbSet<PetAppearanceChangeLog> PetAppearanceChangeLogs { get; set; } = null!;

        // 小遊戲相關表
        public DbSet<MiniGame> MiniGames { get; set; } = null!;
        public DbSet<LeaderboardSnapshot> LeaderboardSnapshots { get; set; } = null!;

        // 用戶代幣表
        public DbSet<UserToken> UserTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 設定主鍵
            modelBuilder.Entity<ManagerData>(entity =>
            {
                entity.HasKey(m => m.Manager_Id);
                entity.HasIndex(m => m.Manager_Account).IsUnique();
                entity.HasIndex(m => m.Manager_Email).IsUnique();
            });

            modelBuilder.Entity<ManagerRolePermission>(entity =>
            {
                entity.HasKey(m => m.ManagerRole_Id);
            });

            modelBuilder.Entity<ManagerRole>(entity =>
            {
                entity.HasKey(m => m.Id);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.User_Id);
                entity.HasIndex(u => u.User_name).IsUnique();
                entity.HasIndex(u => u.User_Account).IsUnique();
            });

            modelBuilder.Entity<UserWallet>(entity =>
            {
                entity.HasKey(w => w.User_Id);
            });

            modelBuilder.Entity<WalletHistory>(entity =>
            {
                entity.HasKey(w => w.LogID);
            });

            // 設定外鍵關係
            modelBuilder.Entity<ManagerRole>(entity =>
            {
                entity.HasOne(mr => mr.Manager)
                      .WithMany(m => m.ManagerRoles)
                      .HasForeignKey(mr => mr.Manager_Id)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mr => mr.ManagerRolePermission)
                      .WithMany(mrp => mrp.ManagerRoles)
                      .HasForeignKey(mr => mr.ManagerRole_Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserWallet>(entity =>
            {
                entity.HasOne(w => w.User)
                      .WithOne(u => u.UserWallet)
                      .HasForeignKey<UserWallet>(w => w.User_Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WalletHistory>(entity =>
            {
                entity.HasOne(w => w.User)
                      .WithMany(u => u.WalletHistories)
                      .HasForeignKey(w => w.UserID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // 設定預設值
            modelBuilder.Entity<ManagerData>(entity =>
            {
                entity.Property(m => m.Manager_EmailConfirmed).HasDefaultValue(false);
                entity.Property(m => m.Manager_AccessFailedCount).HasDefaultValue(0);
                entity.Property(m => m.Manager_LockoutEnabled).HasDefaultValue(true);
                entity.Property(m => m.Administrator_registration_date).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.User_EmailConfirmed).HasDefaultValue(false);
                entity.Property(u => u.User_AccessFailedCount).HasDefaultValue(0);
                entity.Property(u => u.User_LockoutEnabled).HasDefaultValue(true);
                entity.Property(u => u.User_registration_date).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<UserWallet>(entity =>
            {
                entity.Property(w => w.User_Point).HasDefaultValue(0);
            });

            modelBuilder.Entity<WalletHistory>(entity =>
            {
                entity.Property(w => w.ChangeTime).HasDefaultValueSql("GETUTCDATE()");
            });

            // 設定字串長度
            modelBuilder.Entity<ManagerData>(entity =>
            {
                entity.Property(m => m.Manager_Name).HasMaxLength(50);
                entity.Property(m => m.Manager_Account).HasMaxLength(50);
                entity.Property(m => m.Manager_Password).HasMaxLength(100);
                entity.Property(m => m.Manager_Email).HasMaxLength(100);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.User_name).HasMaxLength(50);
                entity.Property(u => u.User_Account).HasMaxLength(50);
                entity.Property(u => u.User_Password).HasMaxLength(100);
                entity.Property(u => u.User_Email).HasMaxLength(100);
            });

            modelBuilder.Entity<WalletHistory>(entity =>
            {
                entity.Property(w => w.ChangeType).HasMaxLength(20);
                entity.Property(w => w.ItemCode).HasMaxLength(50);
                entity.Property(w => w.Description).HasMaxLength(200);
            });
        }
    }
}
