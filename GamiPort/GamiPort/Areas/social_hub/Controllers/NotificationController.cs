using GamiPort.Infrastructure.Security;
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GamiPort.Areas.social_hub.Controllers
{
    [Area("social_hub")]
    public class NotificationController : Controller
    {
        private readonly GameSpacedatabaseContext _context;
        private readonly IAppCurrentUser _appCurrentUser;

        public NotificationController(GameSpacedatabaseContext context, IAppCurrentUser appCurrentUser)
        {
            _context = context;
            _appCurrentUser = appCurrentUser;
        }

        // GET: social_hub/Notification
        public async Task<IActionResult> Index()
        {
            // 專案說明：
            // 這個 Index action 會取得目前登入使用者的通知。
            // 1. 透過 IAppCurrentUser 取得目前登入的使用者 ID。
            // 2. 如果使用者未登入，則顯示一個錯誤訊息或導向登入頁面。
            // 3. 從資料庫中查詢與該使用者相關的通知收件人紀錄 (NotificationRecipient)。
            // 4. 透過收件人紀錄，取得完整的通知內容 (Notification)。
            // 5. 為了避免延遲載入 (lazy loading) 的問題，使用 Include 來預先載入相關的資料 (例如：寄件人、通知來源)。
            // 6. 將查詢結果依照建立時間排序，最新的通知會排在最前面。
            // 7. 最後，將整理好的通知列表傳遞到 View。

            var userId = await _appCurrentUser.GetUserIdAsync();

            if (userId == 0)
            {
                // 如果使用者未登入，可以選擇顯示一個錯誤頁面，或導向到登入頁面
                return Challenge(); // Challenge 會觸發導向登入頁面
            }

            var notifications = await _context.NotificationRecipients
                .Where(nr => nr.UserId == userId)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.SenderUser)
                .Include(nr => nr.Notification)
                    .ThenInclude(n => n.Source)
                .OrderByDescending(nr => nr.Notification.CreatedAt)
                .Select(nr => nr.Notification)
                .ToListAsync();

            return View(notifications);
        }
    }
}
