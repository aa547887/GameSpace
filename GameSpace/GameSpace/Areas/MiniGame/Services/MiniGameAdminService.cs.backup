using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public class MiniGameAdminService : IMiniGameAdminService
    {
        private readonly GameSpacedatabaseContext _context;

        public MiniGameAdminService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // SignIn 相關方法
        public async Task<SignInSummaryReadModel> GetSignInSummaryAsync()
        {
            // 簡化實作，返回空資料
            return new SignInSummaryReadModel();
        }

        public async Task<PagedResult<SignInStatsReadModel>> GetSignInStatsAsync(SignInQueryModel query)
        {
            // 簡化實作，返回空資料
            return new PagedResult<SignInStatsReadModel>();
        }

        public async Task<IEnumerable<UserSignInHistoryReadModel>> GetUserSignInHistoryAsync(int userId)
        {
            // 簡化實作，返回空資料
            return new List<UserSignInHistoryReadModel>();
        }

        public async Task<UserInfoReadModel> GetUserInfoAsync(int userId)
        {
            // 簡化實作，返回空資料
            return new UserInfoReadModel();
        }

        public async Task<bool> AddUserSignInRecordAsync(int userId, DateTime signInDate)
        {
            // 簡化實作，返回 true
            return true;
        }

        public async Task<bool> RemoveUserSignInRecordAsync(int userId, DateTime signInDate)
        {
            // 簡化實作，返回 true
            return true;
        }

        public async Task<bool> ToggleSignInAsync(int userId, bool isAdd)
        {
            // 簡化實作，返回 true
            return true;
        }

        public async Task<bool> BulkToggleSignInAsync(List<int> userIds, bool isAdd)
        {
            // 簡化實作，返回 true
            return true;
        }

        // Pet 相關方法
        public async Task<PagedResult<PetReadModel>> GetPetsAsync(PetQueryModel query)
        {
            // 簡化實作，返回空資料
            return new PagedResult<PetReadModel>();
        }

        public async Task<PetSummaryReadModel> GetPetSummaryAsync()
        {
            // 簡化實作，返回空資料
            return new PetSummaryReadModel();
        }

        public async Task<PetDetailReadModel> GetPetDetailAsync(int petId)
        {
            // 簡化實作，返回空資料
            return new PetDetailReadModel();
        }

        // Game 相關方法
        public async Task<PagedResult<GameRecordReadModel>> GetGameRecordsAsync(GameQueryModel query)
        {
            // 簡化實作，返回空資料
            return new PagedResult<GameRecordReadModel>();
        }

        public async Task<GameSummaryReadModel> GetGameSummaryAsync()
        {
            // 簡化實作，返回空資料
            return new GameSummaryReadModel();
        }

        public async Task<GameDetailReadModel> GetGameDetailAsync(int gameId)
        {
            // 簡化實作，返回空資料
            return new GameDetailReadModel();
        }

        // Coupon 相關方法
        public async Task<PagedResult<UserCouponReadModel>> QueryUserCouponsAsync(CouponQueryModel query)
        {
            // 簡化實作，返回空資料
            return new PagedResult<UserCouponReadModel>();
        }

        public async Task<bool> IssueCouponToUserAsync(int userId, int couponId, int quantity)
        {
            // 簡化實作，返回 true
            return true;
        }

        // EVoucher 相關方法
        public async Task<PagedResult<UserEVoucherReadModel>> QueryUserEVouchersAsync(EVoucherQueryModel query)
        {
            // 簡化實作，返回空資料
            return new PagedResult<UserEVoucherReadModel>();
        }

        public async Task<bool> IssueEVoucherToUserAsync(int userId, int eVoucherId, int quantity)
        {
            // 簡化實作，返回 true
            return true;
        }
    }
}
