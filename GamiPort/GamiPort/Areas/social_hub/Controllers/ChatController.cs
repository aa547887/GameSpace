using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GamiPort.Areas.social_hub.Services.Abstractions; // ★ IProfanityFilter（輸出前遮蔽）
using GamiPort.Infrastructure.Security;                 // IAppCurrentUser（吃登入）
using GamiPort.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.social_hub.Controllers
{
	/// <summary>
	/// 一對一聊天：歷史查詢 / 好友最新預覽 + 未讀
	/// 注意：DB 仍存原文；本 Controller 在輸出給前端前會透過 IProfanityFilter 進行遮蔽。
	/// </summary>
	[Area("social_hub")]
	[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
	public sealed class ChatController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly IAppCurrentUser _me;
		private readonly IProfanityFilter _profanity; // ★ 新增：輸出前的最後一道遮蔽

		public ChatController(GameSpacedatabaseContext db, IAppCurrentUser me, IProfanityFilter profanity)
		{
			_db = db;
			_me = me;
			_profanity = profanity;
		}

		private async Task<int> GetMeAsync()
		{
			var uid = _me.UserId;
			return uid > 0 ? uid : await _me.GetUserIdAsync();
		}

		private Task<bool> ExistsAsync(int userId) =>
			_db.Users.AsNoTracking().AnyAsync(u => u.UserId == userId);

		private static string UtcIso(DateTime dt)
		{
			if (dt.Kind == DateTimeKind.Unspecified)
				dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
			if (dt.Kind == DateTimeKind.Local)
				dt = dt.ToUniversalTime();
			return dt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
		}

		// ==========================================================
		// 1) 歷史（分頁）
		//    GET /social_hub/Chat/History?otherId=1002&take=20
		//    beforeIso：抓更舊（結果「舊→新」）
		//    direction=latest：第一次載入最新 N 筆（結果「舊→新」）
		//    afterIso：抓更新（結果「舊→新」）
		//    ★ 本法輸出前一律 _profanity.Censor(...)
		// ==========================================================
		[HttpGet]
		public async Task<IActionResult> History(
			int otherId,
			string? afterIso,
			string? beforeIso,
			int take = 20,
			string? direction = null)
		{
			var me = await GetMeAsync();
			if (me <= 0) return Unauthorized();
			if (otherId <= 0 || otherId == me) return BadRequest();

			if (!await ExistsAsync(me) || !await ExistsAsync(otherId)) return NotFound();

			// 解析時間
			DateTime? after = null, before = null;
			if (!string.IsNullOrWhiteSpace(afterIso) &&
				DateTime.TryParse(afterIso, null, DateTimeStyles.RoundtripKind, out var a)) after = a;
			if (!string.IsNullOrWhiteSpace(beforeIso) &&
				DateTime.TryParse(beforeIso, null, DateTimeStyles.RoundtripKind, out var b)) before = b;

			take = Math.Clamp(take, 1, 100);

			// 找會話（僅 me/other 之間）
			var p1 = Math.Min(me, otherId);
			var p2 = Math.Max(me, otherId);

			var conv = await _db.DmConversations
				.AsNoTracking()
				.FirstOrDefaultAsync(c => !c.IsManagerDm && c.Party1Id == p1 && c.Party2Id == p2);

			// 尚未對話 → 空陣列
			if (conv == null) return Json(Array.Empty<object>());

			var iAmP1 = (me == conv.Party1Id);

			// 共同投影器（★這裡做遮蔽）
			IEnumerable<object> Project(IEnumerable<DmMessage> src) =>
				src.Select(m => new
				{
					MessageId = m.MessageId,
					SenderId = m.SenderIsParty1 ? conv.Party1Id : conv.Party2Id,
					ReceiverId = m.SenderIsParty1 ? conv.Party2Id : conv.Party1Id,
					Content = _profanity.Censor(m.MessageText), // ★ 只在輸出時遮蔽；DB 仍存原文
					SentAtIso = UtcIso(m.EditedAt),
					IsMine = (m.SenderIsParty1 == iAmP1),
					IsRead = m.IsRead
				});

			// ---------- A) beforeIso：載入更舊 ----------
			if (before.HasValue)
			{
				var older = await _db.DmMessages.AsNoTracking()
					.Where(m => m.ConversationId == conv.ConversationId && m.EditedAt < before.Value)
					.OrderByDescending(m => m.EditedAt)
					.Take(take)
					.ToListAsync();

				older.Reverse(); // 舊→新
				return Json(Project(older));
			}

			// ---------- B) latest：進對話第一次載入 ----------
			if (string.Equals(direction, "latest", StringComparison.OrdinalIgnoreCase))
			{
				var latest = await _db.DmMessages.AsNoTracking()
					.Where(m => m.ConversationId == conv.ConversationId)
					.OrderByDescending(m => m.EditedAt)
					.Take(take)
					.ToListAsync();

				latest.Reverse(); // 舊→新
				return Json(Project(latest));
			}

			// ---------- C) afterIso：載入更新 ----------
			if (after.HasValue)
			{
				var newer = await _db.DmMessages.AsNoTracking()
					.Where(m => m.ConversationId == conv.ConversationId && m.EditedAt > after.Value)
					.OrderBy(m => m.EditedAt)
					.Take(take)
					.ToListAsync();

				return Json(Project(newer));
			}

			// ---------- D) 預設 = latest ----------
			var def = await _db.DmMessages.AsNoTracking()
				.Where(m => m.ConversationId == conv.ConversationId)
				.OrderByDescending(m => m.EditedAt)
				.Take(take)
				.ToListAsync();

			def.Reverse();
			return Json(Project(def));
		}

		// ==========================================================
		// 2) 好友們的「最新」 + 未讀
		//    GET /social_hub/Chat/PeersLatest?peerIds=1001,1002
		//    ★ LastContent 也做遮蔽（DB 仍是原文）
		// ==========================================================
		public sealed record PeerLatestDto(int PeerId, string? LastContent, string? LastIso, int Unread);

		[HttpGet]
		public async Task<IActionResult> PeersLatest(string? peerIds = null)
		{
			var me = await GetMeAsync();
			if (me <= 0) return Unauthorized();

			List<int>? filter = null;
			if (!string.IsNullOrWhiteSpace(peerIds))
			{
				filter = peerIds
					.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
					.Select(s => int.TryParse(s, out var v) ? v : 0)
					.Where(v => v > 0 && v != me)
					.Distinct()
					.ToList();
			}

			// 我參與的會話
			var convs = await _db.DmConversations.AsNoTracking()
				.Where(c => !c.IsManagerDm && (c.Party1Id == me || c.Party2Id == me))
				.Select(c => new
				{
					c.ConversationId,
					PeerId = (c.Party1Id == me ? c.Party2Id : c.Party1Id),
					IAmP1 = (c.Party1Id == me)
				})
				.ToListAsync();

			if (filter is { Count: > 0 })
				convs = convs.Where(c => filter.Contains(c.PeerId)).ToList();

			var list = new List<PeerLatestDto>(capacity: convs.Count);

			foreach (var c in convs)
			{
				// 最新一則（★ 遮蔽在輸出）
				var last = await _db.DmMessages.AsNoTracking()
					.Where(m => m.ConversationId == c.ConversationId)
					.OrderByDescending(m => m.EditedAt)
					.Select(m => new { m.MessageText, m.EditedAt })
					.FirstOrDefaultAsync();

				// 未讀數：對「我」而言，對方發的且未讀
				var unread = await _db.DmMessages.AsNoTracking()
					.Where(m => m.ConversationId == c.ConversationId
							 && !m.IsRead
							 && m.SenderIsParty1 != c.IAmP1)
					.CountAsync();

				list.Add(new PeerLatestDto(
					PeerId: c.PeerId,
					LastContent: last is null ? "" : _profanity.Censor(last.MessageText), // ★ 遮蔽
					LastIso: last is null ? null : UtcIso(last.EditedAt),
					Unread: unread
				));
			}

			// 依最新時間排序（null 放最後）
			list = list
				.OrderByDescending(x =>
				{
					if (string.IsNullOrEmpty(x.LastIso)) return DateTime.MinValue;
					return DateTime.Parse(x.LastIso, null, DateTimeStyles.RoundtripKind);
				})
				.ToList();

			return Json(list);
		}
	}
}
