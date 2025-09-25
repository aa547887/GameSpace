using Microsoft.AspNetCore.Mvc;
using GameSpace.Models;
using GameSpace.Areas.MiniGame.Models;

namespace GameSpace.Areas.MiniGame.Controllers
{
    [Area("MiniGame")]
    public class MiniGameBaseController : Controller
    {
        protected readonly GameSpacedatabaseContext _context;

        public MiniGameBaseController(GameSpacedatabaseContext context)
        {
            _context = context;
        }

        protected int GetCurrentUserId()
        {
            return 1;
        }

        protected GameSpace.Models.User GetCurrentUser()
        {
            return new GameSpace.Models.User
            {
                UserId = GetCurrentUserId(),
                UserName = "測試用戶"
            };
        }

        protected UserWallet GetUserWallet(int userId)
        {
            return new UserWallet
            {
                UserId = userId,
                UserPoint = 1000
            };
        }

        protected GameSpace.Models.Pet GetUserPet(int userId)
        {
            return new GameSpace.Models.Pet
            {
                UserId = userId,
                PetName = "測試寵物",
                Level = 1
            };
        }

        protected bool CanPlayGame(int userId)
        {
            return true;
        }

        protected void UpdateUserPoints(int userId, int points)
        {
        }

        protected void UpdatePetExperience(int petId, int exp)
        {
        }

        protected void LogGamePlay(int userId, int miniGameId, bool isWin, int pointsGained, int expGained)
        {
        }

        protected bool IsAdmin()
        {
            return false;
        }

        protected IActionResult Return404()
        {
            return NotFound();
        }
    }
}
