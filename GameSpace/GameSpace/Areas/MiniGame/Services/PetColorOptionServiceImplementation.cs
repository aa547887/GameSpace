using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物顏色選項服務實作
    /// </summary>
    public class PetColorOptionService : IPetColorOptionService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<PetColorOptionService> _logger;

        public PetColorOptionService(MiniGameDbContext context, ILogger<PetColorOptionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PetColorOption>> GetAllColorOptionsAsync()
        {
            try
            {
                return await _context.PetColorOptions
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.ColorName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \
取得所有顏色選項時發生錯誤\);
                return new List<PetColorOption>();
            }
        }

        public async Task<List<PetColorOption>> GetActiveColorOptionsAsync()
        {
            try
            {
                return await _context.PetColorOptions
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.ColorName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \取得啟用顏色選項時發生錯誤\);
                return new List<PetColorOption>();
            }
        }

        public async Task<PetColorOption?> GetColorOptionByIdAsync(int id)
        {
            try
            {
                return await _context.PetColorOptions.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \根據ID取得顏色選項時發生錯誤，ID:
Id
\, id);
                return null;
            }
        }

        public async Task<bool> CreateColorOptionAsync(PetColorOptionViewModel model, int createdBy)
        {
            try
            {
                var colorOption = new PetColorOption
                {
                    ColorName = model.ColorName,
                    ColorCode = model.ColorCode,
                    IsActive = model.IsActive,
                    SortOrder = model.SortOrder,
                    Remarks = model.Remarks,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PetColorOptions.Add(colorOption);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation(\成功建立顏色選項，ID:
Id
\, colorOption.ColorOptionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \建立顏色選項時發生錯誤\);
                return false;
            }
        }

        public async Task<bool> UpdateColorOptionAsync(PetColorOptionViewModel model, int updatedBy)
        {
            try
            {
                var colorOption = await _context.PetColorOptions.FindAsync(model.ColorOptionId);
                if (colorOption == null)
                {
                    _logger.LogWarning(\找不到要更新的顏色選項，ID:
Id
\, model.ColorOptionId);
                    return false;
                }

                colorOption.ColorName = model.ColorName;
                colorOption.ColorCode = model.ColorCode;
                colorOption.IsActive = model.IsActive;
                colorOption.SortOrder = model.SortOrder;
                colorOption.Remarks = model.Remarks;
                colorOption.UpdatedBy = updatedBy;
                colorOption.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation(\成功更新顏色選項，ID:
Id
\, colorOption.ColorOptionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \更新顏色選項時發生錯誤，ID:
Id
\, model.ColorOptionId);
                return false;
            }
        }

        public async Task<bool> DeleteColorOptionAsync(int id)
        {
            try
            {
                var colorOption = await _context.PetColorOptions.FindAsync(id);
                if (colorOption == null)
                {
                    _logger.LogWarning(\找不到要刪除的顏色選項，ID:
Id
\, id);
                    return false;
                }

                _context.PetColorOptions.Remove(colorOption);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation(\成功刪除顏色選項，ID:
Id
\, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \刪除顏色選項時發生錯誤，ID:
Id
\, id);
                return false;
            }
        }

        public async Task<bool> ToggleColorOptionStatusAsync(int id, int updatedBy)
        {
            try
            {
                var colorOption = await _context.PetColorOptions.FindAsync(id);
                if (colorOption == null)
                {
                    _logger.LogWarning(\找不到要切換狀態的顏色選項，ID:
Id
\, id);
                    return false;
                }

                colorOption.IsActive = !colorOption.IsActive;
                colorOption.UpdatedBy = updatedBy;
                colorOption.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation(\成功切換顏色選項狀態，ID:
Id
，新狀態:
Status
\, id, colorOption.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \切換顏色選項狀態時發生錯誤，ID:
Id
\, id);
                return false;
            }
        }

        public async Task<List<PetColorOption>> SearchColorOptionsAsync(string keyword, bool activeOnly = false)
        {
            try
            {
                var query = _context.PetColorOptions.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(x => x.IsActive);
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(x => x.ColorName.Contains(keyword) || 
                                           x.ColorCode.Contains(keyword) ||
                                           (x.Remarks != null && x.Remarks.Contains(keyword)));
                }

                return await query
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.ColorName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \搜尋顏色選項時發生錯誤，關鍵字:
Keyword
\, keyword);
                return new List<PetColorOption>();
            }
        }

        public async Task<(int total, int active, int inactive)> GetColorOptionStatisticsAsync()
        {
            try
            {
                var total = await _context.PetColorOptions.CountAsync();
                var active = await _context.PetColorOptions.CountAsync(x => x.IsActive);
                var inactive = total - active;

                return (total, active, inactive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, \取得顏色選項統計資料時發生錯誤\);
                return (0, 0, 0);
            }
        }
    }
}
