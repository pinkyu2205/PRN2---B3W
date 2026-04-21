using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class OrderDetail : BaseFullEntity
{
    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? LineTotal { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}

