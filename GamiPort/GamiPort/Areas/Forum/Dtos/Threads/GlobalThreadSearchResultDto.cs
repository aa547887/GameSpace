namespace GamiPort.Areas.Forum.Dtos.Threads
{
    public class GlobalThreadSearchResultDto
    {
        public long ThreadId { get; set; }
        public int ForumId { get; set; }
        public string ForumName { get; set; } = "";
        public string Title { get; set; } = "";
        public long AuthorId { get; set; }
        public string AuthorName { get; set; } = "";
        public int ReplyCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
