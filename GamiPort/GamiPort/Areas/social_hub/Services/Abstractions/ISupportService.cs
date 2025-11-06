using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
	/// <summary>發訊息後回傳的簡易 DTO（供前端即時 append）</summary>
	public sealed class SupportMessageDto
	{
		public int MessageId { get; init; }
		public int TicketId { get; init; }
		public bool SenderIsUser { get; init; }
		public int SenderId { get; init; }
		public string Text { get; init; } = "";
		public DateTime SentAt { get; init; }
	}

	public interface ISupportService
	{
		/// <summary>我是否有權加入/觀看此工單</summary>
		Task<bool> CanJoinAsync(int ticketId, int userId, int? managerId, CancellationToken ct = default);

		/// <summary>寫入訊息（會做基本驗證），成功回 DTO 供廣播</summary>
		Task<(bool ok, string? error, SupportMessageDto? msg)> SendAsync(
			int ticketId, int actorUserId, int? actorManagerId, string text, CancellationToken ct = default);
	}
}
