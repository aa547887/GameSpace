using System;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	// [用途] Chat/Index 清單使用的精簡對話項目
	public class ConversationListItemVM
	{
		public int ConversationId { get; set; }   // [說明] 對話ID
		public int OtherId { get; set; }          // [說明] 對方使用者ID
		public DateTime? LastMessageAt { get; set; } // [說明] 最後訊息時間（可空）
		public int UnreadCount { get; set; }      // [說明] 對方→我的未讀數
	}
}
