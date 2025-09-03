namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class MessageViewModel
	{
		public string User { get; set; }        // 送訊息的人
		public string Content { get; set; }     // 訊息內容
		public DateTime Time { get; set; }      // 發送時間
		public bool IsMine { get; set; }        // 是否為自己發送的訊息
	}
}
