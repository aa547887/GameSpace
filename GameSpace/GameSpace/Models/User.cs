using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string UserAccount { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public bool UserEmailConfirmed { get; set; }

    public bool UserPhoneNumberConfirmed { get; set; }

    public bool UserTwoFactorEnabled { get; set; }

    public int UserAccessFailedCount { get; set; }

    public bool UserLockoutEnabled { get; set; }

    public DateTime? UserLockoutEnd { get; set; }

    // Alias properties for backward compatibility with views
    public int User_Id
    {
        get => UserId;
        set => UserId = value;
    }

    public string User_name
    {
        get => UserName;
        set => UserName = value;
    }

    public string User_account
    {
        get => UserAccount;
        set => UserAccount = value;
    }

    public string User_email { get; set; } = null!;
    public string? User_phone { get; set; }
    public DateTime? User_birthday { get; set; }
    public string? User_gender { get; set; }
    public DateTime? User_registration_date { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? User_address { get; set; }

    // Additional alias properties for database column compatibility
    public string? UserEmail => User_email;
    public DateTime? CreateAccount => User_registration_date;
    public string? User_Status { get; set; }
    public DateTime? User_CreatedAt { get; set; }

    // Alias for UserStatus (backward compatibility)
    public string? UserStatus
    {
        get => User_Status;
        set => User_Status = value;
    }

    // Case-variant aliases for queries (User_Name vs User_name)
    public string User_Name
    {
        get => UserName;
        set => UserName = value;
    }

    public string User_Account
    {
        get => UserAccount;
        set => UserAccount = value;
    }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<GroupBlock> GroupBlockBlockedByUsers { get; set; } = new List<GroupBlock>();

    public virtual ICollection<GroupBlock> GroupBlockUsers { get; set; } = new List<GroupBlock>();

    public virtual ICollection<GroupChat> GroupChats { get; set; } = new List<GroupChat>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<GroupReadState> GroupReadStates { get; set; } = new List<GroupReadState>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual MemberSalesProfile? MemberSalesProfile { get; set; }

    public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PlayerMarketOrderInfo> PlayerMarketOrderInfoBuyers { get; set; } = new List<PlayerMarketOrderInfo>();

    public virtual ICollection<PlayerMarketOrderInfo> PlayerMarketOrderInfoSellers { get; set; } = new List<PlayerMarketOrderInfo>();

    public virtual ICollection<PlayerMarketProductInfo> PlayerMarketProductInfos { get; set; } = new List<PlayerMarketProductInfo>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

    public virtual ICollection<Relation> RelationRequestedByNavigations { get; set; } = new List<Relation>();

    public virtual ICollection<Relation> RelationUserIdLargeNavigations { get; set; } = new List<Relation>();

    public virtual ICollection<Relation> RelationUserIdSmallNavigations { get; set; } = new List<Relation>();

    public virtual ICollection<SupportTicketMessage> SupportTicketMessages { get; set; } = new List<SupportTicketMessage>();

    public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();

    public virtual ICollection<ThreadPost> ThreadPosts { get; set; } = new List<ThreadPost>();

    public virtual ICollection<Thread> Threads { get; set; } = new List<Thread>();

    public virtual UserIntroduce? UserIntroduce { get; set; }

    public virtual UserRight? UserRight { get; set; }

    public virtual UserSalesInformation? UserSalesInformation { get; set; }

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();

    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

    public virtual ICollection<UserWallet> Wallets { get; set; } = new List<UserWallet>();

    public virtual ICollection<WalletHistory> WalletHistories { get; set; } = new List<WalletHistory>();

    public virtual ICollection<UserSignInStat> SignIns { get; set; } = new List<UserSignInStat>();

    public virtual ICollection<MiniGame> GamePlayRecords { get; set; } = new List<MiniGame>();
}

