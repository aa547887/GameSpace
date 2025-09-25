using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    public class AdminSignInIndexViewModel
    {
        public PagedResult<UserSignInStat> SignInStats { get; set; } = new PagedResult<UserSignInStat>();
        public List<GameSpace.Models.User> Users { get; set; } = new List<GameSpace.Models.User>();
        public string SearchTerm { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public SidebarViewModel Sidebar { get; set; } = new SidebarViewModel();
    }
}
