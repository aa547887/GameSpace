namespace GamiPort.Areas.Forum.Dtos.Threads
{
    public record ThreadDetailDto(
        long ThreadId,
        int ForumId,
        string Title,
        string Status,
        long AuthorUserId,
        DateTime CreatedAt,
        DateTime? LastReplyAt,
        int RepliesCount,
        int LikeCount,
        bool IsLikedByMe
    );
}
