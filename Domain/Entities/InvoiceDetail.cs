using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class InvoiceDetail : BaseFullEntity
{
    public int InvoiceId { get; set; }

    public int? ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? LineTotal { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}

