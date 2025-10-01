using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物互動狀態增益規則服務
    /// </summary>
    public class PetInteractionBonusRuleService : IPetInteractionBonusRuleService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<PetInteractionBonusRuleService> _logger;
        private readonly IPetInteractionBonusRuleValidationService _validationService;
        private readonly IPetInteractionBonusRuleValidationService _validationService;

        public PetInteractionBonusRuleService(MiniGameDbContext context, ILogger<PetInteractionBonusRuleService> logger, IPetInteractionBonusRuleValidationService validationService)
        {
            _context = context;
            _logger = logger;
            _validationService = validationService;
        }

        public async Task<List<PetInteractionBonusRule>> GetAllAsync()
        {
            try
            {
                return await _context.PetInteractionBonusRules
                    .OrderBy(x => x.InteractionType)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有互動狀態增益規則時發生錯誤");
                return new List<PetInteractionBonusRule>();
            }
        }

        public async Task<List<PetInteractionBonusRule>> GetActiveAsync()
        {
            try
            {
                return await _context.PetInteractionBonusRules
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.InteractionType)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得啟用的互動狀態增益規則時發生錯誤");
                return new List<PetInteractionBonusRule>();
            }
        }

        public async Task<PetInteractionBonusRule?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.PetInteractionBonusRules
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據ID取得互動狀態增益規則時發生錯誤，ID: {Id}", id);
                return null;
            }
        }

        public async Task<PetInteractionBonusRule?> GetByInteractionTypeAsync(string interactionType)
        {
            try
            {
                return await _context.PetInteractionBonusRules
                    .FirstOrDefaultAsync(x => x.InteractionType == interactionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據互動類型取得規則時發生錯誤，類型: {InteractionType}", interactionType);
                return null;
            }
        }

        public async Task<bool> CreateAsync(PetInteractionBonusRuleCreateViewModel model)
        {
            try
            {
                // 檢查互動類型是否已存在
                var existing = await _context.PetInteractionBonusRules
                    .AnyAsync(x => x.InteractionType == model.InteractionType);
                
                if (existing)
                {
                    _logger.LogWarning("互動類型已存在: {InteractionType}", model.InteractionType);
                    return false;
                }

                var rule = new PetInteractionBonusRule
                {
                    InteractionType = model.InteractionType,
                    InteractionName = model.InteractionName,
                    PointsCost = model.PointsCost,
                    HappinessGain = model.HappinessGain,
                    ExpGain = model.ExpGain,
                    CooldownMinutes = model.CooldownMinutes,
                    IsActive = model.IsActive,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PetInteractionBonusRules.Add(rule);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功建立互動狀態增益規則: {InteractionType}", model.InteractionType);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立互動狀態增益規則時發生錯誤");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PetInteractionBonusRuleViewModel model)
        {
            try
            {
                var rule = await _context.PetInteractionBonusRules
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (rule == null)
                {
                    _logger.LogWarning("找不到指定的互動狀態增益規則，ID: {Id}", model.Id);
                    return false;
                }

                // 檢查互動類型是否已被其他規則使用
                var existing = await _context.PetInteractionBonusRules
                    .AnyAsync(x => x.InteractionType == model.InteractionType && x.Id != model.Id);
                
                if (existing)
                {
                    _logger.LogWarning("互動類型已被其他規則使用: {InteractionType}", model.InteractionType);
                    return false;
                }

                rule.InteractionType = model.InteractionType;
                rule.InteractionName = model.InteractionName;
                rule.PointsCost = model.PointsCost;
                rule.HappinessGain = model.HappinessGain;
                rule.ExpGain = model.ExpGain;
                rule.CooldownMinutes = model.CooldownMinutes;
                rule.IsActive = model.IsActive;
                rule.Description = model.Description;
                rule.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功更新互動狀態增益規則: {Id}", model.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新互動狀態增益規則時發生錯誤，ID: {Id}", model.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var rule = await _context.PetInteractionBonusRules
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (rule == null)
                {
                    _logger.LogWarning("找不到指定的互動狀態增益規則，ID: {Id}", id);
                    return false;
                }

                _context.PetInteractionBonusRules.Remove(rule);
                await _context.SaveChangesAsync();

                _logger.LogInformation("成功刪除互動狀態增益規則: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除互動狀態增益規則時發生錯誤，ID: {Id}", id);
                return false;
            }
        }

        public async Task<bool> ToggleStatusAsync(int id)
        {
            try
            {
                var rule = await _context.PetInteractionBonusRules
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (rule == null)
                {
                    _logger.LogWarning("找不到指定的互動狀態增益規則，ID: {Id}", id);
                    return false;
                }

                rule.IsActive = !rule.IsActive;
                rule.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("成功切換互動狀態增益規則狀態: {Id}, 新狀態: {IsActive}", id, rule.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換互動狀態增益規則狀態時發生錯誤，ID: {Id}", id);
                return false;
            }
        }

        public async Task<(List<PetInteractionBonusRuleListViewModel> Items, int TotalCount)> SearchAsync(PetInteractionBonusRuleSearchViewModel searchModel)
        {
            try
            {
                var query = _context.PetInteractionBonusRules.AsQueryable();

                // 套用搜尋條件
                if (!string.IsNullOrEmpty(searchModel.InteractionType))
                {
                    query = query.Where(x => x.InteractionType.Contains(searchModel.InteractionType));
                }

                if (!string.IsNullOrEmpty(searchModel.InteractionName))
                {
                    query = query.Where(x => x.InteractionName.Contains(searchModel.InteractionName));
                }

                if (searchModel.IsActive.HasValue)
                {
                    query = query.Where(x => x.IsActive == searchModel.IsActive.Value);
                }

                // 取得總數
                var totalCount = await query.CountAsync();

                // 套用排序
                query = searchModel.SortBy?.ToLower() switch
                {
                    "interactiontype" => searchModel.SortDescending ? query.OrderByDescending(x => x.InteractionType) : query.OrderBy(x => x.InteractionType),
                    "interactionname" => searchModel.SortDescending ? query.OrderByDescending(x => x.InteractionName) : query.OrderBy(x => x.InteractionName),
                    "pointscost" => searchModel.SortDescending ? query.OrderByDescending(x => x.PointsCost) : query.OrderBy(x => x.PointsCost),
                    "createdat" => searchModel.SortDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                    _ => query.OrderBy(x => x.InteractionType)
                };

                // 套用分頁
                var items = await query
                    .Skip((searchModel.Page - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .Select(x => new PetInteractionBonusRuleListViewModel
                    {
                        Id = x.Id,
                        InteractionType = x.InteractionType,
                        InteractionName = x.InteractionName,
                        PointsCost = x.PointsCost,
                        HappinessGain = x.HappinessGain,
                        ExpGain = x.ExpGain,
                        CooldownMinutes = x.CooldownMinutes,
                        IsActive = x.IsActive,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    })
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋互動狀態增益規則時發生錯誤");
                return (new List<PetInteractionBonusRuleListViewModel>(), 0);
            }
        }

        public async Task<object> GetStatisticsAsync()
        {
            try
            {
                var totalCount = await _context.PetInteractionBonusRules.CountAsync();
                var activeCount = await _context.PetInteractionBonusRules.CountAsync(x => x.IsActive);
                var inactiveCount = totalCount - activeCount;

                var avgPointsCost = await _context.PetInteractionBonusRules
                    .Where(x => x.IsActive)
                    .AverageAsync(x => (double?)x.PointsCost) ?? 0;

                var avgHappinessGain = await _context.PetInteractionBonusRules
                    .Where(x => x.IsActive)
                    .AverageAsync(x => (double?)x.HappinessGain) ?? 0;

                var avgExpGain = await _context.PetInteractionBonusRules
                    .Where(x => x.IsActive)
                    .AverageAsync(x => (double?)x.ExpGain) ?? 0;

                return new
                {
                    TotalCount = totalCount,
                    ActiveCount = activeCount,
                    InactiveCount = inactiveCount,
                    AveragePointsCost = Math.Round(avgPointsCost, 2),
                    AverageHappinessGain = Math.Round(avgHappinessGain, 2),
                    AverageExpGain = Math.Round(avgExpGain, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得互動狀態增益規則統計資料時發生錯誤");
                return new
                {
                    TotalCount = 0,
                    ActiveCount = 0,
                    InactiveCount = 0,
                    AveragePointsCost = 0,
                    AverageHappinessGain = 0,
                    AverageExpGain = 0
                };
            }
        }
    }
}



