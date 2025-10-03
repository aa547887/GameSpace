using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換色成本設定服務介面
    /// </summary>
    public interface IPetSkinColorCostSettingService
    {
        Task<IEnumerable<PetSkinColorCostSetting>> GetAllAsync();
        Task<PetSkinColorCostSetting?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PetSkinColorCostSetting model);
        Task<bool> UpdateAsync(PetSkinColorCostSetting model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
    }

    /// <summary>
    /// 寵物換背景成本設定服務介面
    /// </summary>
    public interface IPetBackgroundCostSettingService
    {
        Task<IEnumerable<PetBackgroundCostSetting>> GetAllAsync();
        Task<PetBackgroundCostSetting?> GetByIdAsync(int id);
        Task<bool> CreateAsync(PetBackgroundCostSetting model);
        Task<bool> UpdateAsync(PetBackgroundCostSetting model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
    }

    /// <summary>
    /// 寵物換色成本設定服務實作
    /// </summary>
    public class PetSkinColorCostSettingService : IPetSkinColorCostSettingService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetSkinColorCostSettingService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PetSkinColorCostSetting>> GetAllAsync()
        {
            return await _context.PetSkinColorCostSettings
                .OrderBy(x => x.ColorName)
                .ToListAsync();
        }

        public async Task<PetSkinColorCostSetting?> GetByIdAsync(int id)
        {
            return await _context.PetSkinColorCostSettings
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> CreateAsync(PetSkinColorCostSetting model)
        {
            try
            {
                _context.PetSkinColorCostSettings.Add(model);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PetSkinColorCostSetting model)
        {
            try
            {
                model.UpdatedAt = DateTime.UtcNow;
                _context.PetSkinColorCostSettings.Update(model);
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
                var model = await GetByIdAsync(id);
                if (model == null) return false;

                _context.PetSkinColorCostSettings.Remove(model);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            try
            {
                var model = await GetByIdAsync(id);
                if (model == null) return false;

                model.IsActive = !model.IsActive;
                model.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 寵物換背景成本設定服務實作
    /// </summary>
    public class PetBackgroundCostSettingService : IPetBackgroundCostSettingService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetBackgroundCostSettingService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PetBackgroundCostSetting>> GetAllAsync()
        {
            return await _context.PetBackgroundCostSettings
                .OrderBy(x => x.BackgroundName)
                .ToListAsync();
        }

        public async Task<PetBackgroundCostSetting?> GetByIdAsync(int id)
        {
            return await _context.PetBackgroundCostSettings
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> CreateAsync(PetBackgroundCostSetting model)
        {
            try
            {
                _context.PetBackgroundCostSettings.Add(model);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PetBackgroundCostSetting model)
        {
            try
            {
                model.UpdatedAt = DateTime.UtcNow;
                _context.PetBackgroundCostSettings.Update(model);
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
                var model = await GetByIdAsync(id);
                if (model == null) return false;

                _context.PetBackgroundCostSettings.Remove(model);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            try
            {
                var model = await GetByIdAsync(id);
                if (model == null) return false;

                model.IsActive = !model.IsActive;
                model.UpdatedAt = DateTime.UtcNow;
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

