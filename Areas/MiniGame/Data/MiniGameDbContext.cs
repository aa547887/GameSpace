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

        // 寵物升級規則表
        public DbSet<PetLevelUpRule> PetLevelUpRules { get; set; } = null!;
        public DbSet<PetLevelExperienceSetting> PetLevelExperienceSettings { get; set; } = null!;

        // 寵物互動狀態增益規則表
        public DbSet<PetInteractionBonusRule> PetInteractionBonusRules { get; set; } = null!;

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

            // 寵物升級規則表的配置
            modelBuilder.Entity<PetLevelUpRule>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Level).IsRequired();
                entity.Property(p => p.ExperienceRequired).IsRequired();
                entity.Property(p => p.PointsReward).IsRequired();
                entity.Property(p => p.ExpReward).IsRequired();
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.Remarks).HasMaxLength(500);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // 建立索引
                entity.HasIndex(p => p.Level).IsUnique();
                entity.HasIndex(p => p.IsActive);
            });

            // 寵物等級經驗設定表的配置
            modelBuilder.Entity<PetLevelExperienceSetting>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Level).IsRequired();
                entity.Property(p => p.RequiredExperience).IsRequired();
                entity.Property(p => p.LevelName).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Description).HasMaxLength(200);
                entity.Property(p => p.IsEnabled).HasDefaultValue(true);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // 建立索引
                entity.HasIndex(p => p.Level).IsUnique();
                entity.HasIndex(p => p.IsEnabled);
            });

            // 寵物互動狀態增益規則表的配置
            modelBuilder.Entity<PetInteractionBonusRule>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.InteractionType).HasMaxLength(50).IsRequired();
                entity.Property(p => p.InteractionName).HasMaxLength(100).IsRequired();
                entity.Property(p => p.PointsCost).IsRequired();
                entity.Property(p => p.HappinessGain).IsRequired();
                entity.Property(p => p.ExpGain).IsRequired();
                entity.Property(p => p.CooldownMinutes).IsRequired();
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.Description).HasMaxLength(500);
                entity.Property(p => p.CreatedBy).HasMaxLength(50);
                entity.Property(p => p.UpdatedBy).HasMaxLength(50);
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // 建立索引
                entity.HasIndex(p => p.InteractionType).IsUnique();
                entity.HasIndex(p => p.IsActive);
            });
        }
    }
}
