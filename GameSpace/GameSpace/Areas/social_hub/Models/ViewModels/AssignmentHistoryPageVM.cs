// Areas/social_hub/Models/ViewModels/AssignmentHistoryPageVM.cs
using System.Collections.Generic;

namespace GameSpace.Areas.social_hub.Models.ViewModels
{
	public class AssignmentHistoryPageVM
	{
		public int TicketId { get; set; }
		public int UserId { get; set; }
		public string Subject { get; set; } = string.Empty;

		// 指派歷程清單（用 Scaffold 出來的根層實體）
		public IReadOnlyList<GameSpace.Models.SupportTicketAssignment> Items { get; set; }
			= new List<GameSpace.Models.SupportTicketAssignment>();
	}
}
