namespace GameSpace.Areas.MiniGame.Models.Settings
{
    /// <summary>
    /// 寵物背景點數設定
    /// </summary>
    public class PetBackgroundPointSettings
    {
        public int SettingId { get; set; }
        public string BackgroundName { get; set; } = string.Empty;
        public string BackgroundCode { get; set; } = string.Empty;
        public int PointCost { get; set; }
        public int RequiredLevel { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

