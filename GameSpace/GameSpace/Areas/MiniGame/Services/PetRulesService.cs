using GameSpace.Areas.MiniGame.Models;
using GameSpace.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class PetRulesService : IPetRulesService
    {
        private readonly GameSpacedatabaseContext _context;

        public PetRulesService(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        // Pet 升級規則
        public async Task<IEnumerable<PetLevelUpRule>> GetAllLevelUpRulesAsync()
        {
            return await _context.PetLevelUpRules
                .OrderBy(r => r.Level)
                .ToListAsync();
        }

        public async Task<PetLevelUpRule?> GetLevelUpRuleByLevelAsync(int level)
        {
            return await _context.PetLevelUpRules
                .FirstOrDefaultAsync(r => r.Level == level);
        }

        public async Task<bool> CreateLevelUpRuleAsync(PetLevelUpRule rule)
        {
            try
            {
                rule.IsActive = true;
                _context.PetLevelUpRules.Add(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateLevelUpRuleAsync(PetLevelUpRule rule)
        {
            try
            {
                _context.PetLevelUpRules.Update(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteLevelUpRuleAsync(int ruleId)
        {
            try
            {
                var rule = await _context.PetLevelUpRules
                    .FirstOrDefaultAsync(r => r.Id == ruleId);
                if (rule == null) return false;

                _context.PetLevelUpRules.Remove(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetRequiredExpForLevelAsync(int level)
        {
            var rule = await GetLevelUpRuleByLevelAsync(level);
            if (rule != null)
            {
                return rule.RequiredExp;
            }

            // 預設公式: Level * 100 + (Level - 1) * 50
            if (level <= 1) return 0;
            return level * 100 + (level - 1) * 50;
        }

        // Pet 膚色變更規則
        public async Task<IEnumerable<PetSkinColorPointSettings>> GetAllSkinColorSettingsAsync()
        {
            return await _context.PetSkinColorPointSettings
                .OrderBy(s => s.Level)
                .ToListAsync();
        }

        public async Task<PetSkinColorPointSettings?> GetSkinColorSettingByLevelAsync(int level)
        {
            return await _context.PetSkinColorPointSettings
                .FirstOrDefaultAsync(s => s.Level == level);
        }

        public async Task<bool> UpdateSkinColorSettingAsync(PetSkinColorPointSettings setting)
        {
            try
            {
                _context.PetSkinColorPointSettings.Update(setting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetSkinColorCostForLevelAsync(int level)
        {
            var setting = await GetSkinColorSettingByLevelAsync(level);
            return setting?.PointsCost ?? 50; // 預設 50 點數
        }

        // Pet 背景變更規則
        public async Task<IEnumerable<PetBackgroundPointSettings>> GetAllBackgroundSettingsAsync()
        {
            return await _context.PetBackgroundPointSettings
                .OrderBy(s => s.Level)
                .ToListAsync();
        }

        public async Task<PetBackgroundPointSettings?> GetBackgroundSettingByLevelAsync(int level)
        {
            return await _context.PetBackgroundPointSettings
                .FirstOrDefaultAsync(s => s.Level == level);
        }

        public async Task<bool> UpdateBackgroundSettingAsync(PetBackgroundPointSettings setting)
        {
            try
            {
                _context.PetBackgroundPointSettings.Update(setting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetBackgroundCostForLevelAsync(int level)
        {
            var setting = await GetBackgroundSettingByLevelAsync(level);
            return setting?.PointsCost ?? 30; // 預設 30 點數
        }

        // Pet 互動獎勵規則
        public async Task<IEnumerable<PetInteractionBonusRules>> GetAllInteractionRulesAsync()
        {
            return await _context.PetInteractionBonusRules
                .OrderBy(r => r.InteractionType)
                .ToListAsync();
        }

        public async Task<PetInteractionBonusRules?> GetInteractionRuleByTypeAsync(string interactionType)
        {
            return await _context.PetInteractionBonusRules
                .FirstOrDefaultAsync(r => r.InteractionType == interactionType);
        }

        public async Task<bool> CreateInteractionRuleAsync(PetInteractionBonusRules rule)
        {
            try
            {
                rule.IsActive = true;
                _context.PetInteractionBonusRules.Add(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateInteractionRuleAsync(PetInteractionBonusRules rule)
        {
            try
            {
                _context.PetInteractionBonusRules.Update(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteInteractionRuleAsync(int ruleId)
        {
            try
            {
                var rule = await _context.PetInteractionBonusRules
                    .FirstOrDefaultAsync(r => r.RuleID == ruleId);
                if (rule == null) return false;

                _context.PetInteractionBonusRules.Remove(rule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleInteractionRuleAsync(int ruleId)
        {
            try
            {
                var rule = await _context.PetInteractionBonusRules
                    .FirstOrDefaultAsync(r => r.RuleID == ruleId);
                if (rule == null) return false;

                rule.IsActive = !rule.IsActive;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Pet 顏色選項管理
        public async Task<IEnumerable<PetColorOptions>> GetAllColorOptionsAsync()
        {
            return await _context.PetColorOptions
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<PetColorOptions>> GetActiveColorOptionsAsync()
        {
            return await _context.PetColorOptions
                .Where(o => o.IsActive)
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();
        }

        public async Task<bool> CreateColorOptionAsync(PetColorOptions option)
        {
            try
            {
                option.IsActive = true;
                _context.PetColorOptions.Add(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateColorOptionAsync(PetColorOptions option)
        {
            try
            {
                _context.PetColorOptions.Update(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteColorOptionAsync(int optionId)
        {
            try
            {
                var option = await _context.PetColorOptions
                    .FirstOrDefaultAsync(o => o.OptionID == optionId);
                if (option == null) return false;

                _context.PetColorOptions.Remove(option);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleColorOptionAsync(int optionId)
        {
            try
            {
                var option = await _context.PetColorOptions
                    .FirstOrDefaultAsync(o => o.OptionID == optionId);
                if (option == null) return false;

                option.IsActive = !option.IsActive;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
