using Microsoft.EntityFrameworkCore;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Data
{
    public class MiniGameDbContext : DbContext
    {
        public MiniGameDbContext(DbContextOptions<MiniGameDbContext> options) : base(options)
        {
        }

        // 管理者相關表
        public DbSet<ManagerData> ManagerData { get; set; }
        public DbSet<ManagerRolePermission> ManagerRolePermission { get; set; }
        public DbSet<ManagerRole> ManagerRole { get; set; }

        // 使用者相關表
        public DbSet<Users> Users { get; set; }
        public DbSet<User_Wallet> User_Wallet { get; set; }
        public DbSet<WalletHistory> WalletHistory { get; set; }
        public DbSet<UserTokens> UserTokens { get; set; }

        // 優惠券相關表
        public DbSet<CouponType> CouponType { get; set; }
        public DbSet<Coupon> Coupon { get; set; }

        // 電子禮券相關表
        public DbSet<EVoucherType> EVoucherType { get; set; }
        public DbSet<EVoucher> EVoucher { get; set; }
        public DbSet<EVoucherToken> EVoucherToken { get; set; }
        public DbSet<EVoucherRedeemLog> EVoucherRedeemLog { get; set; }

        // 簽到相關表
        public DbSet<UserSignInStats> UserSignInStats { get; set; }

        // 寵物相關表
        public DbSet<Pet> Pet { get; set; }
        public DbSet<PetAppearanceChangeLog> PetAppearanceChangeLog { get; set; }

        // 小遊戲相關表
        public DbSet<MiniGame> MiniGame { get; set; }
        public DbSet<leaderboard_snapshots> leaderboard_snapshots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 設定複合主鍵
            modelBuilder.Entity<ManagerRole>()
                .HasKey(mr => new { mr.Manager_Id, mr.ManagerRole_Id });

            // 設定外鍵關係
            modelBuilder.Entity<ManagerRole>()
                .HasOne(mr => mr.ManagerData)
                .WithMany()
                .HasForeignKey(mr => mr.Manager_Id);

            modelBuilder.Entity<ManagerRole>()
                .HasOne(mr => mr.ManagerRolePermission)
                .WithMany()
                .HasForeignKey(mr => mr.ManagerRole_Id);

            modelBuilder.Entity<User_Wallet>()
                .HasOne(uw => uw.Users)
                .WithMany()
                .HasForeignKey(uw => uw.User_Id);

            modelBuilder.Entity<WalletHistory>()
                .HasOne(wh => wh.Users)
                .WithMany()
                .HasForeignKey(wh => wh.UserID);

            modelBuilder.Entity<Coupon>()
                .HasOne(c => c.CouponType)
                .WithMany()
                .HasForeignKey(c => c.CouponTypeID);

            modelBuilder.Entity<Coupon>()
                .HasOne(c => c.Users)
                .WithMany()
                .HasForeignKey(c => c.UserID);

            modelBuilder.Entity<EVoucher>()
                .HasOne(ev => ev.EVoucherType)
                .WithMany()
                .HasForeignKey(ev => ev.EVoucherTypeID);

            modelBuilder.Entity<EVoucher>()
                .HasOne(ev => ev.Users)
                .WithMany()
                .HasForeignKey(ev => ev.UserID);

            modelBuilder.Entity<EVoucherToken>()
                .HasOne(et => et.EVoucher)
                .WithMany()
                .HasForeignKey(et => et.EVoucherID);

            modelBuilder.Entity<EVoucherRedeemLog>()
                .HasOne(er => er.EVoucher)
                .WithMany()
                .HasForeignKey(er => er.EVoucherID);

            modelBuilder.Entity<EVoucherRedeemLog>()
                .HasOne(er => er.EVoucherToken)
                .WithMany()
                .HasForeignKey(er => er.TokenID);

            modelBuilder.Entity<EVoucherRedeemLog>()
                .HasOne(er => er.Users)
                .WithMany()
                .HasForeignKey(er => er.UserID);

            modelBuilder.Entity<UserSignInStats>()
                .HasOne(us => us.Users)
                .WithMany()
                .HasForeignKey(us => us.UserID);

            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Users)
                .WithMany()
                .HasForeignKey(p => p.UserID);

            modelBuilder.Entity<PetAppearanceChangeLog>()
                .HasOne(pacl => pacl.Pet)
                .WithMany()
                .HasForeignKey(pacl => pacl.PetID);

            modelBuilder.Entity<MiniGame>()
                .HasOne(mg => mg.Users)
                .WithMany()
                .HasForeignKey(mg => mg.UserID);

            modelBuilder.Entity<MiniGame>()
                .HasOne(mg => mg.Pet)
                .WithMany()
                .HasForeignKey(mg => mg.PetID);

            modelBuilder.Entity<leaderboard_snapshots>()
                .HasOne(ls => ls.Users)
                .WithMany()
                .HasForeignKey(ls => ls.user_id);

            modelBuilder.Entity<UserTokens>()
                .HasOne(ut => ut.Users)
                .WithMany()
                .HasForeignKey(ut => ut.User_ID);

            // 設定索引
            modelBuilder.Entity<ManagerData>()
                .HasIndex(m => m.Manager_Account)
                .IsUnique();

            modelBuilder.Entity<ManagerData>()
                .HasIndex(m => m.Manager_Email)
                .IsUnique();

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.User_Account)
                .IsUnique();

            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.CouponCode)
                .IsUnique();

            modelBuilder.Entity<EVoucher>()
                .HasIndex(ev => ev.EVoucherCode)
                .IsUnique();

            modelBuilder.Entity<EVoucherToken>()
                .HasIndex(et => et.Token)
                .IsUnique();

            modelBuilder.Entity<MiniGame>()
                .HasIndex(mg => mg.SessionID);

            modelBuilder.Entity<leaderboard_snapshots>()
                .HasIndex(ls => new { ls.game_id, ls.period, ls.rank });

            modelBuilder.Entity<UserTokens>()
                .HasIndex(ut => new { ut.User_ID, ut.TokenType, ut.TokenValue });
        }
    }
}
