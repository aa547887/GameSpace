using GameSpace.Models;
using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.Models
{
    public class PetViewModels
    {
        public class PetIndexViewModel
        {
            public List<GameSpace.Models.Pet> Pets { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class PetDetailsViewModel
        {
            public GameSpace.Models.Pet Pet { get; set; } = new();
            public UserWallet Wallet { get; set; } = new();
            public int SenderID { get; set; }
        }

        public class PetCreateViewModel
        {
            [Required]
            [StringLength(50)]
            public string Name { get; set; } = string.Empty;

            [StringLength(100)]
            public string? Species { get; set; }
        }

        public class PetEditViewModel
        {
            public int PetId { get; set; }
            
            [Required]
            [StringLength(50)]
            public string Name { get; set; } = string.Empty;

            [StringLength(100)]
            public string? Species { get; set; }
        }
    }
}
