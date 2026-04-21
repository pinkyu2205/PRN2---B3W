namespace Application.DTOs;

public class ShopDTO
{
    public int Id { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}
