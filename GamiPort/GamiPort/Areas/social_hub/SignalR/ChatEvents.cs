using System;

namespace GamiPort.Areas.social_hub.SignalR
{
	/// <summary>
	/// 聊天相關的「前後端共用事件名稱」集中處理，避免魔術字串。
	/// </summary>
	public static class ChatEvents
	{
		/// <summary>伺服器推「新訊息」給雙方。</summary>
		public const string ReceiveDirect = "ReceiveDirect";

		/// <summary>伺服器推「已讀回執」給訊息發送者。</summary>
		public const string ReadReceipt = "ReadReceipt";

		/// <summary>伺服器推「未讀數量更新」給指定使用者（用於清單紅點）。</summary>
		public const string UnreadUpdate = "UnreadUpdate";

		/// <summary>
		/// ★ 新增：穢語規則有更新（由後台 Reload 觸發）。  
		/// 負責通知所有前端「請立即抓新規則」。
		/// </summary>
		public const string ProfanityUpdated = "ProfanityUpdated";
	}
}
