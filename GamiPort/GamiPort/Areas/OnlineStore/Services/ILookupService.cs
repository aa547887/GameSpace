using System.Collections.Generic;
using System.Threading.Tasks;
using GamiPort.Areas.OnlineStore.DTO;

namespace GamiPort.Areas.OnlineStore.Services
{
	/// <summary>
	/// 前台結帳用到的「查詢下拉清單／存在性檢查」統一入口
	/// </summary>
	public interface ILookupService
	{
		// 配送方式
		Task<IReadOnlyList<ShipMethodDto>> GetShipMethodsAsync();
		Task<bool> ShipMethodExistsAsync(int shipMethodId);

		// 付款方式
		Task<IReadOnlyList<PayMethodDto>> GetPayMethodsAsync();
		Task<bool> PayMethodExistsAsync(int payMethodId);
	}
}
