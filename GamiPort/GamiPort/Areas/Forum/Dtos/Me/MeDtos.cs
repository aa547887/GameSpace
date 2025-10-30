namespace GamiPort.Areas.Forum.Dtos.Me
{
    public record MyThreadRowDto(
        long ThreadId,
        int ForumId,
        string Title,
        DateTime CreatedAt,
        DateTime? LastReplyAt,
        int RepliesCount,
        int LikeCount
    );

    public record MyPostRowDto(
        long PostId,
        long ThreadId,
        string ThreadTitle,
        string ContentMd,
        DateTime CreatedAt,
        long? ParentPostId,
        int LikeCount
    );

    public record MyLikedThreadRowDto(
        long ThreadId,
        int ForumId,
        string Title,
        DateTime LikedAt,
        int LikeCount,
        int RepliesCount
    );
}
