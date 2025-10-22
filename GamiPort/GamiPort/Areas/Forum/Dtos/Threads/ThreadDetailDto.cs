namespace GamiPort.Areas.Forum.Dtos.Threads
{
    public sealed record ThreadDetailDto(
    long ThreadId,
    string Title,
    string Status,
    int AuthorUserId,
    DateTime CreatedAt,
    IReadOnlyList<ThreadPostRowDto> Posts
);
}
