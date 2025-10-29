using GamiPort.Areas.social_hub.Services.Abstractions;
using GamiPort.Infrastructure.Security;
using GamiPort.Models; // ★ 新增
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ★ 新增

namespace GamiPort.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	[Authorize] // 前台需要登入
	public sealed class SupportController : Controller
	{
		private readonly ISupportService _support;
		private readonly IAppCurrentUser _me;
		private readonly GameSpacedatabaseContext _db; // ★ 新增

		public SupportController(ISupportService support, IAppCurrentUser me, GameSpacedatabaseContext db) // ★ 改：注入 db
		{
			_support = support;
			_me = me;
			_db = db; // ★ 新增
		}

		[HttpGet]
		public IActionResult Ticket(int id) => View(new TicketVM { TicketId = id });

		/// <summary>送出訊息（前台版：只有使用者本人能送）</summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> SendMessage(int id, [FromForm] string text, CancellationToken ct)
		{
			var userId = await _me.GetUserIdAsync();
			int? managerId = null; // 前台不處理管理員身分

			var r = await _support.SendAsync(id, userId, managerId, text, ct);
			if (!r.ok) return BadRequest(r.error ?? "送出失敗");

			return Json(r.msg); // 回傳訊息 DTO，前端可直接 append
		}

		/// <summary>訊息清單（簡版 HTML，給前端 innerHTML）</summary>
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

			// 直接在這裡產出最簡 HTML，省去再建 Partial 的成本（之後你要美化再抽 View）
			var sb = new System.Text.StringBuilder();
			foreach (var m in msgs)
			{
				var isUser = m.SenderManagerId == null;
				var who = isUser ? "user" : "mgr";
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

		// GamiPort/Areas/social_hub/Controllers/SupportController.cs（節錄新增）
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

			if (!string.IsNullOrEmpty(content))
			{
				_db.SupportTicketMessages.Add(new SupportTicketMessage
				{
					TicketId = ticket.TicketId,
					SenderUserId = userId,
					MessageText = content,
					SentAt = now
				});
				await _db.SaveChangesAsync(ct);
			}
			return Json(new { ok = true, ticketId = ticket.TicketId });
		}

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
