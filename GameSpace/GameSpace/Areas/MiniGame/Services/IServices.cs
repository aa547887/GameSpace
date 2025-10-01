namespace GameSpace.Areas.MiniGame.Services
{
    // 基礎服務介面
    public interface IBaseService<T, TQuery, TRead, TWrite> where T : class
    {
        Task<PagedResult<TRead>> GetPagedAsync(TQuery query);
        Task<TRead?> GetByIdAsync(int id);
        Task<TRead> CreateAsync(TWrite model);
        Task<TRead> UpdateAsync(int id, TWrite model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }

    // 優惠券服務介面
    public interface ICouponService : IBaseService<Coupon, CouponQueryModel, CouponReadModel, CouponWriteModel>
    {
        Task<List<CouponReadModel>> GetByUserIdAsync(int userId);
        Task<List<CouponReadModel>> GetByCouponTypeIdAsync(int couponTypeId);
        Task<bool> UseCouponAsync(int couponId, int orderId);
        Task<bool> IsCouponValidAsync(int couponId);
        Task<CouponReadModel> GenerateCouponAsync(int userId, int couponTypeId);
    }

    // 電子券服務介面
    public interface IEVoucherService : IBaseService<EVoucher, EVoucherQueryModel, EVoucherReadModel, EVoucherWriteModel>
    {
        Task<List<EVoucherReadModel>> GetByUserIdAsync(int userId);
        Task<List<EVoucherReadModel>> GetByEVoucherTypeIdAsync(int eVoucherTypeId);
        Task<bool> UseEVoucherAsync(int eVoucherId);
        Task<bool> IsEVoucherValidAsync(int eVoucherId);
        Task<EVoucherReadModel> GenerateEVoucherAsync(int userId, int eVoucherTypeId);
        Task<string> GenerateTokenAsync(int eVoucherId);
        Task<bool> RedeemTokenAsync(string token, string storeLocation, string staffId);
    }

    // 寵物服務介面
    public interface IPetService : IBaseService<Pet, PetQueryModel, PetReadModel, PetWriteModel>
    {
        Task<List<PetReadModel>> GetByUserIdAsync(int userId);
        Task<PetReadModel> FeedPetAsync(int petId);
        Task<PetReadModel> PlayWithPetAsync(int petId);
        Task<PetReadModel> BathePetAsync(int petId);
        Task<PetReadModel> ChangeSkinColorAsync(int petId, string skinColor);
        Task<PetReadModel> ChangeBackgroundColorAsync(int petId, string backgroundColor);
        Task<PetReadModel> LevelUpPetAsync(int petId);
        Task<bool> IsPetReadyForLevelUpAsync(int petId);
        Task<PetReadModel> DecayAttributesAsync(int petId);
    }

    // 遊戲服務介面
    public interface IGameService : IBaseService<MiniGame, GameQueryModel, GameReadModel, GameWriteModel>
    {
        Task<List<GameReadModel>> GetByUserIdAsync(int userId);
        Task<List<GameReadModel>> GetByPetIdAsync(int petId);
        Task<List<GameReadModel>> GetByGameTypeAsync(string gameType);
        Task<GameReadModel> StartGameAsync(int userId, int petId, string gameType);
        Task<GameReadModel> EndGameAsync(int gameId, string result, int pointsEarned, int expEarned, int couponEarned);
        Task<bool> CanPlayGameAsync(int userId, string gameType);
        Task<int> GetDailyGameCountAsync(int userId, string gameType);
        Task<List<GameReadModel>> GetLeaderboardAsync(string gameType, int topCount = 10);
    }

    // 錢包服務介面
    public interface IWalletService
    {
        Task<UserWallet?> GetUserWalletAsync(int userId);
        Task<bool> AddPointsAsync(int userId, int points, string description);
        Task<bool> DeductPointsAsync(int userId, int points, string description);
        Task<bool> TransferPointsAsync(int fromUserId, int toUserId, int points, string description);
        Task<List<WalletHistoryReadModel>> GetWalletHistoryAsync(int userId, WalletHistoryQueryModel query);
        Task<bool> CanAffordAsync(int userId, int points);
        Task<int> GetUserPointsAsync(int userId);
    }

    // 簽到服務介面
    public interface ISignInService
    {
        Task<bool> CanSignInAsync(int userId);
        Task<SignInReadModel> SignInAsync(int userId);
        Task<List<SignInReadModel>> GetSignInHistoryAsync(int userId, SignInQueryModel query);
        Task<int> GetConsecutiveDaysAsync(int userId);
        Task<bool> HasSignedInTodayAsync(int userId);
        Task<SignInReadModel> GetTodaySignInAsync(int userId);
    }

    // 管理員服務介面
    public interface IManagerService : IBaseService<ManagerData, ManagerQueryModel, ManagerReadModel, ManagerWriteModel>
    {
        Task<ManagerReadModel?> GetByAccountAsync(string account);
        Task<bool> ValidatePasswordAsync(string account, string password);
        Task<List<string>> GetPermissionsAsync(int managerId);
        Task<bool> HasPermissionAsync(int managerId, string permission);
        Task<ManagerReadModel> CreateManagerAsync(ManagerWriteModel model);
        Task<ManagerReadModel> UpdateManagerAsync(int managerId, ManagerWriteModel model);
        Task<bool> AssignRoleAsync(int managerId, int roleId);
        Task<bool> RemoveRoleAsync(int managerId, int roleId);
    }

    // 用戶服務介面
    public interface IUserService : IBaseService<User, UserQueryModel, UserReadModel, UserWriteModel>
    {
        Task<UserReadModel?> GetByAccountAsync(string account);
        Task<bool> ValidatePasswordAsync(string account, string password);
        Task<UserReadModel> CreateUserAsync(UserWriteModel model);
        Task<UserReadModel> UpdateUserAsync(int userId, UserWriteModel model);
        Task<bool> LockUserAsync(int userId, DateTime? lockoutEnd);
        Task<bool> UnlockUserAsync(int userId);
        Task<bool> ConfirmEmailAsync(int userId);
        Task<StatisticsReadModel> GetStatisticsAsync();
    }

    // 權限服務介面
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int managerId, string permission);
        Task<List<string>> GetManagerPermissionsAsync(int managerId);
        Task<List<ManagerRolePermission>> GetRolePermissionsAsync(int roleId);
        Task<bool> CanManageUsersAsync(int managerId);
        Task<bool> CanManagePetsAsync(int managerId);
        Task<bool> CanManageShoppingAsync(int managerId);
        Task<bool> CanManageMessagesAsync(int managerId);
        Task<bool> CanManageAdministratorsAsync(int managerId);
        Task<bool> CanProvideCustomerServiceAsync(int managerId);
    }
}
