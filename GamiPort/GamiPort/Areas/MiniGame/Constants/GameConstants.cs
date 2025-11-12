namespace GamiPort.Areas.MiniGame.Constants
{
	/// <summary>
	/// 遊戲系統常數定義（前台）
	/// 定義小遊戲相關的所有 Magic Numbers
	/// </summary>
	public static class GameConstants
	{
		// ========== 遊戲限制 ==========

		/// <summary>
		/// 每日遊戲次數預設上限（從 SystemSettings 讀取，若失敗則使用此預設值）
		/// </summary>
		public const int DEFAULT_DAILY_GAME_LIMIT = 3;

		/// <summary>
		/// 最低難度等級
		/// </summary>
		public const int MIN_DIFFICULTY_LEVEL = 1;

		/// <summary>
		/// 最高難度等級
		/// </summary>
		public const int MAX_DIFFICULTY_LEVEL = 3;

		/// <summary>
		/// 剩餘次數下限（不可為負數）
		/// </summary>
		public const int REMAINING_PLAYS_FLOOR = 0;

		// ========== 怪物數量配置 ==========

		/// <summary>
		/// Level 1 難度的怪物數量
		/// </summary>
		public const int MONSTER_COUNT_LEVEL_1 = 3;

		/// <summary>
		/// Level 2 難度的怪物數量
		/// </summary>
		public const int MONSTER_COUNT_LEVEL_2 = 5;

		/// <summary>
		/// Level 3 難度的怪物數量
		/// </summary>
		public const int MONSTER_COUNT_LEVEL_3 = 7;

		// ========== 速度倍數配置 ==========

		/// <summary>
		/// Level 1 難度的遊戲速度倍數
		/// </summary>
		public const decimal SPEED_MULTIPLIER_LEVEL_1 = 1.0m;

		/// <summary>
		/// Level 2 難度的遊戲速度倍數
		/// </summary>
		public const decimal SPEED_MULTIPLIER_LEVEL_2 = 1.2m;

		/// <summary>
		/// Level 3 難度的遊戲速度倍數
		/// </summary>
		public const decimal SPEED_MULTIPLIER_LEVEL_3 = 1.5m;

		// ========== 寵物屬性變化（勝利） ==========

		/// <summary>
		/// 遊戲勝利時飢餓值變化（降低）
		/// </summary>
		public const int PET_HUNGER_DELTA_WIN = -20;

		/// <summary>
		/// 遊戲勝利時心情值變化（提升）
		/// </summary>
		public const int PET_MOOD_DELTA_WIN = 30;

		/// <summary>
		/// 遊戲勝利時體力值變化（降低）
		/// </summary>
		public const int PET_STAMINA_DELTA_WIN = -20;

		/// <summary>
		/// 遊戲勝利時清潔值變化（降低）
		/// </summary>
		public const int PET_CLEANLINESS_DELTA_WIN = -20;

		// ========== 寵物屬性變化（失敗/中止） ==========

		/// <summary>
		/// 遊戲失敗/中止時飢餓值變化（降低）
		/// </summary>
		public const int PET_HUNGER_DELTA_LOSE = -20;

		/// <summary>
		/// 遊戲失敗/中止時心情值變化（降低）
		/// </summary>
		public const int PET_MOOD_DELTA_LOSE = -30;

		/// <summary>
		/// 遊戲失敗/中止時體力值變化（降低）
		/// </summary>
		public const int PET_STAMINA_DELTA_LOSE = -20;

		/// <summary>
		/// 遊戲失敗/中止時清潔值變化（降低）
		/// </summary>
		public const int PET_CLEANLINESS_DELTA_LOSE = -20;

		// ========== 遊戲初始化默認值 ==========

		/// <summary>
		/// 遊戲開始時的經驗值初始值
		/// </summary>
		public const int INITIAL_EXP_GAINED = 0;

		/// <summary>
		/// 遊戲開始時的點數初始值
		/// </summary>
		public const int INITIAL_POINTS_GAINED = 0;

		/// <summary>
		/// 遊戲開始時的飢餓值變化初始值
		/// </summary>
		public const int INITIAL_HUNGER_DELTA = 0;

		/// <summary>
		/// 遊戲開始時的心情值變化初始值
		/// </summary>
		public const int INITIAL_MOOD_DELTA = 0;

		/// <summary>
		/// 遊戲開始時的體力值變化初始值
		/// </summary>
		public const int INITIAL_STAMINA_DELTA = 0;

		/// <summary>
		/// 遊戲開始時的清潔值變化初始值
		/// </summary>
		public const int INITIAL_CLEANLINESS_DELTA = 0;

		/// <summary>
		/// 遊戲開始時的優惠券初始值（空字串）
		/// </summary>
		public const string INITIAL_COUPON_GAINED = "";

		// ========== 遊戲狀態字串 ==========

		/// <summary>
		/// 遊戲進行中狀態
		/// </summary>
		public const string GAME_RESULT_IN_PROGRESS = "進行中";

		/// <summary>
		/// 遊戲勝利狀態
		/// </summary>
		public const string GAME_RESULT_WIN = "Win";

		/// <summary>
		/// 遊戲失敗狀態
		/// </summary>
		public const string GAME_RESULT_LOSE = "Lose";

		/// <summary>
		/// 遊戲中止狀態
		/// </summary>
		public const string GAME_RESULT_ABORT = "Abort";

		// ========== SystemSettings 鍵名 ==========

		/// <summary>
		/// SystemSettings 表中每日遊戲限制的鍵名
		/// </summary>
		public const string SETTING_KEY_DAILY_GAME_LIMIT = "DailyGameLimit";

		// ========== 時間計算 ==========

		/// <summary>
		/// 計算當日結束時間時加上的天數
		/// </summary>
		public const int DAYS_TO_ADD_FOR_DAY_END = 1;

		/// <summary>
		/// 計算 23:59:59.9999999 時減去的 Tick 數
		/// </summary>
		public const long TICKS_TO_SUBTRACT_FOR_DAY_END = -1;
	}
}
