using Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("Shops")]
public partial class Shop : BaseFullEntity
{
    [Required(ErrorMessage = "Tên shop là bắt buộc.")]
    [StringLength(255, ErrorMessage = "Tên shop tối đa 255 ký tự.")]
    public string ShopName { get; set; } = null!;

    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? LogoUrl { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public int OwnerId { get; set; }

    public virtual Account Owner { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
