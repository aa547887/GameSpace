using GamiPort.Models;

namespace GamiPort.Areas.MiniGame.Services
{
	/// <summary>
	/// 小遊戲玩法服務介面（前台）
	/// 負責獲取今日剩餘遊戲次數、開始遊戲、結束遊戲並發放獎勵、查詢遊戲歷史
	/// </summary>
	public interface IGamePlayService
	{
		/// <summary>
		/// 獲取用戶今日剩餘遊戲次數
		/// </summary>
		/// <param name="userId">用戶 ID</param>
		/// <returns>剩餘次數</returns>
		Task<int> GetUserRemainingPlaysAsync(int userId);

		/// <summary>
		/// 開始遊戲
		/// 檢查今日剩餘次數、建立遊戲記錄
		/// </summary>
		/// <param name="userId">用戶 ID</param>
		/// <param name="level">遊戲難度等級 (1-3)</param>
		/// <returns>成功狀態、訊息、遊戲記錄 ID</returns>
		Task<(bool success, string message, int? playId)> StartGameAsync(int userId, int level);

		/// <summary>
		/// 結束遊戲並發放獎勵
		/// 規則: 只有勝利才發放獎勵（點數、經驗值、優惠券）
		/// </summary>
		/// <param name="userId">用戶 ID</param>
		/// <param name="playId">遊戲記錄 ID</param>
		/// <param name="level">遊戲難度等級</param>
		/// <param name="result">遊戲結果 ('Win'/'Lose'/'Abort')</param>
		/// <param name="experience">獲得經驗值</param>
		/// <param name="points">獲得點數</param>
		/// <returns>成功狀態、訊息</returns>
		Task<(bool success, string message)> EndGameAsync(
			int userId,
			int playId,
			int level,
			string result,
			int experience,
			int points);

		/// <summary>
		/// 獲取遊戲歷史（分頁、支持篩選）
		/// </summary>
		/// <param name="userId">用戶 ID</param>
		/// <param name="page">分頁號 (從 1 開始)</param>
		/// <param name="pageSize">每頁筆數</param>
		/// <param name="level">篩選難度（可選，null 為全部）</param>
		/// <param name="startDate">篩選開始日期（可選，UTC+8）</param>
		/// <param name="endDate">篩選結束日期（可選，UTC+8）</param>
		/// <returns>遊戲記錄列表、總筆數</returns>
		Task<(List<Models.MiniGame> games, int totalCount)> GetGameHistoryAsync(
			int userId,
			int page,
			int pageSize,
			int? level = null,
			DateTime? startDate = null,
			DateTime? endDate = null);
	}
}
