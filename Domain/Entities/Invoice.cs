using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Invoice : BaseFullEntity
{
    public int OrderId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal Subtotal { get; set; }

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual Order Order { get; set; } = null!;
}

