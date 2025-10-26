namespace GamiPort.Areas.Forum.Dtos.Threads
{
    public sealed record ThreadPostRowDto(
    long Id,
    long ThreadId,
    int AuthorUserId,
    string ContentMd,
    DateTime CreatedAt,
    long? ParentPostId
);
}
