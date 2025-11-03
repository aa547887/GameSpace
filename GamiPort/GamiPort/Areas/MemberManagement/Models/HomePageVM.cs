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

		public bool IsPublic { get; set; }

		// Posts
		public List<HomePostRowVM> Posts { get; set; } = new();

		// Friends summary
		public int FriendAcceptedCount { get; set; }
		public int FriendPendingCount { get; set; }

		// Friends detail list
		public List<FriendInfoVM> FriendAcceptedList { get; set; } = new();
		public List<FriendInfoVM> FriendPendingList { get; set; } = new();

		/// <summary>
		/// 當前登入使用者與此小屋擁有者之間的關係狀態代碼 (例如: NONE, PENDING, ACCEPTED, BLOCKED)。
		/// 僅當當前使用者已登入且非小屋擁有者時有意義。
		/// </summary>
		public string CurrentRelationStatus { get; set; } = "NONE";

		/// <summary>
		/// 若目前關係為 PENDING，這裡帶出發起請求者的 UserId（Relations.RequestedBy）。
		/// 其他狀態可為 null。
		/// </summary>
		public int? CurrentRelationRequestedByUserId { get; set; }
	}
	public class FriendInfoVM
	{
		public int UserId { get; set; }
		public string NickName { get; set; } = "";
	}
}
