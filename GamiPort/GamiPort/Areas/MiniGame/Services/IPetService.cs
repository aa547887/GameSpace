using GamiPort.Models;

namespace GamiPort.Areas.MiniGame.Services
{
	/// <summary>
	/// 寵物服務介面 (GamiPort 前台)
	/// 處理寵物查詢、互動、定制等功能
	/// </summary>
	public interface IPetService
	{
		/// <summary>
		/// 獲取用戶寵物信息
		/// </summary>
		/// <param name="userId">用戶ID</param>
		/// <returns>寵物信息，如果不存在或已刪除則返回null</returns>
		Task<Pet?> GetUserPetAsync(int userId);

		/// <summary>
		/// 執行寵物互動（餵食/洗澡/玩耍/睡覺）
		/// </summary>
		/// <param name="userId">用戶ID</param>
		/// <param name="action">互動類型：feed/bath/play/sleep</param>
		/// <returns>操作結果</returns>
		Task<PetInteractionResult> InteractWithPetAsync(int userId, string action);

		/// <summary>
		/// 更新寵物外觀（膚色和背景）
		/// </summary>
		/// <param name="userId">用戶ID</param>
		/// <param name="skinColor">膚色代碼</param>
		/// <param name="background">背景代碼</param>
		/// <returns>操作結果</returns>
		Task<PetUpdateAppearanceResult> UpdatePetAppearanceAsync(int userId, string skinColor, string background);

		/// <summary>
		/// 獲取可用的膚色列表
		/// </summary>
		/// <returns>膚色設置列表</returns>
		Task<IEnumerable<PetSkinColorCostSetting>> GetAvailableSkinsAsync();

		/// <summary>
		/// 獲取可用的背景列表
		/// </summary>
		/// <returns>背景設置列表</returns>
		Task<IEnumerable<PetBackgroundCostSetting>> GetAvailableBackgroundsAsync();

		/// <summary>
		/// 增加寵物經驗值，並自動檢查升級
		/// </summary>
		/// <param name="petId">寵物ID</param>
		/// <param name="exp">經驗值</param>
		/// <returns>是否成功</returns>
		Task<bool> AddExperienceAsync(int petId, int exp);

		/// <summary>
		/// 寵物升級並發放獎勵
		/// </summary>
		/// <param name="petId">寵物ID</param>
		/// <returns>是否成功</returns>
		Task<bool> LevelUpPetAsync(int petId);

		/// <summary>
		/// 獲取指定等級所需的經驗值
		/// </summary>
		/// <param name="level">等級</param>
		/// <returns>所需經驗值（0表示已達最高等級）</returns>
		Task<int> GetRequiredExpForLevelAsync(int level);

		/// <summary>
		/// 修改寵物名稱
		/// </summary>
		/// <param name="userId">用戶ID</param>
		/// <param name="newName">新名稱</param>
		/// <returns>操作結果</returns>
		Task<PetUpdateNameResult> UpdatePetNameAsync(int userId, string newName);
	}

	/// <summary>
	/// 寵物互動結果
	/// </summary>
	public class PetInteractionResult
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public Pet? Pet { get; set; }
	}

	/// <summary>
	/// 寵物外觀更新結果
	/// </summary>
	public class PetUpdateAppearanceResult
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public Pet? Pet { get; set; }
	}

	/// <summary>
	/// 寵物名稱更新結果
	/// </summary>
	public class PetUpdateNameResult
	{
		public bool Success { get; set; }
		public string Message { get; set; } = string.Empty;
		public Pet? Pet { get; set; }
	}
}
