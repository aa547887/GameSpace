using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace GamiPort.Areas.OnlineStore.Utils
{
	/// <summary>
	/// ECPay CheckMacValue 工具（SHA256, encType=1）
	/// </summary>
	public static class EcpayChecksum
	{
		public static string Generate(IDictionary<string, string> fields, string hashKey, string hashIV)
		{
			// 1) 排除空值與 CheckMacValue 自身
			var filtered = fields
				.Where(kv => !string.IsNullOrWhiteSpace(kv.Value)
							 && !string.Equals(kv.Key, "CheckMacValue", StringComparison.OrdinalIgnoreCase))
				.ToDictionary(kv => kv.Key, kv => kv.Value);

			// 2) 依字母順序排序（不分大小寫）
			var sorted = filtered.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase);

			// 3) 串接成 querystring（key=value&...）
			var s = string.Join("&", sorted.Select(kv => $"{kv.Key}={kv.Value}"));

			// 4) 前後加上 HashKey/HashIV 並 URL encode（注意大小寫不敏感）
			var raw = $"HashKey={hashKey}&{s}&HashIV={hashIV}";
			var urlEncoded = HttpUtility.UrlEncode(raw).ToLowerInvariant();

			// 5) SHA256 → 十六進位大寫
			using var sha = SHA256.Create();
			var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(urlEncoded));
			var sb = new StringBuilder();
			foreach (var b in bytes) sb.Append(b.ToString("X2"));
			return sb.ToString();
		}

		public static bool Verify(IDictionary<string, string> fields, string hashKey, string hashIV)
		{
			if (!fields.TryGetValue("CheckMacValue", out var provided)) return false;
			var calculated = Generate(fields, hashKey, hashIV);
			return string.Equals(provided, calculated, StringComparison.OrdinalIgnoreCase);
		}
	}
}
