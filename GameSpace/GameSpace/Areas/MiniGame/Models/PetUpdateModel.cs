using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class PetUpdateModel
    {
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public string SkinColor { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public int Experience { get; set; }
        public int Level { get; set; }
        public int Hunger { get; set; }
        public int Mood { get; set; }
        public int Stamina { get; set; }
        public int Health { get; set; }
        public int Cleanliness { get; set; }
    }
}
