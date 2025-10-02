using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.Settings;

namespace GameSpace.Areas.MiniGame.Data
{
    public class MiniGameDbContext : DbContext
    {
        public MiniGameDbContext(DbContextOptions<MiniGameDbContext> options)
            : base(options) { }

        // 核心業務表
        public DbSet<ManagerData> ManagerData { get; set; } = null!;
        public DbSet<ManagerRole> ManagerRoles { get; set; } = null!;
        public DbSet<ManagerRolePermission> ManagerRolePermissions { get; set; } = null!;
        public DbSet<UserWallet> UserWallets { get; set; } = null!;
        public DbSet<WalletHistory> WalletHistories { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<CouponType> CouponTypes { get; set; } = null!;
        public DbSet<EVoucher> EVouchers { get; set; } = null!;
        public DbSet<EVoucherType> EVoucherTypes { get; set; } = null!;
        public DbSet<UserSignInStats> UserSignInStats { get; set; } = null!;

        // 寵物成本設定表
        public DbSet<PetSkinColorCostSetting> PetSkinColorCostSettings { get; set; } = null!;
        public DbSet<PetBackgroundCostSetting> PetBackgroundCostSettings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 優惠券相關配置
            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.HasKey(c => c.CouponID);
                entity.Property(c => c.CouponCode).HasMaxLength(50).IsRequired();
                entity.Property(c => c.IsUsed).HasDefaultValue(false);
                entity.Property(c => c.AcquiredTime).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(c => c.CouponType)
                      .WithMany(ct => ct.Coupons)
                      .HasForeignKey(c => c.CouponTypeID);
            });

            modelBuilder.Entity<CouponType>(entity =>
            {
                entity.HasKey(ct => ct.CouponTypeID);
                entity.Property(ct => ct.Name).HasMaxLength(100).IsRequired();
                entity.Property(ct => ct.DiscountType).HasMaxLength(20).IsRequired();
                entity.Property(ct => ct.DiscountValue).HasColumnType("decimal(10,2)");
                entity.Property(ct => ct.MinSpend).HasColumnType("decimal(10,2)");
                entity.Property(ct => ct.PointsCost).HasDefaultValue(0);
                entity.Property(ct => ct.Description).HasMaxLength(500);
            });

            // 電子禮券相關配置
            modelBuilder.Entity<EVoucher>(entity =>
            {
                entity.HasKey(ev => ev.EVoucherID);
                entity.Property(ev => ev.EVoucherCode).HasMaxLength(50).IsRequired();
                entity.Property(ev => ev.IsUsed).HasDefaultValue(false);
                entity.Property(ev => ev.AcquiredTime).HasDefaultValueSql("GETDATE()");
                
                entity.HasOne(ev => ev.EVoucherType)
                      .WithMany(evt => evt.EVouchers)
                      .HasForeignKey(ev => ev.EVoucherTypeID);
            });

            modelBuilder.Entity<EVoucherType>(entity =>
            {
                entity.HasKey(evt => evt.EVoucherTypeID);
                entity.Property(evt => evt.Name).HasMaxLength(100).IsRequired();
                entity.Property(evt => evt.ValueAmount).HasColumnType("decimal(10,2)");
                entity.Property(evt => evt.PointsCost).HasDefaultValue(0);
                entity.Property(evt => evt.TotalAvailable).HasDefaultValue(0);
                entity.Property(evt => evt.Description).HasMaxLength(500);
            });

            // 簽到統計配置
            modelBuilder.Entity<UserSignInStats>(entity =>
            {
                entity.HasKey(s => s.LogID);
                entity.Property(s => s.SignTime).HasDefaultValueSql("GETDATE()");
                entity.Property(s => s.PointsGained).HasDefaultValue(0);
                entity.Property(s => s.ExpGained).HasDefaultValue(0);
                entity.Property(s => s.PointsGainedTime).HasDefaultValueSql("GETDATE()");
                entity.Property(s => s.ExpGainedTime).HasDefaultValueSql("GETDATE()");
            });

            // 寵物成本設定表的配置
            modelBuilder.Entity<PetSkinColorCostSetting>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.ColorName).HasMaxLength(50).IsRequired();
                entity.Property(p => p.ColorCode).HasMaxLength(7);
                entity.Property(p => p.Cost).IsRequired();
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<PetBackgroundCostSetting>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.BackgroundName).HasMaxLength(50).IsRequired();
                entity.Property(p => p.BackgroundCode).HasMaxLength(7);
                entity.Property(p => p.Cost).IsRequired();
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
