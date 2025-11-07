using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GamiPort.Areas.MiniGame.Services;
using GamiPort.Areas.MiniGame.Filters;

namespace GamiPort.Areas.MiniGame.config
{
	/// <summary>
	/// MiniGame Area 服務註冊擴展方法（前台）
	/// 集中管理所有 MiniGame Area 相關的服務註冊
	/// </summary>
	public static class ServiceExtensions
	{
		/// <summary>
		/// 註冊 MiniGame Area 所有服務
		/// </summary>
		/// <param name="services">服務集合</param>
		/// <param name="configuration">配置</param>
		/// <returns>服務集合</returns>
		public static IServiceCollection AddMiniGameServices(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			// ==================== Filters（需要依賴注入的 Filters） ====================
			// 註冊冪等性過濾器（需要注入 IMemoryCache + ILogger）
			services.AddScoped<IdempotencyFilter>();

			// 註冊統一異常處理過濾器（需要注入 ILogger）
			services.AddScoped<FrontendProblemDetailsFilter>();
			// =========================================================================

			// ==================== 基礎設施服務 ====================
			// 註冊模糊搜尋服務（5 級優先順序匹配）
			services.AddScoped<IFuzzySearchService, FuzzySearchService>();

			// 註冊 QR Code 生成服務（電子禮券核銷）
			services.AddScoped<IQRCodeService, QRCodeService>();
			// =========================================================================

			// ==================== 寵物系統 ====================
			// 註冊寵物管理服務（互動、升級、外觀變更）
			services.AddScoped<IPetService, PetService>();
			// =========================================================================

			// ==================== 簽到系統 ====================
			// 註冊簽到管理服務（每日簽到、獎勵發放）
			services.AddScoped<ISignInService, SignInService>();
			// =========================================================================

			// ==================== 錢包系統 ====================
			// 註冊錢包管理服務（點數、優惠券、電子禮券）
			services.AddScoped<IWalletService, WalletService>();
			// =========================================================================

			// ==================== 小遊戲系統 ====================
			// 註冊遊戲玩法服務（開始遊戲、結束遊戲、獎勵發放）
			services.AddScoped<IGamePlayService, GamePlayService>();
			// =========================================================================

			return services;
		}
	}
}
