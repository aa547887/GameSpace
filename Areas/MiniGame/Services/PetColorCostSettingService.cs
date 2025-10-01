using Microsoft.EntityFrameworkCore;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using GameSpace.Areas.MiniGame.Data;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物換色所需點數設定服務實作
    /// </summary>
    public class PetColorCostSettingService : IPetColorCostSettingService
    {
        private readonly MiniGameDbContext _context;

        public PetColorCostSettingService(MiniGameDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PetColorCostSetting>> GetAllAsync()
        {
            return await _context.PetColorCostSettings
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.SettingName)
                .ToListAsync();
        }

        public async Task<PetColorCostSetting?> GetByIdAsync(int id)
        {
            return await _context.PetColorCostSettings.FindAsync(id);
        }

        public async Task<PetColorCostSetting> CreateAsync(PetColorCostSettingViewModel model)
        {
            var entity = new PetColorCostSetting
            {
                SettingName = model.SettingName,
                RequiredPoints = model.RequiredPoints,
                IsActive = model.IsActive,
                Description = model.Description,
                SortOrder = model.SortOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PetColorCostSettings.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<PetColorCostSetting> UpdateAsync(int id, PetColorCostSettingViewModel model)
        {
            var entity = await _context.PetColorCostSettings.FindAsync(id);
            if (entity == null)
                throw new ArgumentException("找不到指定的設定項目");

            entity.SettingName = model.SettingName;
            entity.RequiredPoints = model.RequiredPoints;
            entity.IsActive = model.IsActive;
            entity.Description = model.Description;
            entity.SortOrder = model.SortOrder;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.PetColorCostSettings.FindAsync(id);
            if (entity == null)
                return false;

            _context.PetColorCostSettings.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PetColorCostSettingListViewModel> GetListAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool? isActiveFilter = null)
        {
            var query = _context.PetColorCostSettings.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.SettingName.Contains(searchTerm) || 
                                        (x.Description != null && x.Description.Contains(searchTerm)));
            }

            if (isActiveFilter.HasValue)
            {
                query = query.Where(x => x.IsActive == isActiveFilter.Value);
            }

            var totalCount = await query.CountAsync();
            var settings = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.SettingName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PetColorCostSettingViewModel
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

            return new PetColorCostSettingListViewModel
            {
                Settings = settings,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                IsActiveFilter = isActiveFilter
            };
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PetColorCostSettings.AnyAsync(x => x.Id == id);
        }
    }
}
