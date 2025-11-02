namespace GamiPort.Areas.Forum.Dtos.AdminPosts
{
    // 查詢參數（前台）
    public sealed class PostQuery
    {
        public string Type { get; set; } = "insight";   // 預設只抓洞察
        public int? GameId { get; set; }                // 可選
        public int Page { get; set; } = 1;              // 1-based
        public int Size { get; set; } = 20;             // 建議 20~100
    }

    // 列表用
    public sealed class PostListDto
    {
        public int PostId { get; set; }
        public string Type { get; set; } = "";
        public int? GameId { get; set; }
        public string Title { get; set; } = "";
        public string? Tldr { get; set; }
        public string? BodyPreview { get; set; }        // 只截前 200～300
        public string Status { get; set; } = "";
        public bool Pinned { get; set; }
        public AuthorDto Author { get; set; } = new();
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public LinkDto Links { get; set; } = new();
    }

    // 明細用
    public sealed class PostDetailDto
    {
        public int PostId { get; set; }
        public string Type { get; set; } = "";
        public int? GameId { get; set; }
        public string Title { get; set; } = "";
        public string? Tldr { get; set; }
        public string BodyMd { get; set; } = "";        // 全文 Markdown
        public string Status { get; set; } = "";
        public bool Pinned { get; set; }
        public AuthorDto Author { get; set; } = new();
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class AuthorDto
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = "Admin"; // 先給預設；之後可 join Users
    }

    public sealed class LinkDto
    {
        public string Detail { get; set; } = "";
    }

    // 泛型分頁包裝
    public sealed class PagedResult<T>
    {
        public int Page { get; }
        public int Size { get; }
        public int Total { get; }
        public IReadOnlyList<T> Items { get; }

        public PagedResult(int page, int size, int total, IReadOnlyList<T> items)
        {
            Page = page; Size = size; Total = total; Items = items;
        }
    }
}
