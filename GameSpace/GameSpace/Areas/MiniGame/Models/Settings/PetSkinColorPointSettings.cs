namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物膚色點數設定
    /// </summary>
    public class PetSkinColorPointSettings
    {
        public int SettingId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string ColorCode { get; set; } = string.Empty;
        public int PointCost { get; set; }
        public int RequiredLevel { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

