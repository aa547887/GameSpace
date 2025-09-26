using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Minigame Area DbSets
        public DbSet<Users> Users { get; set; }
        public DbSet<User_Wallet> User_Wallet { get; set; }
        public DbSet<User_Rights> User_Rights { get; set; }
        public DbSet<User_Introduce> User_Introduce { get; set; }
        public DbSet<Pet> Pet { get; set; }
        public DbSet<MiniGame> MiniGame { get; set; }
        public DbSet<UserSignInStats> UserSignInStats { get; set; }
        public DbSet<WalletHistory> WalletHistory { get; set; }
        public DbSet<Coupon> Coupon { get; set; }
        public DbSet<CouponType> CouponType { get; set; }
        public DbSet<EVoucher> EVoucher { get; set; }
        public DbSet<EVoucherType> EVoucherType { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Users entity
            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.User_ID);
                entity.Property(e => e.User_ID).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.User_name).IsUnique();
                entity.HasIndex(e => e.User_Account).IsUnique();
                
                // Set default values
                entity.Property(e => e.User_EmailConfirmed).HasDefaultValue(false);
                entity.Property(e => e.User_PhoneNumberConfirmed).HasDefaultValue(false);
                entity.Property(e => e.User_TwoFactorEnabled).HasDefaultValue(false);
                entity.Property(e => e.User_AccessFailedCount).HasDefaultValue(0);
                entity.Property(e => e.User_LockoutEnabled).HasDefaultValue(true);
            });

            // Configure User_Wallet entity
            modelBuilder.Entity<User_Wallet>(entity =>
            {
                entity.HasKey(e => e.User_Id);
                entity.Property(e => e.User_Point).HasDefaultValue(0);
                entity.HasOne(e => e.User)
                    .WithOne(u => u.User_Wallet)
                    .HasForeignKey<User_Wallet>(e => e.User_Id);
            });

            // Configure User_Rights entity
            modelBuilder.Entity<User_Rights>(entity =>
            {
                entity.HasKey(e => e.User_Id);
                entity.HasOne(e => e.User)
                    .WithOne(u => u.User_Rights)
                    .HasForeignKey<User_Rights>(e => e.User_Id);
            });

            // Configure User_Introduce entity
            modelBuilder.Entity<User_Introduce>(entity =>
            {
                entity.HasKey(e => e.User_ID);
                entity.HasIndex(e => e.User_NickName).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Cellphone).IsUnique();
                entity.HasIndex(e => e.IdNumber).IsUnique();
                entity.HasOne(e => e.User)
                    .WithOne(u => u.User_Introduce)
                    .HasForeignKey<User_Introduce>(e => e.User_ID);
            });

            // Configure Pet entity
            modelBuilder.Entity<Pet>(entity =>
            {
                entity.HasKey(e => e.PetID);
                entity.Property(e => e.PetID).ValueGeneratedOnAdd();
                entity.Property(e => e.LevelUpTime).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.SkinColorChangedTime).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.BackgroundColorChangedTime).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.PointsGainedTime_LevelUp).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Pets)
                    .HasForeignKey(e => e.UserID);
                
                // Add check constraints for attribute ranges
                entity.HasCheckConstraint("CK_Pet_Hunger", "[Hunger] >= 0 AND [Hunger] <= 100");
                entity.HasCheckConstraint("CK_Pet_Mood", "[Mood] >= 0 AND [Mood] <= 100");
                entity.HasCheckConstraint("CK_Pet_Stamina", "[Stamina] >= 0 AND [Stamina] <= 100");
                entity.HasCheckConstraint("CK_Pet_Cleanliness", "[Cleanliness] >= 0 AND [Cleanliness] <= 100");
                entity.HasCheckConstraint("CK_Pet_Health", "[Health] >= 0 AND [Health] <= 100");
            });

            // Configure MiniGame entity
            modelBuilder.Entity<MiniGame>(entity =>
            {
                entity.HasKey(e => e.PlayID);
                entity.Property(e => e.PlayID).ValueGeneratedOnAdd();
                entity.Property(e => e.SpeedMultiplier).HasPrecision(5, 2);
                entity.Property(e => e.ExpGainedTime).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.PointsGainedTime).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CouponGainedTime).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.StartTime).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.User)
                    .WithMany(u => u.MiniGames)
                    .HasForeignKey(e => e.UserID);
                entity.HasOne(e => e.Pet)
                    .WithMany(p => p.MiniGames)
                    .HasForeignKey(e => e.PetID);
                
                // Add index for user and time queries
                entity.HasIndex(e => new { e.UserID, e.StartTime });
            });

            // Configure UserSignInStats entity
            modelBuilder.Entity<UserSignInStats>(entity =>
            {
                entity.HasKey(e => e.LogID);
                entity.Property(e => e.LogID).ValueGeneratedOnAdd();
                entity.Property(e => e.SignTime).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.PointsGainedTime).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ExpGainedTime).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.CouponGainedTime).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.User)
                    .WithMany(u => u.SignInStats)
                    .HasForeignKey(e => e.UserID);
                
                // Add index for user and time queries
                entity.HasIndex(e => new { e.UserID, e.SignTime });
            });

            // Configure WalletHistory entity
            modelBuilder.Entity<WalletHistory>(entity =>
            {
                entity.HasKey(e => e.LogID);
                entity.Property(e => e.LogID).ValueGeneratedOnAdd();
                entity.Property(e => e.ChangeTime).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.User)
                    .WithMany(u => u.WalletHistory)
                    .HasForeignKey(e => e.UserID);
                
                // Add index for user and time queries
                entity.HasIndex(e => new { e.UserID, e.ChangeTime });
            });

            // Configure Coupon entity
            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.HasKey(e => e.CouponID);
                entity.Property(e => e.CouponID).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.CouponCode).IsUnique();
                entity.Property(e => e.AcquiredTime).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserID);
                entity.HasOne(e => e.CouponType)
                    .WithMany(ct => ct.Coupons)
                    .HasForeignKey(e => e.CouponTypeID);
                
                // Add index for user and usage queries
                entity.HasIndex(e => new { e.UserID, e.IsUsed, e.AcquiredTime });
            });

            // Configure CouponType entity
            modelBuilder.Entity<CouponType>(entity =>
            {
                entity.HasKey(e => e.CouponTypeID);
                entity.Property(e => e.CouponTypeID).ValueGeneratedOnAdd();
                entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
                entity.Property(e => e.MinSpend).HasPrecision(18, 2);
            });

            // Configure EVoucher entity
            modelBuilder.Entity<EVoucher>(entity =>
            {
                entity.HasKey(e => e.EVoucherID);
                entity.Property(e => e.EVoucherID).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.EVoucherCode).IsUnique();
                entity.Property(e => e.AcquiredTime).HasDefaultValueSql("GETUTCDATE()");
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserID);
                entity.HasOne(e => e.EVoucherType)
                    .WithMany(evt => evt.EVouchers)
                    .HasForeignKey(e => e.EVoucherTypeID);
                
                // Add index for user and usage queries
                entity.HasIndex(e => new { e.UserID, e.IsUsed, e.AcquiredTime });
            });

            // Configure EVoucherType entity
            modelBuilder.Entity<EVoucherType>(entity =>
            {
                entity.HasKey(e => e.EVoucherTypeID);
                entity.Property(e => e.EVoucherTypeID).ValueGeneratedOnAdd();
                entity.Property(e => e.ValueAmount).HasPrecision(18, 2);
            });
        }
    }
}
