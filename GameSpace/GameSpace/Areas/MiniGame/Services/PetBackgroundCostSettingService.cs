using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物背景成本設定服務實作
    /// </summary>
    public class PetBackgroundCostSettingService : IPetBackgroundCostSettingService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetBackgroundCostSettingService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // 基本 CRUD
        public async Task<IEnumerable<PetBackgroundCostSetting>> GetAllAsync()
        {
            return await _context.PetBackgroundCostSettings
                .OrderBy(s => s.RequiredPoints)
                .ThenBy(s => s.BackgroundName)
                .ToListAsync();
        }

        public async Task<PetBackgroundCostSetting?> GetByIdAsync(int id)
        {
            return await _context.PetBackgroundCostSettings
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> CreateAsync(PetBackgroundCostSetting setting)
        {
            try
            {
                // 檢查背景代碼是否已存在
                if (await ExistsByBackgroundCodeAsync(setting.BackgroundCode))
                {
                    return false;
                }

                setting.CreatedAt = DateTime.UtcNow;
                setting.IsActive = true;

                _context.PetBackgroundCostSettings.Add(setting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PetBackgroundCostSetting setting)
        {
            try
            {
                var existing = await GetByIdAsync(setting.Id);
                if (existing == null) return false;

                // 檢查新背景代碼是否與其他設定衝突
                if (existing.BackgroundCode != setting.BackgroundCode &&
                    await ExistsByBackgroundCodeAsync(setting.BackgroundCode))
                {
                    return false;
                }

                existing.BackgroundName = setting.BackgroundName;
                existing.BackgroundCode = setting.BackgroundCode;
                existing.RequiredPoints = setting.RequiredPoints;
                existing.IsActive = setting.IsActive;
                existing.Description = setting.Description;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var setting = await GetByIdAsync(id);
                if (setting == null) return false;

                _context.PetBackgroundCostSettings.Remove(setting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 查詢功能
        public async Task<IEnumerable<PetBackgroundCostSetting>> GetActiveSettingsAsync()
        {
            return await _context.PetBackgroundCostSettings
                .Where(s => s.IsActive)
                .OrderBy(s => s.RequiredPoints)
                .ThenBy(s => s.BackgroundName)
                .ToListAsync();
        }

        public async Task<PetBackgroundCostSetting?> GetByBackgroundCodeAsync(string backgroundCode)
        {
            return await _context.PetBackgroundCostSettings
                .FirstOrDefaultAsync(s => s.BackgroundCode == backgroundCode);
        }

        public async Task<PetBackgroundCostSetting?> GetByBackgroundNameAsync(string backgroundName)
        {
            return await _context.PetBackgroundCostSettings
                .FirstOrDefaultAsync(s => s.BackgroundName == backgroundName);
        }

        public async Task<int> GetCostByBackgroundCodeAsync(string backgroundCode)
        {
            var setting = await GetByBackgroundCodeAsync(backgroundCode);
            return setting?.RequiredPoints ?? 30; // 預設 30 點
        }

        public async Task<bool> ExistsByBackgroundCodeAsync(string backgroundCode)
        {
            return await _context.PetBackgroundCostSettings
                .AnyAsync(s => s.BackgroundCode == backgroundCode);
        }

        // 排序與分頁
        public async Task<IEnumerable<PetBackgroundCostSetting>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.PetBackgroundCostSettings
                .OrderBy(s => s.RequiredPoints)
                .ThenBy(s => s.BackgroundName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.PetBackgroundCostSettings.CountAsync();
        }

        // 狀態管理
        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            try
            {
                var setting = await GetByIdAsync(id);
                if (setting == null) return false;

                setting.IsActive = !setting.IsActive;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetActiveStatusAsync(int id, bool isActive)
        {
            try
            {
                var setting = await GetByIdAsync(id);
                if (setting == null) return false;

                setting.IsActive = isActive;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleActiveAsync(int settingId)
        {
            try
            {
                var setting = await GetByIdAsync(settingId);
                if (setting == null) return false;

                setting.IsActive = !setting.IsActive;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 批次操作
        public async Task<bool> UpdateMultipleCostsAsync(Dictionary<int, int> costMapping)
        {
            try
            {
                foreach (var kvp in costMapping)
                {
                    var setting = await GetByIdAsync(kvp.Key);
                    if (setting != null)
                    {
                        setting.RequiredPoints = kvp.Value;
                        setting.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
