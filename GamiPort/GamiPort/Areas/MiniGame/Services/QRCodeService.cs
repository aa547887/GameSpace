using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace GamiPort.Areas.MiniGame.Services
{
	/// <summary>
	/// QR Code 生成服務實作
	/// 使用 QRCoder 套件生成 QR Code 圖片
	/// </summary>
	public class QRCodeService : IQRCodeService
	{
		private readonly ILogger<QRCodeService> _logger;

		public QRCodeService(ILogger<QRCodeService> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// 生成 QR Code 圖片（Base64 格式）
		/// </summary>
		/// <param name="content">要編碼的內容（如電子禮券代碼）</param>
		/// <param name="pixelsPerModule">每個模組的像素大小（預設 20）</param>
		/// <returns>Base64 編碼的 PNG 圖片字串（格式：data:image/png;base64,...）</returns>
		public string GenerateQRCodeBase64(string content, int pixelsPerModule = 20)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(content))
				{
					_logger.LogWarning("QR Code 內容為空，無法生成");
					return string.Empty;
				}

				// 驗證參數範圍
				if (pixelsPerModule < 1 || pixelsPerModule > 100)
				{
					_logger.LogWarning("pixelsPerModule 超出範圍 (1-100)，使用預設值 20");
					pixelsPerModule = 20;
				}

				var qrBytes = GenerateQRCodeBytes(content, pixelsPerModule);
				if (qrBytes == null || qrBytes.Length == 0)
				{
					return string.Empty;
				}

				var base64String = Convert.ToBase64String(qrBytes);
				return $"data:image/png;base64,{base64String}";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "生成 QR Code Base64 失敗: Content={Content}", content);
				return string.Empty;
			}
		}

		/// <summary>
		/// 生成 QR Code 圖片（Byte Array 格式）
		/// </summary>
		/// <param name="content">要編碼的內容</param>
		/// <param name="pixelsPerModule">每個模組的像素大小（預設 20）</param>
		/// <returns>PNG 圖片的 Byte Array</returns>
		public byte[] GenerateQRCodeBytes(string content, int pixelsPerModule = 20)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(content))
				{
					_logger.LogWarning("QR Code 內容為空，無法生成");
					return Array.Empty<byte>();
				}

				// 驗證參數範圍
				if (pixelsPerModule < 1 || pixelsPerModule > 100)
				{
					_logger.LogWarning("pixelsPerModule 超出範圍 (1-100)，使用預設值 20");
					pixelsPerModule = 20;
				}

				// 使用 QRCodeGenerator 生成 QR Code 資料
				using var qrGenerator = new QRCodeGenerator();
				using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
				using var qrCode = new QRCode(qrCodeData);

				// 生成 Bitmap 圖片
				using var qrBitmap = qrCode.GetGraphic(pixelsPerModule);

				// 將 Bitmap 轉換為 Byte Array
				using var ms = new MemoryStream();
				qrBitmap.Save(ms, ImageFormat.Png);
				return ms.ToArray();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "生成 QR Code Bytes 失敗: Content={Content}, PixelsPerModule={PixelsPerModule}",
					content, pixelsPerModule);
				return Array.Empty<byte>();
			}
		}
	}
}
