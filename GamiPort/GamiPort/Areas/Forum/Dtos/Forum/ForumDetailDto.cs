namespace GamiPort.Areas.Forum.Dtos.Forum
{
    public sealed record ForumDetailDto(
     int ForumId,
     int GameId,
     string Name,
     string? Description
 );
}
