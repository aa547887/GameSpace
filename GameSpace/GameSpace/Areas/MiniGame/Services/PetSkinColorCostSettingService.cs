using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物膚色成本設定服務實作
    /// </summary>
    public class PetSkinColorCostSettingService : IPetSkinColorCostSettingService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetSkinColorCostSettingService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // 基本 CRUD
        public async Task<IEnumerable<PetSkinColorCostSetting>> GetAllAsync()
        {
            return await _context.PetSkinColorCostSettings
                .OrderBy(s => s.RequiredPoints)
                .ThenBy(s => s.ColorName)
                .ToListAsync();
        }

        public async Task<PetSkinColorCostSetting?> GetByIdAsync(int id)
        {
            return await _context.PetSkinColorCostSettings
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<bool> CreateAsync(PetSkinColorCostSetting setting)
        {
            try
            {
                // 檢查顏色代碼是否已存在
                if (await ExistsByColorCodeAsync(setting.ColorCode))
                {
                    return false;
                }

                setting.CreatedAt = DateTime.UtcNow;
                setting.IsActive = true;

                _context.PetSkinColorCostSettings.Add(setting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PetSkinColorCostSetting setting)
        {
            try
            {
                var existing = await GetByIdAsync(setting.Id);
                if (existing == null) return false;

                // 檢查新顏色代碼是否與其他設定衝突
                if (existing.ColorCode != setting.ColorCode &&
                    await ExistsByColorCodeAsync(setting.ColorCode))
                {
                    return false;
                }

                existing.ColorName = setting.ColorName;
                existing.ColorCode = setting.ColorCode;
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

                _context.PetSkinColorCostSettings.Remove(setting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 查詢功能
        public async Task<IEnumerable<PetSkinColorCostSetting>> GetActiveSettingsAsync()
        {
            return await _context.PetSkinColorCostSettings
                .Where(s => s.IsActive)
                .OrderBy(s => s.RequiredPoints)
                .ThenBy(s => s.ColorName)
                .ToListAsync();
        }

        public async Task<PetSkinColorCostSetting?> GetByColorCodeAsync(string colorCode)
        {
            return await _context.PetSkinColorCostSettings
                .FirstOrDefaultAsync(s => s.ColorCode == colorCode);
        }

        public async Task<PetSkinColorCostSetting?> GetByColorNameAsync(string colorName)
        {
            return await _context.PetSkinColorCostSettings
                .FirstOrDefaultAsync(s => s.ColorName == colorName);
        }

        public async Task<int> GetCostByColorCodeAsync(string colorCode)
        {
            var setting = await GetByColorCodeAsync(colorCode);
            return setting?.RequiredPoints ?? 50; // 預設 50 點
        }

        public async Task<bool> ExistsByColorCodeAsync(string colorCode)
        {
            return await _context.PetSkinColorCostSettings
                .AnyAsync(s => s.ColorCode == colorCode);
        }

        // 排序與分頁
        public async Task<IEnumerable<PetSkinColorCostSetting>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.PetSkinColorCostSettings
                .OrderBy(s => s.RequiredPoints)
                .ThenBy(s => s.ColorName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.PetSkinColorCostSettings.CountAsync();
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
