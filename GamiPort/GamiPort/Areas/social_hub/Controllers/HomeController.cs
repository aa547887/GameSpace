using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.social_hub.Controllers
{
	public class HomeController : Controller
	{
		[Area("social_hub")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
