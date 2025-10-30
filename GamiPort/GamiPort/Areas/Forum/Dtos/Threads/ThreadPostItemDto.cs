namespace GamiPort.Areas.Forum.Dtos.Threads
{
    public sealed record ThreadPostItemDto(
     long PostId,
     long AuthorId,
     string AuthorName,     // ← 暱稱（可中文）
     DateTime CreatedAt,
     string? ContentMd,
     string? ContentHtml,   // 若你有預先轉好的 HTML，沒有就給 null
     long? ParentPostId,
     int LikeCount,
     bool IsLikedByMe,      // ← 當前使用者是否已讚
     bool CanDelete         // ← 是否可刪（作者本人或管理員）
 );
}
