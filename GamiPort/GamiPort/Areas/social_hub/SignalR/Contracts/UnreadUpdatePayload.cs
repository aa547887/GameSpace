using System.Text.Json.Serialization;

namespace GamiPort.Areas.social_hub.SignalR.Contracts
{
	/// <summary>
	/// 未讀數更新的傳輸物件（前後端共用）。
	/// - PeerId：針對哪一位對象（對話對方）的未讀量；若為 0，通常代表「僅回全站總未讀」的情境。
	/// - Unread：與該 Peer 的未讀數（例如「你與 user 42 的未讀 = 3」）。
	/// - Total：全站總未讀數（加總所有 DM 對話的未讀）。
	/// </summary>
	public sealed class UnreadUpdatePayload
	{
		/// <summary>
		/// 對象使用者 Id。
		/// - 當你需要更新「與某位對象」的未讀數時，填入對方 userId。
		/// - 若只想告知「全站總未讀」，可置 0（前端 UI 可據此選擇性忽略 Peer 區塊）。
		/// </summary>
		[JsonPropertyName("peerId")]
		public int PeerId { get; init; }

		/// <summary>與該 Peer 的未讀數量。</summary>
		[JsonPropertyName("unread")]
		public int Unread { get; init; }

		/// <summary>全站總未讀數（所有對話加總）。</summary>
		[JsonPropertyName("total")]
		public int Total { get; init; }
	}
}
