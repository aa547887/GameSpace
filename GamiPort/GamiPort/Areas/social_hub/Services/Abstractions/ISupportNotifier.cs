using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.social_hub.Services.Abstractions
{
	/// <summary>把訊息廣播到 ticket 群組</summary>
	public interface ISupportNotifier
	{
		Task BroadcastMessageAsync(SupportMessageDto msg, CancellationToken ct = default);
	}
}
