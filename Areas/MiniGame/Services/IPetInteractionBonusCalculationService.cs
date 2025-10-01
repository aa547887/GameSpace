using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Services
{
    public interface IPetInteractionBonusCalculationService
    {
        Task<PetInteractionResult> CalculateInteractionBonusAsync(int petId, string interactionType, int userId);
        Task<List<AvailableInteraction>> GetAvailableInteractionsAsync(int petId, int userId);
        Task<(List<PetInteractionHistory> Items, int TotalCount)> GetInteractionHistoryAsync(int petId, int page = 1, int pageSize = 20);
    }

    public class PetInteractionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ExpGain { get; set; }
        public int HappinessGain { get; set; }
        public int PointsCost { get; set; }
        public int NewExperience { get; set; }
        public int NewMood { get; set; }
    }

    public class StatusBonusResult
    {
        public int ExpGain { get; set; }
        public int HappinessGain { get; set; }
    }

    public class AvailableInteraction
    {
        public string InteractionType { get; set; } = string.Empty;
        public string InteractionName { get; set; } = string.Empty;
        public int PointsCost { get; set; }
        public int ExpGain { get; set; }
        public int HappinessGain { get; set; }
        public int CooldownMinutes { get; set; }
        public bool IsAvailable { get; set; }
        public int RemainingCooldown { get; set; }
        public string? Description { get; set; }
    }
}
