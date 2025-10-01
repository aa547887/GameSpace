using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換色點數設定服務實作
    /// </summary>
    public class PetColorChangeSettingsService : IPetColorChangeSettingsService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<PetColorChangeSettingsService> _logger;

        public PetColorChangeSettingsService(MiniGameDbContext context, ILogger<PetColorChangeSettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PetColorChangeSettings>> GetAllAsync()
        {
            try
            {
                return await _context.PetColorChangeSettings
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有寵物換色設定時發生錯誤");
                throw;
            }
        }

        public async Task<PetColorChangeSettings?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.PetColorChangeSettings.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物換色設定 {Id} 時發生錯誤", id);
                throw;
            }
        }

        public async Task<PetColorChangeSettings> CreateAsync(PetColorChangeSettings model)
        {
            try
            {
                model.CreatedAt = DateTime.UtcNow;
                _context.PetColorChangeSettings.Add(model);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功建立寵物換色設定 {Id}", model.Id);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立寵物換色設定時發生錯誤");
                throw;
            }
        }

        public async Task<PetColorChangeSettings> UpdateAsync(int id, PetColorChangeSettings model)
        {
            try
            {
                var existing = await _context.PetColorChangeSettings.FindAsync(id);
                if (existing == null)
                {
                    throw new ArgumentException($"找不到 ID 為 {id} 的寵物換色設定");
                }

                existing.ColorName = model.ColorName;
                existing.RequiredPoints = model.RequiredPoints;
                existing.ColorCode = model.ColorCode;
                existing.IsActive = model.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功更新寵物換色設定 {Id}", id);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物換色設定 {Id} 時發生錯誤", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var model = await _context.PetColorChangeSettings.FindAsync(id);
                if (model == null)
                {
                    return false;
                }

                _context.PetColorChangeSettings.Remove(model);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功刪除寵物換色設定 {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物換色設定 {Id} 時發生錯誤", id);
                throw;
            }
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            try
            {
                var model = await _context.PetColorChangeSettings.FindAsync(id);
                if (model == null)
                {
                    return false;
                }

                model.IsActive = !model.IsActive;
                model.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功切換寵物換色設定 {Id} 的啟用狀態為 {IsActive}", id, model.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換寵物換色設定 {Id} 啟用狀態時發生錯誤", id);
                throw;
            }
        }

        public async Task<List<PetColorChangeSettings>> GetActiveSettingsAsync()
        {
            try
            {
                return await _context.PetColorChangeSettings
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.ColorName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得啟用的寵物換色設定時發生錯誤");
                throw;
            }
        }

        public async Task<int> GetTotalActiveSettingsAsync()
        {
            try
            {
                return await _context.PetColorChangeSettings
                    .CountAsync(x => x.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算啟用的寵物換色設定數量時發生錯誤");
                throw;
            }
        }

        public async Task<int> GetTotalPointsAsync()
        {
            try
            {
                return await _context.PetColorChangeSettings
                    .Where(x => x.IsActive)
                    .SumAsync(x => x.RequiredPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算寵物換色設定總點數時發生錯誤");
                throw;
            }
        }
    }

    /// <summary>
    /// 寵物換背景點數設定服務實作
    /// </summary>
    public class PetBackgroundChangeSettingsService : IPetBackgroundChangeSettingsService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<PetBackgroundChangeSettingsService> _logger;

        public PetBackgroundChangeSettingsService(MiniGameDbContext context, ILogger<PetBackgroundChangeSettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PetBackgroundChangeSettings>> GetAllAsync()
        {
            try
            {
                return await _context.PetBackgroundChangeSettings
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有寵物換背景設定時發生錯誤");
                throw;
            }
        }

        public async Task<PetBackgroundChangeSettings?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.PetBackgroundChangeSettings.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物換背景設定 {Id} 時發生錯誤", id);
                throw;
            }
        }

        public async Task<PetBackgroundChangeSettings> CreateAsync(PetBackgroundChangeSettings model)
        {
            try
            {
                model.CreatedAt = DateTime.UtcNow;
                _context.PetBackgroundChangeSettings.Add(model);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功建立寵物換背景設定 {Id}", model.Id);
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立寵物換背景設定時發生錯誤");
                throw;
            }
        }

        public async Task<PetBackgroundChangeSettings> UpdateAsync(int id, PetBackgroundChangeSettings model)
        {
            try
            {
                var existing = await _context.PetBackgroundChangeSettings.FindAsync(id);
                if (existing == null)
                {
                    throw new ArgumentException($"找不到 ID 為 {id} 的寵物換背景設定");
                }

                existing.BackgroundName = model.BackgroundName;
                existing.RequiredPoints = model.RequiredPoints;
                existing.BackgroundCode = model.BackgroundCode;
                existing.IsActive = model.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功更新寵物換背景設定 {Id}", id);
                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新寵物換背景設定 {Id} 時發生錯誤", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var model = await _context.PetBackgroundChangeSettings.FindAsync(id);
                if (model == null)
                {
                    return false;
                }

                _context.PetBackgroundChangeSettings.Remove(model);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功刪除寵物換背景設定 {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除寵物換背景設定 {Id} 時發生錯誤", id);
                throw;
            }
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            try
            {
                var model = await _context.PetBackgroundChangeSettings.FindAsync(id);
                if (model == null)
                {
                    return false;
                }

                model.IsActive = !model.IsActive;
                model.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功切換寵物換背景設定 {Id} 的啟用狀態為 {IsActive}", id, model.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換寵物換背景設定 {Id} 啟用狀態時發生錯誤", id);
                throw;
            }
        }

        public async Task<List<PetBackgroundChangeSettings>> GetActiveSettingsAsync()
        {
            try
            {
                return await _context.PetBackgroundChangeSettings
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.BackgroundName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得啟用的寵物換背景設定時發生錯誤");
                throw;
            }
        }

        public async Task<int> GetTotalActiveSettingsAsync()
        {
            try
            {
                return await _context.PetBackgroundChangeSettings
                    .CountAsync(x => x.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算啟用的寵物換背景設定數量時發生錯誤");
                throw;
            }
        }

        public async Task<int> GetTotalPointsAsync()
        {
            try
            {
                return await _context.PetBackgroundChangeSettings
                    .Where(x => x.IsActive)
                    .SumAsync(x => x.RequiredPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算寵物換背景設定總點數時發生錯誤");
                throw;
            }
        }
    }

    /// <summary>
    /// 點數設定統計服務實作
    /// </summary>
    public class PointsSettingsStatisticsService : IPointsSettingsStatisticsService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<PointsSettingsStatisticsService> _logger;

        public PointsSettingsStatisticsService(MiniGameDbContext context, ILogger<PointsSettingsStatisticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> GetTotalColorSettingsAsync()
        {
            try
            {
                return await _context.PetColorChangeSettings.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算寵物換色設定總數時發生錯誤");
                throw;
            }
        }

        public async Task<int> GetTotalBackgroundSettingsAsync()
        {
            try
            {
                return await _context.PetBackgroundChangeSettings.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算寵物換背景設定總數時發生錯誤");
                throw;
            }
        }

        public async Task<int> GetActiveColorSettingsAsync()
        {
            try
            {
                return await _context.PetColorChangeSettings.CountAsync(x => x.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算啟用的寵物換色設定數量時發生錯誤");
                throw;
            }
        }

        public async Task<int> GetActiveBackgroundSettingsAsync()
        {
            try
            {
                return await _context.PetBackgroundChangeSettings.CountAsync(x => x.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算啟用的寵物換背景設定數量時發生錯誤");
                throw;
            }
        }

        public async Task<int> GetTotalColorPointsAsync()
        {
            try
            {
                return await _context.PetColorChangeSettings
                    .Where(x => x.IsActive)
                    .SumAsync(x => x.RequiredPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算寵物換色設定總點數時發生錯誤");
                throw;
            }
        }

        public async Task<int> GetTotalBackgroundPointsAsync()
        {
            try
            {
                return await _context.PetBackgroundChangeSettings
                    .Where(x => x.IsActive)
                    .SumAsync(x => x.RequiredPoints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算寵物換背景設定總點數時發生錯誤");
                throw;
            }
        }

        public async Task<int> GetTotalPointsAsync()
        {
            try
            {
                var colorPoints = await GetTotalColorPointsAsync();
                var backgroundPoints = await GetTotalBackgroundPointsAsync();
                return colorPoints + backgroundPoints;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算總點數時發生錯誤");
                throw;
            }
        }
    }
}
