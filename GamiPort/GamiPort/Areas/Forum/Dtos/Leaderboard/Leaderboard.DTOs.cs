namespace GamiPort.Areas.Forum.Dtos.Leaderboard
{
    // 回傳的單筆排行榜項目
    public sealed class LeaderboardItemDto
    {
        public int Rank { get; set; }            // 1..N（Dense Rank）
        public int GameId { get; set; }
        public string Name { get; set; } = "";
        public decimal Index { get; set; }       // popularity_index_daily.index_value
        public int? Delta { get; set; }          // 與上一個日期的名次差：+2 / -1 / null(新進)
        public string Trend { get; set; } = "";  // "up" | "down" | "same" | "new"
    }

    // 排行榜回包（含日期資訊）
    public sealed class LeaderboardDailyResponseDto
    {
        public string Date { get; set; } = "";       // yyyy-MM-dd
        public string? PrevDate { get; set; }        // yyyy-MM-dd or null
        public List<LeaderboardItemDto> Items { get; set; } = new();
    }
}
