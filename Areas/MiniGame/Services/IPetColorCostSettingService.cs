using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換色所需點數設定服務介面
    /// </summary>
    public interface IPetColorCostSettingService
    {
        Task<IEnumerable<PetColorCostSetting>> GetAllAsync();
        Task<PetColorCostSetting?> GetByIdAsync(int id);
        Task<PetColorCostSetting> CreateAsync(PetColorCostSettingViewModel model);
        Task<PetColorCostSetting> UpdateAsync(int id, PetColorCostSettingViewModel model);
        Task<bool> DeleteAsync(int id);
        Task<PetColorCostSettingListViewModel> GetListAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool? isActiveFilter = null);
        Task<bool> ExistsAsync(int id);
    }
}
