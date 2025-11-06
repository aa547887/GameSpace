// 目的：後台用來「跨站」把事件送到前台 7160 的 SignalR Hub
namespace GameSpace.Areas.social_hub.Services.Abstractions
{
	public sealed class SupportMessageDto
	{
		public int TicketId { get; set; }
		public int? SenderUserId { get; set; }
		public int? SenderManagerId { get; set; }
		public string MessageText { get; set; } = "";
		public DateTime SentAt { get; set; } // UTC
	}

	public interface ISupportNotifier
	{
		/// <summary>廣播一則訊息到 ticket:{TicketId} 群組（推播到前台/後台頁面）。</summary>
		Task BroadcastMessageAsync(SupportMessageDto msg, CancellationToken ct = default);

		// （可選擴充）
		// Task BroadcastAssignedAsync(int ticketId, int? toManagerId, CancellationToken ct = default);
		// Task BroadcastReassignedAsync(int ticketId, int toManagerId, CancellationToken ct = default);
		// Task BroadcastClosedAsync(int ticketId, CancellationToken ct = default);
	}
}
