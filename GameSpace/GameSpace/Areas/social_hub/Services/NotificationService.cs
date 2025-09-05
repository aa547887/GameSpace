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
			// 1) 決定 Sender 欄位（避免 FK 衝突）
			if (senderManagerId.HasValue && senderManagerId.Value > 0)
			{
				notification.SenderManagerId = senderManagerId;
				//notification.SenderId = null;
			}
			else if (senderUserId.HasValue && senderUserId.Value > 0)
			{
				//notification.SenderId = senderUserId;
				notification.SenderManagerId = null;
			}
			else
			{
				// 系統訊息
				//notification.SenderId = null;
				notification.SenderManagerId = null;
			}

			// 2) CreatedAt 若為非 Nullable DateTime，就用 default 判斷
			if (notification.CreatedAt == default(DateTime))
			{
				notification.CreatedAt = DateTime.UtcNow;
			}

			// 3) 寫主檔
			_db.Notifications.Add(notification);
			await _db.SaveChangesAsync();

			// 4) 去重 + 過濾非法 ID
			var targets = (userIds ?? Array.Empty<int>())
				.Distinct()
				.Where(id => id > 0)
				.ToList();

			if (targets.Count == 0) return 0;

			// 5) 僅保留存在的使用者（避免 FK 失敗）
			var validUserIds = await _db.Users.AsNoTracking()
				.Where(u => targets.Contains(u.UserId))
				.Select(u => u.UserId)
				.ToListAsync();

			// 6) 寫收件人明細（移除 DeliveredAt，避免找不到欄位）
			foreach (var uid in validUserIds)
			{
				var rec = new NotificationRecipient
				{
					NotificationId = notification.NotificationId,
					UserId = uid,
					IsRead = false
					// 如果你的模型有下列欄位，可自行解除註解：
					// CreatedAt = DateTime.UtcNow,
					// SendAt = DateTime.UtcNow
				};
				_db.NotificationRecipients.Add(rec);
			}

			await _db.SaveChangesAsync();
			return validUserIds.Count;
		}
	}
}
