using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Models;

public partial class GameSpacedatabaseContext : DbContext
{
    public GameSpacedatabaseContext(DbContextOptions<GameSpacedatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<BannedWord> BannedWords { get; set; }

    public virtual DbSet<Bookmark> Bookmarks { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<CouponType> CouponTypes { get; set; }

    public virtual DbSet<Evoucher> Evouchers { get; set; }

    public virtual DbSet<EvoucherRedeemLog> EvoucherRedeemLogs { get; set; }

    public virtual DbSet<EvoucherToken> EvoucherTokens { get; set; }

    public virtual DbSet<EvoucherType> EvoucherTypes { get; set; }

    public virtual DbSet<Forum> Forums { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GameMetricDaily> GameMetricDailies { get; set; }

    public virtual DbSet<GameProductDetail> GameProductDetails { get; set; }

    public virtual DbSet<GameSourceMap> GameSourceMaps { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupBlock> GroupBlocks { get; set; }

    public virtual DbSet<GroupChat> GroupChats { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<LeaderboardSnapshot> LeaderboardSnapshots { get; set; }

    public virtual DbSet<ManagerDatum> ManagerData { get; set; }

    public virtual DbSet<ManagerRole> ManagerRoles { get; set; }

    public virtual DbSet<ManagerRolePermission> ManagerRolePermissions { get; set; }

    public virtual DbSet<MemberSalesProfile> MemberSalesProfiles { get; set; }

    public virtual DbSet<Metric> Metrics { get; set; }

    public virtual DbSet<MetricSource> MetricSources { get; set; }

    public virtual DbSet<MiniGame> MiniGames { get; set; }

    public virtual DbSet<Mute> Mutes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationAction> NotificationActions { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<NotificationSource> NotificationSources { get; set; }

    public virtual DbSet<OfficialStoreRanking> OfficialStoreRankings { get; set; }

    public virtual DbSet<OrderInfo> OrderInfos { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OtherProductDetail> OtherProductDetails { get; set; }

    public virtual DbSet<Pet> Pets { get; set; }

    public virtual DbSet<PlayerMarketOrderInfo> PlayerMarketOrderInfos { get; set; }

    public virtual DbSet<PlayerMarketOrderTradepage> PlayerMarketOrderTradepages { get; set; }

    public virtual DbSet<PlayerMarketProductImg> PlayerMarketProductImgs { get; set; }

    public virtual DbSet<PlayerMarketProductInfo> PlayerMarketProductInfos { get; set; }

    public virtual DbSet<PlayerMarketRanking> PlayerMarketRankings { get; set; }

    public virtual DbSet<PlayerMarketTradeMsg> PlayerMarketTradeMsgs { get; set; }

    public virtual DbSet<PopularityIndexDaily> PopularityIndexDailies { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostMetricSnapshot> PostMetricSnapshots { get; set; }

    public virtual DbSet<PostSource> PostSources { get; set; }

    public virtual DbSet<ProductInfo> ProductInfos { get; set; }

    public virtual DbSet<ProductInfoAuditLog> ProductInfoAuditLogs { get; set; }

    public virtual DbSet<Reaction> Reactions { get; set; }

    public virtual DbSet<Relation> Relations { get; set; }

    public virtual DbSet<RelationStatus> RelationStatuses { get; set; }

    public virtual DbSet<Style> Styles { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Thread> Threads { get; set; }

    public virtual DbSet<ThreadPost> ThreadPosts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserIntroduce> UserIntroduces { get; set; }

    public virtual DbSet<UserRight> UserRights { get; set; }

    public virtual DbSet<UserSalesInformation> UserSalesInformations { get; set; }

    public virtual DbSet<UserSignInStat> UserSignInStats { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    public virtual DbSet<UserWallet> UserWallets { get; set; }

    public virtual DbSet<WalletHistory> WalletHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.ManagerId).HasName("PK__Admins__5A6073FC4CDD9A7F");

            entity.Property(e => e.ManagerId)
                .ValueGeneratedNever()
                .HasColumnName("manager_id");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");
        });

        modelBuilder.Entity<BannedWord>(entity =>
        {
            entity.HasKey(e => e.WordId).HasName("PK__banned_w__7FFA1D409D6C126F");

            entity.ToTable("banned_words");

            entity.Property(e => e.WordId).HasColumnName("word_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Word)
                .HasMaxLength(50)
                .HasColumnName("word");
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bookmark__3213E83F57B7297F");

            entity.ToTable("bookmarks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("target_type");
            entity.Property(e => e.UserId).HasColumnName("User_ID");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Chat_Mes__0BBF6EE6C0CA5B32");

            entity.ToTable("Chat_Message");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.ChatContent)
                .HasMaxLength(255)
                .HasColumnName("chat_content");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.IsSent)
                .HasDefaultValue(true)
                .HasColumnName("is_sent");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("PK__Coupon__384AF1DA8BE2EC1B");

            entity.ToTable("Coupon");

            entity.HasIndex(e => e.CouponCode, "UQ__Coupon__D3490800E02D5094").IsUnique();

            entity.Property(e => e.CouponId).HasColumnName("CouponID");
            entity.Property(e => e.AcquiredTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.CouponCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CouponTypeId).HasColumnName("CouponTypeID");
            entity.Property(e => e.UsedInOrderId).HasColumnName("UsedInOrderID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<CouponType>(entity =>
        {
            entity.HasKey(e => e.CouponTypeId).HasName("PK__CouponTy__095BEDBB40C27770");

            entity.ToTable("CouponType");

            entity.Property(e => e.CouponTypeId).HasColumnName("CouponTypeID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.DiscountType).HasMaxLength(10);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MinSpend).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Evoucher>(entity =>
        {
            entity.HasKey(e => e.EvoucherId).HasName("PK__EVoucher__BC18D35E7FED1B00");

            entity.ToTable("EVoucher");

            entity.HasIndex(e => e.EvoucherCode, "UQ__EVoucher__FFC16097600462FB").IsUnique();

            entity.Property(e => e.EvoucherId).HasColumnName("EVoucherID");
            entity.Property(e => e.AcquiredTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.EvoucherCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EVoucherCode");
            entity.Property(e => e.EvoucherTypeId).HasColumnName("EVoucherTypeID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<EvoucherRedeemLog>(entity =>
        {
            entity.HasKey(e => e.RedeemId).HasName("PK__EVoucher__C9E468F7D96CA46A");

            entity.ToTable("EVoucherRedeemLog");

            entity.Property(e => e.RedeemId).HasColumnName("RedeemID");
            entity.Property(e => e.EvoucherId).HasColumnName("EVoucherID");
            entity.Property(e => e.ScannedAt).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TokenId).HasColumnName("TokenID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<EvoucherToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__EVoucher__658FEE8A56682205");

            entity.ToTable("EVoucherToken");

            entity.HasIndex(e => e.Token, "UQ__EVoucher__1EB4F8172885416F").IsUnique();

            entity.Property(e => e.TokenId).HasColumnName("TokenID");
            entity.Property(e => e.EvoucherId).HasColumnName("EVoucherID");
            entity.Property(e => e.Token)
                .HasMaxLength(64)
                .IsUnicode(false);
        });

        modelBuilder.Entity<EvoucherType>(entity =>
        {
            entity.HasKey(e => e.EvoucherTypeId).HasName("PK__EVoucher__CC0CEC9BF0544113");

            entity.ToTable("EVoucherType");

            entity.Property(e => e.EvoucherTypeId).HasColumnName("EVoucherTypeID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ValueAmount).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Forum>(entity =>
        {
            entity.HasKey(e => e.ForumId).HasName("PK__forums__69A2FA58F53835D6");

            entity.ToTable("forums");

            entity.Property(e => e.ForumId).HasColumnName("forum_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("PK__games__FFE11FCF02E04A95");

            entity.ToTable("games");

            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Genre)
                .HasMaxLength(50)
                .HasColumnName("genre");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.NameZh)
                .HasMaxLength(100)
                .HasColumnName("name_zh");
        });

        modelBuilder.Entity<GameMetricDaily>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__game_met__3213E83F5EF2AAAD");

            entity.ToTable("game_metric_daily");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AggMethod)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("agg_method");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.MetricId).HasColumnName("metric_id");
            entity.Property(e => e.Quality)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("quality");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Value)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("value");
        });

        modelBuilder.Entity<GameProductDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__GameProd__47027DF58E7C3B6C");

            entity.Property(e => e.ProductId)
                .ValueGeneratedNever()
                .HasColumnName("product_id");
            entity.Property(e => e.DownloadLink)
                .HasMaxLength(1)
                .HasColumnName("download_link");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.GameName)
                .HasMaxLength(1)
                .HasColumnName("game_name");
            entity.Property(e => e.PlatformId).HasColumnName("platform_id");
            entity.Property(e => e.ProductDescription)
                .HasMaxLength(1)
                .HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(1)
                .HasColumnName("product_name");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
        });

        modelBuilder.Entity<GameSourceMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__game_sou__3213E83F5691E0A8");

            entity.ToTable("game_source_map");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExternalKey)
                .HasMaxLength(255)
                .HasColumnName("external_key");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Groups__D57795A09031C152");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.GroupName)
                .HasMaxLength(10)
                .HasColumnName("group_name");
        });

        modelBuilder.Entity<GroupBlock>(entity =>
        {
            entity.HasKey(e => e.BlockId).HasName("PK__Group_Bl__A67E647D3FB3BC58");

            entity.ToTable("Group_Block");

            entity.Property(e => e.BlockId).HasColumnName("block_id");
            entity.Property(e => e.BlockedBy).HasColumnName("blocked_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.UserId).HasColumnName("User_ID");
        });

        modelBuilder.Entity<GroupChat>(entity =>
        {
            entity.HasKey(e => e.GroupChatId).HasName("PK__Group_Ch__C4565A195736EBC9");

            entity.ToTable("Group_Chat");

            entity.Property(e => e.GroupChatId).HasColumnName("group_chat_id");
            entity.Property(e => e.GroupChatContent)
                .HasMaxLength(255)
                .HasColumnName("group_chat_content");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.IsSent)
                .HasDefaultValue(true)
                .HasColumnName("is_sent");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => new { e.GroupId, e.UserId }).HasName("PK__Group_Me__C7714CB90F2F0F35");

            entity.ToTable("Group_Member");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.JoinedAt).HasColumnName("joined_at");
        });

        modelBuilder.Entity<LeaderboardSnapshot>(entity =>
        {
            entity.HasKey(e => e.SnapshotId).HasName("PK__leaderbo__C27CFBF7C0659D76");

            entity.ToTable("leaderboard_snapshots");

            entity.Property(e => e.SnapshotId).HasColumnName("snapshot_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.IndexValue)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("index_value");
            entity.Property(e => e.Period)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("period");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.Ts).HasColumnName("ts");
        });

        modelBuilder.Entity<ManagerDatum>(entity =>
        {
            entity.HasKey(e => e.ManagerId).HasName("PK__ManagerD__AE5FEFADE4B0BC60");

            entity.HasIndex(e => e.ManagerEmail, "UQ__ManagerD__0890969E1181AD83").IsUnique();

            entity.HasIndex(e => e.ManagerAccount, "UQ__ManagerD__62B5E211CDD5CA1F").IsUnique();

            entity.Property(e => e.ManagerId)
                .ValueGeneratedNever()
                .HasColumnName("Manager_Id");
            entity.Property(e => e.AdministratorRegistrationDate).HasColumnName("Administrator_registration_date");
            entity.Property(e => e.ManagerAccessFailedCount).HasColumnName("Manager_AccessFailedCount");
            entity.Property(e => e.ManagerAccount)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("Manager_Account");
            entity.Property(e => e.ManagerEmail)
                .HasMaxLength(255)
                .HasColumnName("Manager_Email");
            entity.Property(e => e.ManagerEmailConfirmed).HasColumnName("Manager_EmailConfirmed");
            entity.Property(e => e.ManagerLockoutEnabled)
                .HasDefaultValue(true)
                .HasColumnName("Manager_LockoutEnabled");
            entity.Property(e => e.ManagerLockoutEnd).HasColumnName("Manager_LockoutEnd");
            entity.Property(e => e.ManagerName)
                .HasMaxLength(30)
                .HasColumnName("Manager_Name");
            entity.Property(e => e.ManagerPassword)
                .HasMaxLength(200)
                .HasColumnName("Manager_Password");
        });

        modelBuilder.Entity<ManagerRole>(entity =>
        {
            entity.HasKey(e => new { e.ManagerId, e.ManagerRoleId }).HasName("PK__ManagerR__6270897E1F2E67E2");

            entity.ToTable("ManagerRole");

            entity.Property(e => e.ManagerId).HasColumnName("Manager_Id");
            entity.Property(e => e.ManagerRoleId).HasColumnName("ManagerRole_Id");
            entity.Property(e => e.ManagerRole1)
                .HasMaxLength(30)
                .HasColumnName("ManagerRole");
        });

        modelBuilder.Entity<ManagerRolePermission>(entity =>
        {
            entity.HasKey(e => e.ManagerRoleId).HasName("PK__ManagerR__C2F66D3D5F28809B");

            entity.ToTable("ManagerRolePermission");

            entity.Property(e => e.ManagerRoleId)
                .ValueGeneratedNever()
                .HasColumnName("ManagerRole_Id");
            entity.Property(e => e.CustomerService).HasColumnName("customer_service");
            entity.Property(e => e.PetRightsManagement).HasColumnName("Pet_Rights_Management");
            entity.Property(e => e.RoleName)
                .HasMaxLength(30)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<MemberSalesProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__MemberSa__206D917065DCCCC2");

            entity.ToTable("MemberSalesProfile");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_Id");
            entity.Property(e => e.BankAccountNumber)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Metric>(entity =>
        {
            entity.HasKey(e => e.MetricId).HasName("PK__metrics__13D5DCA43E71B6AD");

            entity.ToTable("metrics");

            entity.Property(e => e.MetricId).HasColumnName("metric_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.Unit)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("unit");
        });

        modelBuilder.Entity<MetricSource>(entity =>
        {
            entity.HasKey(e => e.SourceId).HasName("PK__metric_s__3035A9B66F25A9ED");

            entity.ToTable("metric_sources");

            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(50)
                .HasColumnName("note");
        });

        modelBuilder.Entity<MiniGame>(entity =>
        {
            entity.HasKey(e => e.PlayId).HasName("PK__MiniGame__7CA45E841627F5AB");

            entity.ToTable("MiniGame");

            entity.Property(e => e.PlayId).HasColumnName("PlayID");
            entity.Property(e => e.CouponGained)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("0");
            entity.Property(e => e.CouponGainedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.ExpGainedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.PetId).HasColumnName("PetID");
            entity.Property(e => e.PointsGainedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.Result)
                .HasMaxLength(10)
                .HasDefaultValue("Abort");
            entity.Property(e => e.SpeedMultiplier)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.StartTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<Mute>(entity =>
        {
            entity.HasKey(e => e.MuteId).HasName("PK__Mutes__84EE96EB3976FAA5");

            entity.Property(e => e.MuteId).HasColumnName("mute_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.MuteName)
                .HasMaxLength(10)
                .HasColumnName("mute_name");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F51D097D2");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.ActionId).HasColumnName("action_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.NotificationMessage)
                .HasMaxLength(255)
                .HasColumnName("notification_message");
            entity.Property(e => e.NotificationTitle)
                .HasMaxLength(20)
                .HasColumnName("notification_title");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SenderManagerId).HasColumnName("sender_manager_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
        });

        modelBuilder.Entity<NotificationAction>(entity =>
        {
            entity.HasKey(e => e.ActionId).HasName("PK__Notifica__74EFC2173EF82DD6");

            entity.ToTable("Notification_Actions");

            entity.Property(e => e.ActionId).HasColumnName("action_id");
            entity.Property(e => e.ActionName)
                .HasMaxLength(10)
                .HasColumnName("action_name");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => e.RecipientId).HasName("PK__Notifica__FA0A40273B6FB4E0");

            entity.ToTable("Notification_Recipients");

            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.UserId).HasColumnName("User_ID");
        });

        modelBuilder.Entity<NotificationSource>(entity =>
        {
            entity.HasKey(e => e.SourceId).HasName("PK__Notifica__3035A9B6A438C994");

            entity.ToTable("Notification_Sources");

            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceName)
                .HasMaxLength(10)
                .HasColumnName("source_name");
        });

        modelBuilder.Entity<OfficialStoreRanking>(entity =>
        {
            entity.HasKey(e => e.RankingId).HasName("PK__Official__95F5B23D1A6C24A6");

            entity.ToTable("Official_Store_Ranking");

            entity.Property(e => e.RankingId)
                .ValueGeneratedNever()
                .HasColumnName("ranking_id");
            entity.Property(e => e.PeriodType)
                .HasMaxLength(1)
                .HasColumnName("period_type");
            entity.Property(e => e.ProductId).HasColumnName("product_ID");
            entity.Property(e => e.RankingDate).HasColumnName("ranking_date");
            entity.Property(e => e.RankingMetric)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("ranking_metric");
            entity.Property(e => e.RankingPosition).HasColumnName("ranking_position");
            entity.Property(e => e.RankingUpdatedAt).HasColumnName("ranking_updated_at");
            entity.Property(e => e.TradingAmount)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("trading_amount");
            entity.Property(e => e.TradingVolume).HasColumnName("trading_volume");
        });

        modelBuilder.Entity<OrderInfo>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__OrderInf__46596229A3B8C636");

            entity.ToTable("OrderInfo");

            entity.Property(e => e.OrderId)
                .ValueGeneratedNever()
                .HasColumnName("order_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.OrderDate).HasColumnName("order_date");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(1)
                .HasColumnName("order_status");
            entity.Property(e => e.OrderTotal)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("order_total");
            entity.Property(e => e.PaymentAt).HasColumnName("payment_at");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(1)
                .HasColumnName("payment_status");
            entity.Property(e => e.ShippedAt).HasColumnName("shipped_at");
            entity.Property(e => e.UserId).HasColumnName("User_ID");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__OrderIte__52020FDD3AE70050");

            entity.Property(e => e.ItemId)
                .ValueGeneratedNever()
                .HasColumnName("item_id");
            entity.Property(e => e.LineNo).HasColumnName("line_no");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("subtotal");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("unit_price");
        });

        modelBuilder.Entity<OtherProductDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__OtherPro__47027DF5B3F8E7A5");

            entity.Property(e => e.ProductId)
                .ValueGeneratedNever()
                .HasColumnName("product_id");
            entity.Property(e => e.Color)
                .HasMaxLength(1)
                .HasColumnName("color");
            entity.Property(e => e.DigitalCode)
                .HasMaxLength(1)
                .HasColumnName("digital_code");
            entity.Property(e => e.Dimensions)
                .HasMaxLength(1)
                .HasColumnName("dimensions");
            entity.Property(e => e.Material)
                .HasMaxLength(1)
                .HasColumnName("material");
            entity.Property(e => e.PlatformId).HasColumnName("platform_id");
            entity.Property(e => e.ProductDescription)
                .HasMaxLength(1)
                .HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(1)
                .HasColumnName("product_name");
            entity.Property(e => e.Size)
                .HasMaxLength(1)
                .HasColumnName("size");
            entity.Property(e => e.StockQuantity)
                .HasMaxLength(1)
                .HasColumnName("stock_quantity");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Weight)
                .HasMaxLength(1)
                .HasColumnName("weight");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PK__Pet__48E53802FF2786AE");

            entity.ToTable("Pet");

            entity.Property(e => e.PetId).HasColumnName("PetID");
            entity.Property(e => e.BackgroundColor)
                .HasMaxLength(50)
                .HasDefaultValue("??");
            entity.Property(e => e.BackgroundColorChangedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.LevelUpTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.PetName)
                .HasMaxLength(50)
                .HasDefaultValue("???");
            entity.Property(e => e.PointsChangedBackgroundColor).HasColumnName("PointsChanged_BackgroundColor");
            entity.Property(e => e.PointsChangedSkinColor).HasColumnName("PointsChanged_SkinColor");
            entity.Property(e => e.PointsGainedLevelUp).HasColumnName("PointsGained_LevelUp");
            entity.Property(e => e.PointsGainedTimeLevelUp)
                .HasDefaultValueSql("('SYSUTCDATETIME()')")
                .HasColumnName("PointsGainedTime_LevelUp");
            entity.Property(e => e.SkinColor)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("#ADD8E6");
            entity.Property(e => e.SkinColorChangedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<PlayerMarketOrderInfo>(entity =>
        {
            entity.HasKey(e => e.POrderId).HasName("PK__PlayerMa__AACAAD70FD95035E");

            entity.ToTable("PlayerMarketOrderInfo");

            entity.Property(e => e.POrderId)
                .ValueGeneratedNever()
                .HasColumnName("p_order_id");
            entity.Property(e => e.BuyerId).HasColumnName("buyer_id");
            entity.Property(e => e.POrderCreatedAt).HasColumnName("p_order_created_at");
            entity.Property(e => e.POrderDate).HasColumnName("p_order_date");
            entity.Property(e => e.POrderStatus)
                .HasMaxLength(1)
                .HasColumnName("p_order_status");
            entity.Property(e => e.POrderTotal).HasColumnName("p_order_total");
            entity.Property(e => e.POrderUpdatedAt).HasColumnName("p_order_updated_at");
            entity.Property(e => e.PPaymentStatus)
                .HasMaxLength(1)
                .HasColumnName("p_payment_status");
            entity.Property(e => e.PProductId).HasColumnName("p_product_id");
            entity.Property(e => e.PQuantity).HasColumnName("p_quantity");
            entity.Property(e => e.PUnitPrice).HasColumnName("p_unit_price");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
        });

        modelBuilder.Entity<PlayerMarketOrderTradepage>(entity =>
        {
            entity.HasKey(e => e.POrderTradepageId).HasName("PK__PlayerMa__4E2C726D66020635");

            entity.ToTable("PlayerMarketOrderTradepage");

            entity.Property(e => e.POrderTradepageId)
                .ValueGeneratedNever()
                .HasColumnName("p_order_tradepage_id");
            entity.Property(e => e.BuyerReceivedAt).HasColumnName("buyer_received_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.POrderId).HasColumnName("p_order_id");
            entity.Property(e => e.POrderPlatformFee).HasColumnName("p_order_platform_fee");
            entity.Property(e => e.PProductId).HasColumnName("p_product_id");
            entity.Property(e => e.SellerTransferredAt).HasColumnName("seller_transferred_at");
        });

        modelBuilder.Entity<PlayerMarketProductImg>(entity =>
        {
            entity.HasKey(e => e.PProductImgId).HasName("PK__PlayerMa__75AAE6F071912594");

            entity.Property(e => e.PProductImgId)
                .ValueGeneratedNever()
                .HasColumnName("p_product_img_id");
            entity.Property(e => e.PProductId).HasColumnName("p_product_id");
            entity.Property(e => e.PProductImgUrl).HasColumnName("p_product_img_url");
        });

        modelBuilder.Entity<PlayerMarketProductInfo>(entity =>
        {
            entity.HasKey(e => e.PProductId).HasName("PK__PlayerMa__A33C8165839F029D");

            entity.ToTable("PlayerMarketProductInfo");

            entity.Property(e => e.PProductId)
                .ValueGeneratedNever()
                .HasColumnName("p_product_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.PProductDescription)
                .HasMaxLength(1)
                .HasColumnName("p_product_description");
            entity.Property(e => e.PProductImgId)
                .HasMaxLength(1)
                .HasColumnName("p_product_img_id");
            entity.Property(e => e.PProductName)
                .HasMaxLength(1)
                .HasColumnName("p_product_name");
            entity.Property(e => e.PProductTitle)
                .HasMaxLength(1)
                .HasColumnName("p_product_title");
            entity.Property(e => e.PProductType)
                .HasMaxLength(1)
                .HasColumnName("p_product_type");
            entity.Property(e => e.PStatus)
                .HasMaxLength(1)
                .HasColumnName("p_status");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.SellerId).HasColumnName("seller_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<PlayerMarketRanking>(entity =>
        {
            entity.HasKey(e => e.PRankingId).HasName("PK__PlayerMa__2B50ED3233169DE1");

            entity.ToTable("PlayerMarketRanking");

            entity.Property(e => e.PRankingId)
                .ValueGeneratedNever()
                .HasColumnName("p_ranking_id");
            entity.Property(e => e.PPeriodType)
                .HasMaxLength(255)
                .HasColumnName("p_period_type");
            entity.Property(e => e.PProductId).HasColumnName("p_product_id");
            entity.Property(e => e.PRankingDate).HasColumnName("p_ranking_date");
            entity.Property(e => e.PRankingMetric)
                .HasMaxLength(255)
                .HasColumnName("p_ranking_metric");
            entity.Property(e => e.PRankingPosition).HasColumnName("p_ranking_position");
            entity.Property(e => e.PTradingAmount)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("p_trading_amount");
            entity.Property(e => e.PTradingVolume).HasColumnName("p_trading_volume");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<PlayerMarketTradeMsg>(entity =>
        {
            entity.HasKey(e => e.TradeMsgId).HasName("PK__PlayerMa__C2FA77A2C7BF562C");

            entity.ToTable("PlayerMarketTradeMsg");

            entity.Property(e => e.TradeMsgId)
                .ValueGeneratedNever()
                .HasColumnName("trade_msg_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.MessageText)
                .HasMaxLength(1)
                .HasColumnName("message_text");
            entity.Property(e => e.MsgFrom)
                .HasMaxLength(1)
                .HasColumnName("msg_from");
            entity.Property(e => e.POrderTradepageId).HasColumnName("p_order_tradepage_id");
        });

        modelBuilder.Entity<PopularityIndexDaily>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__populari__3213E83F6DAE1ACC");

            entity.ToTable("popularity_index_daily");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.IndexValue)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("index_value");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__posts__3ED787660041BA3D");

            entity.ToTable("posts");

            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.BodyMd)
                .HasMaxLength(3000)
                .HasColumnName("body_md");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Pinned).HasColumnName("pinned");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
            entity.Property(e => e.Tldr)
                .HasMaxLength(50)
                .HasColumnName("tldr");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<PostMetricSnapshot>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__post_met__3ED787661D1BD6DE");

            entity.ToTable("post_metric_snapshot");

            entity.Property(e => e.PostId)
                .ValueGeneratedNever()
                .HasColumnName("post_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.DetailsJson).HasColumnName("details_json");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.IndexValue)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("index_value");
        });

        modelBuilder.Entity<PostSource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__post_sou__3213E83FD66C029A");

            entity.ToTable("post_sources");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.SourceName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("source_name");
            entity.Property(e => e.Url)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("url");
        });

        modelBuilder.Entity<ProductInfo>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__ProductI__47027DF58125E1D7");

            entity.ToTable("ProductInfo");

            entity.Property(e => e.ProductId)
                .ValueGeneratedNever()
                .HasColumnName("product_id");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(1)
                .HasColumnName("currency_code");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("price");
            entity.Property(e => e.ProductCreatedAt).HasColumnName("product_created_at");
            entity.Property(e => e.ProductCreatedBy)
                .HasMaxLength(1)
                .HasColumnName("product_created_by");
            entity.Property(e => e.ProductName)
                .HasMaxLength(1)
                .HasColumnName("product_name");
            entity.Property(e => e.ProductType)
                .HasMaxLength(1)
                .HasColumnName("product_type");
            entity.Property(e => e.ProductUpdatedAt).HasColumnName("product_updated_at");
            entity.Property(e => e.ProductUpdatedBy)
                .HasMaxLength(1)
                .HasColumnName("product_updated_by");
            entity.Property(e => e.ShipmentQuantity).HasColumnName("Shipment_Quantity");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<ProductInfoAuditLog>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("ProductInfoAuditLog");

            entity.Property(e => e.ActionType)
                .HasMaxLength(1)
                .HasColumnName("action_type");
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            entity.Property(e => e.FieldName)
                .HasMaxLength(1)
                .HasColumnName("field_name");
            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.ManagerId).HasColumnName("Manager_Id");
            entity.Property(e => e.NewValue)
                .HasMaxLength(1)
                .HasColumnName("new_value");
            entity.Property(e => e.OldValue)
                .HasMaxLength(1)
                .HasColumnName("old_value");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
        });

        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reaction__3213E83FB43B5B91");

            entity.ToTable("reactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Kind)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("kind");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("target_type");
            entity.Property(e => e.UserId).HasColumnName("User_ID");
        });

        modelBuilder.Entity<Relation>(entity =>
        {
            entity.HasKey(e => e.RelationId).HasName("PK__Relation__C409F323D069FBCA");

            entity.ToTable("Relation");

            entity.Property(e => e.RelationId).HasColumnName("relation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.FriendId).HasColumnName("friend_id");
            entity.Property(e => e.FriendNickname)
                .HasMaxLength(10)
                .HasColumnName("friend_nickname");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("User_ID");
        });

        modelBuilder.Entity<RelationStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Relation__3683B5314ED71F1C");

            entity.ToTable("Relation_Status");

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(10)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Style>(entity =>
        {
            entity.HasKey(e => e.StyleId).HasName("PK__Styles__D333B39778E07B07");

            entity.Property(e => e.StyleId).HasColumnName("style_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.EffectDesc)
                .HasMaxLength(255)
                .HasColumnName("effect_desc");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.StyleName)
                .HasMaxLength(10)
                .HasColumnName("style_name");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__6EE594E8E2D3D0CE");

            entity.ToTable("Supplier");

            entity.Property(e => e.SupplierId)
                .ValueGeneratedNever()
                .HasColumnName("supplier_id");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(1)
                .HasColumnName("supplier_name");
        });

        modelBuilder.Entity<Thread>(entity =>
        {
            entity.HasKey(e => e.ThreadId).HasName("PK__threads__7411E2F0BB8D8D3A");

            entity.ToTable("threads");

            entity.Property(e => e.ThreadId).HasColumnName("thread_id");
            entity.Property(e => e.AuthorUserId).HasColumnName("author_User_ID");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ForumId).HasColumnName("forum_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ThreadPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__thread_p__3213E83F62A6D951");

            entity.ToTable("thread_posts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorUserId).HasColumnName("author_User_ID");
            entity.Property(e => e.ContentMd)
                .HasMaxLength(3000)
                .HasColumnName("content_md");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ParentPostId).HasColumnName("parent_post_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.ThreadId).HasColumnName("thread_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__206D9190F94E2868");

            entity.HasIndex(e => e.UserName, "UQ__Users__5F1A1086A04695C2").IsUnique();

            entity.HasIndex(e => e.UserAccount, "UQ__Users__899F4A91548A4715").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.UserAccessFailedCount).HasColumnName("User_AccessFailedCount");
            entity.Property(e => e.UserAccount)
                .HasMaxLength(30)
                .HasColumnName("User_Account");
            entity.Property(e => e.UserEmailConfirmed).HasColumnName("User_EmailConfirmed");
            entity.Property(e => e.UserLockoutEnabled)
                .HasDefaultValue(true)
                .HasColumnName("User_LockoutEnabled");
            entity.Property(e => e.UserLockoutEnd).HasColumnName("User_LockoutEnd");
            entity.Property(e => e.UserName)
                .HasMaxLength(30)
                .HasColumnName("User_name");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(30)
                .HasColumnName("User_Password");
            entity.Property(e => e.UserPhoneNumberConfirmed).HasColumnName("User_PhoneNumberConfirmed");
            entity.Property(e => e.UserTwoFactorEnabled).HasColumnName("User_TwoFactorEnabled");
        });

        modelBuilder.Entity<UserIntroduce>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_Int__206D91903B31A98C");

            entity.ToTable("User_Introduce");

            entity.HasIndex(e => e.IdNumber, "UQ__User_Int__62DF80337E950A85").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User_Int__A9D1053412D26DAF").IsUnique();

            entity.HasIndex(e => e.Cellphone, "UQ__User_Int__CDE19CF2CA57F01A").IsUnique();

            entity.HasIndex(e => e.UserNickName, "UQ__User_Int__DAFD02CF6E64C5AF").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_ID");
            entity.Property(e => e.Address).HasMaxLength(30);
            entity.Property(e => e.Cellphone)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CreateAccount).HasColumnName("Create_Account");
            entity.Property(e => e.Email).HasMaxLength(30);
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.IdNumber)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UserIntroduce1)
                .HasMaxLength(200)
                .HasColumnName("User_Introduce");
            entity.Property(e => e.UserNickName)
                .HasMaxLength(30)
                .HasColumnName("User_NickName");
            entity.Property(e => e.UserPicture).HasColumnName("User_Picture");
        });

        modelBuilder.Entity<UserRight>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_Rig__206D917090437E3C");

            entity.ToTable("User_Rights");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_Id");
            entity.Property(e => e.UserStatus).HasColumnName("User_Status");
        });

        modelBuilder.Entity<UserSalesInformation>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_Sal__206D91708AEF3B87");

            entity.ToTable("User_Sales_Information");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_Id");
            entity.Property(e => e.UserSalesWallet).HasColumnName("UserSales_Wallet");
        });

        modelBuilder.Entity<UserSignInStat>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__UserSign__5E5499A8CAE9D505");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.CouponGained)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("0");
            entity.Property(e => e.CouponGainedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.ExpGainedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.PointsGainedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.SignTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__UserToke__AA16D54011F2FC03");

            entity.Property(e => e.TokenId).HasColumnName("Token_ID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.Value).HasMaxLength(255);
        });

        modelBuilder.Entity<UserWallet>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_Wal__206D9170ED22670A");

            entity.ToTable("User_Wallet");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_Id");
            entity.Property(e => e.UserPoint).HasColumnName("User_Point");
        });

        modelBuilder.Entity<WalletHistory>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__WalletHi__5E5499A83ECD128B");

            entity.ToTable("WalletHistory");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.ChangeTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.ChangeType).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
