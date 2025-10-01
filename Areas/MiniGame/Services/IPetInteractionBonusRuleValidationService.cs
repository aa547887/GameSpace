using GameSpace.Areas.MiniGame.Models.ViewModels;

namespace GameSpace.Areas.MiniGame.Services
{
    /// <summary>
    /// 寵物互動狀態增益規則驗證服務介面
    /// </summary>
    public interface IPetInteractionBonusRuleValidationService
    {
        /// <summary>
        /// 驗證互動類型唯一性
        /// </summary>
        Task<ValidationResult> ValidateInteractionTypeUniqueAsync(string interactionType, int? excludeId = null);

        /// <summary>
        /// 驗證點數成本合理性
        /// </summary>
        ValidationResult ValidatePointsCost(int pointsCost);

        /// <summary>
        /// 驗證快樂度增益合理性
        /// </summary>
        ValidationResult ValidateHappinessGain(int happinessGain);

        /// <summary>
        /// 驗證經驗值增益合理性
        /// </summary>
        ValidationResult ValidateExpGain(int expGain);

        /// <summary>
        /// 驗證冷卻時間合理性
        /// </summary>
        ValidationResult ValidateCooldownMinutes(int cooldownMinutes);

        /// <summary>
        /// 驗證增益比例合理性
        /// </summary>
        ValidationResult ValidateGainRatio(int pointsCost, int happinessGain, int expGain);

        /// <summary>
        /// 驗證互動類型格式
        /// </summary>
        ValidationResult ValidateInteractionTypeFormat(string interactionType);

        /// <summary>
        /// 驗證互動名稱格式
        /// </summary>
        ValidationResult ValidateInteractionNameFormat(string interactionName);

        /// <summary>
        /// 綜合驗證建立規則
        /// </summary>
        Task<ValidationResult> ValidateCreateRuleAsync(PetInteractionBonusRuleCreateViewModel model);

        /// <summary>
        /// 綜合驗證更新規則
        /// </summary>
        Task<ValidationResult> ValidateUpdateRuleAsync(PetInteractionBonusRuleViewModel model);

        /// <summary>
        /// 驗證規則是否可以刪除
        /// </summary>
        Task<ValidationResult> ValidateDeleteRuleAsync(int id);
    }
}
