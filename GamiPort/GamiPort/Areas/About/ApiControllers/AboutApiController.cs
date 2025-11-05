using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.About.ApiControllers
{
	public class AboutApiController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
