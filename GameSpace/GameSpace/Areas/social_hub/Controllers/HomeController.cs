using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
