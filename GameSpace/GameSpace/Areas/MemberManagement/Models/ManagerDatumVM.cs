using GameSpace.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GameSpace.Areas.MemberManagement.Models;

public partial class ManagerDatumVM
{
    [Display(Name ="管理者編號")]
	public int ManagerId { get; set; } 
	[Display(Name = "管理者姓名")]
	public string? ManagerName { get; set; } = string.Empty;
	[Display(Name = "管理者帳號")]
	public string? ManagerAccount { get; set; } = string.Empty;
	[Display(Name = "管理者密碼")]
	public string? ManagerPassword { get; set; }

    public DateTime? AdministratorRegistrationDate { get; set; }
	[Display(Name = "管理者信箱")]
	public string ManagerEmail { get; set; } = null!;

    public bool ManagerEmailConfirmed { get; set; }

    public int ManagerAccessFailedCount { get; set; }

    public bool ManagerLockoutEnabled { get; set; }

    public DateTime? ManagerLockoutEnd { get; set; }


    public virtual ICollection<Mute> Mutes { get; set; } = new List<Mute>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();


    
	public ICollection<ManagerRoleEntry> ManagerRoles { get; set; } = new List<ManagerRoleEntry>();

}
