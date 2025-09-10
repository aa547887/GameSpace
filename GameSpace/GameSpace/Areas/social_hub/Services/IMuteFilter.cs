// Areas/social_hub/Services/IMuteFilter.cs
using System.Threading;
using System.Threading.Tasks;

namespace GameSpace.Areas.social_hub.Services
{
	public interface IMuteFilter
	{
		/// 重新載入資料庫詞庫
		Task RefreshAsync(CancellationToken ct = default);

		/// 核心過濾（同步）
		string Apply(string input);

		/// 核心過濾（非同步便捷）
		Task<string> ApplyAsync(string input, CancellationToken ct = default);

		/// === 舊程式相容用別名 ===
		string Filter(string input);
		Task<string> FilterAsync(string input, CancellationToken ct = default);
	}

	public sealed class MuteFilterOptions
	{
		public MaskStyle MaskStyle { get; set; } = MaskStyle.Asterisks;
		public string FixedLabel { get; set; } = "【封鎖】";
		public bool FuzzyBetweenCjkChars { get; set; } = true;
	}

	public enum MaskStyle
	{
		Asterisks,
		FixedLabel
	}
}
