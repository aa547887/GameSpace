using System;
using System.Collections.Generic;

namespace GamiPort.Areas.MemberManagement.ViewModels
{
	public class HomePostRowVM
	{
		public long ThreadId { get; set; }
		public string Title { get; set; } = "";
		public int ReactionsCount { get; set; }
		public int CommentsCount { get; set; }
		public int BookmarksCount { get; set; }
		public DateTime? CreatedAt { get; set; }
	}

	public class HomePageVM
	{
		public int OwnerUserId { get; set; }
		public bool IsOwner { get; set; }

		// Header / 主視覺
		public string UserNickName { get; set; } = "";
		public string Gender { get; set; } = "";
		public string? IntroText { get; set; }
		public string Title { get; set; } = "";
		public string ThemeSrc { get; set; } = "";
		public string UserPictureSrc { get; set; } = "";
		public int VisitCount { get; set; }
		public string? HomeCode { get; set; }

		// Posts
		public List<HomePostRowVM> Posts { get; set; } = new();

		// Friends summary
		public int FriendAcceptedCount { get; set; }
		public int FriendPendingCount { get; set; }
	}
}
