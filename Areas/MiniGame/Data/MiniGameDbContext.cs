using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.Settings;

namespace GameSpace.Areas.MiniGame.Data
{
    public class MiniGameDbContext : DbContext
    {
        public MiniGameDbContext(DbContextOptions<MiniGameDbContext> options)
            : base(options) { }

        // 寵物成本設定表
        public DbSet<PetSkinColorCostSetting> PetSkinColorCostSettings { get; set; } = null!;
        public DbSet<PetBackgroundCostSetting> PetBackgroundCostSettings { get; set; } = null!;

        // 寵物選項表
        public DbSet<PetColorOption> PetColorOptions { get; set; } = null!;
        public DbSet<PetBackgroundOption> PetBackgroundOptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 寵物成本設定表的配置
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

            // 寵物顏色選項表的配置
            modelBuilder.Entity<PetColorOption>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(200);
                entity.Property(p => p.ColorCode).HasMaxLength(7).IsRequired();
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.SortOrder).HasDefaultValue(0);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // 建立索引
                entity.HasIndex(p => p.Name).IsUnique();
                entity.HasIndex(p => p.ColorCode).IsUnique();
                entity.HasIndex(p => new { p.IsActive, p.SortOrder });
            });

            // 寵物背景選項表的配置
            modelBuilder.Entity<PetBackgroundOption>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(200);
                entity.Property(p => p.BackgroundColorCode).HasMaxLength(7).IsRequired();
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.SortOrder).HasDefaultValue(0);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // 建立索引
                entity.HasIndex(p => p.Name).IsUnique();
                entity.HasIndex(p => p.BackgroundColorCode).IsUnique();
                entity.HasIndex(p => new { p.IsActive, p.SortOrder });
            });
        }
    }
}
