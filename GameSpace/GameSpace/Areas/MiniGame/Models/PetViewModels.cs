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
            [Required(ErrorMessage = "請輸入寵物名稱")]
            [StringLength(50, ErrorMessage = "寵物名稱不能超過50字")]
            public string Name { get; set; } = string.Empty;

            [StringLength(100, ErrorMessage = "物種不能超過100字")]
            public string? Species { get; set; }
        }

        public class PetEditViewModel
        {
            public int PetId { get; set; }
            
            [Required(ErrorMessage = "請輸入寵物名稱")]
            [StringLength(50, ErrorMessage = "寵物名稱不能超過50字")]
            public string Name { get; set; } = string.Empty;

            [StringLength(100, ErrorMessage = "物種不能超過100字")]
            public string? Species { get; set; }
        }
    }

    // 寵物管理相關 ViewModels
    public class PetManagementViewModel
    {
        public List<Pet> Pets { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public PetQueryModel Query { get; set; } = new();
        public PetSummary Summary { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class PetColorChangeViewModel
    {
        [Required(ErrorMessage = "請選擇寵物")]
        public int PetId { get; set; }
        
        [Required(ErrorMessage = "請選擇新膚色")]
        public string NewColor { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入原因")]
        [StringLength(200, ErrorMessage = "原因不能超過200字")]
        public string Reason { get; set; } = string.Empty;
        
        public List<string> AvailableColors { get; set; } = new();
        public int Cost { get; set; }
    }

    public class PetBackgroundChangeViewModel
    {
        [Required(ErrorMessage = "請選擇寵物")]
        public int PetId { get; set; }
        
        [Required(ErrorMessage = "請選擇新背景")]
        public string NewBackground { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "請輸入原因")]
        [StringLength(200, ErrorMessage = "原因不能超過200字")]
        public string Reason { get; set; } = string.Empty;
        
        public List<string> AvailableBackgrounds { get; set; } = new();
        public int Cost { get; set; }
    }

    public class PetInteractionViewModel
    {
        [Required(ErrorMessage = "請選擇寵物")]
        public int PetId { get; set; }
        
        [Required(ErrorMessage = "請選擇互動類型")]
        public string InteractionType { get; set; } = string.Empty;
        
        public List<string> AvailableInteractions { get; set; } = new();
        public Dictionary<string, int> InteractionEffects { get; set; } = new();
    }

    public class PetLevelUpViewModel
    {
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int CurrentLevel { get; set; }
        public int CurrentExperience { get; set; }
        public int RequiredExperience { get; set; }
        public int ExperienceNeeded { get; set; }
        public List<string> LevelUpRewards { get; set; } = new();
    }

    public class PetStatusViewModel
    {
        public int PetId { get; set; }
        public string PetName { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Hunger { get; set; }
        public int Happiness { get; set; }
        public int Health { get; set; }
        public int Energy { get; set; }
        public int Cleanliness { get; set; }
        public string SkinColor { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public DateTime LastInteraction { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
