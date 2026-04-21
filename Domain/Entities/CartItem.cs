using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class CartItem : BaseFullEntity
{
    public int CartId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
