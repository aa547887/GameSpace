namespace GamiPort.Areas.Forum.Dtos.Threads
{
    public record ThreadPostRowDto(
       long PostId,
       long ThreadId,
       long AuthorUserId,
       string ContentMd,
       DateTime CreatedAt,
       long? ParentPostId,
       int LikeCount
   )
    {
       

        
    }
}
