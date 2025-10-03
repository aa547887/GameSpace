﻿using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IPetLevelRewardSettingService
    {
        Task<IEnumerable<PetLevelRewardSettingListViewModel>> GetAllAsync();
        Task<PetLevelRewardSettingViewModel?> GetByIdAsync(int id);
        Task<PetLevelRewardSettingViewModel?> CreateAsync(PetLevelRewardSettingCreateViewModel model);
        Task<PetLevelRewardSettingViewModel?> UpdateAsync(PetLevelRewardSettingEditViewModel model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleStatusAsync(int id);
        Task<IEnumerable<PetLevelRewardSettingListViewModel>> SearchAsync(PetLevelRewardSettingSearchViewModel searchModel);
        Task<(int TotalCount, int TotalPages)> GetPaginationInfoAsync(PetLevelRewardSettingSearchViewModel searchModel);
        Task<Dictionary<string, object>> GetStatisticsAsync();
        Task<bool> ValidateLevelAsync(int level, int? excludeId = null);
        Task<IEnumerable<string>> GetRewardTypesAsync();
    }
}

