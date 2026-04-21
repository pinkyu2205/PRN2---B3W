using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Cart : BaseFullEntity
{
    public int AccountId { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Account Account { get; set; } = null!;
}

