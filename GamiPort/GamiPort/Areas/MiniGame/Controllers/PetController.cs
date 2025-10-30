using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.MiniGame.Controllers
{
	[Area("MiniGame")]
	public class PetController : Controller
	{
		// TODO: 注入 GameSpacedatabaseContext 和相關服務
		// private readonly GameSpacedatabaseContext _context;
		// private readonly IPetService _petService;
		// private readonly IMiniGameService _miniGameService;

		public IActionResult Index()
		{
			// Nick鐘新增：手動檢查登入狀態，未登入跳轉到正確的登入頁面（4個Login）
			if (User.Identity?.IsAuthenticated != true)
			{
				var returnUrl = "/MiniGame/Pet/Index";
				return Redirect($"/Login/Login/Login/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
			}

			// TODO: 實作寵物互動功能（根據 README_合併版.md 第 3.3 和 3.4 節）
			// 寵物系統：
			// 1. 寵物名字修改
			// 2. 寵物互動（餵食/洗澡/玩耍/哄睡）
			// 3. 寵物換膚色（扣會員點數）
			// 4. 寵物換背景（可免費或需點數）
			//
			// 小遊戲系統（整合在此頁面）：
			// 5. 【出發冒險】按鈕：啟動遊戲流程，回傳 sessionId、startTime、預估結束時間、當日剩餘可玩次數
			// 6. 查看遊戲紀錄：每場 startTime/endTime/result(win|lose|abort)/獎勵（點數/寵物經驗/商城優惠券）

			return View();
		}
	}
}
