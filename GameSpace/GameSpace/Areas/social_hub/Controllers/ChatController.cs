using GameSpace.Areas.social_hub.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GameSpace.Areas.social_hub.Controllers
{
	[Area("social_hub")]
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

		[HttpGet]
		public JsonResult GetMessages()
		{
			return Json(Messages);
		}

		[HttpPost]
		public JsonResult SendMessage([FromForm] string message)
		{
			if (!string.IsNullOrWhiteSpace(message))
			{
				// 加入自己的訊息
				Messages.Add(new MessageViewModel
				{
					User = "Me",
					Content = message,
					Time = DateTime.Now,
					IsMine = true
				});

				// 模擬對方的回覆（延遲 1 秒）
				Task.Run(async () =>
				{
					await Task.Delay(1000);
					Messages.Add(new MessageViewModel
					{
						User = "Alice",
						Content = GetAutoReply(message),
						Time = DateTime.Now,
						IsMine = false
					});
				});
			}
			return Json(new { success = true });
		}

		// 簡單的自動回覆邏輯
		private string GetAutoReply(string input)
		{
			if (input.Contains("你好") || input.Contains("hi", StringComparison.OrdinalIgnoreCase))
				return "嗨嗨！很高興跟你聊天～";
			if (input.Contains("天氣"))
				return "今天天氣還不錯，你有出去走走嗎？";
			if (input.Contains("掰"))
				return "好呀～下次再聊！";
			return "嗯嗯，我懂你的意思！";
		}
	}
}
