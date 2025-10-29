using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Models;
using Microsoft.EntityFrameworkCore;

namespace GamiPort.Areas.social_hub.Services.Application
{
	/// <summary>
	/// Support 服務（權限、寫訊息、更新 last message 等）
	/// 已對齊現有實體：
	/// - SupportTicket(TicketId, UserId, AssignedManagerId, IsClosed, LastMessageAt, ...)
	/// - SupportTicketMessage(MessageId, TicketId, SenderUserId, SenderManagerId, MessageText, SentAt, ReadByUserAt, ReadByManagerAt)
	/// </summary>
	public sealed class SupportService : ISupportService
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly ISupportNotifier _notifier;

		public SupportService(GameSpacedatabaseContext db, ISupportNotifier notifier)
		{
			_db = db;
			_notifier = notifier;
		}

		public async Task<bool> CanJoinAsync(int ticketId, int userId, int? managerId, CancellationToken ct = default)
		{
			var t = await _db.SupportTickets
				.AsNoTracking()
				.Where(x => x.TicketId == ticketId)
				.Select(x => new { x.UserId, x.AssignedManagerId })
				.FirstOrDefaultAsync(ct);

			if (t == null) return false;
			if (t.UserId == userId) return true;
			if (managerId.HasValue && t.AssignedManagerId == managerId.Value) return true;
			return false;
		}

		public async Task<(bool ok, string? error, SupportMessageDto? msg)> SendAsync(
			int ticketId, int actorUserId, int? actorManagerId, string text, CancellationToken ct = default)
		{
			text = (text ?? string.Empty).Trim();
			if (string.IsNullOrEmpty(text))
				return (false, "內容不可為空白", null);

			// 最長度保護（你的欄位通常是 nvarchar(255)）
			if (text.Length > 255)
				return (false, "內容過長（最多 255 字）", null);

			// 權限：必須先能 Join
			if (!await CanJoinAsync(ticketId, actorUserId, actorManagerId, ct))
				return (false, "無權限", null);

			// 讀取工單狀態
			var t = await _db.SupportTickets.FirstOrDefaultAsync(x => x.TicketId == ticketId, ct);
			if (t == null)
				return (false, "工單不存在", null);

			if (t.IsClosed)
				return (false, "此工單已結單，無法發送訊息", null);

			var utcNow = DateTime.UtcNow;

			// 建立訊息（依角色設定 SenderUserId / SenderManagerId）
			var msg = new SupportTicketMessage
			{
				TicketId = ticketId,
				SenderUserId = actorManagerId == null ? actorUserId : (int?)null,
				SenderManagerId = actorManagerId,
				MessageText = text,
				SentAt = utcNow,
				ReadByUserAt = null,
				ReadByManagerAt = null
			};

			_db.SupportTicketMessages.Add(msg);

			// 更新工單最後訊息時間
			t.LastMessageAt = utcNow;

			await _db.SaveChangesAsync(ct);

			// 回前端用的最小 DTO（沿用既有結構）
			var dto = new SupportMessageDto
			{
				MessageId = msg.MessageId,
				TicketId = msg.TicketId,
				SenderIsUser = actorManagerId == null,
				SenderId = actorManagerId == null ? actorUserId : actorManagerId.Value,
				Text = msg.MessageText,
				SentAt = msg.SentAt
			};

			// SignalR 廣播到 ticket 群組
			await _notifier.BroadcastMessageAsync(dto, ct);

			return (true, null, dto);
		}
	}
}
