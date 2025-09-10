using System;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class NotificationViewModel
	{
		public int RecipientId { get; set; }
		public int NotificationId { get; set; }

		public string? NotificationTitle { get; set; }
		public string? NotificationMessage { get; set; }

		public string? SourceName { get; set; }
		public string? ActionName { get; set; }
		public string? GroupName { get; set; }

		public string? SenderName { get; set; }   // 可能來自使用者/管理員或系統
		public DateTime CreatedAt { get; set; }
		public bool IsRead { get; set; }
	}
}
