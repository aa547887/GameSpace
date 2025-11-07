namespace GamiPort.Areas.Forum.Dtos.Threads
{
    public sealed record ThreadListItemDto
    {
        public long ThreadId { get; init; }
        public string Title { get; init; } = "";
        public string Status { get; init; } = "";
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public int Replies { get; init; }
        public int LikeCount { get; init; }
        // 新增：列表直接帶出權限
        public bool IsOwner { get; init; }
        public bool CanDelete { get; init; }
        public double? HotScore { get; init; }
    }
}
