namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class NotificationViewModel
	{
		public int NotificationId { get; set; }
		public string NotificationTitle { get; set; } = string.Empty;
		public string NotificationMessage { get; set; } = string.Empty;
		public string SourceName { get; set; } = string.Empty;
		public string ActionName { get; set; } = string.Empty;
		public string SenderName { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public bool IsRead { get; set; }
	}

}
