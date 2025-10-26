namespace GamiPort.Areas.Forum.Dtos.Threads
{
    public sealed record ThreadListItemDto(
    long ThreadId,
    string Title,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int Replies
);
}
