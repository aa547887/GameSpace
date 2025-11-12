//using System.Net.Http.Headers;
//using Microsoft.Extensions.Options;

//public class ImgBbOptions { public string ApiKey { get; set; } = ""; }

//public class ImgBbService
//{
//    private readonly HttpClient _http;
//    private readonly ImgBbOptions _opt;
//    public ImgBbService(HttpClient http, IOptions<ImgBbOptions> opt) { _http = http; _opt = opt.Value; }

//    // 傳入 Base64 或 byte[] 上傳，回傳圖檔 URL
//    public async Task<string?> UploadAsync(byte[] bytes)
//    {
//        using var content = new MultipartFormDataContent();
//        content.Add(new StringContent(_opt.ApiKey), "key");
//        content.Add(new ByteArrayContent(bytes) { Headers = { ContentType = MediaTypeHeaderValue.Parse("image/png") } }, "image", "upload.png");

//        var resp = await _http.PostAsync("https://api.imgbb.com/1/upload", content);
//        if (!resp.IsSuccessStatusCode) return null;

//        var json = await resp.Content.ReadAsStringAsync();
//        // 簡單解析（你可用 System.Text.Json 建 model）
//        var displayUrl = System.Text.Json.JsonDocument.Parse(json).RootElement
//            .GetProperty("data").GetProperty("display_url").GetString();
//        return displayUrl;
//    }
//}


//升級版本:新版功能向下相容、參數不變
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

public class ImgBbOptions { public string ApiKey { get; set; } = ""; }

public class ImgBbResponse
{
    public string? Url { get; set; }
    public string? DeleteUrl { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? Size { get; set; }
}

public class ImgBbService
{
    private readonly HttpClient _http;
    private readonly ImgBbOptions _opt;
    public ImgBbService(HttpClient http, IOptions<ImgBbOptions> opt) { _http = http; _opt = opt.Value; }

    public async Task<ImgBbResponse?> UploadAsync(byte[] bytes, string fileName = "upload")
    {
        if (bytes == null || bytes.Length == 0) return null;
        var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        var mime = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "image/png"
        };

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(_opt.ApiKey), "key");
        var ba = new ByteArrayContent(bytes);
        ba.Headers.ContentType = MediaTypeHeaderValue.Parse(mime);
        form.Add(ba, "image", string.IsNullOrWhiteSpace(fileName) ? "upload" : fileName);

        var resp = await _http.PostAsync("https://api.imgbb.com/1/upload", form);
        var json = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode) return null;

        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement.GetProperty("data");
        return new ImgBbResponse
        {
            Url = root.GetProperty("display_url").GetString(),
            DeleteUrl = root.TryGetProperty("delete_url", out var del) ? del.GetString() : null,
            Width = root.TryGetProperty("width", out var w) ? w.GetInt32() : null,
            Height = root.TryGetProperty("height", out var h) ? h.GetInt32() : null,
            Size = root.TryGetProperty("size", out var s) ? s.GetInt64() : null
        };
    }
}

