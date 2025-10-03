using Microsoft.EntityFrameworkCore;

namespace GameSpace.Models
{
    public partial class GameSpacedatabaseContext
    {
        // Partial class for custom extensions to GameSpacedatabaseContext
        // DbSets are defined in the main GameSpacedatabaseContext.cs file

        // MiniGame Area 專用 DbSets（新增）
        public virtual DbSet<GameSpace.Areas.MiniGame.Models.DailyGameLimit>? DailyGameLimits { get; set; }
        public virtual DbSet<GameSpace.Areas.MiniGame.Models.ErrorLog>? ErrorLogs { get; set; }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // Custom model configurations can be added here if needed
            // Entity configurations are already defined in the main context file

            // DailyGameLimits 實體設定
            if (DailyGameLimits != null)
            {
                modelBuilder.Entity<GameSpace.Areas.MiniGame.Models.DailyGameLimit>(entity =>
                {
                    entity.ToTable("DailyGameLimits");
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.DailyLimit).IsRequired();
                    entity.Property(e => e.SettingName).HasMaxLength(100).IsRequired();
                    entity.Property(e => e.Description).HasMaxLength(500);
                    entity.Property(e => e.CreatedBy).HasMaxLength(50);
                    entity.Property(e => e.UpdatedBy).HasMaxLength(50);
                });
            }

            // ErrorLogs 實體設定
            if (ErrorLogs != null)
            {
                modelBuilder.Entity<GameSpace.Areas.MiniGame.Models.ErrorLog>(entity =>
                {
                    entity.ToTable("ErrorLogs");
                    entity.HasKey(e => e.LogId);
                    entity.Property(e => e.Level).HasMaxLength(20).IsRequired();
                    entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
                    entity.Property(e => e.Source).HasMaxLength(255);
                    entity.Property(e => e.RequestPath).HasMaxLength(500);
                    entity.Property(e => e.IpAddress).HasMaxLength(50);
                    entity.Property(e => e.UserAgent).HasMaxLength(500);
                });
            }
        }
    }
}

