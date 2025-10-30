using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.MiniGame.Controllers
{
	[Area("MiniGame")]
	public class SignInController : Controller
	{
		// TODO: 注入 GameSpacedatabaseContext 和相關服務
		// private readonly GameSpacedatabaseContext _context;
		// private readonly ISignInService _signInService;

		public IActionResult Index()
		{
			// Nick鐘新增：手動檢查登入狀態，未登入跳轉到正確的登入頁面（4個Login）
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/SignIn/Index";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			// TODO: 實作簽到功能（根據 README_合併版.md 第 3.2 節）
			// 1. 查看月曆型簽到簿並執行簽到
			// 2. 查看簽到歷史紀錄（何時簽到與獎品：點數/寵物經驗/商城優惠券）

			return View();
		}
	}
}
