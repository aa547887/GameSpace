using System;
using System.Collections.Generic;

namespace GamiPort.Models;

public partial class SVRankingFavorite
{
    public DateOnly RankingDate { get; set; }

    public byte PeriodType { get; set; }

    public int ProductId { get; set; }

    public int RankingPosition { get; set; }

    public decimal? FavoriteCount { get; set; }

    public string? MetricNote { get; set; }
}
