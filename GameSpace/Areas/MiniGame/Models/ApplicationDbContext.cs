using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for all your entities
        public DbSet<User> Users { get; set; }
        public DbSet<UserIntroduce> UserIntroduces { get; set; }
        public DbSet<UserRights> UserRights { get; set; }
        public DbSet<UserWallet> UserWallets { get; set; }
        public DbSet<WalletHistory> WalletHistories { get; set; }
        public DbSet<UserSignInStats> UserSignInStats { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<MiniGame> MiniGames { get; set; }
        public DbSet<CouponType> CouponTypes { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<EVoucherType> EVoucherTypes { get; set; }
        public DbSet<EVoucher> EVouchers { get; set; }
        public DbSet<ManagerData> ManagerData { get; set; }
        public DbSet<ManagerRolePermission> ManagerRolePermissions { get; set; }
        public DbSet<ManagerRole> ManagerRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure primary keys for composite keys if any
            modelBuilder.Entity<ManagerRole>().HasKey(mr => new { mr.Manager_Id, mr.ManagerRole_Id });

            // Configure relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserWallet)
                .WithOne(uw => uw.User)
                .HasForeignKey<UserWallet>(uw => uw.User_Id);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserRights)
                .WithOne(ur => ur.User)
                .HasForeignKey<UserRights>(ur => ur.User_Id);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserIntroduce)
                .WithOne(ui => ui.User)
                .HasForeignKey<UserIntroduce>(ui => ui.User_ID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.WalletHistories)
                .WithOne(wh => wh.User)
                .HasForeignKey(wh => wh.UserID);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Pet)
                .WithOne(p => p.User)
                .HasForeignKey<Pet>(p => p.UserID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.MiniGames)
                .WithOne(mg => mg.User)
                .HasForeignKey(mg => mg.UserID);

            modelBuilder.Entity<Pet>()
                .HasMany(p => p.MiniGames)
                .WithOne(mg => mg.Pet)
                .HasForeignKey(mg => mg.PetID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Coupons)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserID);

            modelBuilder.Entity<CouponType>()
                .HasMany(ct => ct.Coupons)
                .WithOne(c => c.CouponType)
                .HasForeignKey(c => c.CouponTypeID);

            modelBuilder.Entity<User>()
                .HasMany(u => u.EVouchers)
                .WithOne(ev => ev.User)
                .HasForeignKey(ev => ev.UserID);

            modelBuilder.Entity<EVoucherType>()
                .HasMany(evt => evt.EVouchers)
                .WithOne(ev => ev.EVoucherType)
                .HasForeignKey(ev => ev.EVoucherTypeID);

            modelBuilder.Entity<ManagerData>()
                .HasMany(md => md.ManagerRoles)
                .WithOne(mr => mr.ManagerData)
                .HasForeignKey(mr => mr.Manager_Id);

            modelBuilder.Entity<ManagerRolePermission>()
                .HasMany(mrp => mrp.ManagerRoles)
                .WithOne(mr => mr.ManagerRolePermission)
                .HasForeignKey(mr => mr.ManagerRole_Id);

            // Add unique constraints from schema
            modelBuilder.Entity<ManagerData>().HasIndex(md => md.Manager_Account).IsUnique();
            modelBuilder.Entity<ManagerData>().HasIndex(md => md.Manager_Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.User_name).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.User_Account).IsUnique();
            modelBuilder.Entity<UserIntroduce>().HasIndex(ui => ui.IdNumber).IsUnique();
            modelBuilder.Entity<UserIntroduce>().HasIndex(ui => ui.Cellphone).IsUnique();
            modelBuilder.Entity<UserIntroduce>().HasIndex(ui => ui.Email).IsUnique();
            modelBuilder.Entity<UserIntroduce>().HasIndex(ui => ui.User_NickName).IsUnique();
            modelBuilder.Entity<Coupon>().HasIndex(c => c.CouponCode).IsUnique();
            modelBuilder.Entity<EVoucher>().HasIndex(ev => ev.EVoucherCode).IsUnique();

            // Default values (as per schema, some are handled by C# defaults or database defaults)
            modelBuilder.Entity<UserWallet>().Property(uw => uw.User_Point).HasDefaultValue(0);
            modelBuilder.Entity<User>().Property(u => u.User_EmailConfirmed).HasDefaultValue(false);
            modelBuilder.Entity<User>().Property(u => u.User_PhoneNumberConfirmed).HasDefaultValue(false);
            modelBuilder.Entity<User>().Property(u => u.User_TwoFactorEnabled).HasDefaultValue(false);
            modelBuilder.Entity<User>().Property(u => u.User_AccessFailedCount).HasDefaultValue(0);
            modelBuilder.Entity<User>().Property(u => u.User_LockoutEnabled).HasDefaultValue(true);
            modelBuilder.Entity<ManagerData>().Property(md => md.Manager_EmailConfirmed).HasDefaultValue(false);
            modelBuilder.Entity<ManagerData>().Property(md => md.Manager_AccessFailedCount).HasDefaultValue(0);
            modelBuilder.Entity<ManagerData>().Property(md => md.Manager_LockoutEnabled).HasDefaultValue(true);
        }
    }
}
