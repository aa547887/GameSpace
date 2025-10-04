using System.ComponentModel.DataAnnotations;

namespace GameSpace.Areas.MiniGame.ViewModels;

public class SignInRuleConfigViewModel
{
    [Display(Name = "每日點數")]
    [Range(0, 1000)]
    public int DailyPoints { get; set; }

    [Display(Name = "每週獎勵點數")]
    [Range(0, 5000)]
    public int WeeklyBonusPoints { get; set; }

    [Display(Name = "每月獎勵點數")]
    [Range(0, 10000)]
    public int MonthlyBonusPoints { get; set; }

    [Display(Name = "連續天數要求")]
    [Range(1, 30)]
    public int ConsecutiveDaysRequired { get; set; }

    [Display(Name = "規則描述")]
    [StringLength(500)]
    public string? Description { get; set; }
}
