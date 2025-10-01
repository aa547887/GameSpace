using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Data
{
    /// <summary>
    /// MiniGame Area 專用 DbContext
    /// </summary>
    public class MiniGameDbContext : DbContext
    {
        public MiniGameDbContext(DbContextOptions<MiniGameDbContext> options) : base(options)
        {
        }

        // 管理者權限相關
        public DbSet<ManagerData> ManagerData { get; set; }
        public DbSet<ManagerRolePermission> ManagerRolePermission { get; set; }
        public DbSet<ManagerRole> ManagerRole { get; set; }

        // 使用者相關
        public DbSet<Users> Users { get; set; }
        public DbSet<User_Wallet> User_Wallet { get; set; }
        public DbSet<WalletHistory> WalletHistory { get; set; }

        // 優惠券相關
        public DbSet<CouponType> CouponType { get; set; }
        public DbSet<Coupon> Coupon { get; set; }

        // 電子禮券相關
        public DbSet<EVoucherType> EVoucherType { get; set; }
        public DbSet<EVoucher> EVoucher { get; set; }

        // 簽到相關
        public DbSet<UserSignInStats> UserSignInStats { get; set; }

        // 寵物相關
        public DbSet<Pet> Pet { get; set; }

        // 小遊戲相關
        public DbSet<MiniGame> MiniGame { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 設定複合主鍵
            modelBuilder.Entity<ManagerRole>()
                .HasKey(e => new { e.Manager_Id, e.ManagerRole_Id });

            // 設定外鍵關係
            modelBuilder.Entity<ManagerRole>()
                .HasOne(e => e.ManagerData)
                .WithMany()
                .HasForeignKey(e => e.Manager_Id);

            modelBuilder.Entity<ManagerRole>()
                .HasOne(e => e.ManagerRolePermission)
                .WithMany()
                .HasForeignKey(e => e.ManagerRole_Id);

            // 設定索引
            modelBuilder.Entity<Coupon>()
                .HasIndex(e => new { e.UserID, e.IsUsed, e.AcquiredTime });

            modelBuilder.Entity<EVoucher>()
                .HasIndex(e => new { e.UserID, e.IsUsed, e.AcquiredTime });

            modelBuilder.Entity<MiniGame>()
                .HasIndex(e => new { e.UserID, e.StartTime });

            modelBuilder.Entity<Pet>()
                .HasIndex(e => e.UserID);

            modelBuilder.Entity<UserSignInStats>()
                .HasIndex(e => new { e.UserID, e.SignTime });

            modelBuilder.Entity<WalletHistory>()
                .HasIndex(e => new { e.UserID, e.ChangeTime });
        }
    }
}