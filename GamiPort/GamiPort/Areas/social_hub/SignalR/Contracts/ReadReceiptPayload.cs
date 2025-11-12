using System.Text.Json.Serialization;

namespace GamiPort.Areas.social_hub.SignalR.Contracts
{
	/// <summary>
	/// 「已讀回執」的傳輸物件（前後端共用）。
	/// - FromUserId：誰回報了已讀（回執的發出者）。
	/// - UpToIso：已讀到的時間界線（ISO 8601，UTC）。
	/// - RowsAffected：本次在資料庫實際被標記為已讀的訊息筆數。
	///   * 這是伺服器端為了避免「前端假已讀」新增的欄位：
	///     - 當 rowsAffected > 0：代表真的有把未讀寫入 DB，才會廣播到前端。
	///     - 當 rowsAffected = 0：代表 DB 沒更新任何列（例如沒有未讀、或 DB 規則攔住），Hub 會跳過廣播。
	///   * 前端若要顯示除錯資訊，可選擇性讀取；不讀也不影響功能。
	/// </summary>
	public sealed class ReadReceiptPayload
	{
		/// <summary>回報已讀的使用者 Id（發出回執的人）。</summary>
		[JsonPropertyName("fromUserId")]
		public int FromUserId { get; init; }

		/// <summary>已讀到的時間界線（UTC ISO-8601 格式，如 2025-10-27T02:35:12.3456789Z）。</summary>
		[JsonPropertyName("upToIso")]
		public string UpToIso { get; init; } = "";

		/// <summary>
		/// 本次在資料庫更新為已讀的訊息數。
		/// - 伺服器端用於決定是否要廣播 ReadReceipt/UnreadUpdate（>0 才廣播）。
		/// - 前端可用於診斷（例如顯示「已標記 X 則為已讀」），亦可忽略。
		/// </summary>
		[JsonPropertyName("rowsAffected")]
		public int RowsAffected { get; init; }
	}
}
