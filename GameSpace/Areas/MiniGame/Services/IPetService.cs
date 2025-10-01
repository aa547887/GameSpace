using GameSpace.Areas.MiniGame.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物管理服務介面
    /// </summary>
    public interface IPetService
    {
        /// <summary>
        /// 取得會員寵物資訊
        /// </summary>
        Task<PetViewModel?> GetUserPetAsync(int userId);
        
        /// <summary>
        /// 建立新寵物
        /// </summary>
        Task<bool> CreatePetAsync(PetViewModel pet);
        
        /// <summary>
        /// 更新寵物資訊
        /// </summary>
        Task<bool> UpdatePetAsync(PetViewModel pet);
        
        /// <summary>
        /// 刪除寵物
        /// </summary>
        Task<bool> DeletePetAsync(int petId);
        
        /// <summary>
        /// 餵食寵物
        /// </summary>
        Task<bool> FeedPetAsync(int petId);
        
        /// <summary>
        /// 與寵物玩耍
        /// </summary>
        Task<bool> PlayWithPetAsync(int petId);
        
        /// <summary>
        /// 幫寵物洗澡
        /// </summary>
        Task<bool> BathePetAsync(int petId);
        
        /// <summary>
        /// 讓寵物睡覺
        /// </summary>
        Task<bool> LetPetSleepAsync(int petId);
        
        /// <summary>
        /// 更換寵物膚色
        /// </summary>
        Task<bool> ChangePetSkinAsync(int petId, string newSkin, int cost);
        
        /// <summary>
        /// 更換寵物背景
        /// </summary>
        Task<bool> ChangePetBackgroundAsync(int petId, string newBackground, int cost);
        
        /// <summary>
        /// 取得寵物規則設定
        /// </summary>
        Task<PetRulesViewModel> GetPetRulesAsync();
        
        /// <summary>
        /// 更新寵物規則設定
        /// </summary>
        Task<bool> UpdatePetRulesAsync(PetRulesViewModel rules);
        
        /// <summary>
        /// 取得所有寵物列表
        /// </summary>
        Task<List<PetViewModel>> GetAllPetsAsync(int page = 1, int pageSize = 20);
        
        /// <summary>
        /// 執行每日屬性衰減
        /// </summary>
        Task<bool> ProcessDailyDecayAsync();
    }
}