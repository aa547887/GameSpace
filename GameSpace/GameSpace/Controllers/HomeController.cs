// Controllers/HomeController.cs
using System;
using System.Diagnostics;
using System.Security.Claims;
using GameSpace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Controllers
{
	[AllowAnonymous] // �����n�J���X�Ȥ]��ݨ���׭�
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		public HomeController(ILogger<HomeController> logger) => _logger = logger;

		public IActionResult Index()
		{
			try
			{
				if (User?.Identity?.IsAuthenticated == true && User.HasClaim(c => c.Type == "ManagerId"))
				{
					// �Y AdminDashboard �b�Y�� Area�A�令�Gnew { area = "MemberManagement" }
					return RedirectToAction("Index", "AdminDashboard");
				}
				return View();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Home/Index �o�ͨҥ~�A��ɺ��׭�");
				return RedirectToAction(nameof(Maintenance));
			}
		}

		public IActionResult Privacy() => View();

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
			=> View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

		public IActionResult Dashboard() => View();

		// ===== ���׭� =====
		[HttpGet]
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Maintenance()
		{
			Response.StatusCode = 503; // Service Unavailable
			return View();             // ���� Views/Home/Maintenance.cshtml
		}

		// �Τ@�B�z���A�X�]�t�X Program.cs �� UseStatusCodePagesWithReExecute("/Home/Http{0}")�^
		[HttpGet("Home/Http{code:int}")]
		public IActionResult Http(int code)
		{
			if (code >= 500 || code == 404)
			{
				Response.StatusCode = 503;
				return View("Maintenance");
			}
			return View("Error"); // 404 ���i�ΧA�{���� Error.cshtml
		}

		// ���աG�y�@�ӥ��B�z�ҥ~�]�s�� /Home/Boom ���ɦV���׭��^
		[HttpGet("Home/Force500")]
		public IActionResult Force500() => StatusCode(500);
	}
}

