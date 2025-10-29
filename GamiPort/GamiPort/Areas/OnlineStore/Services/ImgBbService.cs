using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

public class ImgBbOptions { public string ApiKey { get; set; } = ""; }

public class ImgBbService
{
    private readonly HttpClient _http;
    private readonly ImgBbOptions _opt;
    public ImgBbService(HttpClient http, IOptions<ImgBbOptions> opt) { _http = http; _opt = opt.Value; }

    // 傳入 Base64 或 byte[] 上傳，回傳圖檔 URL
    public async Task<string?> UploadAsync(byte[] bytes)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(_opt.ApiKey), "key");
        content.Add(new ByteArrayContent(bytes) { Headers = { ContentType = MediaTypeHeaderValue.Parse("image/png") } }, "image", "upload.png");

        var resp = await _http.PostAsync("https://api.imgbb.com/1/upload", content);
        if (!resp.IsSuccessStatusCode) return null;

        var json = await resp.Content.ReadAsStringAsync();
        // 簡單解析（你可用 System.Text.Json 建 model）
        var displayUrl = System.Text.Json.JsonDocument.Parse(json).RootElement
            .GetProperty("data").GetProperty("display_url").GetString();
        return displayUrl;
    }
}
