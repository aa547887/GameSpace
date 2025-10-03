using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換色點數設定服務實作
    /// </summary>
    public class PetColorChangeSettingsService : IPetColorChangeSettingsService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetColorChangeSettingsService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有寵物換色點數設定
        /// </summary>
        /// <returns>寵物換色點數設定清單</returns>
        public async Task<IEnumerable<PetColorChangeSettings>> GetAllSettingsAsync()
        {
            return await _context.PetColorChangeSettings
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 根據ID取得寵物換色點數設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>寵物換色點數設定</returns>
        public async Task<PetColorChangeSettings?> GetSettingByIdAsync(int id)
        {
            return await _context.PetColorChangeSettings
                .FirstOrDefaultAsync(s => s.SettingId == id);
        }

        /// <summary>
        /// 新增寵物換色點數設定
        /// </summary>
        /// <param name="setting">寵物換色點數設定</param>
        /// <returns>新增的設定</returns>
        public async Task<PetColorChangeSettings> CreateSettingAsync(PetColorChangeSettings setting)
        {
            setting.CreatedAt = DateTime.UtcNow;
            _context.PetColorChangeSettings.Add(setting);
            await _context.SaveChangesAsync();
            return setting;
        }

        /// <summary>
        /// 更新寵物換色點數設定
        /// </summary>
        /// <param name="setting">寵物換色點數設定</param>
        /// <returns>更新後的設定</returns>
        public async Task<PetColorChangeSettings> UpdateSettingAsync(PetColorChangeSettings setting)
        {
            setting.UpdatedAt = DateTime.UtcNow;
            _context.PetColorChangeSettings.Update(setting);
            await _context.SaveChangesAsync();
            return setting;
        }

        /// <summary>
        /// 刪除寵物換色點數設定
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否刪除成功</returns>
        public async Task<bool> DeleteSettingAsync(int id)
        {
            var setting = await _context.PetColorChangeSettings
                .FirstOrDefaultAsync(s => s.SettingId == id);
            
            if (setting == null)
                return false;

            _context.PetColorChangeSettings.Remove(setting);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 取得目前啟用的換色點數設定
        /// </summary>
        /// <returns>啟用的換色點數設定</returns>
        public async Task<PetColorChangeSettings?> GetActiveSettingAsync()
        {
            return await _context.PetColorChangeSettings
                .FirstOrDefaultAsync(s => s.IsActive);
        }

        /// <summary>
        /// 切換設定的啟用狀態
        /// </summary>
        /// <param name="id">設定ID</param>
        /// <returns>是否切換成功</returns>
        public async Task<bool> ToggleActiveAsync(int id)
        {
            var setting = await _context.PetColorChangeSettings
                .FirstOrDefaultAsync(s => s.SettingId == id);

            if (setting == null)
                return false;

            setting.IsActive = !setting.IsActive;
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}

