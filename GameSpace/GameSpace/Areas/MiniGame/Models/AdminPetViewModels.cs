using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class AdminPetIndexViewModel
    {
        public List<GameSpace.Models.Pet> Pets { get; set; } = new();
        public SidebarViewModel Sidebar { get; set; } = new();
        public PetSummary PetSummary { get; set; } = new();
        public PetQueryModel Query { get; set; } = new();
    }

    public class AdminPetRulesViewModel
    {
        public PetRuleReadModel PetRule { get; set; } = new();
        public SidebarViewModel Sidebar { get; set; } = new();
    }

    public class AdminPetDetailsViewModel
    {
        public GameSpace.Models.Pet Pet { get; set; } = new();
        public SidebarViewModel Sidebar { get; set; } = new();
    }

    public class AdminPetEditViewModel
    {
        public GameSpace.Models.Pet Pet { get; set; } = new();
        public SidebarViewModel Sidebar { get; set; } = new();
    }

    public class AdminPetSkinColorChangeLogViewModel
    {
        public List<PetSkinColorChangeLog> SkinColorChangeLogs { get; set; } = new();
        public SidebarViewModel Sidebar { get; set; } = new();
        public PetQueryModel Query { get; set; } = new();
    }

    public class AdminPetBackgroundColorChangeLogViewModel
    {
        public List<PetBackgroundColorChangeLog> BackgroundColorChangeLogs { get; set; } = new();
        public SidebarViewModel Sidebar { get; set; } = new();
        public PetQueryModel Query { get; set; } = new();
    }
}
