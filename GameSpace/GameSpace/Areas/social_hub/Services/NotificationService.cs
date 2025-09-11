using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GameSpace.Data;
using GameSpace.Models;

namespace GameSpace.Areas.social_hub.Services
{
	public class NotificationService : INotificationService
	{
		private readonly GameSpacedatabaseContext _db;

		public NotificationService(GameSpacedatabaseContext db)
		{
			_db = db;
		}

		public async Task<int> CreateAsync(
			Notification notification,
			IEnumerable<int> userIds,
			int? senderUserId,
			int? senderManagerId
		)
		{
			// 1) 寄件者二擇一（或兩者皆空＝系統）
			if (senderManagerId.HasValue && senderManagerId.Value > 0)
			{
				notification.SenderManagerId = senderManagerId;
				notification.SenderUserId = null;          // ← 改這裡（不要用 SenderId）
			}
			else if (senderUserId.HasValue && senderUserId.Value > 0)
			{
				notification.SenderUserId = senderUserId;  // ← 改這裡
				notification.SenderManagerId = null;
			}
			else
			{
				notification.SenderUserId = null;
				notification.SenderManagerId = null;
			}

			// 2) 建立時間（若模型非 nullable）
			if (notification.CreatedAt == default)
				notification.CreatedAt = DateTime.UtcNow;

			// 3) 寫入主表
			_db.Notifications.Add(notification);
			await _db.SaveChangesAsync(); // 取得 NotificationId

			// 4) 目標名單：去重、過濾非法
			var targets = (userIds ?? Array.Empty<int>())
				.Distinct()
				.Where(id => id > 0)
				.ToList();

			if (targets.Count == 0) return 0;

			// 5) 僅保留存在的使用者（避免 FK 失敗）
			var validUserIds = await _db.Users
				.AsNoTracking()
				.Where(u => targets.Contains(u.UserId))
				.Select(u => u.UserId)
				.ToListAsync();

			if (validUserIds.Count == 0) return 0;

			// 6) 寫入收件人（ReadAt 預設 NULL，不要設定 IsRead）
			var recs = validUserIds.Select(uid => new NotificationRecipient
			{
				NotificationId = notification.NotificationId,
				UserId = uid,
				// ReadAt = null  // 預設就是 NULL
			});
			_db.NotificationRecipients.AddRange(recs);

			await _db.SaveChangesAsync();
			return validUserIds.Count;
		}
	}
}
