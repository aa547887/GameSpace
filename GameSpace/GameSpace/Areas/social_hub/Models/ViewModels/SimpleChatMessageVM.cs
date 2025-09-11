using System;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	// [用途] Chat/With 與 Chat/History 的訊息投影
	public class SimpleChatMessageVM
	{
		public int MessageId { get; set; }     // [說明] 訊息ID
		public int SenderId { get; set; }      // [說明] 寄件者ID（由 SenderIsParty1 + 對話雙方推導）
		public int ReceiverId { get; set; }    // [說明] 收件者ID（由 SenderIsParty1 + 對話雙方推導）
		public string Text { get; set; } = ""; // [說明] 訊息內容（已過濾）
		public DateTime At { get; set; }       // [說明] 時間（對應 EditedAt）
		public bool IsMine { get; set; }       // [說明] 是否本人所發
		public bool IsRead { get; set; }       // [說明] 是否已讀（ReadAt != null）
		public DateTime? ReadAt { get; set; }  // [說明] 已讀時間（null=未讀）
	}
}
