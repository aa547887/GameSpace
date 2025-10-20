using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.Forum.Controllers
{
	public class HomeController : Controller
	{
		[Area("Forum")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
