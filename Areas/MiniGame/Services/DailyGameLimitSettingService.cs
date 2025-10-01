using Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;

namespace Areas.MiniGame.Services
{
    /// <summary>
    /// 每日遊戲次數限制設定服務
    /// </summary>
    public class DailyGameLimitSettingService : IDailyGameLimitSettingService
    {
        private readonly ApplicationDbContext _context;

        public DailyGameLimitSettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有設定
        /// </summary>
        public async Task<IEnumerable<DailyGameLimitSetting>> GetAllAsync()
        {
            return await _context.DailyGameLimitSettings
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 根據ID取得設定
        /// </summary>
        public async Task<DailyGameLimitSetting?> GetByIdAsync(int id)
        {
            return await _context.DailyGameLimitSettings
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// 新增設定
        /// </summary>
        public async Task<DailyGameLimitSetting> CreateAsync(DailyGameLimitSettingFormViewModel model, int userId)
        {
            var setting = new DailyGameLimitSetting
            {
                SettingName = model.SettingName,
                DailyLimit = model.DailyLimit,
                IsEnabled = model.IsEnabled,
                Description = model.Description,
                CreatedBy = userId,
                UpdatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.DailyGameLimitSettings.Add(setting);
            await _context.SaveChangesAsync();

            return setting;
        }

        /// <summary>
        /// 更新設定
        /// </summary>
        public async Task<bool> UpdateAsync(int id, DailyGameLimitSettingFormViewModel model, int userId)
        {
            var setting = await _context.DailyGameLimitSettings
                .FirstOrDefaultAsync(x => x.Id == id);

            if (setting == null)
                return false;

            setting.SettingName = model.SettingName;
            setting.DailyLimit = model.DailyLimit;
            setting.IsEnabled = model.IsEnabled;
            setting.Description = model.Description;
            setting.UpdatedBy = userId;
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 刪除設定
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var setting = await _context.DailyGameLimitSettings
                .FirstOrDefaultAsync(x => x.Id == id);

            if (setting == null)
                return false;

            _context.DailyGameLimitSettings.Remove(setting);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        public async Task<bool> ToggleStatusAsync(int id)
        {
            var setting = await _context.DailyGameLimitSettings
                .FirstOrDefaultAsync(x => x.Id == id);

            if (setting == null)
                return false;

            setting.IsEnabled = !setting.IsEnabled;
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 取得啟用的設定
        /// </summary>
        public async Task<DailyGameLimitSetting?> GetActiveSettingAsync()
        {
            return await _context.DailyGameLimitSettings
                .FirstOrDefaultAsync(x => x.IsEnabled);
        }

        /// <summary>
        /// 檢查設定名稱是否重複
        /// </summary>
        public async Task<bool> IsNameDuplicateAsync(string name, int? excludeId = null)
        {
            var query = _context.DailyGameLimitSettings
                .Where(x => x.SettingName == name);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
