using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物等級經驗值設定服務實作
    /// </summary>
    public class PetLevelExperienceSettingService : IPetLevelExperienceSettingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PetLevelExperienceSettingService> _logger;

        public PetLevelExperienceSettingService(ApplicationDbContext context, ILogger<PetLevelExperienceSettingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得所有等級經驗值設定
        /// </summary>
        public async Task<(List<PetLevelExperienceSettingListViewModel> Items, int TotalCount)> GetAllAsync(PetLevelExperienceSettingSearchViewModel searchModel)
        {
            try
            {
                var query = _context.PetLevelExperienceSettings.AsQueryable();

                // 搜尋條件
                if (!string.IsNullOrEmpty(searchModel.Keyword))
                {
                    query = query.Where(x => x.LevelName.Contains(searchModel.Keyword) || 
                                           (x.Description != null && x.Description.Contains(searchModel.Keyword)));
                }

                if (searchModel.MinLevel.HasValue)
                {
                    query = query.Where(x => x.Level >= searchModel.MinLevel.Value);
                }

                if (searchModel.MaxLevel.HasValue)
                {
                    query = query.Where(x => x.Level <= searchModel.MaxLevel.Value);
                }

                if (searchModel.IsEnabled.HasValue)
                {
                    query = query.Where(x => x.IsEnabled == searchModel.IsEnabled.Value);
                }

                // 取得總數
                var totalCount = await query.CountAsync();

                // 分頁和排序
                var items = await query
                    .OrderBy(x => x.Level)
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(x => new PetLevelExperienceSettingListViewModel
                    {
                        Id = x.Id,
                        Level = x.Level,
                        RequiredExperience = x.RequiredExperience,
                        LevelName = x.LevelName,
                        Description = x.Description,
                        IsEnabled = x.IsEnabled,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        CreatedBy = x.CreatedBy,
                        UpdatedBy = x.UpdatedBy
                    })
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得等級經驗值設定列表時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 根據ID取得等級經驗值設定
        /// </summary>
        public async Task<PetLevelExperienceSetting?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.PetLevelExperienceSettings.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得等級經驗值設定 {Id} 時發生錯誤", id);
                throw;
            }
        }

        /// <summary>
        /// 建立等級經驗值設定
        /// </summary>
        public async Task<bool> CreateAsync(PetLevelExperienceSettingCreateViewModel model, string? createdBy = null)
        {
            try
            {
                // 檢查等級是否已存在
                if (await IsLevelExistsAsync(model.Level))
                {
                    return false;
                }

                var entity = new PetLevelExperienceSetting
                {
                    Level = model.Level,
                    RequiredExperience = model.RequiredExperience,
                    LevelName = model.LevelName,
                    Description = model.Description,
                    IsEnabled = model.IsEnabled,
                    CreatedBy = createdBy,
                    UpdatedBy = createdBy
                };

                _context.PetLevelExperienceSettings.Add(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功建立等級經驗值設定: Level {Level}, Name {LevelName}", model.Level, model.LevelName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立等級經驗值設定時發生錯誤");
                return false;
            }
        }

        /// <summary>
        /// 更新等級經驗值設定
        /// </summary>
        public async Task<bool> UpdateAsync(PetLevelExperienceSettingEditViewModel model, string? updatedBy = null)
        {
            try
            {
                var entity = await _context.PetLevelExperienceSettings.FindAsync(model.Id);
                if (entity == null)
                {
                    return false;
                }

                // 檢查等級是否已存在（排除自己）
                if (await IsLevelExistsAsync(model.Level, model.Id))
                {
                    return false;
                }

                entity.Level = model.Level;
                entity.RequiredExperience = model.RequiredExperience;
                entity.LevelName = model.LevelName;
                entity.Description = model.Description;
                entity.IsEnabled = model.IsEnabled;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = updatedBy;

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功更新等級經驗值設定: ID {Id}, Level {Level}", model.Id, model.Level);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新等級經驗值設定 {Id} 時發生錯誤", model.Id);
                return false;
            }
        }

        /// <summary>
        /// 刪除等級經驗值設定
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.PetLevelExperienceSettings.FindAsync(id);
                if (entity == null)
                {
                    return false;
                }

                _context.PetLevelExperienceSettings.Remove(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功刪除等級經驗值設定: ID {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除等級經驗值設定 {Id} 時發生錯誤", id);
                return false;
            }
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        public async Task<bool> ToggleStatusAsync(int id, string? updatedBy = null)
        {
            try
            {
                var entity = await _context.PetLevelExperienceSettings.FindAsync(id);
                if (entity == null)
                {
                    return false;
                }

                entity.IsEnabled = !entity.IsEnabled;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = updatedBy;

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功切換等級經驗值設定狀態: ID {Id}, IsEnabled {IsEnabled}", id, entity.IsEnabled);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換等級經驗值設定狀態 {Id} 時發生錯誤", id);
                return false;
            }
        }

        /// <summary>
        /// 檢查等級是否已存在
        /// </summary>
        public async Task<bool> IsLevelExistsAsync(int level, int? excludeId = null)
        {
            try
            {
                var query = _context.PetLevelExperienceSettings.Where(x => x.Level == level);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查等級 {Level} 是否存在時發生錯誤", level);
                return false;
            }
        }

        /// <summary>
        /// 取得等級統計資料
        /// </summary>
        public async Task<object> GetStatisticsAsync()
        {
            try
            {
                var totalCount = await _context.PetLevelExperienceSettings.CountAsync();
                var enabledCount = await _context.PetLevelExperienceSettings.CountAsync(x => x.IsEnabled);
                var disabledCount = totalCount - enabledCount;
                var maxLevel = await _context.PetLevelExperienceSettings.MaxAsync(x => (int?)x.Level) ?? 0;
                var minLevel = await _context.PetLevelExperienceSettings.MinAsync(x => (int?)x.Level) ?? 0;
                var totalExperience = await _context.PetLevelExperienceSettings.SumAsync(x => x.RequiredExperience);

                return new
                {
                    TotalCount = totalCount,
                    EnabledCount = enabledCount,
                    DisabledCount = disabledCount,
                    MaxLevel = maxLevel,
                    MinLevel = minLevel,
                    TotalExperience = totalExperience
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得等級統計資料時發生錯誤");
                return new
                {
                    TotalCount = 0,
                    EnabledCount = 0,
                    DisabledCount = 0,
                    MaxLevel = 0,
                    MinLevel = 0,
                    TotalExperience = 0
                };
            }
        }
    }
}
