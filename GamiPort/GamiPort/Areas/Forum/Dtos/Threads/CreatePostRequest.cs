namespace GamiPort.Areas.Forum.Dtos.Threads
{
    public record CreatePostRequest(
         string ContentMd,
         long? ParentPostId
     );
}
