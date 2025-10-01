using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class PetLevelUpRuleService : IPetLevelUpRuleService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<PetLevelUpRuleService> _logger;

        public PetLevelUpRuleService(MiniGameDbContext context, ILogger<PetLevelUpRuleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PetLevelUpRule>> GetAllLevelUpRulesAsync()
        {
            try
            {
                return await _context.PetLevelUpRules
                    .OrderBy(x => x.Level)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得所有升級規則時發生錯誤");
                return new List<PetLevelUpRule>();
            }
        }

        public async Task<List<PetLevelUpRule>> GetActiveLevelUpRulesAsync()
        {
            try
            {
                return await _context.PetLevelUpRules
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Level)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得啟用升級規則時發生錯誤");
                return new List<PetLevelUpRule>();
            }
        }

        public async Task<PetLevelUpRule?> GetLevelUpRuleByIdAsync(int id)
        {
            try
            {
                return await _context.PetLevelUpRules.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據ID取得升級規則時發生錯誤，ID: {Id}", id);
                return null;
            }
        }

        public async Task<bool> CreateLevelUpRuleAsync(PetLevelUpRuleCreateViewModel model, int createdBy)
        {
            try
            {
                // 檢查等級是否已存在
                var existingRule = await _context.PetLevelUpRules
                    .FirstOrDefaultAsync(x => x.Level == model.Level);
                
                if (existingRule != null)
                {
                    _logger.LogWarning("等級 {Level} 的升級規則已存在", model.Level);
                    return false;
                }

                var levelUpRule = new PetLevelUpRule
                {
                    Level = model.Level,
                    ExperienceRequired = model.ExperienceRequired,
                    PointsReward = model.PointsReward,
                    ExpReward = model.ExpReward,
                    IsActive = model.IsActive,
                    Remarks = model.Remarks,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PetLevelUpRules.Add(levelUpRule);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功建立升級規則，ID: {Id}", levelUpRule.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立升級規則時發生錯誤");
                return false;
            }
        }

        public async Task<bool> UpdateLevelUpRuleAsync(PetLevelUpRuleEditViewModel model, int updatedBy)
        {
            try
            {
                var levelUpRule = await _context.PetLevelUpRules.FindAsync(model.Id);
                if (levelUpRule == null)
                {
                    _logger.LogWarning("找不到ID為 {Id} 的升級規則", model.Id);
                    return false;
                }

                // 檢查等級是否已被其他規則使用
                var existingRule = await _context.PetLevelUpRules
                    .FirstOrDefaultAsync(x => x.Level == model.Level && x.Id != model.Id);
                
                if (existingRule != null)
                {
                    _logger.LogWarning("等級 {Level} 已被其他升級規則使用", model.Level);
                    return false;
                }

                levelUpRule.Level = model.Level;
                levelUpRule.ExperienceRequired = model.ExperienceRequired;
                levelUpRule.PointsReward = model.PointsReward;
                levelUpRule.ExpReward = model.ExpReward;
                levelUpRule.IsActive = model.IsActive;
                levelUpRule.Remarks = model.Remarks;
                levelUpRule.UpdatedBy = updatedBy;
                levelUpRule.UpdatedAt = DateTime.UtcNow;

                _context.PetLevelUpRules.Update(levelUpRule);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功更新升級規則，ID: {Id}", levelUpRule.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新升級規則時發生錯誤，ID: {Id}", model.Id);
                return false;
            }
        }

        public async Task<bool> DeleteLevelUpRuleAsync(int id)
        {
            try
            {
                var levelUpRule = await _context.PetLevelUpRules.FindAsync(id);
                if (levelUpRule == null)
                {
                    _logger.LogWarning("找不到ID為 {Id} 的升級規則", id);
                    return false;
                }

                _context.PetLevelUpRules.Remove(levelUpRule);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功刪除升級規則，ID: {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除升級規則時發生錯誤，ID: {Id}", id);
                return false;
            }
        }

        public async Task<bool> ToggleLevelUpRuleStatusAsync(int id)
        {
            try
            {
                var levelUpRule = await _context.PetLevelUpRules.FindAsync(id);
                if (levelUpRule == null)
                {
                    _logger.LogWarning("找不到ID為 {Id} 的升級規則", id);
                    return false;
                }

                levelUpRule.IsActive = !levelUpRule.IsActive;
                levelUpRule.UpdatedAt = DateTime.UtcNow;

                _context.PetLevelUpRules.Update(levelUpRule);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("成功切換升級規則狀態，ID: {Id}, 新狀態: {IsActive}", id, levelUpRule.IsActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切換升級規則狀態時發生錯誤，ID: {Id}", id);
                return false;
            }
        }

        public async Task<List<PetLevelUpRule>> GetLevelUpRulesByLevelRangeAsync(int minLevel, int maxLevel)
        {
            try
            {
                return await _context.PetLevelUpRules
                    .Where(x => x.Level >= minLevel && x.Level <= maxLevel)
                    .OrderBy(x => x.Level)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據等級範圍取得升級規則時發生錯誤，範圍: {MinLevel}-{MaxLevel}", minLevel, maxLevel);
                return new List<PetLevelUpRule>();
            }
        }

        public async Task<bool> ValidateLevelUpRuleAsync(PetLevelUpRuleCreateViewModel model)
        {
            try
            {
                // 檢查等級是否已存在
                var existingRule = await _context.PetLevelUpRules
                    .FirstOrDefaultAsync(x => x.Level == model.Level);
                
                if (existingRule != null)
                {
                    return false;
                }

                // 檢查經驗值是否合理
                if (model.ExperienceRequired < 0)
                {
                    return false;
                }

                // 檢查獎勵是否合理
                if (model.PointsReward < 0 || model.ExpReward < 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證升級規則時發生錯誤");
                return false;
            }
        }

        public async Task<bool> BulkUpdateLevelUpRulesAsync(List<PetLevelUpRuleEditViewModel> models, int updatedBy)
        {
            try
            {
                foreach (var model in models)
                {
                    var levelUpRule = await _context.PetLevelUpRules.FindAsync(model.Id);
                    if (levelUpRule != null)
                    {
                        levelUpRule.Level = model.Level;
                        levelUpRule.ExperienceRequired = model.ExperienceRequired;
                        levelUpRule.PointsReward = model.PointsReward;
                        levelUpRule.ExpReward = model.ExpReward;
                        levelUpRule.IsActive = model.IsActive;
                        levelUpRule.Remarks = model.Remarks;
                        levelUpRule.UpdatedBy = updatedBy;
                        levelUpRule.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("成功批量更新升級規則，數量: {Count}", models.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新升級規則時發生錯誤");
                return false;
            }
        }
    }
}
