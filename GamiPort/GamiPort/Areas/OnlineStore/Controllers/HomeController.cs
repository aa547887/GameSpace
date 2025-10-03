using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.OnlineStore.Controllers
{
	public class HomeController : Controller
	{
		[Area("OnlineStore")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
