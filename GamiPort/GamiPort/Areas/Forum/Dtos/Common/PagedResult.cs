namespace GamiPort.Areas.Forum.Dtos.Common
{
    public sealed record PagedResult<T>(
     IReadOnlyList<T> Items,
     int Page,
     int Size,
     int Total
 );
}
