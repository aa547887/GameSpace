// Areas/OnlineStore/ViewModels/Orders VMs
using System;
using System.Collections.Generic;

namespace GamiPort.Areas.OnlineStore.ViewModels
{
    // 列表頁每列資料
    public sealed class OrdersListItemVm
    {
		public int OrderId { get; set; }
		public string OrderCode { get; set; } = "";
		public DateTime CreatedAt { get; set; }
		public decimal GrandTotal { get; set; }

		// 👇 新增：畫面使用
		public string StatusText { get; set; } = "";   // 中文「未出貨/已出貨/...」
		public string StatusKey { get; set; } = "";   // unpaid/paid/shipped/completed/canceled
		public string PayMethod { get; set; } = "";   // 付款方式名稱
		public string PayStatus { get; set; } = "";   // 付款狀態
	}

    // 列表頁整體 VM（含統計與分頁）
    public sealed class OrdersListVm
    {
        public List<OrdersListItemVm> Items { get; } = new();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int UnpaidCount { get; set; }
        public int PaidCount { get; set; }
        public int ShippedCount { get; set; }
        public int CompletedCount { get; set; }
        public int CanceledCount { get; set; }

        public string? Status { get; set; }
        public string? Keyword { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    // 明細頁頭
    public sealed class OrderHeadVm
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; } = default!;
        public string StatusText { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Recipient { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
    }

	// 明細頁的商品
	// 明細中的每一項
	public sealed class OrderItemVm
	{
		public int LineNo { get; set; }
		public string ProductName { get; set; } = "";
		public decimal UnitPrice { get; set; }
		public int Quantity { get; set; }
		public decimal LineTotal { get; set; }
		public int ProductId { get; set; }  // ★ 新增
	}

	// 明細頁的付款紀錄
	public sealed class PaymentVm
    {
        public string? Provider { get; set; }
        public string? ProviderTxn { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? StatusText { get; set; }
    }

    // 明細頁的物流紀錄
    public sealed class ShipmentVm
    {
        public string? ShipmentCode { get; set; }
        public string? Provider { get; set; }
        public string? TrackingNo { get; set; }
        public DateTime? TrackTime { get; set; }
        public string? Message { get; set; }
    }

    // 明細頁整體 VM
    public sealed class OrderDetailVm
    {
        public OrderHeadVm Head { get; set; } = new();
        public List<OrderItemVm> Items { get; } = new();
        public List<PaymentVm> Payments { get; } = new();
        public List<ShipmentVm> Shipments { get; } = new();
    }
}
