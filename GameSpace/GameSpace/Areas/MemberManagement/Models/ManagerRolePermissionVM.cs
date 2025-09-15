using GameSpace.Models;
using System;
using System.ComponentModel.DataAnnotations;
using GameSpace.Areas.MemberManagement.Models;

namespace GameSpace.Areas.MemberManagement.Models
{ 

public partial class ManagerRolePermissionVM
{
    [Display(Name = "管理員角色編號")]
	public int ManagerRoleId { get; set; }
    [Display(Name = "管理員角色名稱")]
	public string RoleName { get; set; } = string.Empty;
		[Display(Name = "最高權限系統管理")] 
	public bool AdministratorPrivilegesManagement { get; set; }
    [Display(Name = "使用者系統管理")]
	public bool UserStatusManagement { get; set; }
    [Display(Name = "商品系統管理")]
	public bool ShoppingPermissionManagement { get; set; }
    [Display(Name = "論壇系統管理")]

	public bool MessagePermissionManagement { get; set; }
    [Display(Name = "寵物系統管理")]
	public bool PetRightsManagement { get; set; }
    [Display(Name = "客服系統管理")]
	public bool CustomerService { get; set; }

		//public List<string> ManagerNames { get; set; } = new List<string>();
		//public ManagerRoleEntry ManagerRole { get; set; }
		
	}
}
