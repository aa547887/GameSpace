using Microsoft.AspNetCore.Mvc;

namespace GamiPort.Areas.MemberManagement.Controllers
{
	public class HomeController : Controller
	{
		[Area("MemberManagement")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
