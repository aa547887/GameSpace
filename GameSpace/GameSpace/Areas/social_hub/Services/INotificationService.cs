using System.Collections.Generic;
using System.Threading.Tasks;
using GameSpace.Models;

namespace GameSpace.Areas.social_hub.Services
{
	public interface INotificationService
	{
		/// <summary>
		/// 建立通知主檔並寫入收件人明細。
		/// 會自動判斷 SenderId（使用者）或 SenderManagerId（管理員），避免 FK 衝突。
		/// 回傳「實際新增的收件人數」。
		/// </summary>
		Task<int> CreateAsync(
			Notification notification,
			IEnumerable<int> userIds,
			int? senderUserId,
			int? senderManagerId
		);
	}
}
