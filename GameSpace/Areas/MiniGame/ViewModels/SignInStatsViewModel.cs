using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.ViewModels
{
    /// <summary>
    /// 簽到管理 ViewModel
    /// </summary>
    public class SignInStatsViewModel
    {
        public int UserID { get; set; }
        
        [Display(Name = "會員帳號")]
        public string UserAccount { get; set; } = string.Empty;
        
        [Display(Name = "會員姓名")]
        public string UserName { get; set; } = string.Empty;
        
        [Display(Name = "簽到日期")]
        [DataType(DataType.Date)]
        public DateTime SignDate { get; set; } = DateTime.Today;
        
        [Display(Name = "連續簽到天數")]
        public int ConsecutiveDays { get; set; }
        
        [Display(Name = "獲得點數")]
        public int PointsEarned { get; set; }
        
        [Display(Name = "寵物經驗值")]
        public int PetExpEarned { get; set; }
        
        [Display(Name = "獲得優惠券ID")]
        public int? CouponEarned { get; set; }
        
        public List<SignInHistoryViewModel> SignInHistory { get; set; } = new();
    }

    public class SignInHistoryViewModel
    {
        public int StatsID { get; set; }
        public DateTime SignTime { get; set; }
        public int PointsEarned { get; set; }
        public int PetExpEarned { get; set; }
        public int? CouponEarned { get; set; }
        public int ConsecutiveDays { get; set; }
    }

    /// <summary>
    /// 簽到規則設定 ViewModel
    /// </summary>
    public class SignInRulesViewModel
    {
        [Display(Name = "平日簽到點數")]
        [Range(0, 1000, ErrorMessage = "點數範圍為 0-1000")]
        public int WeekdayPoints { get; set; } = 20;
        
        [Display(Name = "假日簽到點數")]
        [Range(0, 1000, ErrorMessage = "點數範圍為 0-1000")]
        public int WeekendPoints { get; set; } = 30;
        
        [Display(Name = "平日寵物經驗值")]
        [Range(0, 1000, ErrorMessage = "經驗值範圍為 0-1000")]
        public int WeekdayPetExp { get; set; } = 0;
        
        [Display(Name = "假日寵物經驗值")]
        [Range(0, 1000, ErrorMessage = "經驗值範圍為 0-1000")]
        public int WeekendPetExp { get; set; } = 200;
        
        [Display(Name = "連續7天額外點數")]
        [Range(0, 1000, ErrorMessage = "點數範圍為 0-1000")]
        public int Consecutive7DaysPoints { get; set; } = 40;
        
        [Display(Name = "連續7天額外經驗值")]
        [Range(0, 1000, ErrorMessage = "經驗值範圍為 0-1000")]
        public int Consecutive7DaysPetExp { get; set; } = 300;
        
        [Display(Name = "當月全勤額外點數")]
        [Range(0, 1000, ErrorMessage = "點數範圍為 0-1000")]
        public int MonthlyFullAttendancePoints { get; set; } = 200;
        
        [Display(Name = "當月全勤額外經驗值")]
        [Range(0, 1000, ErrorMessage = "經驗值範圍為 0-1000")]
        public int MonthlyFullAttendancePetExp { get; set; } = 2000;
        
        [Display(Name = "當月全勤優惠券ID")]
        public int? MonthlyFullAttendanceCouponId { get; set; }
    }
}