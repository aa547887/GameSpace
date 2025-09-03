using GameSpace.Areas.social_hub.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;


namespace YourNamespace.Controllers
{

	//[Area(social_hub)]
	public class ChatController : Controller
	{
		private static List<MessageViewModel> Messages = new List<MessageViewModel>
		{
			new MessageViewModel{ User="Alice", Content="嗨，你好！", Time=DateTime.Now.AddMinutes(-10), IsMine=false },
			new MessageViewModel{ User="Me", Content="你好！最近怎麼樣？", Time=DateTime.Now.AddMinutes(-9), IsMine=true }
		};

		public IActionResult Index()
		{
			return View();
		}

		// 回傳 JSON 訊息列表
		[HttpGet]
		public JsonResult GetMessages()
		{
			return Json(Messages);
		}

		// 發送訊息
		[HttpPost]
		public JsonResult SendMessage([FromForm] string message)
		{
			if (!string.IsNullOrWhiteSpace(message))
			{
				Messages.Add(new MessageViewModel
				{
					User = "Me",
					Content = message,
					Time = DateTime.Now,
					IsMine = true
				});
			}
			return Json(new { success = true });
		}
	}
}
