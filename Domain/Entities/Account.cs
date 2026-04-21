using Domain.Common;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Account : BaseFullEntity
{
    public string UserName { get; set; } = null!;

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string Address { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string AvatarUrl { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string? Status { get; set; }

    public bool IsExternal { get; set; } = false;

    public string? ExternalProvider { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

