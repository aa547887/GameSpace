using System.Threading.Tasks;

namespace GamiPort.Areas.OnlineStore.Services
{
	/// <summary>
	/// 查核用輕服務：只負責確認「配送方式 / 付款方式」是否存在且可用
	/// </summary>
	public interface ILookupService
	{
		/// <summary>配送方式是否存在（true=合法）</summary>
		Task<bool> ShipMethodExistsAsync(int shipMethodId);

		/// <summary>付款方式是否存在（true=合法）</summary>
		Task<bool> PayMethodExistsAsync(int payMethodId);
	}
}
