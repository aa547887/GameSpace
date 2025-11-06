namespace GamiPort.Areas.Forum.Dtos.Common
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int Page { get; }
        public int Size { get; }
        public int Total { get; }
        public PagedResult(IReadOnlyList<T> items, int page, int size, int total)
        {
            Items = items;
            Page = page;
            Size = size;
            Total = total;
        }
        public static PagedResult<T> Empty(int page, int size)
        => new(Array.Empty<T>(), 0, page, size);

    }
}
