using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamiPort.Models;
using GamiPort.Areas.MemberManagement.ViewModels;
using GamiPort.Services; // ICurrentUserService

namespace GamiPort.Areas.MemberManagement.Controllers
{
	[Area("MemberManagement")]
	[Route("MemberManagement/[controller]/[action]")]
	public class MyHomeController : Controller
	{
		private readonly GameSpacedatabaseContext _db;
		private readonly ICurrentUserService _current;

		public MyHomeController(GameSpacedatabaseContext db, ICurrentUserService current)
		{
			_db = db;
			_current = current;
		}

		// --- 小工具：把 byte[] 轉為 <img src> 可用的 Data URL ---
		private static string? ToBase64Src(byte[]? bytes, string mime = "image/png")
			=> (bytes is null || bytes.Length == 0) ? null
			   : $"data:{mime};base64,{Convert.ToBase64String(bytes)}";

		// 依 UserId 取得完整頁面 VM（共用）
		private async Task<HomePageVM?> BuildHomeVmAsync(int ownerUserId, bool isSelf)
		{
			// 主資料：Introduce + UserHome
			var introduce = await _db.UserIntroduces
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.UserId == ownerUserId);

			if (introduce == null) return null;

			var home = await _db.UserHomes
				.AsNoTracking()
				.FirstOrDefaultAsync(h => h.UserId == ownerUserId);

			// 頭像 / 背景：處理預設圖
			var userPictureSrc = ToBase64Src(introduce.UserPicture)
								 ?? Url.Content("~/images/default-avatar.png"); // 若你的 ImagePathService 有更好的預設，可改這行

			var themeSrc = ToBase64Src(home?.Theme)
						   ?? Url.Content("/images/UserHome_DefaultPicture.png");

			// 我的貼文資料（作者 = 屋主）
			var posts = await _db.Threads
				.AsNoTracking()
				.Where(t => t.AuthorUserId == ownerUserId)
				.Select(t => new HomePostRowVM
				{
					ThreadId = t.ThreadId,
					Title = t.Title ?? "(未命名)",
					ReactionsCount = _db.Reactions.Count(r =>
						r.TargetType == "Thread" && r.TargetId == t.ThreadId),
					CommentsCount = _db.ThreadPosts.Count(p => p.ThreadId == t.ThreadId),
					BookmarksCount = _db.Bookmarks.Count(b =>
						b.TargetType == "Thread" && b.TargetId == t.ThreadId),
					CreatedAt = t.CreatedAt
				})
				.OrderByDescending(r => r.CreatedAt)
				.ToListAsync();

			// 好友統計（右側欄使用）
			var statusAcceptedId = await _db.RelationStatuses
				.Where(s => s.StatusCode == "ACCEPTED")
				.Select(s => s.StatusId)
				.FirstOrDefaultAsync();

			var statusPendingId = await _db.RelationStatuses
				.Where(s => s.StatusCode == "PENDING")
				.Select(s => s.StatusId)
				.FirstOrDefaultAsync();

			// 與 owner 相關的好友關係
			var friendAcceptedCount = await _db.Relations
				.CountAsync(r =>
					(r.UserIdSmall == ownerUserId || r.UserIdLarge == ownerUserId) &&
					r.StatusId == statusAcceptedId);

			var friendPendingCount = await _db.Relations
				.CountAsync(r =>
					(r.UserIdSmall == ownerUserId || r.UserIdLarge == ownerUserId) &&
					r.StatusId == statusPendingId);

			var vm = new HomePageVM
			{
				OwnerUserId = ownerUserId,
				IsOwner = isSelf,
				UserNickName = introduce.UserNickName,
				Gender = introduce.Gender,
				IntroText = introduce.UserIntroduce1,
				Title = home?.Title ?? "這裡還沒有標題",
				ThemeSrc = themeSrc,
				UserPictureSrc = userPictureSrc,
				VisitCount = home?.VisitCount ?? 0,
				HomeCode = home?.HomeCode,
				Posts = posts,
				FriendAcceptedCount = friendAcceptedCount,
				FriendPendingCount = friendPendingCount
			};

			return vm;
		}

		// --- 自己的小屋（預設） ---
		[HttpGet]
		public async Task<IActionResult> Index()
		{
			if (!_current.IsAuthenticated)
				return RedirectToAction("Login", "Login", new { area = "Login" });

			int userId = _current.UserId.GetValueOrDefault(); // int? -> int
			var vm = await BuildHomeVmAsync(userId, isSelf: true);
			if (vm == null) return NotFound();

			return View("Index", vm);
		}

		// --- 以 UserId 造訪他人小屋 ---
		[HttpGet("{id:int}")]
		[ActionName("User")] // 路由仍是 /MemberManagement/MyHome/User/{id}
		public async Task<IActionResult> VisitById(int id)
		{
			var isSelf = _current.IsAuthenticated && _current.UserId == id;

			// 先查小屋公開狀態以決定是否可看
			var homeRow = await _db.UserHomes.AsNoTracking().FirstOrDefaultAsync(h => h.UserId == id);
			if (homeRow == null)
			{
				// 沒建立小屋資料，直接 404
				return NotFound();
			}

			// ★ 非屋主且未公開 → 阻擋
			if (!isSelf && !homeRow.IsPublic)
			{
				return View("PrivateBlocked");
			}

			var vm = await BuildHomeVmAsync(id, isSelf);
			if (vm == null) return NotFound();

			// 計數：只有「非本人且可見」時才 +1
			if (!isSelf)
			{
				try
				{
					var forCount = await _db.UserHomes.FirstAsync(h => h.UserId == id);
					forCount.VisitCount += 1;
					forCount.UpdatedAt = DateTime.Now;
					await _db.SaveChangesAsync();
					vm.VisitCount = forCount.VisitCount;
				}
				catch
				{
					// 計數失敗不影響瀏覽，略過即可或寫入 log
				}
			}

			return View("Index", vm);
		}

		// --- 小屋搜尋（暱稱 / HomeCode / UserId） ---
		[HttpGet]
		public async Task<IActionResult> Search(string? q)
		{
			if (string.IsNullOrWhiteSpace(q))
				return RedirectToAction(nameof(Index));

			q = q.Trim();

			if (int.TryParse(q, out var uid))
			{
				var exists = await _db.UserIntroduces.AnyAsync(u => u.UserId == uid);
				if (exists) return RedirectToAction("User", new { id = uid });
			}

			var byCode = await _db.UserHomes
				.Where(h => h.HomeCode != null && h.HomeCode == q)
				.Select(h => h.UserId)
				.FirstOrDefaultAsync();
			if (byCode != 0) return RedirectToAction("User", new { id = byCode });

			var byNick = await _db.UserIntroduces
				.Where(u => u.UserNickName == q)
				.Select(u => u.UserId)
				.FirstOrDefaultAsync();
			if (byNick != 0) return RedirectToAction("User", new { id = byNick });

			TempData["SearchError"] = "找不到符合的小屋。";
			return RedirectToAction(nameof(Index));
		}
	}
}
