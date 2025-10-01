using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物背景選項服務實作
    /// </summary>
    public class PetBackgroundOptionService : IPetBackgroundOptionService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<PetBackgroundOptionService> _logger;

        public PetBackgroundOptionService(MiniGameDbContext context, ILogger<PetBackgroundOptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PetBackgroundOption>> GetAllBackgroundOptionsAsync()
        {
            try
            {
                return await _context.PetBackgroundOptions
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.BackgroundName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \
取得所有背景選項時發生錯誤\);
                return new List<PetBackgroundOption>();
            }
        }

        public async Task<List<PetBackgroundOption>> GetActiveBackgroundOptionsAsync()
        {
            try
            {
                return await _context.PetBackgroundOptions
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.BackgroundName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \取得啟用背景選項時發生錯誤\);
                return new List<PetBackgroundOption>();
            }
        }

        public async Task<PetBackgroundOption?> GetBackgroundOptionByIdAsync(int id)
        {
            try
            {
                return await _context.PetBackgroundOptions.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \根據ID取得背景選項時發生錯誤，ID:
Id
\, id);
                return null;
            }
        }

        public async Task<bool> CreateBackgroundOptionAsync(PetBackgroundOptionViewModel model, int createdBy)
        {
            try
            {
                var backgroundOption = new PetBackgroundOption
                {
                    BackgroundName = model.BackgroundName,
                    Description = model.Description,
                    ImagePath = model.ImagePath,
                    IsActive = model.IsActive,
                    SortOrder = model.SortOrder,
                    Remarks = model.Remarks,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PetBackgroundOptions.Add(backgroundOption);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation(\成功建立背景選項，ID:
Id
\, backgroundOption.BackgroundOptionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \建立背景選項時發生錯誤\);
                return false;
            }
        }

        public async Task<bool> UpdateBackgroundOptionAsync(PetBackgroundOptionViewModel model, int updatedBy)
        {
            try
            {
                var backgroundOption = await _context.PetBackgroundOptions.FindAsync(model.BackgroundOptionId);
                if (backgroundOption == null)
                {
                    _logger.LogWarning(\找不到要更新的背景選項，ID:
Id
\, model.BackgroundOptionId);
                    return false;
                }

                backgroundOption.BackgroundName = model.BackgroundName;
                backgroundOption.Description = model.Description;
                backgroundOption.ImagePath = model.ImagePath;
                backgroundOption.IsActive = model.IsActive;
                backgroundOption.SortOrder = model.SortOrder;
                backgroundOption.Remarks = model.Remarks;
                backgroundOption.UpdatedBy = updatedBy;
                backgroundOption.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation(\成功更新背景選項，ID:
Id
\, backgroundOption.BackgroundOptionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \更新背景選項時發生錯誤，ID:
Id
\, model.BackgroundOptionId);
                return false;
            }
        }

        public async Task<bool> DeleteBackgroundOptionAsync(int id)
        {
            try
            {
                var backgroundOption = await _context.PetBackgroundOptions.FindAsync(id);
                if (backgroundOption == null)
                {
                    _logger.LogWarning(\找不到要刪除的背景選項，ID:
Id
\, id);
                    return false;
                }

                _context.PetBackgroundOptions.Remove(backgroundOption);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation(\成功刪除背景選項，ID:
Id
\, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \刪除背景選項時發生錯誤，ID:
Id
\, id);
                return false;
            }
        }

        public async Task<bool> ToggleBackgroundOptionStatusAsync(int id, int updatedBy)
        {
            try
            {
                var backgroundOption = await _context.PetBackgroundOptions.FindAsync(id);
                if (backgroundOption == null)
                {
                    _logger.LogWarning(\找不到要切換狀態的背景選項，ID:
Id
\, id);
                    return false;
                }

                backgroundOption.IsActive = !backgroundOption.IsActive;
                backgroundOption.UpdatedBy = updatedBy;
                backgroundOption.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation(\成功切換背景選項狀態，ID:
Id
，新狀態:
Status
\, id, backgroundOption.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \切換背景選項狀態時發生錯誤，ID:
Id
\, id);
                return false;
            }
        }

        public async Task<List<PetBackgroundOption>> SearchBackgroundOptionsAsync(string keyword, bool activeOnly = false)
        {
            try
            {
                var query = _context.PetBackgroundOptions.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.BackgroundName.Contains(keyword) || 
                                           (x.Description != null && x.Description.Contains(keyword)) ||
                                           (x.Remarks != null && x.Remarks.Contains(keyword)));
                }

                return await query
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.BackgroundName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \搜尋背景選項時發生錯誤，關鍵字:
Keyword
\, keyword);
                return new List<PetBackgroundOption>();
            }
        }

        public async Task<(int total, int active, int inactive)> GetBackgroundOptionStatisticsAsync()
        {
            try
            {
                var total = await _context.PetBackgroundOptions.CountAsync();
                var active = await _context.PetBackgroundOptions.CountAsync(x => x.IsActive);
                var inactive = total - active;

                return (total, active, inactive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \取得背景選項統計資料時發生錯誤\);
                return (0, 0, 0);
            }
        }
    }
}
