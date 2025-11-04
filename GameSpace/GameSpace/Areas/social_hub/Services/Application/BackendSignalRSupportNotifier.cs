// =============================================================
// File: Areas/social_hub/Services/Application/BackendSignalRSupportNotifier.cs
// Purpose: 後台(GameSpace) 透過 SignalR.Client 連線到「前台(GamiPort, 7160)」的 SupportHub，
//          在 DB 寫入成功後即可跨站廣播到 ticket:{id} 群組。
// Security: 使用 appsettings 中的 Support:JoinSecret 以 access_token 方式帶入連線，
//           前台 Hub 的伺服器方法需驗證此 secret 才允許呼叫。
// Notes   :
//   - 無 HubConnectionState.Disconnecting 列舉，僅有 Disconnected/Connecting/Connected/Reconnecting
//   - 用 SemaphoreSlim 做 StartAsync 防抖，避免並行競態
//   - 建議將 JoinSecret 放在 User Secrets 或環境變數，不要入版控
// =============================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using GameSpace.Areas.social_hub.Services.Abstractions; // ISupportNotifier / SupportMessageDto (你在 Abstractions 定義)
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GameSpace.Areas.social_hub.Services.Application
{
	/// <summary>
	/// 後台 → 前台 Hub 的「跨站推播」實作。
	/// - 讀取設定：
	///   * Support:HubUrl   → 例如 https://localhost:7160/hubs/support
	///   * Support:JoinSecret → 前後台共用密鑰（會以 access_token 夾帶）
	/// - 生命週期：
	///   * 懶啟動：第一次廣播前才 StartAsync()
	///   * 自動重連：WithAutomaticReconnect()
	/// </summary>
	public sealed class BackendSignalRSupportNotifier : ISupportNotifier, IAsyncDisposable
	{
		private readonly HubConnection _conn;
		private readonly ILogger<BackendSignalRSupportNotifier>? _logger;

		// 防止多執行緒同時呼叫 StartAsync()
		private readonly SemaphoreSlim _startLock = new(1, 1);

		// BackendSignalRSupportNotifier.cs（關鍵片段）
		public BackendSignalRSupportNotifier(IConfiguration cfg, ILogger<BackendSignalRSupportNotifier>? logger = null)
		{
			_logger = logger;

			var hubUrl = cfg["Support:HubUrl"] ?? "http://localhost:7160/hubs/support"; // ← 用你實際的協定/連接埠
			var secret = cfg["Support:JoinSecret"] ?? string.Empty;

			_conn = new HubConnectionBuilder()
				.WithUrl(hubUrl, opt =>
				{
					// ★ 一定要有：把 JoinSecret 當 access_token 帶到前台
					opt.AccessTokenProvider = () => Task.FromResult(secret);
				})
				.WithAutomaticReconnect()
				.Build();
		}


		/// <summary>
		/// 確保連線已啟動：
		/// - Connected / Connecting / Reconnecting：不動作
		/// - Disconnected：上鎖後呼叫 StartAsync()
		/// </summary>
		private async Task EnsureStartedAsync()
		{
			if (_conn.State == HubConnectionState.Connected
			 || _conn.State == HubConnectionState.Connecting
			 || _conn.State == HubConnectionState.Reconnecting)
			{
				return;
			}

			if (_conn.State == HubConnectionState.Disconnected)
			{
				await _startLock.WaitAsync().ConfigureAwait(false);
				try
				{
					// 二次確認，避免等待鎖期間狀態改變
					if (_conn.State == HubConnectionState.Disconnected)
					{
						_logger?.LogInformation("Starting SignalR connection to frontend SupportHub...");
						await _conn.StartAsync().ConfigureAwait(false);
						_logger?.LogInformation("SignalR connection started.");
					}
				}
				finally
				{
					_startLock.Release();
				}
			}
		}

		/// <summary>
		/// 廣播一則訊息到 ticket:{TicketId} 群組。
		/// 這裡呼叫的是「前台 Hub 的伺服器方法」ServerSendToTicketMessage，
		/// 由前台 Hub 代為：Clients.Group($"ticket:{id}").SendAsync("msg"/"ticket.message", ...);
		/// </summary>
		public async Task BroadcastMessageAsync(SupportMessageDto msg, CancellationToken ct = default)
		{
			await EnsureStartedAsync().ConfigureAwait(false);

			try
			{
				// 舊的：await _conn.InvokeAsync("ServerSendToTicketMessage", msg, ct);
				await _conn.InvokeAsync("ServerSendToTicketMessage", msg).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "BroadcastMessageAsync 失敗（TicketId={TicketId}）", msg.TicketId);
				throw;
			}
		}


		/// <summary>
		/// （如未來擴充）可加：BroadcastAssignedAsync / BroadcastReassignedAsync / BroadcastClosedAsync ...
		/// 只要照樣呼叫對應的 Hub 伺服器方法即可。
		/// </summary>

		public async ValueTask DisposeAsync()
		{
			try
			{
				await _conn.DisposeAsync().ConfigureAwait(false);
			}
			catch { /* ignore */ }
			_startLock.Dispose();
		}
	}
}
