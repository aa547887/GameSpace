using Areas.MiniGame.Models;
using Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換背景所需點數設定服務實作
    /// </summary>
    public class PetBackgroundCostSettingService : IPetBackgroundCostSettingService
    {
        private readonly GameSpaceContext _context;

        public PetBackgroundCostSettingService(GameSpaceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有寵物換背景所需點數設定
        /// </summary>
        public async Task<List<PetBackgroundCostSetting>> GetAllAsync()
        {
            return await _context.PetBackgroundCostSettings
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.SettingName)
                .ToListAsync();
        }

        /// <summary>
        /// 取得分頁的寵物換背景所需點數設定
        /// </summary>
        public async Task<PetBackgroundCostSettingViewModels.IndexViewModel> GetPagedAsync(
            int page = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool? isActive = null)
        {
            var query = _context.PetBackgroundCostSettings.AsQueryable();

            // 搜尋條件
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.SettingName.Contains(searchTerm) || 
                                        (x.Description != null && x.Description.Contains(searchTerm)));
            }

            // 啟用狀態篩選
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            // 排序
            query = query.OrderBy(x => x.SortOrder).ThenBy(x => x.SettingName);

            // 計算總數
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // 分頁
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PetBackgroundCostSettingViewModels.PetBackgroundCostSettingItem
                {
                    Id = x.Id,
                    SettingName = x.SettingName,
                    RequiredPoints = x.RequiredPoints,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return new PetBackgroundCostSettingViewModels.IndexViewModel
            {
                Items = items,
                SearchTerm = searchTerm,
                IsActiveFilter = isActive,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        /// <summary>
        /// 根據ID取得寵物換背景所需點數設定
        /// </summary>
        public async Task<PetBackgroundCostSetting?> GetByIdAsync(int id)
        {
            return await _context.PetBackgroundCostSettings.FindAsync(id);
        }

        /// <summary>
        /// 建立新的寵物換背景所需點數設定
        /// </summary>
        public async Task<bool> CreateAsync(PetBackgroundCostSettingViewModels.CreateViewModel model)
        {
            try
            {
                var setting = new PetBackgroundCostSetting
                {
                    SettingName = model.SettingName,
                    RequiredPoints = model.RequiredPoints,
                    IsActive = model.IsActive,
                    Description = model.Description,
                    SortOrder = model.SortOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PetBackgroundCostSettings.Add(setting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 更新寵物換背景所需點數設定
        /// </summary>
        public async Task<bool> UpdateAsync(PetBackgroundCostSettingViewModels.EditViewModel model)
        {
            try
            {
                var setting = await _context.PetBackgroundCostSettings.FindAsync(model.Id);
                if (setting == null) return false;

                setting.SettingName = model.SettingName;
                setting.RequiredPoints = model.RequiredPoints;
                setting.IsActive = model.IsActive;
                setting.Description = model.Description;
                setting.SortOrder = model.SortOrder;
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 刪除寵物換背景所需點數設定
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var setting = await _context.PetBackgroundCostSettings.FindAsync(id);
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

        /// <summary>
        /// 檢查設定名稱是否已存在
        /// </summary>
        public async Task<bool> SettingNameExistsAsync(string settingName, int? excludeId = null)
        {
            var query = _context.PetBackgroundCostSettings.Where(x => x.SettingName == settingName);
            
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// 取得啟用的寵物換背景所需點數設定
        /// </summary>
        public async Task<List<PetBackgroundCostSetting>> GetActiveSettingsAsync()
        {
            return await _context.PetBackgroundCostSettings
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.SettingName)
                .ToListAsync();
        }
    }
}
