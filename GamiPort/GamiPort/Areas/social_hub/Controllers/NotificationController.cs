using Microsoft.AspNetCore.Mvc;
using GamiPort.Models; // For GameSpacedatabaseContext, Notification, and NotificationRecipient models
using GamiPort.Infrastructure.Security; // For IAppCurrentUser
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For ToListAsync and Include

namespace GamiPort.Areas.social_hub.Controllers
{
    [Area("social_hub")]
    public class NotificationController : Controller
    {
        private readonly GameSpacedatabaseContext _db;
        private readonly IAppCurrentUser _me;

        public NotificationController(GameSpacedatabaseContext db, IAppCurrentUser me)
        {
            _db = db;
            _me = me;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = await _me.GetUserIdAsync(); // Get current user ID

            if (currentUserId <= 0)
            {
                // User is not logged in, or ID is invalid.
                // Redirect to login or show an unauthorized message.
                return Unauthorized();
            }

            var notificationRecipients = await _db.NotificationRecipients
                .Include(nr => nr.Notification) // Include the Notification details
                .Where(nr => nr.UserId == currentUserId) // Filter by current user's ID
                .OrderByDescending(nr => nr.Notification.CreatedAt) // Order by notification creation date
                .ToListAsync();

            return View(notificationRecipients);
        }
    }
}