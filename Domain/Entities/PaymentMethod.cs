using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class PaymentMethod : BaseFullEntity
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

