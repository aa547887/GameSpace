using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.MemberManagement.Models
{
	public class HomeSettingEditVM
	{
		[Display(Name = "小屋標題")]
		[MaxLength(100)]
		public string? Title { get; set; }

		[Display(Name = "是否公開")]
		public bool IsPublic { get; set; }

		[Display(Name = "主題檔案（覆蓋上傳）")]
		public IFormFile? ThemeFile { get; set; }

		[Display(Name = "移除主題")]
		public bool RemoveTheme { get; set; }

		// 顯示用
		public bool HasTheme { get; set; }
		public int? ThemeSizeBytes { get; set; }
		public string? UpdatedAtText { get; set; }
	}
}
