using System.Collections.Generic;

namespace GamiPort.Areas.OnlineStore.DTO.Store
{
    public class PagedResult<T>
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public int totalCount { get; set; }
        public IEnumerable<T> items { get; set; } = new List<T>();
    }
}

