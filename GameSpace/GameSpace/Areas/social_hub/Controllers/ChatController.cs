using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GameSpace.Areas.social_hub.Hubs;
using GameSpace.Areas.social_hub.Models.ViewModels;
using GameSpace.Data;
using GameSpace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

// ⬇️ 顯示端遮罩服務（用別名，與 Program/ChatHub 一致）
using IMuteFilterAlias = GameSpace.Areas.social_hub.Services.IMuteFilter;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class ChatController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IHubContext<ChatHub> _hub;
		private readonly IMuteFilterAlias _mute; // ⬅️ 新增

		public ChatController(GameSpacedatabaseContext db, IHubContext<ChatHub> hub, IMuteFilterAlias mute)
		{
			_db = db;
			_hub = hub;
			_mute = mute; // ⬅️ 新增
		}

		private (int meId, bool isManager) GetIdentity()
		{
			var idStr = Request.Cookies["gs_id"] ?? Request.Cookies["sh_uid"] ?? "0";
			int.TryParse(idStr, out var id);
			var kind = (Request.Cookies["gs_kind"] ?? "user").ToLowerInvariant();
			return (id, kind == "manager");
		}

		private static bool AmIParty1(int meId, DmConversation c) => meId == c.Party1Id;
		private static string UserGroup(int userId) => $"U_{userId}";

		// ================ 入口頁：最近對話 + 聯絡人 ================
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return RedirectToAction(nameof(NotLoggedIn));

			// 最近對話（最後訊息時間 desc）
			var myConvs = await _db.DmConversations
				.AsNoTracking()
				.Where(c => !c.IsManagerDm && (c.Party1Id == meId || c.Party2Id == meId))
				.Select(c => new
				{
					c.ConversationId,
					OtherId = (c.Party1Id == meId ? c.Party2Id : c.Party1Id),
					c.LastMessageAt,
					LastText = _db.DmMessages
						.Where(m => m.ConversationId == c.ConversationId)
						.OrderByDescending(m => m.EditedAt)
						.Select(m => m.MessageText)
						.FirstOrDefault(),
					Unread = _db.DmMessages.Count(m =>
						m.ConversationId == c.ConversationId &&
						!m.IsRead &&
						m.SenderIsParty1 != (meId == c.Party1Id))
				})
				.OrderByDescending(x => x.LastMessageAt)
				.ToListAsync();

			// 聯絡人（好友=Relation ACCEPTED）
			var acceptedId = await _db.RelationStatuses
				.AsNoTracking()
				.Where(s => s.StatusCode == "ACCEPTED")
				.Select(s => s.StatusId)
				.FirstOrDefaultAsync();

			var contacts = new List<ContactItemVM>();
			if (acceptedId != 0)
			{
				var friends = await (
					from r in _db.Relations.AsNoTracking()
					where r.StatusId == acceptedId && (r.UserIdSmall == meId || r.UserIdLarge == meId)
					let fid = (r.UserIdSmall == meId ? r.UserIdLarge : r.UserIdSmall)
					join u in _db.Users.AsNoTracking() on fid equals u.UserId
					from conv in _db.DmConversations.AsNoTracking()
						.Where(c => !c.IsManagerDm &&
							((c.Party1Id == meId && c.Party2Id == fid) ||
							 (c.Party1Id == fid && c.Party2Id == meId)))
						.DefaultIfEmpty()
					select new
					{
						fid,
						u.UserName,
						r.FriendNickname,
						ConvId = (int?)conv.ConversationId,
						ConvParty1Id = (int?)conv.Party1Id,
						LastAt = (DateTime?)conv.LastMessageAt
					}
				).ToListAsync();

				foreach (var f in friends)
				{
					int unread = 0;
					if (f.ConvId.HasValue && f.ConvParty1Id.HasValue)
					{
						var iAmP1 = (f.ConvParty1Id.Value == meId);
						unread = await _db.DmMessages
							.AsNoTracking()
							.Where(m => m.ConversationId == f.ConvId.Value && !m.IsRead && m.SenderIsParty1 != iAmP1)
							.CountAsync();
					}

					contacts.Add(new ContactItemVM
					{
						Id = f.fid,
						Name = f.UserName ?? $"User {f.fid}",
						Nick = f.FriendNickname,
						Unread = unread,
						LastAt = f.LastAt
					});
				}

				contacts = contacts
					.OrderByDescending(x => x.LastAt ?? DateTime.MinValue)
					.ThenBy(x => x.Id)
					.ToList();
			}

			var vm = new ChatHomeVM
			{
				MeId = meId,
				Conversations = myConvs.Select(x => new ConversationListItemVM
				{
					ConversationId = x.ConversationId,
					OtherId = x.OtherId,
					LastMessageAt = x.LastMessageAt,
					// ⬇️ 入口頁預覽只顯示遮罩後的文字
					LastPreview = _mute.Apply(x.LastText ?? string.Empty),
					UnreadCount = x.Unread
				}).ToList(),
				Contacts = contacts
			};

			return View("Index", vm);
		}

		public IActionResult NotLoggedIn() => View();

		// ================ 聯絡人（給左欄） ================
		[HttpGet]
		public async Task<IActionResult> Contacts()
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return Unauthorized();

			var acceptedId = await _db.RelationStatuses
				.AsNoTracking()
				.Where(s => s.StatusCode == "ACCEPTED")
				.Select(s => s.StatusId)
				.FirstOrDefaultAsync();

			if (acceptedId == 0) return Json(Array.Empty<ContactItemVM>());

			var friends = await (
				from r in _db.Relations.AsNoTracking()
				where r.StatusId == acceptedId && (r.UserIdSmall == meId || r.UserIdLarge == meId)
				let fid = (r.UserIdSmall == meId ? r.UserIdLarge : r.UserIdSmall)
				join u in _db.Users.AsNoTracking() on fid equals u.UserId
				from conv in _db.DmConversations.AsNoTracking()
					.Where(c => !c.IsManagerDm &&
						((c.Party1Id == meId && c.Party2Id == fid) ||
						 (c.Party1Id == fid && c.Party2Id == meId)))
					.DefaultIfEmpty()
				select new
				{
					fid,
					u.UserName,
					r.FriendNickname,
					ConvId = (int?)conv.ConversationId,
					ConvParty1Id = (int?)conv.Party1Id,
					LastAt = (DateTime?)conv.LastMessageAt
				}
			).ToListAsync();

			var result = new List<ContactItemVM>(friends.Count);
			foreach (var f in friends)
			{
				int unread = 0;
				if (f.ConvId.HasValue && f.ConvParty1Id.HasValue)
				{
					var iAmP1 = (f.ConvParty1Id.Value == meId);
					unread = await _db.DmMessages
						.AsNoTracking()
						.Where(m => m.ConversationId == f.ConvId.Value && !m.IsRead && m.SenderIsParty1 != iAmP1)
						.CountAsync();
				}

				result.Add(new ContactItemVM
				{
					Id = f.fid,
					Name = f.UserName ?? $"User {f.fid}",
					Nick = f.FriendNickname,
					Unread = unread,
					LastAt = f.LastAt
				});
			}

			return Json(result
				.OrderByDescending(x => x.LastAt ?? DateTime.MinValue)
				.ThenBy(x => x.Id));
		}

		// ================ 對話頁（伺服器端強制已讀 + 撈訊息） ================
		[HttpGet]
		public async Task<IActionResult> With(int otherId)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return RedirectToAction(nameof(NotLoggedIn));
			if (otherId <= 0) return RedirectToAction(nameof(Index));

			var conv = await GetOrCreateConversationAsync(meId, otherId);

			// ★ 伺服器端強制已讀（不用等前端）
			var (changed, bound) = await MarkConversationReadAsync(meId, otherId, upTo: null);
			if (changed > 0)
			{
				await PushUnreadUpdate(meId, otherId);
				await _hub.Clients.Group(UserGroup(otherId))
					.SendAsync("ReadReceipt", new { fromUserId = meId, upToIso = (bound ?? DateTime.UtcNow).ToString("o") });
			}

			var iAmP1 = AmIParty1(meId, conv);

			var msgs = await _db.DmMessages
				.AsNoTracking()
				.Where(m => m.ConversationId == conv.ConversationId)
				.OrderBy(m => m.EditedAt)
				.Select(m => new SimpleChatMessageVM
				{
					MessageId = m.MessageId,
					SenderId = m.SenderIsParty1 ? conv.Party1Id : conv.Party2Id,
					ReceiverId = m.SenderIsParty1 ? conv.Party2Id : conv.Party1Id,
					Text = m.MessageText!, // 先取原文，下面統一遮罩
					At = m.EditedAt,
					IsMine = (m.SenderIsParty1 == iAmP1),
					IsRead = m.IsRead,
					ReadAt = m.ReadAt
				})
				.ToListAsync();

			// ⬇️ 顯示前逐筆遮罩（不改 DB）
			foreach (var m in msgs)
				m.Text = _mute.Apply(m.Text ?? string.Empty);

			ViewBag.ConversationId = conv.ConversationId;
			ViewBag.OtherId = otherId;
			return View("With", msgs);
		}

		// ================ 歷史 / 增量 ================
		[HttpGet]
		public async Task<IActionResult> History(int otherId, DateTime? after = null)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return Unauthorized();
			if (otherId <= 0) return BadRequest("otherId required");

			var conv = await GetOrCreateConversationAsync(meId, otherId);
			var iAmP1 = AmIParty1(meId, conv);

			var q = _db.DmMessages.AsNoTracking()
				.Where(m => m.ConversationId == conv.ConversationId);
			if (after is DateTime cut) q = q.Where(m => m.EditedAt > cut);

			var items = await q.OrderBy(m => m.EditedAt)
				.Select(m => new SimpleChatMessageVM
				{
					MessageId = m.MessageId,
					SenderId = m.SenderIsParty1 ? conv.Party1Id : conv.Party2Id,
					ReceiverId = m.SenderIsParty1 ? conv.Party2Id : conv.Party1Id,
					Text = m.MessageText!, // 先取原文
					At = m.EditedAt,
					IsMine = (m.SenderIsParty1 == iAmP1),
					IsRead = m.IsRead,
					ReadAt = m.ReadAt
				})
				.ToListAsync();

			// ⬇️ 回傳前遮罩
			foreach (var it in items)
				it.Text = _mute.Apply(it.Text ?? string.Empty);

			return Json(items);
		}

		// ================ 送訊息（HTTP 後備） ================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Send(int otherId, string text)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return Unauthorized();
			if (otherId <= 0) return BadRequest("otherId required");
			if (string.IsNullOrWhiteSpace(text)) return BadRequest("empty");

			text = text.Trim();
			if (text.Length > 255) text = text[..255];

			var conv = await GetOrCreateConversationAsync(meId, otherId);
			var iAmP1 = AmIParty1(meId, conv);

			var now = DateTime.UtcNow;
			var msg = new DmMessage
			{
				ConversationId = conv.ConversationId,
				SenderIsParty1 = iAmP1,
				MessageText = text, // ✅ DB 存原文
				IsRead = false,
				EditedAt = now
			};

			_db.DmMessages.Add(msg);
			await _db.SaveChangesAsync(); // Trigger 會更新 last_message_at

			// ✅ HTTP 回傳給前端的內容用遮罩後文字
			var display = _mute.Apply(msg.MessageText ?? string.Empty);
			return Ok(new { messageId = msg.MessageId, at = msg.EditedAt, mine = true, text = display });
		}

		// ================ 標記已讀（HTTP API，配合前端 / 視窗回前景） ================
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MarkRead(int otherId, string? upToIso = null)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return Unauthorized();
			if (otherId <= 0) return BadRequest("otherId required");

			DateTime? upTo = null;
			if (!string.IsNullOrWhiteSpace(upToIso) &&
				DateTime.TryParse(upToIso, null, DateTimeStyles.RoundtripKind, out var t))
			{
				upTo = t;
			}

			var (changed, bound) = await MarkConversationReadAsync(meId, otherId, upTo);
			if (changed == 0)
			{
				await PushUnreadUpdate(meId, otherId);
				return Ok(new { changed = 0 });
			}

			await PushUnreadUpdate(meId, otherId);
			await _hub.Clients.Group(UserGroup(otherId))
				.SendAsync("ReadReceipt", new { fromUserId = meId, upToIso = (bound ?? DateTime.UtcNow).ToString("o") });

			return Ok(new { changed });
		}

		// ================ 未讀摘要（左上角總數） ================
		[HttpGet]
		public async Task<IActionResult> UnreadSummary()
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return Unauthorized();

			var myConvs = await _db.DmConversations
				.AsNoTracking()
				.Where(c => !c.IsManagerDm && (c.Party1Id == meId || c.Party2Id == meId))
				.Select(c => new { c.ConversationId, IAmP1 = (c.Party1Id == meId) })
				.ToListAsync();

			if (myConvs.Count == 0) return Json(new { total = 0, items = Array.Empty<object>() });

			var items = new List<object>(myConvs.Count);
			int total = 0;

			foreach (var meta in myConvs)
			{
				var count = await _db.DmMessages
					.AsNoTracking()
					.Where(m => m.ConversationId == meta.ConversationId && !m.IsRead && m.SenderIsParty1 != meta.IAmP1)
					.CountAsync();

				total += count;
				items.Add(new { conversationId = meta.ConversationId, unread = count });
			}

			return Json(new { total, items });
		}

		// ===== internal =====
		private async Task<DmConversation> GetOrCreateConversationAsync(int meId, int otherId)
		{
			var existing = await _db.DmConversations
				.FirstOrDefaultAsync(c => !c.IsManagerDm &&
					((c.Party1Id == meId && c.Party2Id == otherId) ||
					 (c.Party1Id == otherId && c.Party2Id == meId)));
			if (existing != null) return existing;

			var p1 = Math.Min(meId, otherId);
			var p2 = Math.Max(meId, otherId);

			var conv = new DmConversation
			{
				IsManagerDm = false,
				Party1Id = p1,
				Party2Id = p2,
				CreatedAt = DateTime.UtcNow
			};
			_db.DmConversations.Add(conv);
			try
			{
				await _db.SaveChangesAsync();
				return conv;
			}
			catch
			{
				return await _db.DmConversations.FirstAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2);
			}
		}

		/// <summary>將「對方→我」的未讀改已讀（同時維護 IsRead/ReadAt），回傳 (changed, upToBound)</summary>
		private async Task<(int changed, DateTime? upTo)> MarkConversationReadAsync(int meId, int otherId, DateTime? upTo = null)
		{
			var conv = await GetOrCreateConversationAsync(meId, otherId);
			var iAmP1 = (meId == conv.Party1Id);

			var q = _db.DmMessages
				.Where(m => m.ConversationId == conv.ConversationId
							&& !m.IsRead
							&& m.SenderIsParty1 != iAmP1);

			if (upTo.HasValue) q = q.Where(m => m.EditedAt <= upTo.Value);

			var list = await q.ToListAsync();
			if (list.Count == 0) return (0, upTo);

			var now = DateTime.UtcNow;
			foreach (var m in list)
			{
				m.IsRead = true;
				m.ReadAt = now; // 觸發器也會保險補齊
			}
			await _db.SaveChangesAsync();

			var bound = upTo ?? list.Max(x => x.EditedAt);
			return (list.Count, bound);
		}

		private async Task PushUnreadUpdate(int meId, int otherId)
		{
			// 針對此會話的未讀
			var conv = await _db.DmConversations
				.FirstOrDefaultAsync(c => !c.IsManagerDm &&
					((c.Party1Id == meId && c.Party2Id == otherId) ||
					 (c.Party1Id == otherId && c.Party2Id == meId)));

			var peerUnread = 0;
			if (conv != null)
			{
				var iAmP1 = (conv.Party1Id == meId);
				peerUnread = await _db.DmMessages
					.Where(m => m.ConversationId == conv.ConversationId && !m.IsRead && m.SenderIsParty1 != iAmP1)
					.CountAsync();
			}

			// 總未讀
			var total = await _db.DmMessages
				.Where(m => !m.IsRead &&
					!m.Conversation.IsManagerDm &&
					(
						(m.Conversation.Party1Id == meId && !m.SenderIsParty1) ||
						(m.Conversation.Party2Id == meId && m.SenderIsParty1)
					))
				.CountAsync();

			await _hub.Clients.Group(UserGroup(meId))
				.SendAsync("UnreadUpdate", new { total, peerId = otherId, unread = peerUnread });
		}
	}
}
