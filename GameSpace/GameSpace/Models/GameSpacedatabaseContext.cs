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

    public virtual DbSet<ManagerRolePermission> ManagerRolePermissions { get; set; }

    public virtual DbSet<MemberSalesProfile> MemberSalesProfiles { get; set; }

    public virtual DbSet<MerchType> MerchTypes { get; set; }

    public virtual DbSet<Metric> Metrics { get; set; }

    public virtual DbSet<MetricSource> MetricSources { get; set; }

    public virtual DbSet<MiniGame> MiniGames { get; set; }

    public virtual DbSet<Mute> Mutes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationAction> NotificationActions { get; set; }

    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public virtual DbSet<NotificationSource> NotificationSources { get; set; }

    public virtual DbSet<OfficialStoreRanking> OfficialStoreRankings { get; set; }

    public virtual DbSet<OrderAddress> OrderAddresses { get; set; }

    public virtual DbSet<OrderInfo> OrderInfos { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<OtherProductDetail> OtherProductDetails { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

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

    public virtual DbSet<ProductCode> ProductCodes { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductInfo> ProductInfos { get; set; }

    public virtual DbSet<ProductInfoAuditLog> ProductInfoAuditLogs { get; set; }

    public virtual DbSet<Reaction> Reactions { get; set; }

    public virtual DbSet<Relation> Relations { get; set; }

    public virtual DbSet<RelationStatus> RelationStatuses { get; set; }

    public virtual DbSet<Shipment> Shipments { get; set; }

    public virtual DbSet<StockMovement> StockMovements { get; set; }

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

    public virtual DbSet<VwForumOverview> VwForumOverviews { get; set; }

    public virtual DbSet<VwPostsWithSnapshot> VwPostsWithSnapshots { get; set; }

    public virtual DbSet<VwThreadActivity> VwThreadActivities { get; set; }

    public virtual DbSet<VwThreadPostsFlat> VwThreadPostsFlats { get; set; }

    public virtual DbSet<WalletHistory> WalletHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.ManagerId).HasName("PK__Admins__5A6073FC25738460");

            entity.Property(e => e.ManagerId)
                .ValueGeneratedNever()
                .HasColumnName("manager_id");
            entity.Property(e => e.LastLogin).HasColumnName("last_login");

            entity.HasOne(d => d.Manager).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.ManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Admins__manager___2022C2A6");
        });

        modelBuilder.Entity<BannedWord>(entity =>
        {
            entity.HasKey(e => e.WordId).HasName("PK__banned_w__7FFA1D403514B58B");

            entity.ToTable("banned_words");

            entity.Property(e => e.WordId).HasColumnName("word_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Word)
                .HasMaxLength(100)
                .HasColumnName("word");
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bookmark__3213E83FE0BF66EB");

            entity.ToTable("bookmarks");

            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "IX_bookmarks_target");

            entity.HasIndex(e => new { e.UserId, e.TargetType, e.TargetId }, "UQ_bookmarks_user_target").IsUnique();

            entity.HasIndex(e => new { e.UserId, e.TargetType, e.TargetId }, "bookmarks_index_20").IsUnique();

            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "bookmarks_index_21");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("target_type");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.User).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__bookmarks__User___11D4A34F");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Chat_Mes__0BBF6EE69E0E503A");

            entity.ToTable("Chat_Message");

            entity.HasIndex(e => e.SentAt, "Chat_Message_index_3");

            entity.HasIndex(e => new { e.ReceiverId, e.SentAt }, "Chat_Message_index_4");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.ChatContent)
                .HasMaxLength(100)
                .HasColumnName("chat_content");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.IsSent)
                .HasDefaultValue(true)
                .HasColumnName("is_sent");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");

            entity.HasOne(d => d.Manager).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__Chat_Mess__manag__29AC2CE0");

            entity.HasOne(d => d.Receiver).WithMany(p => p.ChatMessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK__Chat_Mess__recei__2B947552");

            entity.HasOne(d => d.Sender).WithMany(p => p.ChatMessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Chat_Mess__sende__2AA05119");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("PK__Coupon__384AF1DAAC746407");

            entity.ToTable("Coupon");

            entity.HasIndex(e => new { e.UserId, e.IsUsed, e.CouponId }, "Coupon_index_26");

            entity.HasIndex(e => e.CouponCode, "UQ__Coupon__D34908004051B80B").IsUnique();

            entity.Property(e => e.CouponId).HasColumnName("CouponID");
            entity.Property(e => e.AcquiredTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.CouponCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CouponTypeId).HasColumnName("CouponTypeID");
            entity.Property(e => e.UsedInOrderId).HasColumnName("UsedInOrderID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.CouponType).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.CouponTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Coupon__CouponTy__178D7CA5");

            entity.HasOne(d => d.User).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Coupon__UserID__1881A0DE");
        });

        modelBuilder.Entity<CouponType>(entity =>
        {
            entity.HasKey(e => e.CouponTypeId).HasName("PK__CouponTy__095BEDBBA997B784");

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
            entity.HasKey(e => e.EvoucherId).HasName("PK__EVoucher__BC18D35E1464F1FB");

            entity.ToTable("EVoucher");

            entity.HasIndex(e => new { e.UserId, e.IsUsed, e.EvoucherId }, "EVoucher_index_27");

            entity.HasIndex(e => e.EvoucherCode, "UQ__EVoucher__FFC16097F4A2727A").IsUnique();

            entity.Property(e => e.EvoucherId).HasColumnName("EVoucherID");
            entity.Property(e => e.AcquiredTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.EvoucherCode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("EVoucherCode");
            entity.Property(e => e.EvoucherTypeId).HasColumnName("EVoucherTypeID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.EvoucherType).WithMany(p => p.Evouchers)
                .HasForeignKey(d => d.EvoucherTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EVoucher__EVouch__1975C517");

            entity.HasOne(d => d.User).WithMany(p => p.Evouchers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EVoucher__UserID__1A69E950");
        });

        modelBuilder.Entity<EvoucherRedeemLog>(entity =>
        {
            entity.HasKey(e => e.RedeemId).HasName("PK__EVoucher__C9E468F725989671");

            entity.ToTable("EVoucherRedeemLog");

            entity.HasIndex(e => new { e.EvoucherId, e.ScannedAt }, "EVoucherRedeemLog_index_28");

            entity.HasIndex(e => new { e.UserId, e.ScannedAt }, "EVoucherRedeemLog_index_29");

            entity.Property(e => e.RedeemId).HasColumnName("RedeemID");
            entity.Property(e => e.EvoucherId).HasColumnName("EVoucherID");
            entity.Property(e => e.ScannedAt).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.TokenId).HasColumnName("TokenID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Evoucher).WithMany(p => p.EvoucherRedeemLogs)
                .HasForeignKey(d => d.EvoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EVoucherR__EVouc__1C5231C2");

            entity.HasOne(d => d.Token).WithMany(p => p.EvoucherRedeemLogs)
                .HasForeignKey(d => d.TokenId)
                .HasConstraintName("FK__EVoucherR__Token__1D4655FB");

            entity.HasOne(d => d.User).WithMany(p => p.EvoucherRedeemLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EVoucherR__UserI__1E3A7A34");
        });

        modelBuilder.Entity<EvoucherToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__EVoucher__658FEE8A7AB9BACD");

            entity.ToTable("EVoucherToken");

            entity.HasIndex(e => e.Token, "UQ__EVoucher__1EB4F81773C394B6").IsUnique();

            entity.Property(e => e.TokenId).HasColumnName("TokenID");
            entity.Property(e => e.EvoucherId).HasColumnName("EVoucherID");
            entity.Property(e => e.Token)
                .HasMaxLength(64)
                .IsUnicode(false);

            entity.HasOne(d => d.Evoucher).WithMany(p => p.EvoucherTokens)
                .HasForeignKey(d => d.EvoucherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EVoucherT__EVouc__1B5E0D89");
        });

        modelBuilder.Entity<EvoucherType>(entity =>
        {
            entity.HasKey(e => e.EvoucherTypeId).HasName("PK__EVoucher__CC0CEC9B7111CD6C");

            entity.ToTable("EVoucherType");

            entity.Property(e => e.EvoucherTypeId).HasColumnName("EVoucherTypeID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.ValueAmount).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Forum>(entity =>
        {
            entity.HasKey(e => e.ForumId).HasName("PK__forums__69A2FA580B436E14");

            entity.ToTable("forums");

            entity.HasIndex(e => e.GameId, "UQ_forums_game_id").IsUnique();

            entity.HasIndex(e => e.GameId, "forums_index_15").IsUnique();

            entity.Property(e => e.ForumId).HasColumnName("forum_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("name");

            entity.HasOne(d => d.Game).WithOne(p => p.Forum)
                .HasForeignKey<Forum>(d => d.GameId)
                .HasConstraintName("FK__forums__game_id__0B27A5C0");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("PK__games__FFE11FCF9BD32B00");

            entity.ToTable("games");

            entity.HasIndex(e => e.NameZh, "IX_games_name_zh");

            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Genre)
                .HasMaxLength(30)
                .HasColumnName("genre");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.NameZh)
                .HasMaxLength(100)
                .HasColumnName("name_zh");
        });

        modelBuilder.Entity<GameMetricDaily>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__game_met__3213E83F9C7B6040");

            entity.ToTable("game_metric_daily");

            entity.HasIndex(e => new { e.GameId, e.MetricId, e.Date }, "game_metric_daily_index_3").IsUnique();

            entity.HasIndex(e => new { e.Date, e.MetricId }, "game_metric_daily_index_4");

            entity.HasIndex(e => new { e.GameId, e.Date }, "game_metric_daily_index_5");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AggMethod)
                .HasMaxLength(255)
                .HasColumnName("agg_method");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.MetricId).HasColumnName("metric_id");
            entity.Property(e => e.Quality)
                .HasMaxLength(255)
                .HasColumnName("quality");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Value)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("value");

            entity.HasOne(d => d.Game).WithMany(p => p.GameMetricDailies)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK__game_metr__game___02925FBF");

            entity.HasOne(d => d.Metric).WithMany(p => p.GameMetricDailies)
                .HasForeignKey(d => d.MetricId)
                .HasConstraintName("FK__game_metr__metri__038683F8");
        });

        modelBuilder.Entity<GameProductDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.ProductId)
                .ValueGeneratedNever()
                .HasColumnName("product_id");
            entity.Property(e => e.DownloadLink)
                .HasMaxLength(500)
                .HasColumnName("download_link");
            entity.Property(e => e.GameType)
                .HasMaxLength(200)
                .HasColumnName("game_type");
            entity.Property(e => e.PlatformId).HasColumnName("platform_id");
            entity.Property(e => e.PlatformName)
                .HasMaxLength(100)
                .HasColumnName("platform_name");
            entity.Property(e => e.ProductDescription).HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .HasColumnName("product_name");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");

            entity.HasOne(d => d.Product).WithOne(p => p.GameProductDetail)
                .HasForeignKey<GameProductDetail>(d => d.ProductId)
                .HasConstraintName("FK_GameDetails_ProductInfo");

            entity.HasOne(d => d.Supplier).WithMany(p => p.GameProductDetails)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GameDetails_Supplier");
        });

        modelBuilder.Entity<GameSourceMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__game_sou__3213E83F09D919AD");

            entity.ToTable("game_source_map");

            entity.HasIndex(e => new { e.GameId, e.SourceId }, "game_source_map_index_1").IsUnique();

            entity.HasIndex(e => new { e.SourceId, e.ExternalKey }, "game_source_map_index_2");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ExternalKey)
                .HasMaxLength(255)
                .HasColumnName("external_key");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");

            entity.HasOne(d => d.Game).WithMany(p => p.GameSourceMaps)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK__game_sour__game___00AA174D");

            entity.HasOne(d => d.Source).WithMany(p => p.GameSourceMaps)
                .HasForeignKey(d => d.SourceId)
                .HasConstraintName("FK__game_sour__sourc__019E3B86");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Groups__D57795A0040F83BA");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.GroupName)
                .HasMaxLength(1)
                .HasColumnName("group_name");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Groups)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__Groups__created___2C88998B");
        });

        modelBuilder.Entity<GroupBlock>(entity =>
        {
            entity.HasKey(e => e.BlockId).HasName("PK__Group_Bl__A67E647DC19BDA0A");

            entity.ToTable("Group_Block");

            entity.HasIndex(e => new { e.GroupId, e.UserId }, "Group_Block_index_7").IsUnique();

            entity.HasIndex(e => e.GroupId, "Group_Block_index_8");

            entity.HasIndex(e => e.UserId, "Group_Block_index_9");

            entity.Property(e => e.BlockId).HasColumnName("block_id");
            entity.Property(e => e.BlockedBy).HasColumnName("blocked_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.BlockedByNavigation).WithMany(p => p.GroupBlockBlockedByNavigations)
                .HasForeignKey(d => d.BlockedBy)
                .HasConstraintName("FK__Group_Blo__block__3335971A");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupBlocks)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Group_Blo__group__314D4EA8");

            entity.HasOne(d => d.User).WithMany(p => p.GroupBlockUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Group_Blo__User___324172E1");
        });

        modelBuilder.Entity<GroupChat>(entity =>
        {
            entity.HasKey(e => e.GroupChatId).HasName("PK__Group_Ch__C4565A19F098E7B2");

            entity.ToTable("Group_Chat");

            entity.HasIndex(e => e.GroupId, "Group_Chat_index_5");

            entity.HasIndex(e => new { e.GroupId, e.SentAt }, "Group_Chat_index_6");

            entity.Property(e => e.GroupChatId).HasColumnName("group_chat_id");
            entity.Property(e => e.GroupChatContent)
                .HasMaxLength(1)
                .HasColumnName("group_chat_content");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.IsSent)
                .HasDefaultValue(true)
                .HasColumnName("is_sent");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupChats)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Group_Cha__group__2F650636");

            entity.HasOne(d => d.Sender).WithMany(p => p.GroupChats)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK__Group_Cha__sende__30592A6F");
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => new { e.GroupId, e.UserId }).HasName("PK__Group_Me__C7714CB903B19631");

            entity.ToTable("Group_Member");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.JoinedAt).HasColumnName("joined_at");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Group_Mem__group__2D7CBDC4");

            entity.HasOne(d => d.User).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Group_Mem__User___2E70E1FD");
        });

        modelBuilder.Entity<LeaderboardSnapshot>(entity =>
        {
            entity.HasKey(e => e.SnapshotId).HasName("PK__leaderbo__C27CFBF7BC5C353C");

            entity.ToTable("leaderboard_snapshots");

            entity.HasIndex(e => new { e.Period, e.Ts, e.GameId }, "leaderboard_snapshots_index_10");

            entity.HasIndex(e => new { e.Period, e.Ts, e.Rank }, "leaderboard_snapshots_index_8");

            entity.HasIndex(e => new { e.Period, e.Ts, e.Rank, e.GameId }, "leaderboard_snapshots_index_9").IsUnique();

            entity.Property(e => e.SnapshotId).HasColumnName("snapshot_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.IndexValue)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("index_value");
            entity.Property(e => e.Period)
                .HasMaxLength(255)
                .HasColumnName("period");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.Ts).HasColumnName("ts");

            entity.HasOne(d => d.Game).WithMany(p => p.LeaderboardSnapshots)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK__leaderboa__game___056ECC6A");
        });

        modelBuilder.Entity<ManagerDatum>(entity =>
        {
            entity.HasKey(e => e.ManagerId).HasName("PK__ManagerD__AE5FEFAD8366CA9D");

            entity.HasIndex(e => e.ManagerEmail, "UQ__ManagerD__0890969E55245EF4").IsUnique();

            entity.HasIndex(e => e.ManagerAccount, "UQ__ManagerD__62B5E211ADDDC932").IsUnique();

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

            entity.HasMany(d => d.ManagerRoles).WithMany(p => p.Managers)
                .UsingEntity<Dictionary<string, object>>(
                    "ManagerRole",
                    r => r.HasOne<ManagerRolePermission>().WithMany()
                        .HasForeignKey("ManagerRoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ManagerRo__Manag__7EC1CEDB"),
                    l => l.HasOne<ManagerDatum>().WithMany()
                        .HasForeignKey("ManagerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__ManagerRo__Manag__7DCDAAA2"),
                    j =>
                    {
                        j.HasKey("ManagerId", "ManagerRoleId").HasName("PK__ManagerR__6270897E1786AF08");
                        j.ToTable("ManagerRole");
                        j.IndexerProperty<int>("ManagerId").HasColumnName("Manager_Id");
                        j.IndexerProperty<int>("ManagerRoleId").HasColumnName("ManagerRole_Id");
                    });
        });

        modelBuilder.Entity<ManagerRolePermission>(entity =>
        {
            entity.HasKey(e => e.ManagerRoleId).HasName("PK__ManagerR__C2F66D3D6305A122");

            entity.ToTable("ManagerRolePermission");

            entity.Property(e => e.ManagerRoleId)
                .ValueGeneratedNever()
                .HasColumnName("ManagerRole_Id");
            entity.Property(e => e.CustomerService).HasColumnName("customer_service");
            entity.Property(e => e.PetRightsManagement).HasColumnName("Pet_Rights_Management");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<MemberSalesProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__MemberSa__206D9170DB01B726");

            entity.ToTable("MemberSalesProfile");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_Id");
            entity.Property(e => e.BankAccountNumber).HasMaxLength(255);

            entity.HasOne(d => d.User).WithOne(p => p.MemberSalesProfile)
                .HasForeignKey<MemberSalesProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MemberSal__User___7BE56230");
        });

        modelBuilder.Entity<MerchType>(entity =>
        {
            entity.ToTable("MerchType");

            entity.Property(e => e.MerchTypeId).HasColumnName("merch_type_id");
            entity.Property(e => e.MerchTypeName)
                .HasMaxLength(50)
                .HasColumnName("merch_type_name");
        });

        modelBuilder.Entity<Metric>(entity =>
        {
            entity.HasKey(e => e.MetricId).HasName("PK__metrics__13D5DCA4426BC063");

            entity.ToTable("metrics");

            entity.HasIndex(e => new { e.SourceId, e.Code }, "UQ_metrics_source_code").IsUnique();

            entity.HasIndex(e => new { e.SourceId, e.Code }, "metrics_index_0").IsUnique();

            entity.Property(e => e.MetricId).HasColumnName("metric_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(150)
                .HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.Unit)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("unit");

            entity.HasOne(d => d.Source).WithMany(p => p.Metrics)
                .HasForeignKey(d => d.SourceId)
                .HasConstraintName("FK__metrics__source___7FB5F314");
        });

        modelBuilder.Entity<MetricSource>(entity =>
        {
            entity.HasKey(e => e.SourceId).HasName("PK__metric_s__3035A9B6A01E7136");

            entity.ToTable("metric_sources");

            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note");
        });

        modelBuilder.Entity<MiniGame>(entity =>
        {
            entity.HasKey(e => e.PlayId).HasName("PK__MiniGame__7CA45E842B3C968E");

            entity.ToTable("MiniGame");

            entity.HasIndex(e => new { e.UserId, e.StartTime }, "MiniGame_index_24");

            entity.HasIndex(e => new { e.PetId, e.StartTime }, "MiniGame_index_25");

            entity.Property(e => e.PlayId).HasColumnName("PlayID");
            entity.Property(e => e.PetId).HasColumnName("PetID");
            entity.Property(e => e.Result)
                .HasMaxLength(10)
                .HasDefaultValue("Unknown");
            entity.Property(e => e.SpeedMultiplier)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.StartTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Pet).WithMany(p => p.MiniGames)
                .HasForeignKey(d => d.PetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MiniGame__PetID__1699586C");

            entity.HasOne(d => d.User).WithMany(p => p.MiniGames)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MiniGame__UserID__15A53433");
        });

        modelBuilder.Entity<Mute>(entity =>
        {
            entity.HasKey(e => e.MuteId).HasName("PK__Mutes__84EE96EB706DFF1A");

            entity.Property(e => e.MuteId).HasColumnName("mute_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.MuteName)
                .HasMaxLength(100)
                .HasColumnName("mute_name");

            entity.HasOne(d => d.Manager).WithMany(p => p.Mutes)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__Mutes__manager_i__2116E6DF");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__E059842F41D185D1");

            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.ActionId).HasColumnName("action_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.NotificationMessage)
                .HasMaxLength(100)
                .HasColumnName("notification_message");
            entity.Property(e => e.NotificationTitle)
                .HasMaxLength(100)
                .HasColumnName("notification_title");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SenderManagerId).HasColumnName("sender_manager_id");
            entity.Property(e => e.SourceId).HasColumnName("source_id");

            entity.HasOne(d => d.Action).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ActionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__actio__1293BD5E");

            entity.HasOne(d => d.Group).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK__Notificat__group__26CFC035");

            entity.HasOne(d => d.Sender).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK__Notificat__sende__24E777C3");

            entity.HasOne(d => d.SenderManager).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.SenderManagerId)
                .HasConstraintName("FK__Notificat__sende__25DB9BFC");

            entity.HasOne(d => d.Source).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__sourc__1387E197");
        });

        modelBuilder.Entity<NotificationAction>(entity =>
        {
            entity.HasKey(e => e.ActionId).HasName("PK__Notifica__74EFC217A15400CA");

            entity.ToTable("Notification_Actions");

            entity.HasIndex(e => e.ActionName, "Notification_Actions_index_0").IsUnique();

            entity.Property(e => e.ActionId).HasColumnName("action_id");
            entity.Property(e => e.ActionName)
                .HasMaxLength(100)
                .HasColumnName("action_name");
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => e.RecipientId).HasName("PK__Notifica__FA0A4027A76D414B");

            entity.ToTable("Notification_Recipients");

            entity.HasIndex(e => new { e.UserId, e.IsRead, e.RecipientId }, "IX_Inbox");

            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.UserId).HasColumnName("User_ID");

            entity.HasOne(d => d.Notification).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__notif__119F9925");

            entity.HasOne(d => d.User).WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__User___28B808A7");
        });

        modelBuilder.Entity<NotificationSource>(entity =>
        {
            entity.HasKey(e => e.SourceId).HasName("PK__Notifica__3035A9B607A7A5F0");

            entity.ToTable("Notification_Sources");

            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.SourceName)
                .HasMaxLength(100)
                .HasColumnName("source_name");
        });

        modelBuilder.Entity<OfficialStoreRanking>(entity =>
        {
            entity.HasKey(e => e.RankingId).HasName("PK_Official_StoreRanking");

            entity.ToTable("Official_Store_Ranking");

            entity.Property(e => e.RankingId).HasColumnName("ranking_id");
            entity.Property(e => e.PeriodType)
                .HasMaxLength(20)
                .HasColumnName("period_type");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.RankingDate).HasColumnName("ranking_date");
            entity.Property(e => e.RankingMetric)
                .HasMaxLength(50)
                .HasColumnName("ranking_metric");
            entity.Property(e => e.RankingPosition).HasColumnName("ranking_position");
            entity.Property(e => e.RankingUpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ranking_updated_at");
            entity.Property(e => e.TradingAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("trading_amount");
            entity.Property(e => e.TradingVolume).HasColumnName("trading_volume");

            entity.HasOne(d => d.Product).WithMany(p => p.OfficialStoreRankings)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_OSR_ProductInfo");
        });

        modelBuilder.Entity<OrderAddress>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__OrderAdd__46596229DBE3931D");

            entity.Property(e => e.OrderId)
                .ValueGeneratedNever()
                .HasColumnName("order_id");
            entity.Property(e => e.Address1)
                .HasMaxLength(200)
                .HasColumnName("address1");
            entity.Property(e => e.Address2)
                .HasMaxLength(200)
                .HasColumnName("address2");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(30)
                .HasColumnName("country");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.Recipient)
                .HasMaxLength(100)
                .HasColumnName("recipient");
            entity.Property(e => e.Zipcode)
                .HasMaxLength(10)
                .HasColumnName("zipcode");

            entity.HasOne(d => d.Order).WithOne(p => p.OrderAddress)
                .HasForeignKey<OrderAddress>(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderAddresses_Order");
        });

        modelBuilder.Entity<OrderInfo>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__OrderInf__46596229184FD12E");

            entity.ToTable("OrderInfo");

            entity.HasIndex(e => new { e.OrderDate, e.OrderCode }, "IX_OrderInfo_OrderDate_OrderCode");

            entity.HasIndex(e => new { e.PaymentStatus, e.OrderDate }, "IX_OrderInfo_PaymentStatus");

            entity.HasIndex(e => new { e.OrderStatus, e.OrderDate }, "IX_OrderInfo_Status_Date");

            entity.HasIndex(e => new { e.UserId, e.OrderDate }, "IX_OrderInfo_User_Date");

            entity.HasIndex(e => e.OrderCode, "UQ_OrderInfo_OrderCode").IsUnique();

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.OrderCode)
                .HasDefaultValueSql("(NEXT VALUE FOR [dbo].[SeqOrderCode])")
                .HasColumnName("order_code");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("order_date");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(30)
                .HasDefaultValue("未出貨")
                .HasColumnName("order_status");
            entity.Property(e => e.OrderTotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("order_total");
            entity.Property(e => e.PaymentAt).HasColumnName("payment_at");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(30)
                .HasDefaultValue("未付款")
                .HasColumnName("payment_status");
            entity.Property(e => e.ShippedAt).HasColumnName("shipped_at");
            entity.Property(e => e.UserId).HasColumnName("User_ID");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__OrderIte__52020FDDEA8DA3E3");

            entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderId");

            entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderId_Cover");

            entity.HasIndex(e => new { e.OrderId, e.LineNo }, "UQ_OrderItems_Order_Line").IsUnique();

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.LineNo).HasColumnName("line_no");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Subtotal)
                .HasComputedColumnSql("([unit_price]*[quantity])", true)
                .HasColumnType("decimal(29, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Order");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__OrderSta__096AA2E982D02A48");

            entity.ToTable("OrderStatusHistory");

            entity.HasIndex(e => e.OrderId, "IX_OrderStatusHistory_OrderId");

            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.FromStatus)
                .HasMaxLength(30)
                .HasColumnName("from_status");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ToStatus)
                .HasMaxLength(30)
                .HasColumnName("to_status");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("FK_OrderStatusHistory_Manager");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderStatusHistory_Order");
        });

        modelBuilder.Entity<OtherProductDetail>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.ProductId)
                .ValueGeneratedNever()
                .HasColumnName("product_id");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.DigitalCode)
                .HasMaxLength(100)
                .HasColumnName("digital_code");
            entity.Property(e => e.Dimensions)
                .HasMaxLength(100)
                .HasColumnName("dimensions");
            entity.Property(e => e.Material)
                .HasMaxLength(50)
                .HasColumnName("material");
            entity.Property(e => e.MerchTypeId).HasColumnName("merch_type_id");
            entity.Property(e => e.ProductDescription).HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .HasColumnName("product_name");
            entity.Property(e => e.Size)
                .HasMaxLength(50)
                .HasColumnName("size");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Weight)
                .HasMaxLength(50)
                .HasColumnName("weight");

            entity.HasOne(d => d.MerchType).WithMany(p => p.OtherProductDetails)
                .HasForeignKey(d => d.MerchTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_OtherDetails_MerchType");

            entity.HasOne(d => d.Product).WithOne(p => p.OtherProductDetail)
                .HasForeignKey<OtherProductDetail>(d => d.ProductId)
                .HasConstraintName("FK_OtherDetails_ProductInfo");

            entity.HasOne(d => d.Supplier).WithMany(p => p.OtherProductDetails)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OtherDetails_Supplier");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__PaymentT__ED1FC9EAA1E2E210");

            entity.HasIndex(e => e.OrderId, "IX_PaymentTransactions_OrderId");

            entity.HasIndex(e => new { e.OrderId, e.Status }, "IX_PaymentTransactions_Order_Status");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.ConfirmedAt).HasColumnName("confirmed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Meta).HasColumnName("meta");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentCode)
                .HasDefaultValueSql("(NEXT VALUE FOR [dbo].[SeqPaymentCode])")
                .HasColumnName("payment_code");
            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .HasColumnName("provider");
            entity.Property(e => e.ProviderTxn)
                .HasMaxLength(100)
                .HasColumnName("provider_txn");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.TxnType)
                .HasMaxLength(30)
                .HasColumnName("txn_type");

            entity.HasOne(d => d.Order).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentTransactions_Order");
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.PetId).HasName("PK__Pet__48E53802B70849C0");

            entity.ToTable("Pet");

            entity.HasIndex(e => e.UserId, "Pet_index_23");

            entity.Property(e => e.PetId).HasColumnName("PetID");
            entity.Property(e => e.BackgroundColor)
                .HasMaxLength(50)
                .HasDefaultValue("粉藍");
            entity.Property(e => e.BackgroundColorChangedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.ColorChangedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.LevelUpTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.PetName)
                .HasMaxLength(50)
                .HasDefaultValue("小可愛");
            entity.Property(e => e.PointsChangedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.SkinColor)
                .HasMaxLength(50)
                .HasDefaultValue("#ADD8E6");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Pets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pet__UserID__14B10FFA");
        });

        modelBuilder.Entity<PlayerMarketOrderInfo>(entity =>
        {
            entity.HasKey(e => e.POrderId).HasName("PK__PlayerMa__AACAAD70386B7CAF");

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
            entity.HasKey(e => e.POrderTradepageId).HasName("PK__PlayerMa__4E2C726DA64EE618");

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
            entity.HasKey(e => e.PProductImgId).HasName("PK__PlayerMa__75AAE6F0158EFF22");

            entity.Property(e => e.PProductImgId)
                .ValueGeneratedNever()
                .HasColumnName("p_product_img_id");
            entity.Property(e => e.PProductId).HasColumnName("p_product_id");
            entity.Property(e => e.PProductImgUrl).HasColumnName("p_product_img_url");
        });

        modelBuilder.Entity<PlayerMarketProductInfo>(entity =>
        {
            entity.HasKey(e => e.PProductId).HasName("PK__PlayerMa__A33C8165042F3EC0");

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
            entity.HasKey(e => e.PRankingId).HasName("PK__PlayerMa__2B50ED32732477C5");

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
            entity.HasKey(e => e.TradeMsgId).HasName("PK__PlayerMa__C2FA77A2B358B180");

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
            entity.HasKey(e => e.Id).HasName("PK__populari__3213E83F1F6E3B21");

            entity.ToTable("popularity_index_daily");

            entity.HasIndex(e => new { e.GameId, e.Date }, "popularity_index_daily_index_6").IsUnique();

            entity.HasIndex(e => e.Date, "popularity_index_daily_index_7");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.IndexValue)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("index_value");

            entity.HasOne(d => d.Game).WithMany(p => p.PopularityIndexDailies)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK__popularit__game___047AA831");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__posts__3ED787667664226C");

            entity.ToTable("posts");

            entity.HasIndex(e => new { e.Type, e.CreatedAt }, "posts_index_11");

            entity.HasIndex(e => new { e.GameId, e.CreatedAt }, "posts_index_12");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "posts_index_13");

            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.BodyMd).HasColumnName("body_md");
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
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Tldr)
                .HasMaxLength(500)
                .HasColumnName("tldr");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Posts)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__posts__created_b__075714DC");

            entity.HasOne(d => d.Game).WithMany(p => p.Posts)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK__posts__game_id__0662F0A3");
        });

        modelBuilder.Entity<PostMetricSnapshot>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__post_met__3ED787667CA8D1EA");

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

            entity.HasOne(d => d.Game).WithMany(p => p.PostMetricSnapshots)
                .HasForeignKey(d => d.GameId)
                .HasConstraintName("FK__post_metr__game___093F5D4E");

            entity.HasOne(d => d.Post).WithOne(p => p.PostMetricSnapshot)
                .HasForeignKey<PostMetricSnapshot>(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__post_metr__post___084B3915");
        });

        modelBuilder.Entity<PostSource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__post_sou__3213E83FBE40E755");

            entity.ToTable("post_sources");

            entity.HasIndex(e => e.PostId, "post_sources_index_14");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.SourceName)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("source_name");
            entity.Property(e => e.Url)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("url");

            entity.HasOne(d => d.Post).WithMany(p => p.PostSources)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK__post_sour__post___0A338187");
        });

        modelBuilder.Entity<ProductCode>(entity =>
        {
            entity.HasKey(e => e.ProductCode1);

            entity.ToTable("ProductCode");

            entity.Property(e => e.ProductCode1)
                .HasMaxLength(50)
                .HasColumnName("product_code");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductCodes)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductCode_ProductInfo");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ProductimgId);

            entity.Property(e => e.ProductimgId).HasColumnName("productimg_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductimgAltText)
                .HasMaxLength(255)
                .HasColumnName("productimg_alt_text");
            entity.Property(e => e.ProductimgUpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("productimg_updated_at");
            entity.Property(e => e.ProductimgUrl)
                .HasMaxLength(500)
                .HasColumnName("productimg_url");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductImages_ProductInfo");
        });

        modelBuilder.Entity<ProductInfo>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.ToTable("ProductInfo");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(10)
                .HasDefaultValue("TWD")
                .HasColumnName("currency_code");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductCreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("product_created_at");
            entity.Property(e => e.ProductCreatedBy)
                .HasMaxLength(50)
                .HasColumnName("product_created_by");
            entity.Property(e => e.ProductName)
                .HasMaxLength(200)
                .HasColumnName("product_name");
            entity.Property(e => e.ProductType)
                .HasMaxLength(50)
                .HasColumnName("product_type");
            entity.Property(e => e.ProductUpdatedAt).HasColumnName("product_updated_at");
            entity.Property(e => e.ProductUpdatedBy)
                .HasMaxLength(50)
                .HasColumnName("product_updated_by");
            entity.Property(e => e.ShipmentQuantity).HasColumnName("Shipment_Quantity");
        });

        modelBuilder.Entity<ProductInfoAuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId);

            entity.ToTable("ProductInfoAuditLog");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(30)
                .HasColumnName("action_type");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("changed_at");
            entity.Property(e => e.FieldName)
                .HasMaxLength(100)
                .HasColumnName("field_name");
            entity.Property(e => e.ManagerId).HasColumnName("Manager_Id");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductInfoAuditLogs)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Audit_ProductInfo");
        });

        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reaction__3213E83FC5B81BAC");

            entity.ToTable("reactions");

            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "IX_reactions_target");

            entity.HasIndex(e => new { e.UserId, e.TargetType, e.TargetId, e.Kind }, "UQ_reactions_user_target_kind").IsUnique();

            entity.HasIndex(e => new { e.UserId, e.TargetType, e.TargetId, e.Kind }, "reactions_index_18").IsUnique();

            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "reactions_index_19");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
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

            entity.HasOne(d => d.User).WithMany(p => p.Reactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__reactions__User___10E07F16");
        });

        modelBuilder.Entity<Relation>(entity =>
        {
            entity.HasKey(e => e.RelationId).HasName("PK__Relation__C409F32355E06628");

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

            entity.HasOne(d => d.Friend).WithMany(p => p.RelationFriends)
                .HasForeignKey(d => d.FriendId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Relation__friend__351DDF8C");

            entity.HasOne(d => d.Status).WithMany(p => p.Relations)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Relation__status__361203C5");

            entity.HasOne(d => d.User).WithMany(p => p.RelationUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Relation__User_I__3429BB53");
        });

        modelBuilder.Entity<RelationStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Relation__3683B53132CDB4CF");

            entity.ToTable("Relation_Status");

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(10)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(e => e.ShipmentId).HasName("PK__Shipment__41466E599DAE56EF");

            entity.HasIndex(e => e.OrderId, "IX_Shipments_OrderId");

            entity.HasIndex(e => e.ShipmentCode, "UQ_Shipments_Code").IsUnique();

            entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");
            entity.Property(e => e.Carrier)
                .HasMaxLength(50)
                .HasColumnName("carrier");
            entity.Property(e => e.DeliveredAt).HasColumnName("delivered_at");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ShipmentCode)
                .HasDefaultValueSql("(NEXT VALUE FOR [dbo].[SeqShipmentCode])")
                .HasColumnName("shipment_code");
            entity.Property(e => e.ShippedAt).HasColumnName("shipped_at");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.TrackingNo)
                .HasMaxLength(100)
                .HasColumnName("tracking_no");

            entity.HasOne(d => d.Order).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Shipments_Order");
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(e => e.MovementId).HasName("PK__StockMov__AB1D1022D261B93A");

            entity.HasIndex(e => e.OrderId, "IX_StockMovements_Order");

            entity.HasIndex(e => new { e.ProductId, e.CreatedAt }, "IX_StockMovements_Product");

            entity.HasIndex(e => new { e.ProductId, e.CreatedAt }, "IX_StockMovements_Product_Time");

            entity.Property(e => e.MovementId).HasColumnName("movement_id");
            entity.Property(e => e.ChangeQty).HasColumnName("change_qty");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasColumnName("note");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(30)
                .HasColumnName("reason");

            entity.HasOne(d => d.Order).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_StockMovements_Order");
        });

        modelBuilder.Entity<Style>(entity =>
        {
            entity.HasKey(e => e.StyleId).HasName("PK__Styles__D333B397618718E7");

            entity.Property(e => e.StyleId).HasColumnName("style_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.EffectDesc)
                .HasMaxLength(100)
                .HasColumnName("effect_desc");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.StyleName)
                .HasMaxLength(100)
                .HasColumnName("style_name");

            entity.HasOne(d => d.Manager).WithMany(p => p.Styles)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__Styles__manager___220B0B18");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Supplier");

            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(100)
                .HasColumnName("supplier_name");
        });

        modelBuilder.Entity<Thread>(entity =>
        {
            entity.HasKey(e => e.ThreadId).HasName("PK__threads__7411E2F0C4FF8B27");

            entity.ToTable("threads");

            entity.HasIndex(e => new { e.ForumId, e.UpdatedAt }, "IX_threads_forum_updated");

            entity.HasIndex(e => new { e.ForumId, e.UpdatedAt }, "threads_index_16");

            entity.Property(e => e.ThreadId).HasColumnName("thread_id");
            entity.Property(e => e.AuthorUserId).HasColumnName("author_User_ID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ForumId).HasColumnName("forum_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.Threads)
                .HasForeignKey(d => d.AuthorUserId)
                .HasConstraintName("FK__threads__author___0D0FEE32");

            entity.HasOne(d => d.Forum).WithMany(p => p.Threads)
                .HasForeignKey(d => d.ForumId)
                .HasConstraintName("FK__threads__forum_i__0C1BC9F9");
        });

        modelBuilder.Entity<ThreadPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__thread_p__3213E83F9154CA26");

            entity.ToTable("thread_posts");

            entity.HasIndex(e => new { e.ThreadId, e.CreatedAt }, "IX_thread_posts_thread_created");

            entity.HasIndex(e => new { e.ThreadId, e.CreatedAt }, "thread_posts_index_17");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorUserId).HasColumnName("author_User_ID");
            entity.Property(e => e.ContentMd)
                .HasMaxLength(3000)
                .HasColumnName("content_md");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ParentPostId).HasColumnName("parent_post_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.ThreadId).HasColumnName("thread_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.AuthorUser).WithMany(p => p.ThreadPosts)
                .HasForeignKey(d => d.AuthorUserId)
                .HasConstraintName("FK__thread_po__autho__0EF836A4");

            entity.HasOne(d => d.ParentPost).WithMany(p => p.InverseParentPost)
                .HasForeignKey(d => d.ParentPostId)
                .HasConstraintName("FK__thread_po__paren__0FEC5ADD");

            entity.HasOne(d => d.Thread).WithMany(p => p.ThreadPosts)
                .HasForeignKey(d => d.ThreadId)
                .HasConstraintName("FK__thread_po__threa__0E04126B");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__206D9190C687FA53");

            entity.HasIndex(e => e.UserName, "UQ__Users__5F1A10861843AB73").IsUnique();

            entity.HasIndex(e => e.UserAccount, "UQ__Users__899F4A917F9CF43B").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.UserAccessFailedCount).HasColumnName("User_AccessFailedCount");
            entity.Property(e => e.UserAccount)
                .HasMaxLength(100)
                .HasColumnName("User_Account");
            entity.Property(e => e.UserEmailConfirmed).HasColumnName("User_EmailConfirmed");
            entity.Property(e => e.UserLockoutEnabled)
                .HasDefaultValue(true)
                .HasColumnName("User_LockoutEnabled");
            entity.Property(e => e.UserLockoutEnd).HasColumnName("User_LockoutEnd");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("User_name");
            entity.Property(e => e.UserPassword)
                .HasMaxLength(200)
                .HasColumnName("User_Password");
            entity.Property(e => e.UserPhoneNumberConfirmed).HasColumnName("User_PhoneNumberConfirmed");
            entity.Property(e => e.UserTwoFactorEnabled).HasColumnName("User_TwoFactorEnabled");
        });

        modelBuilder.Entity<UserIntroduce>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_Int__206D91902661F54D");

            entity.ToTable("User_Introduce");

            entity.HasIndex(e => e.IdNumber, "UQ__User_Int__62DF803312534020").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User_Int__A9D10534A2E81491").IsUnique();

            entity.HasIndex(e => e.Cellphone, "UQ__User_Int__CDE19CF24D69869F").IsUnique();

            entity.HasIndex(e => e.UserNickName, "UQ__User_Int__DAFD02CF7EFC6E03").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_ID");
            entity.Property(e => e.Address).HasMaxLength(100);
            entity.Property(e => e.Cellphone).HasMaxLength(255);
            entity.Property(e => e.CreateAccount).HasColumnName("Create_Account");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.IdNumber).HasMaxLength(255);
            entity.Property(e => e.UserIntroduce1)
                .HasMaxLength(200)
                .HasColumnName("User_Introduce");
            entity.Property(e => e.UserNickName)
                .HasMaxLength(50)
                .HasColumnName("User_NickName");
            entity.Property(e => e.UserPicture).HasColumnName("User_Picture");

            entity.HasOne(d => d.User).WithOne(p => p.UserIntroduce)
                .HasForeignKey<UserIntroduce>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_Intr__User___79FD19BE");
        });

        modelBuilder.Entity<UserRight>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_Rig__206D9170BA32CB4F");

            entity.ToTable("User_Rights");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_Id");
            entity.Property(e => e.UserStatus).HasColumnName("User_Status");

            entity.HasOne(d => d.User).WithOne(p => p.UserRight)
                .HasForeignKey<UserRight>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_Righ__User___7AF13DF7");
        });

        modelBuilder.Entity<UserSalesInformation>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_Sal__206D9170533CB91B");

            entity.ToTable("User_Sales_Information");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_Id");
            entity.Property(e => e.UserSalesWallet).HasColumnName("UserSales_Wallet");

            entity.HasOne(d => d.User).WithOne(p => p.UserSalesInformation)
                .HasForeignKey<UserSalesInformation>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_Sale__User___7CD98669");
        });

        modelBuilder.Entity<UserSignInStat>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__UserSign__5E5499A8754B59EE");

            entity.HasIndex(e => new { e.UserId, e.SignTime }, "UserSignInStats_index_22");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.ExpGainedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.PointsChangedTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.SignTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.UserSignInStats)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserSignI__UserI__13BCEBC1");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__UserToke__AA16D540871A3D02");

            entity.Property(e => e.TokenId).HasColumnName("Token_ID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.Value).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.UserTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserToken__User___4F47C5E3");
        });

        modelBuilder.Entity<UserWallet>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_wal__206D9170931E7DC4");

            entity.ToTable("User_wallet");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("User_Id");
            entity.Property(e => e.CouponNumber)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("Coupon_Number");
            entity.Property(e => e.UserPoint).HasColumnName("User_Point");

            entity.HasOne(d => d.User).WithOne(p => p.UserWallet)
                .HasForeignKey<UserWallet>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User_wall__User___12C8C788");
        });

        modelBuilder.Entity<VwForumOverview>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_forum_overview");

            entity.Property(e => e.ForumId).HasColumnName("forum_id");
            entity.Property(e => e.ForumName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("forum_name");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.LastActivityAt).HasColumnName("last_activity_at");
            entity.Property(e => e.ReplyCount).HasColumnName("reply_count");
            entity.Property(e => e.ThreadCount).HasColumnName("thread_count");
        });

        modelBuilder.Entity<VwPostsWithSnapshot>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_posts_with_snapshot");

            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.IndexValue)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("index_value");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.SnapshotDate).HasColumnName("snapshot_date");
            entity.Property(e => e.SourceCount).HasColumnName("source_count");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Tldr)
                .HasMaxLength(500)
                .HasColumnName("tldr");
        });

        modelBuilder.Entity<VwThreadActivity>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_thread_activity");

            entity.Property(e => e.AuthorUserId).HasColumnName("author_User_ID");
            entity.Property(e => e.BookmarkCount).HasColumnName("bookmark_count");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ForumId).HasColumnName("forum_id");
            entity.Property(e => e.LikeCount).HasColumnName("like_count");
            entity.Property(e => e.ReplyCount).HasColumnName("reply_count");
            entity.Property(e => e.ThreadId)
                .ValueGeneratedOnAdd()
                .HasColumnName("thread_id");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<VwThreadPostsFlat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_thread_posts_flat");

            entity.Property(e => e.AuthorUserId).HasColumnName("author_User_ID");
            entity.Property(e => e.ContentMd)
                .HasMaxLength(3000)
                .HasColumnName("content_md");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ParentPostId).HasColumnName("parent_post_id");
            entity.Property(e => e.PostId)
                .ValueGeneratedOnAdd()
                .HasColumnName("post_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.ThreadId).HasColumnName("thread_id");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<WalletHistory>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__WalletHi__5E5499A8A37D6164");

            entity.ToTable("WalletHistory");

            entity.HasIndex(e => new { e.UserId, e.ChangeTime }, "WalletHistory_index_30");

            entity.HasIndex(e => new { e.ChangeType, e.ChangeTime }, "WalletHistory_index_31");

            entity.Property(e => e.LogId).HasColumnName("LogID");
            entity.Property(e => e.ChangeTime).HasDefaultValueSql("('SYSUTCDATETIME()')");
            entity.Property(e => e.ChangeType).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.WalletHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__WalletHis__UserI__1F2E9E6D");
        });
        modelBuilder.HasSequence("SeqOrderCode")
            .StartsAt(100000000001L)
            .HasMin(100000000001L)
            .HasMax(999999999999L);
        modelBuilder.HasSequence("SeqPaymentCode")
            .StartsAt(1000000000000001L)
            .HasMin(1000000000000001L)
            .HasMax(9999999999999999L);
        modelBuilder.HasSequence("SeqShipmentCode")
            .StartsAt(10000000000001L)
            .HasMin(10000000000001L)
            .HasMax(99999999999999L);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
