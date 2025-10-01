using Microsoft.AspNetCore.Mvc;
using GameSpace.Areas.MiniGame.Services;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    [Route("MiniGame/[controller]")]
    public class PetInteractionController : Controller
    {
        private readonly IPetInteractionBonusCalculationService _interactionService;
        private readonly ILogger<PetInteractionController> _logger;

        public PetInteractionController(
            IPetInteractionBonusCalculationService interactionService,
            ILogger<PetInteractionController> logger)
        {
            _interactionService = interactionService;
            _logger = logger;
        }

        [HttpPost("interact")]
        public async Task<IActionResult> InteractWithPet([FromBody] PetInteractionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _interactionService.CalculateInteractionBonusAsync(
                    request.PetId, request.InteractionType, request.UserId);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        data = new
                        {
                            expGain = result.ExpGain,
                            happinessGain = result.HappinessGain,
                            pointsCost = result.PointsCost,
                            newExperience = result.NewExperience,
                            newMood = result.NewMood
                        }
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "寵物互動時發生錯誤");
                return StatusCode(500, new { success = false, message = "系統錯誤" });
            }
        }

        [HttpGet("available/{petId}/{userId}")]
        public async Task<IActionResult> GetAvailableInteractions(int petId, int userId)
        {
            try
            {
                var interactions = await _interactionService.GetAvailableInteractionsAsync(petId, userId);
                return Ok(new { success = true, data = interactions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得可用互動選項時發生錯誤");
                return StatusCode(500, new { success = false, message = "系統錯誤" });
            }
        }

        [HttpGet("history/{petId}")]
        public async Task<IActionResult> GetInteractionHistory(int petId, int page = 1, int pageSize = 20)
        {
            try
            {
                var (items, totalCount) = await _interactionService.GetInteractionHistoryAsync(petId, page, pageSize);
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        items,
                        totalCount,
                        page,
                        pageSize,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得互動歷史時發生錯誤");
                return StatusCode(500, new { success = false, message = "系統錯誤" });
            }
        }
    }

    public class PetInteractionRequest
    {
        public int PetId { get; set; }
        public string InteractionType { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}
