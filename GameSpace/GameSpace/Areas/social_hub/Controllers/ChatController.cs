using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;
using GameSpace.Areas.social_hub.Models.ViewModels;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class ChatController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		public ChatController(GameSpacedatabaseContext db) => _db = db;

		// [用途] 取得目前登入者（從 cookie）
		// [回傳] (meId, isManager)
		private (int meId, bool isManager) GetIdentity()
		{
			var idStr = Request.Cookies["gs_id"] ?? Request.Cookies["sh_uid"] ?? "0";
			int.TryParse(idStr, out var id);
			var kind = (Request.Cookies["gs_kind"] ?? "user").ToLowerInvariant();
			return (id, kind == "manager");
		}

		// [URL] GET /social_hub/Chat
		// [URL] GET /social_hub/Chat?otherId={對方ID}&isManagerDm={true|false}
		// [用途] 對話列表（可帶 otherId 直接導向對話）
		[HttpGet]
		public async Task<IActionResult> Index(int? otherId = null, bool isManagerDm = false)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return Unauthorized();

			// [說明] 若帶對方ID就直接導至 With 頁
			if (otherId.HasValue && otherId.Value > 0)
				return RedirectToAction(nameof(With), new { otherId = otherId.Value, isManagerDm });

			// [說明] 列出我參與的對話（依最後訊息時間 desc）
			var convs = await _db.DmConversations
				.AsNoTracking()
				.Where(c => c.IsManagerDm == isManagerDm && (c.Party1Id == meId || c.Party2Id == meId))
				.Select(c => new ConversationListItemVM
				{
					ConversationId = c.ConversationId,
					OtherId = (c.Party1Id == meId ? c.Party2Id : c.Party1Id),
					LastMessageAt = c.LastMessageAt,
					// [說明] 計算「對方→我」的未讀數
					UnreadCount = _db.DmMessages.Count(m =>
						m.ConversationId == c.ConversationId &&
						m.ReadAt == null &&
						m.SenderIsParty1 != (meId == c.Party1Id))
				})
				.OrderByDescending(x => x.LastMessageAt)
				.ToListAsync();

			return View("Index", convs);
		}

		// [用途] 依規則找/建 1 對 1 對話（party1 < party2）
		// [穩定] 先查（FirstOrDefaultAsync），沒有就新建；若併發遇到唯一鍵違反，回頭查一次
		private async Task<DmConversation> GetOrCreateConversationAsync(int meId, int otherId, bool isManagerDm)
		{
			if (meId <= 0) throw new ArgumentOutOfRangeException(nameof(meId));
			if (otherId <= 0) throw new ArgumentOutOfRangeException(nameof(otherId));

			var p1 = Math.Min(meId, otherId);
			var p2 = Math.Max(meId, otherId);

			// [讀取] 可能不存在 → 用 FirstOrDefaultAsync（不要用 SingleAsync）
			var existing = await _db.DmConversations
				.FirstOrDefaultAsync(c => c.IsManagerDm == isManagerDm
									   && c.Party1Id == p1
									   && c.Party2Id == p2);

			if (existing != null) return existing;

			// [建立] 嘗試新增；若遇唯一鍵衝突代表有人同時建立過了 → 回頭再查一次
			var conv = new DmConversation
			{
				IsManagerDm = isManagerDm,
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
			catch (DbUpdateException)
			{
				// [併發保護] 若 DB 有唯一索引，這裡可能會噴例外；回頭把已被別人建立的那筆撈出來
				return await _db.DmConversations
					.FirstAsync(c => c.IsManagerDm == isManagerDm
								  && c.Party1Id == p1
								  && c.Party2Id == p2);
			}
		}

		private static bool AmIParty1(int meId, DmConversation conv) => meId == conv.Party1Id;

		// [URL] GET /social_hub/Chat/With?otherId={對方ID}&isManagerDm={true|false]
		// [用途] 與某人對話頁；若 otherId 缺省 → 先渲染空頁，交由前端選擇對象
		[HttpGet]
		public async Task<IActionResult> With(int? otherId = null, bool isManagerDm = false)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return Unauthorized();

			// [otherId 未帶]：先給空清單，ViewBag 設好旗標讓前端腳本正常運作
			if (!otherId.HasValue || otherId.Value <= 0)
			{
				ViewBag.ConversationId = 0;
				ViewBag.OtherId = 0;
				ViewBag.IsManagerDm = isManagerDm;
				return View("With", Enumerable.Empty<SimpleChatMessageVM>());
			}

			// [正常流程] 已指定對象 → 撈對話 + 歷史訊息
			var conv = await GetOrCreateConversationAsync(meId, otherId.Value, isManagerDm);
			ViewBag.ConversationId = conv.ConversationId;
			ViewBag.OtherId = otherId.Value;
			ViewBag.IsManagerDm = isManagerDm;

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
					Text = m.MessageText!,
					At = m.EditedAt,
					IsMine = (m.SenderIsParty1 == iAmP1),
					IsRead = m.ReadAt != null,
					ReadAt = m.ReadAt
				})
				.ToListAsync();

			return View("With", msgs);
		}

		// [URL] POST /social_hub/Chat/Send
		// [Form] otherId={對方ID}&text={訊息}&isManagerDm={true|false}
		// [用途] 送訊息（DB: DM_Messages）
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Send(int otherId, string text, bool isManagerDm = false)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return Unauthorized();
			if (otherId <= 0) return BadRequest("otherId required");

			if (string.IsNullOrWhiteSpace(text)) return BadRequest("empty");
			text = text.Trim();
			if (text.Length > 255) text = text[..255]; // [限制] nvarchar(255)

			var conv = await GetOrCreateConversationAsync(meId, otherId, isManagerDm);
			var iAmP1 = AmIParty1(meId, conv);

			var now = DateTime.UtcNow;
			var msg = new DmMessage
			{
				ConversationId = conv.ConversationId,
				SenderIsParty1 = iAmP1,
				MessageText = text,
				EditedAt = now,
				IsRead = false // [說明] 已讀以 ReadAt 判斷；此欄若存在就同步維護
			};

			_db.DmMessages.Add(msg);

			// [同步更新] 對話最後訊息時間，供列表排序
			conv.LastMessageAt = now;

			await _db.SaveChangesAsync();

			// [回傳] 前端可直接插入
			return Ok(new
			{
				messageId = msg.MessageId,
				at = msg.EditedAt,
				mine = true,
				text = msg.MessageText
			});
		}

		// [URL] POST /social_hub/Chat/MarkRead
		// [Form] otherId={對方ID}&isManagerDm={true|false}&upToIso={ISO8601 可選}
		// [用途] 將「對方發給我的未讀」標記為已讀（可選 upToIso 只標記到某時間點）
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> MarkRead(int otherId, bool isManagerDm = false, string? upToIso = null)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return Unauthorized();
			if (otherId <= 0) return BadRequest("otherId required");

			var conv = await GetOrCreateConversationAsync(meId, otherId, isManagerDm);
			var iAmP1 = AmIParty1(meId, conv);

			var q = _db.DmMessages
				.Where(m => m.ConversationId == conv.ConversationId
							&& m.ReadAt == null
							&& m.SenderIsParty1 != iAmP1); // [說明] 只標記對方的訊息

			if (!string.IsNullOrWhiteSpace(upToIso) &&
				DateTime.TryParse(upToIso, null, DateTimeStyles.RoundtripKind, out var upTo))
			{
				q = q.Where(m => m.EditedAt <= upTo);
			}

			var unread = await q.ToListAsync();
			if (unread.Count == 0) return Ok(new { changed = 0 });

			var now = DateTime.UtcNow;
			foreach (var m in unread)
			{
				m.ReadAt = now;   // [說明] 時間戳代表已讀
				m.IsRead = true;  // [說明] 若模型保留 IsRead，則同步
			}

			await _db.SaveChangesAsync();
			return Ok(new { changed = unread.Count });
		}

		// [URL] GET /social_hub/Chat/History?otherId={對方ID}&after={ISO時間}&isManagerDm={true|false}
		// [用途] 拉歷史 / 增量訊息（after 可選，若帶入則只拿該時間之後）
		[HttpGet]
		public async Task<IActionResult> History(int otherId, DateTime? after = null, bool isManagerDm = false)
		{
			var (meId, _) = GetIdentity();
			if (meId <= 0) return Unauthorized();
			if (otherId <= 0) return BadRequest("otherId required");

			var conv = await GetOrCreateConversationAsync(meId, otherId, isManagerDm);
			var iAmP1 = AmIParty1(meId, conv);

			var q = _db.DmMessages.AsNoTracking()
				.Where(m => m.ConversationId == conv.ConversationId);

			if (after is DateTime t) q = q.Where(m => m.EditedAt > t);

			var items = await q.OrderBy(m => m.EditedAt)
				.Select(m => new SimpleChatMessageVM
				{
					MessageId = m.MessageId,
					SenderId = m.SenderIsParty1 ? conv.Party1Id : conv.Party2Id,
					ReceiverId = m.SenderIsParty1 ? conv.Party2Id : conv.Party1Id,
					Text = m.MessageText!,
					At = m.EditedAt,
					IsMine = (m.SenderIsParty1 == iAmP1),
					IsRead = m.ReadAt != null,
					ReadAt = m.ReadAt
				})
				.ToListAsync();

			return Json(items);
		}
	}
}
