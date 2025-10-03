using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Services;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Models
{
    public partial class GameSpacedatabaseContext
    {
        public virtual DbSet<PetColorOption> PetColorOptions { get; set; } = null!;
        public virtual DbSet<GameSpace.Areas.MiniGame.Models.PetBackgroundOptionEntity> PetBackgroundOptions { get; set; } = null!;
        public virtual DbSet<PetInteractionBonusRules> PetInteractionBonusRules { get; set; } = null!;
        public virtual DbSet<PetLevelUpRule> PetLevelUpRules { get; set; } = null!;

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // Map MiniGame entities if not already mapped in scaffold
            modelBuilder.Entity<PetColorOption>(entity =>
            {
                entity.ToTable("PetColorOptions");
                entity.HasKey(e => e.ColorOptionId);
                entity.Property(e => e.ColorOptionId).HasColumnName("ColorOptionId");
                entity.Property(e => e.ColorName).HasMaxLength(50);
                entity.Property(e => e.ColorCode).HasMaxLength(7);
            });

            modelBuilder.Entity<PetInteractionBonusRules>(entity =>
            {
                entity.ToTable("PetInteractionBonusRules");
                entity.HasKey(e => e.RuleId);
                entity.Property(e => e.InteractionType).HasMaxLength(50);
            });

            modelBuilder.Entity<PetLevelUpRule>(entity =>
            {
                entity.ToTable("PetLevelUpRules");
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<GameSpace.Areas.MiniGame.Models.PetBackgroundOptionEntity>(entity =>
            {
                entity.ToTable("PetBackgroundOptions");
                entity.HasKey(e => e.BackgroundOptionId);
                entity.Property(e => e.BackgroundName).HasMaxLength(50);
                entity.Property(e => e.BackgroundCode).HasMaxLength(20);
            });
        }
    }
}

