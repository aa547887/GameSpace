using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物顏色選項服務介面
    /// </summary>
    public interface IPetColorOptionService
    {
        /// <summary>
        /// 取得顏色選項列表
        /// </summary>
        Task<PetColorOptionListViewModel> GetColorOptionsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? isActiveFilter = null);

        /// <summary>
        /// 根據ID取得顏色選項
        /// </summary>
        Task<PetColorOption?> GetColorOptionByIdAsync(int id);

        /// <summary>
        /// 建立顏色選項
        /// </summary>
        Task<bool> CreateColorOptionAsync(PetColorOptionViewModel model);

        /// <summary>
        /// 更新顏色選項
        /// </summary>
        Task<bool> UpdateColorOptionAsync(int id, PetColorOptionViewModel model);

        /// <summary>
        /// 刪除顏色選項
        /// </summary>
        Task<bool> DeleteColorOptionAsync(int id);

        /// <summary>
        /// 檢查顏色代碼是否重複
        /// </summary>
        Task<bool> IsColorCodeExistsAsync(string colorCode, int? excludeId = null);

        /// <summary>
        /// 檢查顏色名稱是否重複
        /// </summary>
        Task<bool> IsColorNameExistsAsync(string colorName, int? excludeId = null);
    }

    /// <summary>
    /// 寵物顏色選項服務實作
    /// </summary>
    public class PetColorOptionService : IPetColorOptionService
    {
        private readonly List<PetColorOption> _colorOptions;
        private readonly ILogger<PetColorOptionService> _logger;

        public PetColorOptionService(ILogger<PetColorOptionService> logger)
        {
            _logger = logger;
            _colorOptions = InitializeColorOptions();
        }

        /// <summary>
        /// 初始化顏色選項資料
        /// </summary>
        private List<PetColorOption> InitializeColorOptions()
        {
            return new List<PetColorOption>
            {
                new PetColorOption
                {
                    Id = 1,
                    ColorName = "經典紅",
                    ColorCode = "#FF0000",
                    Description = "經典的紅色，充滿活力",
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new PetColorOption
                {
                    Id = 2,
                    ColorName = "天空藍",
                    ColorCode = "#87CEEB",
                    Description = "清新的天空藍色",
                    IsActive = true,
                    SortOrder = 2,
                    CreatedAt = DateTime.UtcNow.AddDays(-29)
                },
                new PetColorOption
                {
                    Id = 3,
                    ColorName = "森林綠",
                    ColorCode = "#228B22",
                    Description = "自然的森林綠色",
                    IsActive = true,
                    SortOrder = 3,
                    CreatedAt = DateTime.UtcNow.AddDays(-28)
                },
                new PetColorOption
                {
                    Id = 4,
                    ColorName = "陽光黃",
                    ColorCode = "#FFD700",
                    Description = "溫暖的陽光黃色",
                    IsActive = true,
                    SortOrder = 4,
                    CreatedAt = DateTime.UtcNow.AddDays(-27)
                },
                new PetColorOption
                {
                    Id = 5,
                    ColorName = "神秘紫",
                    ColorCode = "#8B008B",
                    Description = "神秘優雅的紫色",
                    IsActive = true,
                    SortOrder = 5,
                    CreatedAt = DateTime.UtcNow.AddDays(-26)
                }
            };
        }

        public async Task<PetColorOptionListViewModel> GetColorOptionsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? isActiveFilter = null)
        {
            try
            {
                var query = _colorOptions.AsQueryable();

                // 搜尋條件
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(x => x.ColorName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                           (x.Description != null && x.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
                }

                // 啟用狀態篩選
                if (!string.IsNullOrWhiteSpace(isActiveFilter))
                {
                    var isActive = bool.Parse(isActiveFilter);
                    query = query.Where(x => x.IsActive == isActive);
                }

                // 排序
                query = query.OrderBy(x => x.SortOrder).ThenBy(x => x.ColorName);

                var totalCount = query.Count();
                var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                return new PetColorOptionListViewModel
                {
                    ColorOptions = items,
                    SearchTerm = searchTerm,
                    IsActiveFilter = isActiveFilter,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得顏色選項列表時發生錯誤");
                throw;
            }
        }

        public async Task<PetColorOption?> GetColorOptionByIdAsync(int id)
        {
            try
            {
                return _colorOptions.FirstOrDefault(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得顏色選項時發生錯誤，ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> CreateColorOptionAsync(PetColorOptionViewModel model)
        {
            try
            {
                // 檢查顏色代碼是否重複
                if (await IsColorCodeExistsAsync(model.ColorCode))
                {
                    return false;
                }

                // 檢查顏色名稱是否重複
                if (await IsColorNameExistsAsync(model.ColorName))
                {
                    return false;
                }

                var newId = _colorOptions.Count > 0 ? _colorOptions.Max(x => x.Id) + 1 : 1;
                var colorOption = new PetColorOption
                {
                    Id = newId,
                    ColorName = model.ColorName,
                    ColorCode = model.ColorCode,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    SortOrder = model.SortOrder,
                    CreatedAt = DateTime.UtcNow
                };

                _colorOptions.Add(colorOption);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立顏色選項時發生錯誤");
                throw;
            }
        }

        public async Task<bool> UpdateColorOptionAsync(int id, PetColorOptionViewModel model)
        {
            try
            {
                var existingOption = _colorOptions.FirstOrDefault(x => x.Id == id);
                if (existingOption == null)
                {
                    return false;
                }

                // 檢查顏色代碼是否重複（排除自己）
                if (await IsColorCodeExistsAsync(model.ColorCode, id))
                {
                    return false;
                }

                // 檢查顏色名稱是否重複（排除自己）
                if (await IsColorNameExistsAsync(model.ColorName, id))
                {
                    return false;
                }

                existingOption.ColorName = model.ColorName;
                existingOption.ColorCode = model.ColorCode;
                existingOption.Description = model.Description;
                existingOption.IsActive = model.IsActive;
                existingOption.SortOrder = model.SortOrder;
                existingOption.UpdatedAt = DateTime.UtcNow;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新顏色選項時發生錯誤，ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteColorOptionAsync(int id)
        {
            try
            {
                var option = _colorOptions.FirstOrDefault(x => x.Id == id);
                if (option == null)
                {
                    return false;
                }

                _colorOptions.Remove(option);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除顏色選項時發生錯誤，ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> IsColorCodeExistsAsync(string colorCode, int? excludeId = null)
        {
            try
            {
                return _colorOptions.Any(x => x.ColorCode.Equals(colorCode, StringComparison.OrdinalIgnoreCase) && 
                                            (excludeId == null || x.Id != excludeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查顏色代碼重複時發生錯誤");
                throw;
            }
        }

        public async Task<bool> IsColorNameExistsAsync(string colorName, int? excludeId = null)
        {
            try
            {
                return _colorOptions.Any(x => x.ColorName.Equals(colorName, StringComparison.OrdinalIgnoreCase) && 
                                            (excludeId == null || x.Id != excludeId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查顏色名稱重複時發生錯誤");
                throw;
            }
        }
    }
}
