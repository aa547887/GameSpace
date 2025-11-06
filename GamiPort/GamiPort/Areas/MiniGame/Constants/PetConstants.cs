namespace GamiPort.Areas.MiniGame.Constants
{
	/// <summary>
	/// 寵物系統常數定義（前台）
	/// 定義寵物互動、升級、外觀定制等相關的所有 Magic Numbers
	/// </summary>
	public static class PetConstants
	{
		// ========== 互動點數成本 ==========

		/// <summary>
		/// 每次寵物互動（餵食/洗澡/玩耍/睡覺）消耗的會員點數
		/// </summary>
		public const int INTERACT_POINT_COST = 5;

		// ========== 屬性增量 ==========

		/// <summary>
		/// 餵食操作增加的飢餓值
		/// </summary>
		public const int STAT_INCREMENT_FEED = 10;

		/// <summary>
		/// 洗澡操作增加的清潔值
		/// </summary>
		public const int STAT_INCREMENT_BATH = 10;

		/// <summary>
		/// 玩耍操作增加的心情值
		/// </summary>
		public const int STAT_INCREMENT_PLAY = 10;

		/// <summary>
		/// 睡覺操作增加的體力值
		/// </summary>
		public const int STAT_INCREMENT_SLEEP = 10;

		// ========== 屬性範圍 ==========

		/// <summary>
		/// 所有寵物屬性的最小值（用於健康檢查）
		/// </summary>
		public const int STAT_MIN_VALUE = 0;

		/// <summary>
		/// 所有寵物屬性（飢餓/心情/體力/清潔/健康）的最大值
		/// </summary>
		public const int STAT_MAX_VALUE = 100;

		/// <summary>
		/// 寵物狀態警告閾值（< 20 為不良狀態）
		/// </summary>
		public const int STAT_LOW_THRESHOLD = 20;

		/// <summary>
		/// 四項屬性達到此值時觸發健康值恢復
		/// </summary>
		public const int STAT_FULL_VALUE = 100;

		// ========== 經驗值公式 - 階段1 (Level 1-10) ==========

		/// <summary>
		/// Level 1-10 線性公式係數A：EXP = 40 * level + 60
		/// </summary>
		public const double EXP_FORMULA_TIER1_A = 40;

		/// <summary>
		/// Level 1-10 線性公式係數B：EXP = 40 * level + 60
		/// </summary>
		public const double EXP_FORMULA_TIER1_B = 60;

		/// <summary>
		/// Level 1-10 適用範圍上限
		/// </summary>
		public const int EXP_FORMULA_TIER1_MAX = 10;

		// ========== 經驗值公式 - 階段2 (Level 11-100) ==========

		/// <summary>
		/// Level 11-100 二次公式係數A：EXP = 0.8 * level² + 380
		/// </summary>
		public const double EXP_FORMULA_TIER2_A = 0.8;

		/// <summary>
		/// Level 11-100 二次公式係數B：EXP = 0.8 * level² + 380
		/// </summary>
		public const double EXP_FORMULA_TIER2_B = 380;

		/// <summary>
		/// Level 11-100 適用範圍上限
		/// </summary>
		public const int EXP_FORMULA_TIER2_MAX = 100;

		// ========== 經驗值公式 - 階段3 (Level 101+) ==========

		/// <summary>
		/// Level 101+ 指數公式基數：EXP = 285.69 * 1.06^level
		/// </summary>
		public const double EXP_FORMULA_TIER3_BASE = 285.69;

		/// <summary>
		/// Level 101+ 指數公式增長率：EXP = 285.69 * 1.06^level
		/// </summary>
		public const double EXP_FORMULA_TIER3_RATE = 1.06;

		/// <summary>
		/// Level 101+ 適用範圍起點
		/// </summary>
		public const int EXP_FORMULA_TIER3_MIN = 101;

		// ========== 等級範圍 ==========

		/// <summary>
		/// 寵物創建時的初始等級
		/// </summary>
		public const int PET_INITIAL_LEVEL = 1;

		/// <summary>
		/// 寵物初始經驗值
		/// </summary>
		public const int PET_INITIAL_EXP = 0;

		/// <summary>
		/// 寵物最高等級
		/// </summary>
		public const int PET_MAX_LEVEL = 250;

		/// <summary>
		/// 規則驗證時的最高等級檢查上限
		/// </summary>
		public const int VALIDATION_MAX_LEVEL = 100;

		// ========== 升級獎勵 ==========

		/// <summary>
		/// 階層式獎勵計算係數：tier * 10（例如 Level 1-10 每級 +10 點）
		/// </summary>
		public const int LEVEL_REWARD_TIER_MULTIPLIER = 10;

		/// <summary>
		/// 升級點數獎勵上限（Level 241-250）
		/// </summary>
		public const int LEVEL_REWARD_MAX = 250;

		/// <summary>
		/// 每日首次達到四項屬性全滿時的經驗值獎勵
		/// </summary>
		public const int DAILY_FULL_STATS_REWARD_EXP = 100;

		/// <summary>
		/// 升級獎勵階層數（每10級一個階層）
		/// </summary>
		public const int LEVEL_REWARD_TIER_SIZE = 10;

		/// <summary>
		/// 最大獎勵階層數（Level 241-250 = tier 25）
		/// </summary>
		public const int LEVEL_REWARD_MAX_TIER = 25;

		// ========== 外觀成本預設值 ==========

		/// <summary>
		/// 膚色變更預設點數成本（當資料庫未設定時）
		/// </summary>
		public const int DEFAULT_SKIN_COLOR_COST = 50;

		/// <summary>
		/// 背景變更預設點數成本（當資料庫未設定時）
		/// </summary>
		public const int DEFAULT_BACKGROUND_COST = 30;

		// ========== 每日屬性衰減 ==========

		/// <summary>
		/// 每日凌晨飢餓值減少量
		/// </summary>
		public const int DAILY_HUNGER_DECAY = -20;

		/// <summary>
		/// 每日凌晨心情值減少量
		/// </summary>
		public const int DAILY_MOOD_DECAY = -30;

		/// <summary>
		/// 每日凌晨體力值減少量
		/// </summary>
		public const int DAILY_STAMINA_DECAY = -10;

		/// <summary>
		/// 每日凌晨清潔值減少量
		/// </summary>
		public const int DAILY_CLEANLINESS_DECAY = -20;
	}
}
