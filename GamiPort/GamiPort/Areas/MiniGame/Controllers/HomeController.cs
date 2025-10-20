using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.MiniGame.Controllers
{
	public class HomeController : Controller
	{
		[Area("MiniGame")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
