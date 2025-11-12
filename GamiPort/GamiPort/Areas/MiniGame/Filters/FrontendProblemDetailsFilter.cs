using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GamiPort.Areas.MiniGame.Filters
{
	/// <summary>
	/// 前台統一異常處理過濾器 - 返回符合 RFC 7807 的 ProblemDetails 格式
	/// </summary>
	public class FrontendProblemDetailsFilter : IExceptionFilter
	{
		private readonly ILogger<FrontendProblemDetailsFilter> _logger;

		public FrontendProblemDetailsFilter(ILogger<FrontendProblemDetailsFilter> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// 當 Action 拋出異常時統一處理
		/// </summary>
		public void OnException(ExceptionContext context)
		{
			if (context.Exception == null) return;

			var traceId = context.HttpContext.TraceIdentifier;
			var statusCode = GetStatusCode(context.Exception);

			// 記錄異常（包含 TraceId 用於追蹤）
			_logger.LogError(context.Exception,
				"未處理的異常: TraceId={TraceId}, Path={Path}, StatusCode={StatusCode}",
				traceId, context.HttpContext.Request.Path, statusCode);

			// 構造 ProblemDetails 格式響應
			var problemDetails = new
			{
				type = GetTypeUri(statusCode),
				title = GetUserFriendlyTitle(context.Exception),
				status = statusCode,
				detail = GetUserFriendlyMessage(context.Exception),
				instance = context.HttpContext.Request.Path.Value,
				traceId = traceId
			};

			context.Result = new JsonResult(problemDetails)
			{
				StatusCode = statusCode
			};

			context.HttpContext.Response.ContentType = "application/problem+json";
			context.ExceptionHandled = true;
		}

		/// <summary>
		/// 根據異常類型映射 HTTP 狀態碼
		/// </summary>
		private int GetStatusCode(Exception exception)
		{
			return exception switch
			{
				ArgumentNullException => 400,
				ArgumentException => 400,
				UnauthorizedAccessException => 401,
				KeyNotFoundException => 404,
				InvalidOperationException => 409,
				NotImplementedException => 501,
				_ => 500
			};
		}

		/// <summary>
		/// 獲取 ProblemDetails type URI
		/// </summary>
		private string GetTypeUri(int statusCode)
		{
			return statusCode switch
			{
				400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
				401 => "https://tools.ietf.org/html/rfc7235#section-3.1",
				404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
				409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
				500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
				501 => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
				_ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
			};
		}

		/// <summary>
		/// 獲取用戶友善的錯誤標題
		/// </summary>
		private string GetUserFriendlyTitle(Exception exception)
		{
			return exception switch
			{
				ArgumentNullException => "請求參數缺失",
				ArgumentException => "請求參數錯誤",
				UnauthorizedAccessException => "未授權訪問",
				KeyNotFoundException => "找不到資源",
				InvalidOperationException => "操作失敗",
				NotImplementedException => "功能尚未開放",
				_ => "系統暫時無法處理請求"
			};
		}

		/// <summary>
		/// 獲取用戶友善的錯誤訊息（生產環境不洩漏技術細節）
		/// </summary>
		private string GetUserFriendlyMessage(Exception exception)
		{
			return exception switch
			{
				ArgumentNullException => "請確認所有必填欄位已填寫",
				ArgumentException => "部分欄位格式不正確，請檢查後重試",
				UnauthorizedAccessException => "您沒有權限執行此操作，請先登入",
				KeyNotFoundException => "請求的資源不存在或已被移除",
				InvalidOperationException => "目前無法完成此操作，請稍後再試",
				NotImplementedException => "此功能尚在開發中，敬請期待",
				_ => "伺服器發生錯誤，請稍後再試或聯繫客服"
			};
		}
	}
}
