using System;
using System.Linq;
using System.Threading.Tasks;
using GameSpace.Areas.social_hub.Hubs;
using GameSpace.Areas.social_hub.Models.ViewModels;
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

		// ====== 身分存取（統一：HttpOnly cookie gs_id/gs_kind） ======
		private (int id, string kind) GetIdentity()
		{
			var idStr = Request.Cookies["gs_id"];
			var kind = Request.Cookies["gs_kind"] ?? "user";
			int.TryParse(idStr, out var id);
			return (id, kind);
		}

		// ====== UI ======
		[HttpGet]
		public IActionResult Index() => View();

		// ====== API：聯絡人列表（排除自己） ======
		[HttpGet]
		public async Task<IActionResult> Contacts()
		{
			var (me, _) = GetIdentity();
			if (me <= 0) return Json(Array.Empty<object>());

			var users = await _db.Users
				.AsNoTracking()
				.Where(u => u.UserId != me)
				.OrderBy(u => u.UserId)
				.Take(50)
				.Select(u => new
				{
					id = u.UserId,
					name = u.UserName ?? u.UserAccount ?? $"User {u.UserId}"
				})
				.ToListAsync();

			return Json(users);
		}

		// ====== API：各對象未讀數 ======
		[HttpGet]
		public async Task<IActionResult> UnreadCounts()
		{
			var (me, _) = GetIdentity();
			if (me <= 0) return Json(Array.Empty<object>());

			var list = await _db.ChatMessages.AsNoTracking()
				.Where(m => m.ReceiverId == me && !m.IsRead)
				.GroupBy(m => m.SenderId)
				.Select(g => new { peerId = g.Key, count = g.Count(), lastTime = g.Max(m => m.SentAt) })
				.OrderByDescending(x => x.lastTime)
				.ToListAsync();

			return Json(list);
		}

		// ====== API：雙向歷史（支援 before 分頁） ======
		// GET /social_hub/Chat/History?peerId=123&before=2025-09-05T03:00:00.000Z&take=20
		[HttpGet]
		public async Task<IActionResult> History(int peerId, string? before, int take = 20)
		{
			var (me, _) = GetIdentity();
			if (peerId <= 0 || me <= 0) return Json(Array.Empty<object>());

			DateTime? beforeTime = null;
			if (!string.IsNullOrWhiteSpace(before) && DateTime.TryParse(before, out var b))
				beforeTime = b.ToUniversalTime();

			IQueryable<ChatMessage> q = _db.ChatMessages.AsNoTracking()
				.Where(m =>
					(m.SenderId == me && m.ReceiverId == peerId) ||
					(m.SenderId == peerId && m.ReceiverId == me));

			if (beforeTime.HasValue)
				q = q.Where(m => m.SentAt < beforeTime.Value);

			take = Math.Clamp(take, 1, 100);

			var list = await q
				.OrderByDescending(m => m.SentAt)
				.Take(take)
				.OrderBy(m => m.SentAt)
				.Select(m => new MessageViewModel
				{
					MessageId = m.MessageId,
					SenderId = m.SenderId,
					User = (m.SenderId == me ? "我" : "對方"),
					Content = m.ChatContent,
					Time = m.SentAt,
					IsMine = (m.SenderId == me),
					IsRead = m.IsRead
				})
				.ToListAsync();

			return Json(list);
		}

		// ====== API：將「對方→我」訊息標記已讀（含 Hub 回推 ReadReceipt） ======
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MarkRead(int peerId, string upToIso)
		{
			var (me, _) = GetIdentity();
			if (me <= 0 || peerId <= 0) return Json(new { updated = 0 });

			if (!DateTime.TryParse(upToIso, out var upTo))
				return Json(new { updated = 0 });

			upTo = upTo.ToUniversalTime();

			// 將「對方 → 我」且未讀、時間 <= upTo 的訊息設為已讀
			var updated = await _db.ChatMessages
				.Where(m => m.SenderId == peerId && m.ReceiverId == me && !m.IsRead && m.SentAt <= upTo)
				.ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));

			// 即時通知對方讀取進度
			await _hub.Clients.Group($"U_{peerId}")
				.SendAsync("ReadReceipt", new { fromUserId = me, upToIso = upTo.ToString("o") });

			return Json(new { updated });
		}

		// ====== API：回報目前身分（供前端偵測登入狀態） ======
		[HttpGet]
		public async Task<IActionResult> WhoAmI()
		{
			var (id, kind) = GetIdentity();
			string name = "訪客";

			if (id > 0)
			{
				if (string.Equals(kind, "manager", StringComparison.OrdinalIgnoreCase))
				{
					name = await _db.ManagerData
						.Where(m => m.ManagerId == id)
						.Select(m => m.ManagerName ?? m.ManagerAccount ?? $"管理員#{id}")
						.FirstOrDefaultAsync() ?? $"管理員#{id}";
				}
				else
				{
					name = await _db.Users
						.Where(u => u.UserId == id)
						.Select(u => u.UserName ?? u.UserAccount ?? $"User {id}")
						.FirstOrDefaultAsync() ?? $"User {id}";
				}
			}

			return Json(new { id, kind, name, isLoggedIn = id > 0 });
		}
	}
}
