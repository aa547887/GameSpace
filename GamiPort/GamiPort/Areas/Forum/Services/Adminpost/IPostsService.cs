using GamiPort.Areas.Forum.Dtos.AdminPosts;
using System.Threading.Tasks;  //這不知道
namespace GamiPort.Areas.Forum.Services.Adminpost
{
    public interface IPostsService
    {
        Task<PagedResult<PostListDto>> GetFrontPostsAsync(PostQuery query);
        Task<PostDetailDto?> GetFrontPostAsync(int postId);
    }

}
