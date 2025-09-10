public interface IMuteFilter
{
	Task<string> FilterAsync(string input);

	// 非同步：詞庫更新時可呼叫，立即重建快取
	Task RefreshAsync();

	// 同步：為了相容現有呼叫點（IMuteFilter.Refresh()）
	void Refresh();
}
