using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Payment : BaseFullEntity
{
    public int OrderId { get; set; }

    public int PaymentMethodId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public string? TransactionRef { get; set; }

    public string? RawCallbackJson { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}

