namespace GamiPort.Areas.MiniGame.Services
{
	/// <summary>
	/// QR Code 生成服務介面
	/// 用於電子禮券顯示予店員核銷
	/// </summary>
	public interface IQRCodeService
	{
		/// <summary>
		/// 生成 QR Code 圖片（Base64 格式）
		/// </summary>
		/// <param name="content">要編碼的內容（如電子禮券代碼）</param>
		/// <param name="pixelsPerModule">每個模組的像素大小（預設 20）</param>
		/// <returns>Base64 編碼的 PNG 圖片字串（可直接用於 img src）</returns>
		string GenerateQRCodeBase64(string content, int pixelsPerModule = 20);

		/// <summary>
		/// 生成 QR Code 圖片（Byte Array 格式）
		/// </summary>
		/// <param name="content">要編碼的內容</param>
		/// <param name="pixelsPerModule">每個模組的像素大小（預設 20）</param>
		/// <returns>PNG 圖片的 Byte Array</returns>
		byte[] GenerateQRCodeBytes(string content, int pixelsPerModule = 20);
	}
}
