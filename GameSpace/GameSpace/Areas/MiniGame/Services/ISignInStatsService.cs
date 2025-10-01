using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface ISignInStatsService
    {
        Task<PagedResult<UserSignInStats>> GetSignInStatsAsync(SignInStatsQueryModel query);
        Task<SignInStatsSummary> GetSignInStatsSummaryAsync();
        Task<bool> ConfigureSignInRulesAsync(SignInRulesModel rules);
        Task<SignInRulesModel> GetSignInRulesAsync();
    }
}
