using GameSpace.Areas.MiniGame.Data;
using GameSpace.Areas.MiniGame.Models;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MiniGame.Services
{
    public class PetInteractionBonusCalculationService : IPetInteractionBonusCalculationService
    {
        private readonly MiniGameDbContext _context;
        private readonly ILogger<PetInteractionBonusCalculationService> _logger;

        public PetInteractionBonusCalculationService(MiniGameDbContext context, ILogger<PetInteractionBonusCalculationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PetInteractionResult> CalculateInteractionBonusAsync(int petId, string interactionType, int userId)
        {
            try
            {
                var pet = await _context.Pets.FindAsync(petId);
                if (pet == null)
                {
                    return new PetInteractionResult { Success = false, Message = "找不到寵物" };
                }

                var rule = await _context.PetInteractionBonusRules
                    .FirstOrDefaultAsync(r => r.InteractionType == interactionType && r.IsActive);
                
                if (rule == null)
                {
                    return new PetInteractionResult { Success = false, Message = "找不到互動規則" };
                }

                var userWallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserID == userId);
                
                if (userWallet == null || userWallet.PointBalance < rule.PointsCost)
                {
                    return new PetInteractionResult { Success = false, Message = "點數不足" };
                }

                var lastInteraction = await _context.PetInteractionHistories
                    .Where(h => h.PetID == petId && h.InteractionType == interactionType)
                    .OrderByDescending(h => h.InteractionTime)
                    .FirstOrDefaultAsync();

                if (lastInteraction != null && 
                    DateTime.UtcNow.Subtract(lastInteraction.InteractionTime).TotalMinutes < rule.CooldownMinutes)
                {
                    var remainingCooldown = rule.CooldownMinutes - (int)DateTime.UtcNow.Subtract(lastInteraction.InteractionTime).TotalMinutes;
                    return new PetInteractionResult { Success = false, Message = $"互動冷卻中，還需等待 {remainingCooldown} 分鐘" };
                }

                var bonusResult = CalculateStatusBonus(pet, rule);

                pet.Experience += bonusResult.ExpGain;
                pet.Mood = Math.Min(100, pet.Mood + bonusResult.HappinessGain);
                pet.UpdatedAt = DateTime.UtcNow;

                userWallet.PointBalance -= rule.PointsCost;
                userWallet.UpdatedAt = DateTime.UtcNow;

                var interactionHistory = new PetInteractionHistory
                {
                    PetID = petId,
                    UserID = userId,
                    InteractionType = interactionType,
                    InteractionName = rule.InteractionName,
                    PointsCost = rule.PointsCost,
                    ExpGained = bonusResult.ExpGain,
                    HappinessGained = bonusResult.HappinessGain,
                    InteractionTime = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PetInteractionHistories.Add(interactionHistory);

                var walletHistory = new WalletHistory
                {
                    UserID = userId,
                    ChangeType = "Point",
                    ChangeAmount = -rule.PointsCost,
                    Description = $"寵物互動: {rule.InteractionName}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.WalletHistories.Add(walletHistory);

                await _context.SaveChangesAsync();

                return new PetInteractionResult
                {
                    Success = true,
                    Message = "互動成功",
                    ExpGain = bonusResult.ExpGain,
                    HappinessGain = bonusResult.HappinessGain,
                    PointsCost = rule.PointsCost,
                    NewExperience = pet.Experience,
                    NewMood = pet.Mood
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算寵物互動狀態增益時發生錯誤: PetId={PetId}, InteractionType={InteractionType}", 
                    petId, interactionType);
                return new PetInteractionResult { Success = false, Message = "系統錯誤" };
            }
        }

        private StatusBonusResult CalculateStatusBonus(Pet pet, PetInteractionBonusRule rule)
        {
            var expGain = rule.ExpGain;
            var happinessGain = rule.HappinessGain;

            var levelMultiplier = 1.0 + (pet.Level * 0.1);
            expGain = (int)(expGain * levelMultiplier);
            happinessGain = (int)(happinessGain * levelMultiplier);

            var moodMultiplier = 1.0 + (pet.Mood / 100.0 * 0.5);
            expGain = (int)(expGain * moodMultiplier);

            expGain = Math.Min(expGain, 1000);
            happinessGain = Math.Min(happinessGain, 50);

            return new StatusBonusResult
            {
                ExpGain = expGain,
                HappinessGain = happinessGain
            };
        }

        public async Task<List<AvailableInteraction>> GetAvailableInteractionsAsync(int petId, int userId)
        {
            try
            {
                var userWallet = await _context.UserWallets
                    .FirstOrDefaultAsync(w => w.UserID == userId);

                if (userWallet == null)
                {
                    return new List<AvailableInteraction>();
                }

                var activeRules = await _context.PetInteractionBonusRules
                    .Where(r => r.IsActive)
                    .ToListAsync();

                var availableInteractions = new List<AvailableInteraction>();

                foreach (var rule in activeRules)
                {
                    var hasEnoughPoints = userWallet.PointBalance >= rule.PointsCost;

                    var lastInteraction = await _context.PetInteractionHistories
                        .Where(h => h.PetID == petId && h.InteractionType == rule.InteractionType)
                        .OrderByDescending(h => h.InteractionTime)
                        .FirstOrDefaultAsync();

                    var isOnCooldown = lastInteraction != null && 
                        DateTime.UtcNow.Subtract(lastInteraction.InteractionTime).TotalMinutes < rule.CooldownMinutes;

                    var remainingCooldown = 0;
                    if (isOnCooldown)
                    {
                        remainingCooldown = rule.CooldownMinutes - (int)DateTime.UtcNow.Subtract(lastInteraction.InteractionTime).TotalMinutes;
                    }

                    availableInteractions.Add(new AvailableInteraction
                    {
                        InteractionType = rule.InteractionType,
                        InteractionName = rule.InteractionName,
                        PointsCost = rule.PointsCost,
                        ExpGain = rule.ExpGain,
                        HappinessGain = rule.HappinessGain,
                        CooldownMinutes = rule.CooldownMinutes,
                        IsAvailable = hasEnoughPoints && !isOnCooldown,
                        RemainingCooldown = remainingCooldown,
                        Description = rule.Description
                    });
                }

                return availableInteractions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得可用互動選項時發生錯誤: PetId={PetId}, UserId={UserId}", petId, userId);
                return new List<AvailableInteraction>();
            }
        }

        public async Task<(List<PetInteractionHistory> Items, int TotalCount)> GetInteractionHistoryAsync(int petId, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.PetInteractionHistories
                    .Where(h => h.PetID == petId)
                    .OrderByDescending(h => h.InteractionTime);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得寵物互動歷史時發生錯誤: PetId={PetId}", petId);
                return (new List<PetInteractionHistory>(), 0);
            }
        }
    }
}
