using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物背景選項服務介面
    /// </summary>
    public interface IPetBackgroundOptionService
    {
        /// <summary>
        /// 取得背景選項列表
        /// </summary>
        Task<PetBackgroundOptionListViewModel> GetBackgroundOptionsAsync(string? searchKeyword = null, bool? isActiveFilter = null, int page = 1, int pageSize = 10);

        /// <summary>
        /// 取得背景選項詳情
        /// </summary>
        Task<PetBackgroundOption?> GetBackgroundOptionByIdAsync(int id);

        /// <summary>
        /// 新增背景選項
        /// </summary>
        Task<bool> CreateBackgroundOptionAsync(PetBackgroundOptionFormViewModel model, int? createdBy = null);

        /// <summary>
        /// 更新背景選項
        /// </summary>
        Task<bool> UpdateBackgroundOptionAsync(int id, PetBackgroundOptionFormViewModel model, int? updatedBy = null);

        /// <summary>
        /// 刪除背景選項
        /// </summary>
        Task<bool> DeleteBackgroundOptionAsync(int id);

        /// <summary>
        /// 檢查背景選項名稱是否重複
        /// </summary>
        Task<bool> IsNameDuplicateAsync(string name, int? excludeId = null);

        /// <summary>
        /// 檢查背景選項顏色代碼是否重複
        /// </summary>
        Task<bool> IsColorCodeDuplicateAsync(string colorCode, int? excludeId = null);

        /// <summary>
        /// 取得所有啟用的背景選項
        /// </summary>
        Task<List<PetBackgroundOption>> GetActiveBackgroundOptionsAsync();
    }

    /// <summary>
    /// 寵物背景選項服務實作
    /// </summary>
    public class PetBackgroundOptionService : IPetBackgroundOptionService
    {
        private readonly MiniGameDbContext _context;

        public PetBackgroundOptionService(MiniGameDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得背景選項列表
        /// </summary>
        public async Task<PetBackgroundOptionListViewModel> GetBackgroundOptionsAsync(string? searchKeyword = null, bool? isActiveFilter = null, int page = 1, int pageSize = 10)
        {
            var query = _context.PetBackgroundOptions.AsQueryable();

            // 搜尋條件
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                query = query.Where(x => x.Name.Contains(searchKeyword) || 
                                        (x.Description != null && x.Description.Contains(searchKeyword)));
            }

            // 狀態篩選
            if (isActiveFilter.HasValue)
            {
                query = query.Where(x => x.IsActive == isActiveFilter.Value);
            }

            // 排序
            query = query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name);

            // 分頁
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var backgroundOptions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PetBackgroundOptionListViewModel
            {
                BackgroundOptions = backgroundOptions,
                SearchKeyword = searchKeyword,
                IsActiveFilter = isActiveFilter,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// 取得背景選項詳情
        /// </summary>
        public async Task<PetBackgroundOption?> GetBackgroundOptionByIdAsync(int id)
        {
            return await _context.PetBackgroundOptions.FindAsync(id);
        }

        /// <summary>
        /// 新增背景選項
        /// </summary>
        public async Task<bool> CreateBackgroundOptionAsync(PetBackgroundOptionFormViewModel model, int? createdBy = null)
        {
            try
            {
                var backgroundOption = new PetBackgroundOption
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim(),
                    BackgroundColorCode = model.BackgroundColorCode.Trim().ToUpper(),
                    IsActive = model.IsActive,
                    SortOrder = model.SortOrder,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                _context.PetBackgroundOptions.Add(backgroundOption);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 更新背景選項
        /// </summary>
        public async Task<bool> UpdateBackgroundOptionAsync(int id, PetBackgroundOptionFormViewModel model, int? updatedBy = null)
        {
            try
            {
                var backgroundOption = await _context.PetBackgroundOptions.FindAsync(id);
                if (backgroundOption == null)
                    return false;

                backgroundOption.Name = model.Name.Trim();
                backgroundOption.Description = model.Description?.Trim();
                backgroundOption.BackgroundColorCode = model.BackgroundColorCode.Trim().ToUpper();
                backgroundOption.IsActive = model.IsActive;
                backgroundOption.SortOrder = model.SortOrder;
                backgroundOption.UpdatedAt = DateTime.UtcNow;
                backgroundOption.UpdatedBy = updatedBy;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 刪除背景選項
        /// </summary>
        public async Task<bool> DeleteBackgroundOptionAsync(int id)
        {
            try
            {
                var backgroundOption = await _context.PetBackgroundOptions.FindAsync(id);
                if (backgroundOption == null)
                    return false;

                _context.PetBackgroundOptions.Remove(backgroundOption);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查背景選項名稱是否重複
        /// </summary>
        public async Task<bool> IsNameDuplicateAsync(string name, int? excludeId = null)
        {
            var query = _context.PetBackgroundOptions.Where(x => x.Name == name.Trim());
            
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// 檢查背景選項顏色代碼是否重複
        /// </summary>
        public async Task<bool> IsColorCodeDuplicateAsync(string colorCode, int? excludeId = null)
        {
            var query = _context.PetBackgroundOptions.Where(x => x.BackgroundColorCode == colorCode.Trim().ToUpper());
            
            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// 取得所有啟用的背景選項
        /// </summary>
        public async Task<List<PetBackgroundOption>> GetActiveBackgroundOptionsAsync()
        {
            return await _context.PetBackgroundOptions
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .ToListAsync();
        }
    }
}
