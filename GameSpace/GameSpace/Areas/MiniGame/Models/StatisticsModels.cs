using GameSpace.Models;

namespace GameSpace.Areas.MiniGame.Models
{
    public class SignInStatisticsReadModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public decimal SignInRate { get; set; }
        public List<TopUserReadModel> TopUsers { get; set; } = new();
    }

    public class SystemDiagnosticsReadModel
    {
        public bool DatabaseConnection { get; set; }
        public bool EmailService { get; set; }
        public bool FileSystem { get; set; }
        public DateTime LastChecked { get; set; }
        public string SystemStatus { get; set; } = string.Empty;
    }
}
