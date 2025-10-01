using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.ViewModels
{
    /// <summary>
    /// 寵物管理 ViewModel
    /// </summary>
    public class PetViewModel
    {
        public int PetID { get; set; }
        public int UserID { get; set; }
        
        [Display(Name = "會員帳號")]
        public string UserAccount { get; set; } = string.Empty;
        
        [Display(Name = "會員姓名")]
        public string UserName { get; set; } = string.Empty;
        
        [Display(Name = "寵物名稱")]
        [Required(ErrorMessage = "寵物名稱為必填")]
        [StringLength(50, ErrorMessage = "寵物名稱不能超過50字")]
        public string PetName { get; set; } = string.Empty;
        
        [Display(Name = "寵物類型")]
        [Required(ErrorMessage = "寵物類型為必填")]
        [StringLength(30, ErrorMessage = "寵物類型不能超過30字")]
        public string PetType { get; set; } = string.Empty;
        
        [Display(Name = "寵物等級")]
        [Range(1, 250, ErrorMessage = "寵物等級範圍為 1-250")]
        public int PetLevel { get; set; } = 1;
        
        [Display(Name = "寵物經驗值")]
        [Range(0, int.MaxValue, ErrorMessage = "經驗值不能為負數")]
        public int PetExp { get; set; } = 0;
        
        [Display(Name = "寵物膚色")]
        [Required(ErrorMessage = "寵物膚色為必填")]
        [StringLength(30, ErrorMessage = "寵物膚色不能超過30字")]
        public string PetSkin { get; set; } = "default";
        
        [Display(Name = "寵物背景")]
        [Required(ErrorMessage = "寵物背景為必填")]
        [StringLength(30, ErrorMessage = "寵物背景不能超過30字")]
        public string PetBackground { get; set; } = "default";
        
        [Display(Name = "飢餓值")]
        [Range(0, 100, ErrorMessage = "飢餓值範圍為 0-100")]
        public int Hunger { get; set; } = 100;
        
        [Display(Name = "心情值")]
        [Range(0, 100, ErrorMessage = "心情值範圍為 0-100")]
        public int Happiness { get; set; } = 100;
        
        [Display(Name = "健康值")]
        [Range(0, 100, ErrorMessage = "健康值範圍為 0-100")]
        public int Health { get; set; } = 100;
        
        [Display(Name = "體力值")]
        [Range(0, 100, ErrorMessage = "體力值範圍為 0-100")]
        public int Energy { get; set; } = 100;
        
        [Display(Name = "清潔值")]
        [Range(0, 100, ErrorMessage = "清潔值範圍為 0-100")]
        public int Cleanliness { get; set; } = 100;
        
        public DateTime CreatedAt { get; set; }
        public DateTime? LastFed { get; set; }
        public DateTime? LastPlayed { get; set; }
        public DateTime? LastBathed { get; set; }
        public DateTime? LastSlept { get; set; }
    }

    /// <summary>
    /// 寵物規則設定 ViewModel
    /// </summary>
    public class PetRulesViewModel
    {
        [Display(Name = "可選擇的寵物類型")]
        public List<string> AvailablePetTypes { get; set; } = new();
        
        [Display(Name = "可選擇的寵物膚色")]
        public List<string> AvailablePetSkins { get; set; } = new();
        
        [Display(Name = "可選擇的寵物背景")]
        public List<string> AvailablePetBackgrounds { get; set; } = new();
        
        [Display(Name = "換膚色所需點數")]
        [Range(0, 10000, ErrorMessage = "點數範圍為 0-10000")]
        public int SkinChangeCost { get; set; } = 2000;
        
        [Display(Name = "換背景所需點數")]
        [Range(0, 10000, ErrorMessage = "點數範圍為 0-10000")]
        public int BackgroundChangeCost { get; set; } = 1000;
        
        [Display(Name = "每日屬性衰減 - 飢餓值")]
        [Range(0, 50, ErrorMessage = "衰減值範圍為 0-50")]
        public int DailyHungerDecay { get; set; } = 20;
        
        [Display(Name = "每日屬性衰減 - 心情值")]
        [Range(0, 50, ErrorMessage = "衰減值範圍為 0-50")]
        public int DailyHappinessDecay { get; set; } = 30;
        
        [Display(Name = "每日屬性衰減 - 體力值")]
        [Range(0, 50, ErrorMessage = "衰減值範圍為 0-50")]
        public int DailyEnergyDecay { get; set; } = 10;
        
        [Display(Name = "每日屬性衰減 - 清潔值")]
        [Range(0, 50, ErrorMessage = "衰減值範圍為 0-50")]
        public int DailyCleanlinessDecay { get; set; } = 20;
    }
}