namespace GamiPort.Areas.Forum.Dtos.Threads
{
    public record CreateThreadRequest(
         int ForumId,
         string Title,
         string ContentMd
     );
}
