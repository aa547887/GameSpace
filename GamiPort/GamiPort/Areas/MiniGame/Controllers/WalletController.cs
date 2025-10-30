using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.MiniGame.Controllers
{
	[Area("MiniGame")]
	public class WalletController : Controller
	{
		// TODO: 注入 GameSpacedatabaseContext 和相關服務
		// private readonly GameSpacedatabaseContext _context;
		// private readonly IWalletService _walletService;

		public IActionResult Index()
		{
			// Nick鐘新增：手動檢查登入狀態，未登入跳轉到正確的登入頁面（4個Login）
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/Wallet/Index";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			// TODO: 實作錢包功能（根據 README_合併版.md 第 3.1 節）
			// 1. 查看當前會員點數餘額
			// 2. 使用會員點數兌換商城優惠券及電子優惠券
			// 3. 查看目前擁有商城優惠券
			// 4. 查看目前擁有電子優惠券
			// 5. 使用電子優惠券（以 QRCode/Barcode 顯示予店員核銷）
			// 6. 查看收支明細（點數得到/花費、商城優惠券得到/使用、電子優惠券得到/使用之時間/點數/張數/種類）

			return View();
		}
	}
}
