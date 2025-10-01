using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using GameSpace.Areas.MiniGame.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物互動狀態增益規則驗證服務
    /// </summary>
    public class PetInteractionBonusRuleValidationService : IPetInteractionBonusRuleValidationService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<PetInteractionBonusRuleValidationService> _logger;

        public PetInteractionBonusRuleValidationService(
            MiniGameDbContext context,
            ILogger<PetInteractionBonusRuleValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 驗證互動類型唯一性
        /// </summary>
        public async Task<ValidationResult> ValidateInteractionTypeUniqueAsync(string interactionType, int? excludeId = null)
        {
            try
            {
                var query = _context.PetInteractionBonusRules
                    .Where(x => x.InteractionType == interactionType);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.Id != excludeId.Value);
                }

                var exists = await query.AnyAsync();

                if (exists)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"互動類型「{interactionType}」已存在，請使用其他類型"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證互動類型唯一性時發生錯誤");
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "驗證過程中發生錯誤，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 驗證點數成本合理性
        /// </summary>
        public ValidationResult ValidatePointsCost(int pointsCost)
        {
            if (pointsCost < 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "所需點數不能為負數"
                };
            }

            if (pointsCost > 10000)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "所需點數不能超過10,000點"
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 驗證快樂度增益合理性
        /// </summary>
        public ValidationResult ValidateHappinessGain(int happinessGain)
        {
            if (happinessGain < 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "快樂度增益不能為負數"
                };
            }

            if (happinessGain > 100)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "快樂度增益不能超過100"
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 驗證經驗值增益合理性
        /// </summary>
        public ValidationResult ValidateExpGain(int expGain)
        {
            if (expGain < 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "經驗值增益不能為負數"
                };
            }

            if (expGain > 1000)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "經驗值增益不能超過1,000點"
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 驗證冷卻時間合理性
        /// </summary>
        public ValidationResult ValidateCooldownMinutes(int cooldownMinutes)
        {
            if (cooldownMinutes < 0)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "冷卻時間不能為負數"
                };
            }

            if (cooldownMinutes > 1440) // 24小時
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "冷卻時間不能超過24小時（1,440分鐘）"
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 驗證增益比例合理性
        /// </summary>
        public ValidationResult ValidateGainRatio(int pointsCost, int happinessGain, int expGain)
        {
            // 檢查點數成本與增益的比例是否合理
            if (pointsCost > 0)
            {
                var happinessRatio = (double)happinessGain / pointsCost;
                var expRatio = (double)expGain / pointsCost;

                if (happinessRatio > 2.0) // 快樂度增益不應超過點數成本的2倍
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "快樂度增益相對於點數成本過高，請調整數值"
                    };
                }

                if (expRatio > 5.0) // 經驗值增益不應超過點數成本的5倍
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "經驗值增益相對於點數成本過高，請調整數值"
                    };
                }
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 驗證互動類型格式
        /// </summary>
        public ValidationResult ValidateInteractionTypeFormat(string interactionType)
        {
            if (string.IsNullOrWhiteSpace(interactionType))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "互動類型不能為空"
                };
            }

            // 檢查是否包含特殊字符
            if (interactionType.Any(c => !char.IsLetterOrDigit(c) && c != '_' && c != '-'))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "互動類型只能包含字母、數字、底線和連字符"
                };
            }

            // 檢查長度
            if (interactionType.Length < 2)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "互動類型至少需要2個字符"
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 驗證互動名稱格式
        /// </summary>
        public ValidationResult ValidateInteractionNameFormat(string interactionName)
        {
            if (string.IsNullOrWhiteSpace(interactionName))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "互動名稱不能為空"
                };
            }

            // 檢查長度
            if (interactionName.Length < 2)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "互動名稱至少需要2個字符"
                };
            }

            if (interactionName.Length > 100)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "互動名稱不能超過100個字符"
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 綜合驗證建立規則
        /// </summary>
        public async Task<ValidationResult> ValidateCreateRuleAsync(PetInteractionBonusRuleCreateViewModel model)
        {
            var results = new List<ValidationResult>();

            // 驗證互動類型格式
            results.Add(ValidateInteractionTypeFormat(model.InteractionType));

            // 驗證互動類型唯一性
            results.Add(await ValidateInteractionTypeUniqueAsync(model.InteractionType));

            // 驗證互動名稱格式
            results.Add(ValidateInteractionNameFormat(model.InteractionName));

            // 驗證點數成本
            results.Add(ValidatePointsCost(model.PointsCost));

            // 驗證快樂度增益
            results.Add(ValidateHappinessGain(model.HappinessGain));

            // 驗證經驗值增益
            results.Add(ValidateExpGain(model.ExpGain));

            // 驗證冷卻時間
            results.Add(ValidateCooldownMinutes(model.CooldownMinutes));

            // 驗證增益比例
            results.Add(ValidateGainRatio(model.PointsCost, model.HappinessGain, model.ExpGain));

            var invalidResults = results.Where(r => !r.IsValid).ToList();
            if (invalidResults.Any())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = string.Join("；", invalidResults.Select(r => r.ErrorMessage))
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 綜合驗證更新規則
        /// </summary>
        public async Task<ValidationResult> ValidateUpdateRuleAsync(PetInteractionBonusRuleViewModel model)
        {
            var results = new List<ValidationResult>();

            // 驗證互動類型格式
            results.Add(ValidateInteractionTypeFormat(model.InteractionType));

            // 驗證互動類型唯一性（排除自己）
            results.Add(await ValidateInteractionTypeUniqueAsync(model.InteractionType, model.Id));

            // 驗證互動名稱格式
            results.Add(ValidateInteractionNameFormat(model.InteractionName));

            // 驗證點數成本
            results.Add(ValidatePointsCost(model.PointsCost));

            // 驗證快樂度增益
            results.Add(ValidateHappinessGain(model.HappinessGain));

            // 驗證經驗值增益
            results.Add(ValidateExpGain(model.ExpGain));

            // 驗證冷卻時間
            results.Add(ValidateCooldownMinutes(model.CooldownMinutes));

            // 驗證增益比例
            results.Add(ValidateGainRatio(model.PointsCost, model.HappinessGain, model.ExpGain));

            var invalidResults = results.Where(r => !r.IsValid).ToList();
            if (invalidResults.Any())
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = string.Join("；", invalidResults.Select(r => r.ErrorMessage))
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// 驗證規則是否可以刪除
        /// </summary>
        public async Task<ValidationResult> ValidateDeleteRuleAsync(int id)
        {
            try
            {
                // 檢查是否有相關的使用記錄
                var hasUsage = await _context.MiniGames
                    .AnyAsync(x => x.PetID != null); // 這裡可以根據實際業務邏輯調整

                if (hasUsage)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "此規則已被使用，無法刪除"
                    };
                }

                return new ValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驗證規則刪除時發生錯誤，ID: {Id}", id);
                return new ValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "驗證過程中發生錯誤，請稍後再試"
                };
            }
        }
    }

    /// <summary>
    /// 驗證結果
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}

