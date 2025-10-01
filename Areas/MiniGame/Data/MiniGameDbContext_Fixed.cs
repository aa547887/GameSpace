using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.Settings;

namespace GameSpace.Areas.MiniGame.Data
{
    public class MiniGameDbContext : DbContext
    {
        public MiniGameDbContext(DbContextOptions<MiniGameDbContext> options)
            : base(options) { }

        // 新增：寵物成本設定表
        public DbSet<PetSkinColorCostSetting> PetSkinColorCostSettings { get; set; } = null!;
        public DbSet<PetBackgroundCostSetting> PetBackgroundCostSettings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 新增：寵物成本設定表的配置
            modelBuilder.Entity<PetSkinColorCostSetting>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.ColorName).HasMaxLength(50).IsRequired();
                entity.Property(p => p.ColorCode).HasMaxLength(7);
                entity.Property(p => p.RequiredPoints).IsRequired();
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            modelBuilder.Entity<PetBackgroundCostSetting>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.BackgroundName).HasMaxLength(50).IsRequired();
                entity.Property(p => p.BackgroundCode).HasMaxLength(7);
                entity.Property(p => p.RequiredPoints).IsRequired();
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
