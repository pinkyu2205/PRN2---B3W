using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

[Table("ShopApplications")]
public partial class ShopApplication : BaseFullEntity
{
    public int ApplicantId { get; set; }

    [Required(ErrorMessage = "Tên shop là bắt buộc.")]
    [StringLength(255, ErrorMessage = "Tên shop tối đa 255 ký tự.")]
    public string ShopName { get; set; } = null!;

    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string? Description { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    public ShopApplicationStatus Status { get; set; } = ShopApplicationStatus.Pending;

    [StringLength(1000)]
    public string? AdminNote { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedBy { get; set; }

    public virtual Account Applicant { get; set; } = null!;
}
