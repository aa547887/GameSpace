namespace GamiPort.Areas.social_hub.SignalR.Contracts
{
    public interface IChatClient
    {
        Task FriendAdded(FriendPayload friend);
        Task FriendRemoved(int friendUserId);
        Task UnreadUpdate(UnreadUpdatePayload payload);
        Task ReceiveDirect(DirectMessagePayload payload);
        Task ReadReceipt(ReadReceiptPayload payload);
    }

    public record FriendPayload(
        int FriendUserId,
        string FriendName,
        string? Nickname = null,
        string? AvatarUrl = null
    );
}
