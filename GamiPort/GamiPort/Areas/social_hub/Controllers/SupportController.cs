// GamiPort/Areas/social_hub/Controllers/SupportController.cs
// 說明：前台 SupportController（使用者端）
// 重點修正：寫入成功後，立即以 SignalR 廣播到 ticket 群組，確保雙邊即時顯示。

using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Infrastructure.Security;
using GamiPort.Models;                       // EF DbContext（SupportTicket, SupportTicketMessage）
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;          // ★ 新增：IHubContext
using GamiPort.Areas.social_hub.Hubs;        // ★ 新增：SupportHub（SignalR Hub 型別）

namespace GamiPort.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[Authorize] // 前台需要登入
	public sealed class SupportController : Controller
	{
		private readonly ISupportService _support;
		private readonly IAppCurrentUser _me;
		private readonly GameSpacedatabaseContext _db;
		private readonly IHubContext<SupportHub> _hub; // ★ 新增：廣播放用

		public SupportController(
			ISupportService support,
			IAppCurrentUser me,
			GameSpacedatabaseContext db,
			IHubContext<SupportHub> hub // ★ 新增：注入 HubContext
		)
		{
			_support = support;
			_me = me;
			_db = db;
			_hub = hub;
		}

		[HttpGet]
		public IActionResult Ticket(int id) => View(new TicketVM { TicketId = id });

		/// <summary>
		/// 送出訊息（前台使用者）
		/// 修正點：寫入成功後，**立即推播到 ticket 群組**；避免對方沒發話就看不到。
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SendMessage(int id, [FromForm] string text, CancellationToken ct)
		{
			var userId = await _me.GetUserIdAsync();
			if (userId <= 0) return Unauthorized("尚未登入");

			int? managerId = null; // 前台不處理管理員身分
			var r = await _support.SendAsync(id, userId, managerId, text, ct);
			if (!r.ok) return BadRequest(r.error ?? "送出失敗");

			// ★ 關鍵：即時廣播到 ticket 群組
			// 群組命名慣例：ticket:{id}（若你有 GroupNames.Ticket(...) 可替換）
			var group = $"ticket:{id}";

			// 某些頁面監聽簡單事件名 "msg" 只用來觸發刷新；一併送出以維持相容
			await _hub.Clients.Group(group).SendAsync("msg", new { ticketId = id }, ct);

			// 正式 payload（你前端若有綁 'ticket.message' 事件，這裡直接送 DTO）
			await _hub.Clients.Group(group).SendAsync("ticket.message", r.msg, ct);

			// 也可依你的頁面事件名補發一份（若你使用 "TicketMessage"）
			// await _hub.Clients.Group(group).SendAsync("TicketMessage", r.msg, ct);

			// 回傳訊息 DTO，讓前端可本地先 append，減少閃爍
			return Json(r.msg);
		}

		/// <summary>
		/// 訊息清單（簡版 HTML，給前端 innerHTML）
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> MessageList(int id, int page = 1, int pageSize = 100, CancellationToken ct = default)
		{
			if (page < 1) page = 1;
			if (pageSize < 1 || pageSize > 200) pageSize = 100;

			// 取最新的 N 筆（簡單版）
			var q = _db.SupportTicketMessages
				.AsNoTracking()
				.Where(m => m.TicketId == id)
				.OrderBy(m => m.SentAt);

			var msgs = await q.ToListAsync(ct);

			// 直接在這裡產出最簡 HTML（之後要美化可再抽 Partial View）
			var sb = new System.Text.StringBuilder();
			foreach (var m in msgs)
			{
				var isUser = m.SenderManagerId == null;
				var name = isUser ? $"使用者 {m.SenderUserId}" : $"客服 {m.SenderManagerId}";
				var time = m.SentAt.ToLocalTime().ToString("yyyy/MM/dd HH:mm");
				sb.Append($@"
<div class=""d-flex mb-2 {(isUser ? "" : "justify-content-end")}"">
  <div class=""p-2 rounded-3 border {(isUser ? "bg-light" : "bg-primary text-white")}"" style=""max-width:72%;"">
    <div class=""small opacity-75"">{name} · {time}</div>
    <div>{System.Net.WebUtility.HtmlEncode(m.MessageText)}</div>
  </div>
</div>");
			}
			return Content(sb.ToString(), "text/html; charset=utf-8");
		}

		/// <summary>
		/// 建立新票券；若同時有 content，就直接寫入第一則訊息。
		/// ★ 修正點：建立完成後，推播 "ticket.created" 與第一則訊息（若有）。
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateTicket([FromForm] string subject, [FromForm] string? content, CancellationToken ct)
		{
			var userId = await _me.GetUserIdAsync();
			if (userId <= 0) return Unauthorized("未登入");

			subject = (subject ?? "").Trim();
			content = (content ?? "").Trim();
			if (string.IsNullOrWhiteSpace(subject)) return BadRequest("主旨不得為空白");

			var now = DateTime.UtcNow;
			var ticket = new SupportTicket
			{
				UserId = userId,
				Subject = subject,
				CreatedAt = now,
				LastMessageAt = now,
				AssignedManagerId = null,
				IsClosed = false
			};
			_db.SupportTickets.Add(ticket);
			await _db.SaveChangesAsync(ct);

			SupportTicketMessage? firstMsg = null;
			if (!string.IsNullOrEmpty(content))
			{
				firstMsg = new SupportTicketMessage
				{
					TicketId = ticket.TicketId,
					SenderUserId = userId,
					MessageText = content,
					SentAt = now
				};
				_db.SupportTicketMessages.Add(firstMsg);
				await _db.SaveChangesAsync(ct);
			}

			// ★ 建立後廣播（通知清單刷新）
			var group = $"ticket:{ticket.TicketId}";
			await _hub.Clients.Group(group).SendAsync("ticket.created", new
			{
				ticketId = ticket.TicketId,
				subject = ticket.Subject,
				createdAt = ticket.CreatedAt
			}, ct);

			// ★ 若有第一則訊息，一併廣播
			if (firstMsg is not null)
			{
				await _hub.Clients.Group(group).SendAsync("ticket.message", new
				{
					messageId = firstMsg.MessageId,
					ticketId = firstMsg.TicketId,
					senderUserId = firstMsg.SenderUserId,
					senderManagerId = firstMsg.SenderManagerId,
					messageText = firstMsg.MessageText,
					sentAt = firstMsg.SentAt
				}, ct);
			}

			return Json(new { ok = true, ticketId = ticket.TicketId });
		}

		/// <summary>
		/// 我建立的票券列表（前台）
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> MyTickets(CancellationToken ct)
		{
			var userId = await _me.GetUserIdAsync();
			if (userId <= 0) return Unauthorized();

			var list = await _db.SupportTickets.AsNoTracking()
				.Where(t => t.UserId == userId)
				.OrderByDescending(t => t.LastMessageAt ?? t.CreatedAt)
				.Select(t => new MyTicketRow
				{
					TicketId = t.TicketId,
					Subject = t.Subject,
					CreatedAt = t.CreatedAt,
					LastMessageAt = t.LastMessageAt,
					AssignedManagerId = t.AssignedManagerId,
					IsClosed = t.IsClosed
				})
				.ToListAsync(ct);

			return View(list);
		}

		/// <summary>
		/// （可選）診斷用：對特定 ticket 群組發一個 nudge，前後台都應立刻收到。
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Nudge(int id, CancellationToken ct)
		{
			var group = $"ticket:{id}";
			await _hub.Clients.Group(group).SendAsync("nudge", new { ticketId = id, ts = DateTimeOffset.UtcNow }, ct);
			return Ok(new { ok = true });
		}

		// ===== 極簡 VM =====
		public sealed class TicketVM { public int TicketId { get; set; } }
		public sealed class MyTicketRow
		{
			public int TicketId { get; set; }
			public string Subject { get; set; } = "";
			public DateTime CreatedAt { get; set; }
			public DateTime? LastMessageAt { get; set; }
			public int? AssignedManagerId { get; set; }
			public bool IsClosed { get; set; }
		}
	}
}
