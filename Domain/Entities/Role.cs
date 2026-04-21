using Domain.Common;

namespace Domain.Entities;

public partial class Role : BaseFullEntity
{
    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}

