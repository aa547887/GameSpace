using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class OrderInfo
{
    public int OrderId { get; set; }

    public long OrderCode { get; set; }

    public int UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public decimal OrderTotal { get; set; }

    public DateTime? PaymentAt { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual OrderAddress? OrderAddress { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
