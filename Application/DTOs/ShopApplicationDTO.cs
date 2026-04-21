using Domain.Enums;

namespace Application.DTOs;

public class ShopApplicationDTO
{
    public int Id { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public ShopApplicationStatus Status { get; set; }
    public string? AdminNote { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
