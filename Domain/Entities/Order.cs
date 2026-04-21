using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Order : BaseFullEntity
{
    public int AccountId { get; set; }

    public DateTime? PriceLockedUntil { get; set; }

    public string Status { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal? GrandTotal { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Account Account { get; set; } = null!;
}

