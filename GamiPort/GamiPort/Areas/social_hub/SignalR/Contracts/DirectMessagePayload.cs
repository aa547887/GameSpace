using System.Text.Json.Serialization;

namespace GamiPort.Areas.social_hub.SignalR.Contracts
{
	/// <summary>
	/// DM 訊息 Payload（前後端共用，輸出為 camelCase）。
	/// - MessageId：資料庫訊息主鍵，利於前端做已讀回執或差量更新。
	/// - SenderId / ReceiverId：訊息的雙方。
	/// - Content：訊息內容（字數上限由後端 MessagePolicyOptions 控制）。
	/// - SentAtIso：送出時間（UTC ISO-8601 字串；前端自行轉使用者時區顯示）。
	/// </summary>
	public sealed class DirectMessagePayload
	{
		[JsonPropertyName("messageId")] public int MessageId { get; init; }
		[JsonPropertyName("senderId")] public int SenderId { get; init; }
		[JsonPropertyName("receiverId")] public int ReceiverId { get; init; }
		[JsonPropertyName("content")] public string Content { get; init; } = "";
		[JsonPropertyName("sentAtIso")] public string SentAtIso { get; init; } = "";
	}
}
