// Areas/OnlineStore/ViewModels/OrdersVms.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GamiPort.Areas.OnlineStore.ViewModels
{
	// ─────────────────────────────────────────────────────────────
	// 訂單清單「每一列」VM（Index/_OrderCard.cshtml 會用到）
	// ─────────────────────────────────────────────────────────────
	public sealed class OrdersListItemVm
	{
		public int OrderId { get; set; }
		public string OrderCode { get; set; } = "";
		public DateTime? CreatedAt { get; set; }
		public decimal GrandTotal { get; set; }

		public string? OrderStatus { get; set; }       // 例如：未付款 / 已完成 / 已取消
		public string? PaymentStatus { get; set; }     // 例如：已付款 / 未付款
		public string? ShipmentStatus { get; set; }    // 例如：已出貨 / 未出貨

		// 舊命名相容
		public string? PayStatus { get; set; }
		public string? PayMethod { get; set; }

		// 5 態鍵值（你的前端若有用 stepper 也可以用這個）
		public string StatusKey
		{
			get
			{
				if (string.Equals(OrderStatus, "已取消")) return "canceled";
				if (string.Equals(OrderStatus, "已完成")) return "completed";
				if (string.Equals(ShipmentStatus, "已出貨")) return "shipped";
				if (string.Equals(PaymentStatus, "已付款")) return "paid";
				return "unpaid";
			}
		}

		// ★ 新增：給 Index.cshtml / _OrderStatusBadge 用
		public string StatusText
		{
			get
			{
				if (string.Equals(OrderStatus, "已取消")) return "已取消";
				if (string.Equals(OrderStatus, "已完成")) return "已完成";
				if (string.Equals(ShipmentStatus, "已出貨")) return "已出貨";
				if (string.Equals(PaymentStatus, "已付款")) return "已付款";
				return "未付款";
			}
		}
	}

	// ─────────────────────────────────────────────────────────────
	// 訂單清單頁 VM（Index.cshtml / _OrdersListPartial.cshtml）
	// ─────────────────────────────────────────────────────────────
	public sealed class OrdersListVm
	{
		// 查詢/分頁參數
		public string? Status { get; set; } = "";
		public string? Keyword { get; set; } = "";
		public DateTime? DateFrom { get; set; }
		public DateTime? DateTo { get; set; }
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;

		// 結果
		public List<OrdersListItemVm> Items { get; } = new();
		public int TotalCount { get; set; }
		public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

		// ✅ View 目前使用的命名（Index.cshtml 第 25~29 行）
		public int UnpaidCount { get; set; }
		public int PaidCount { get; set; }
		public int ShippedCount { get; set; }
		public int CompletedCount { get; set; }
		public int CanceledCount { get; set; }

		// 也保留我之前的命名（以防其他地方用到）
		public int CntUnpaid { get => UnpaidCount; set => UnpaidCount = value; }
		public int CntPaid { get => PaidCount; set => PaidCount = value; }
		public int CntShipped { get => ShippedCount; set => ShippedCount = value; }
		public int CntCompleted { get => CompletedCount; set => CompletedCount = value; }
		public int CntCanceled { get => CanceledCount; set => CanceledCount = value; }
	}

	// ─────────────────────────────────────────────────────────────
	// 訂單明細「單一商品列」VM（Detail.cshtml 用）
	// ─────────────────────────────────────────────────────────────
	public sealed class OrderItemVm
	{
		public int LineNo { get; set; }                 // ✅ View 需要（Detail.cshtml 第 38 行）
		public int ProductId { get; set; }
		public string? ProductCode { get; set; }
		public string ProductName { get; set; } = "";
		public decimal UnitPrice { get; set; }
		public int Quantity { get; set; }
		public decimal LineTotal => UnitPrice * Quantity;
	}

	// ─────────────────────────────────────────────────────────────
	// 訂單抬頭 VM（Detail.cshtml 用）
	// ─────────────────────────────────────────────────────────────
	public sealed class OrderHeadVm
	{
		public int OrderId { get; set; }
		public string OrderCode { get; set; } = "";
		public DateTime CreatedAt { get; set; }
		public string? OrderStatus { get; set; }
		public string? PaymentStatus { get; set; }
		public string? ShipmentStatus { get; set; }
		public decimal GrandTotal { get; set; }
		public string? Recipient { get; set; }
		public string? Address { get; set; }
		public string? Phone { get; set; }
		public string? PayMethod { get; set; }

		// ✅ View 需要（Detail.cshtml 第 13、16 行會取 StatusText）
		public string StatusText
		{
			get
			{
				if (string.Equals(OrderStatus, "已取消")) return "已取消";
				if (string.Equals(OrderStatus, "已完成")) return "已完成";
				if (string.Equals(ShipmentStatus, "已出貨")) return "已出貨";
				if (string.Equals(PaymentStatus, "已付款")) return "已付款";
				return "未付款";
			}
		}
	}

	// ─────────────────────────────────────────────────────────────
	// 付款紀錄（Detail.cshtml 用）
	// ─────────────────────────────────────────────────────────────
	public sealed class PaymentVm
	{
		public string? Provider { get; set; }
		public string? ProviderTxn { get; set; }
		public string? Status { get; set; }
		public DateTime? CreatedAt { get; set; }        // ✅ View 需要（Detail.cshtml 第 62 行）
		public decimal? Amount { get; set; }
		public string? Message { get; set; }

		// ✅ View 需要（Detail.cshtml 第 66 行可能使用 StatusText）
		public string StatusText => Status ?? "";

		// ✅ 有的 View 可能誤用成 ToString("yyyy-MM-dd ...")
		//   這不是覆寫（.NET 的 ToString 不能加參數），我們提供同名不同簽章的「工具函式」讓 Razor 呼叫不報錯。
		public string ToString(string? fmt)
		{
			if (CreatedAt.HasValue && !string.IsNullOrWhiteSpace(fmt))
				return CreatedAt.Value.ToString(fmt);
			return CreatedAt?.ToString() ?? "";
		}

		// 仍保留標準的覆寫，避免顯示時是型別名稱
		public override string ToString()
		{
			var when = CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "";
			var amt = Amount.HasValue ? Amount.Value.ToString("0") : "";
			return $"[{when}] {Status ?? ""} {amt}";
		}
	}

	// ─────────────────────────────────────────────────────────────
	// 出貨紀錄（Detail.cshtml 若有需要）
	// ─────────────────────────────────────────────────────────────
	public sealed class ShipmentVm
	{
		public string? Provider { get; set; }
		public string? TrackingNo { get; set; }
		public DateTime? TrackTime { get; set; }
		public string? Message { get; set; }
	}

	// ─────────────────────────────────────────────────────────────
	// 訂單明細頁 VM（Detail.cshtml）
	// ─────────────────────────────────────────────────────────────
	public sealed class OrderDetailVm
	{
		public OrderHeadVm Head { get; set; } = new();
		public List<OrderItemVm> Items { get; } = new();
		public List<PaymentVm> Payments { get; } = new();
		public List<ShipmentVm> Shipments { get; } = new();
	}

	
}
