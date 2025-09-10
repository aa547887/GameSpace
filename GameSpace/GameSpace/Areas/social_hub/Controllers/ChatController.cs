using GameSpace.Areas.social_hub.Models.ViewModels;
using GameSpace.Areas.social_hub.Hubs;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class ChatController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IHubContext<ChatHub> _hub;
		public ChatController(GameSpacedatabaseContext db, IHubContext<ChatHub> hub)
		{
			_db = db;
			_hub = hub;
		}

		public IActionResult Index() => View();

		// 聯絡人（先給前 50 名）
		[HttpGet]
		public async Task<IActionResult> Contacts()
		{
			int.TryParse(Request.Cookies["sh_uid"], out var me);
			var users = await _db.Users
				.AsNoTracking()
				.Where(u => u.UserId != me)
				.OrderBy(u => u.UserId)
				.Take(50)
				.Select(u => new { id = u.UserId, name = u.UserName ?? u.UserAccount ?? $"User {u.UserId}" })
				.ToListAsync();

			return Json(users);
		}

		// 1 對 1 歷史（雙向），支援 before 分頁
		// GET /social_hub/Chat/History?peerId=123&before=2025-09-05T03:00:00.000Z&take=20
		[HttpGet]
		public async Task<IActionResult> History(int peerId, string? before, int take = 20)
		{
			if (peerId <= 0) return Json(Array.Empty<MessageViewModel>());
			int.TryParse(Request.Cookies["sh_uid"], out var me);
			if (me <= 0) return Json(Array.Empty<MessageViewModel>());

			DateTime? beforeTime = null;
			if (!string.IsNullOrWhiteSpace(before) && DateTime.TryParse(before, out var b)) beforeTime = b.ToUniversalTime();

			IQueryable<ChatMessage> q = _db.ChatMessages.AsNoTracking()
				.Where(m =>
					(m.SenderId == me && m.ReceiverId == peerId) ||
					(m.SenderId == peerId && m.ReceiverId == me));

			if (beforeTime.HasValue)
				q = q.Where(m => m.SentAt < beforeTime.Value);

			var rows = await q
				.OrderByDescending(m => m.SentAt)
				.Take(Math.Clamp(take, 1, 50))
				.Select(m => new MessageViewModel
				{
					MessageId = m.MessageId,
					SenderId = m.SenderId,
					User = m.Sender.UserName ?? m.Sender.UserAccount ?? $"User {m.SenderId}",
					Content = m.ChatContent,
					Time = m.SentAt,
					IsMine = (m.SenderId == me),
					IsRead = m.IsRead
				})
				.ToListAsync();

			rows.Reverse(); // 舊->新
			return Json(rows);
		}

		// 對方→我 的未讀訊息，設為已讀（並推播讀回條）
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MarkRead(int peerId, string upToIso)
		{
			if (peerId <= 0) return Json(new { updated = 0 });
			int.TryParse(Request.Cookies["sh_uid"], out var me);
			if (me <= 0) return Json(new { updated = 0 });

			if (!DateTime.TryParse(upToIso, out var upTo))
				upTo = DateTime.UtcNow;

			var msgs = await _db.ChatMessages
				.Where(m => m.SenderId == peerId && m.ReceiverId == me && !m.IsRead && m.SentAt <= upTo)
				.ToListAsync();

			foreach (var m in msgs) m.IsRead = true;
			var updated = await _db.SaveChangesAsync();

			await _hub.Clients.Group($"u:{peerId}")
				.SendAsync("ReadReceipt", new { fromUserId = me, upToIso });

			return Json(new { updated });
		}

		[HttpGet]
		public IActionResult WhoAmI()
		{
			var id = 0;
			int.TryParse(Request.Cookies["sh_uid"], out id);
			var name = Request.Cookies["sh_uname"] ?? "訪客";
			return Json(new { id, name });
		}


	}
}
