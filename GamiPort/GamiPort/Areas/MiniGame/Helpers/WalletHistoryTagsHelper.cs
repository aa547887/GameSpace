using System.Text.Json;

namespace GamiPort.Areas.MiniGame.Helpers
{
	/// <summary>
	/// 錢包歷史記錄多屬性標籤輔助類
	/// 用於管理交易記錄的多重分類（例如：點數+寵物、點數+優惠券等）
	/// </summary>
	public static class WalletHistoryTagsHelper
	{
		/// <summary>
		/// 標籤類型常量
		/// </summary>
		public static class Tags
		{
			public const string Point = "Point";           // 點數
			public const string Pet = "Pet";               // 寵物
			public const string SignIn = "SignIn";         // 簽到
			public const string Coupon = "Coupon";         // 優惠券
			public const string EVoucher = "EVoucher";     // 電子禮券
			public const string MiniGame = "MiniGame";     // 小遊戲
		}

		/// <summary>
		/// 將多個標籤組合成 ChangeType 字符串
		/// 格式：使用 JSON 數組 ["Tag1", "Tag2"] 或單一字符串 "Tag1"（向後兼容）
		/// </summary>
		/// <param name="tags">標籤數組</param>
		/// <returns>ChangeType 字符串</returns>
		public static string BuildChangeType(params string[] tags)
		{
			if (tags == null || tags.Length == 0)
			{
				return Tags.Point; // 預設為點數
			}

			// 去重並排序
			var uniqueTags = tags.Distinct().OrderBy(t => t).ToArray();

			// 如果只有一個標籤，直接返回（向後兼容）
			if (uniqueTags.Length == 1)
			{
				return uniqueTags[0];
			}

			// 多個標籤使用 JSON 格式
			return JsonSerializer.Serialize(uniqueTags);
		}

		/// <summary>
		/// 解析 ChangeType 為標籤數組
		/// </summary>
		/// <param name="changeType">ChangeType 字符串</param>
		/// <returns>標籤數組</returns>
		public static string[] ParseChangeType(string? changeType)
		{
			if (string.IsNullOrWhiteSpace(changeType))
			{
				return new[] { Tags.Point };
			}

			// 嘗試解析 JSON
			if (changeType.StartsWith("[") && changeType.EndsWith("]"))
			{
				try
				{
					var tags = JsonSerializer.Deserialize<string[]>(changeType);
					return tags ?? new[] { Tags.Point };
				}
				catch
				{
					// JSON 解析失敗，當作單一標籤處理
				}
			}

			// 單一標籤（向後兼容）
			return new[] { changeType };
		}

		/// <summary>
		/// 檢查 ChangeType 是否包含指定標籤
		/// </summary>
		/// <param name="changeType">ChangeType 字符串</param>
		/// <param name="tag">要檢查的標籤</param>
		/// <returns>是否包含該標籤</returns>
		public static bool HasTag(string? changeType, string tag)
		{
			var tags = ParseChangeType(changeType);
			return tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// 取得標籤對應的顯示名稱
		/// </summary>
		public static string GetTagDisplayName(string tag)
		{
			return tag switch
			{
				Tags.Point => "點數",
				Tags.Pet => "寵物",
				Tags.SignIn => "簽到",
				Tags.Coupon => "優惠券",
				Tags.EVoucher => "電子禮券",
				Tags.MiniGame => "小遊戲",
				_ => tag
			};
		}

		/// <summary>
		/// 取得標籤對應的 Bootstrap Badge 樣式
		/// </summary>
		public static string GetTagBadgeClass(string tag)
		{
			return tag switch
			{
				Tags.Point => "bg-primary",
				Tags.Pet => "bg-success",
				Tags.SignIn => "bg-info",
				Tags.Coupon => "bg-warning text-dark",
				Tags.EVoucher => "bg-danger",
				Tags.MiniGame => "bg-secondary",
				_ => "bg-secondary"
			};
		}

		/// <summary>
		/// 取得標籤對應的 Font Awesome 圖示
		/// </summary>
		public static string GetTagIcon(string tag)
		{
			return tag switch
			{
				Tags.Point => "fa-coins",
				Tags.Pet => "fa-paw",
				Tags.SignIn => "fa-calendar-check",
				Tags.Coupon => "fa-ticket-alt",
				Tags.EVoucher => "fa-gift",
				Tags.MiniGame => "fa-gamepad",
				_ => "fa-question-circle"
			};
		}

		/// <summary>
		/// 根據描述自動推斷應該添加的標籤
		/// 用於向後兼容舊記錄
		/// </summary>
		public static string[] InferTagsFromDescription(string? description, string? itemCode)
		{
			var tags = new List<string>();

			if (string.IsNullOrWhiteSpace(description))
			{
				return new[] { Tags.Point };
			}

			// 檢查描述中的關鍵字
			var desc = description.ToLower();

			// 簽到相關
			if (desc.Contains("簽到") || itemCode?.Contains("SIGNIN") == true)
			{
				tags.Add(Tags.SignIn);
			}

			// 寵物相關
			if (desc.Contains("寵物") || desc.Contains("升級") || desc.Contains("膚色") ||
			    desc.Contains("背景") || desc.Contains("全滿") || itemCode?.Contains("PET") == true)
			{
				tags.Add(Tags.Pet);
			}

			// 優惠券相關
			if (desc.Contains("優惠券") || desc.Contains("商城優惠券") || itemCode?.Contains("CPN") == true)
			{
				tags.Add(Tags.Coupon);
			}

			// 電子禮券相關
			if (desc.Contains("電子禮券") || desc.Contains("禮券") || itemCode?.Contains("EV-") == true)
			{
				tags.Add(Tags.EVoucher);
			}

			// 小遊戲相關
			if (desc.Contains("遊戲") || desc.Contains("冒險") || itemCode?.Contains("GAME") == true)
			{
				tags.Add(Tags.MiniGame);
			}

			// 點數相關（只要有點數變動就加入）
			if (desc.Contains("點數") || desc.Contains("點") || tags.Count == 0)
			{
				tags.Add(Tags.Point);
			}

			return tags.Distinct().ToArray();
		}
	}
}
