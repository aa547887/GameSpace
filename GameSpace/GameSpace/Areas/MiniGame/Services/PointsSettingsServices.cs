using GameSpace.Areas.MiniGame.Models.Settings;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換色點數設定服務介面
    /// </summary>
    public interface IPetColorChangeSettingsService
    {
        Task<List<PetColorChangeSettings>> GetAllAsync();
        Task<PetColorChangeSettings?> GetByIdAsync(int id);
        Task<PetColorChangeSettings> CreateAsync(PetColorChangeSettings model);
        Task<PetColorChangeSettings> UpdateAsync(int id, PetColorChangeSettings model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
        Task<List<PetColorChangeSettings>> GetActiveSettingsAsync();
        Task<int> GetTotalActiveSettingsAsync();
        Task<int> GetTotalPointsAsync();
    }

    /// <summary>
    /// 寵物換背景點數設定服務介面
    /// </summary>
    public interface IPetBackgroundChangeSettingsService
    {
        Task<List<PetBackgroundChangeSettings>> GetAllAsync();
        Task<PetBackgroundChangeSettings?> GetByIdAsync(int id);
        Task<PetBackgroundChangeSettings> CreateAsync(PetBackgroundChangeSettings model);
        Task<PetBackgroundChangeSettings> UpdateAsync(int id, PetBackgroundChangeSettings model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
        Task<List<PetBackgroundChangeSettings>> GetActiveSettingsAsync();
        Task<int> GetTotalActiveSettingsAsync();
        Task<int> GetTotalPointsAsync();
    }

    /// <summary>
    /// 點數設定統計服務介面
    /// </summary>
    public interface IPointsSettingsStatisticsService
    {
        Task<int> GetTotalColorSettingsAsync();
        Task<int> GetTotalBackgroundSettingsAsync();
        Task<int> GetActiveColorSettingsAsync();
        Task<int> GetActiveBackgroundSettingsAsync();
        Task<int> GetTotalColorPointsAsync();
        Task<int> GetTotalBackgroundPointsAsync();
        Task<int> GetTotalPointsAsync();
    }
}
