using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer;

[Authorize(Roles = "Customer")]
public class RegisterShopModel : PageModel
{
    private readonly IShopApplicationService _shopAppService;
    private readonly IShopService _shopService;
    private readonly ILogger<RegisterShopModel> _logger;

    public RegisterShopModel(
        IShopApplicationService shopAppService,
        IShopService shopService,
        ILogger<RegisterShopModel> logger)
    {
        _shopAppService = shopAppService;
        _shopService = shopService;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "Tên shop là bắt buộc.")]
    [StringLength(255, ErrorMessage = "Tên shop tối đa 255 ký tự.")]
    public string ShopName { get; set; } = string.Empty;

    [BindProperty]
    [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string? Description { get; set; }

    [BindProperty]
    [StringLength(20)]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? PhoneNumber { get; set; }

    [BindProperty]
    [StringLength(500)]
    public string? Address { get; set; }

    public bool HasShop { get; set; }
    public bool HasPendingApplication { get; set; }
    public Shop? ExistingShop { get; set; }
    public List<ShopApplication> MyApplications { get; set; } = new();

    private int GetAccountId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : 0;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var accountId = GetAccountId();
        if (accountId <= 0) return RedirectToPage("/Auth/Login");

        HasShop = await _shopService.HasShopAsync(accountId);
        if (HasShop)
        {
            ExistingShop = await _shopService.GetShopByOwnerIdAsync(accountId);
        }

        HasPendingApplication = await _shopAppService.HasPendingApplicationAsync(accountId);
        MyApplications = await _shopAppService.GetApplicationsByApplicantIdAsync(accountId);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var accountId = GetAccountId();
        if (accountId <= 0) return RedirectToPage("/Auth/Login");

        if (!ModelState.IsValid) return await OnGetAsync();

        // Check if already has shop
        if (await _shopService.HasShopAsync(accountId))
        {
            TempData["Error"] = "Bạn đã có shop rồi.";
            return RedirectToPage();
        }

        // Check if already has pending application
        if (await _shopAppService.HasPendingApplicationAsync(accountId))
        {
            TempData["Error"] = "Bạn đã có đơn đăng ký đang chờ duyệt.";
            return RedirectToPage();
        }

        var application = new ShopApplication
        {
            ApplicantId = accountId,
            ShopName = ShopName,
            Description = Description,
            PhoneNumber = PhoneNumber,
            Address = Address,
            Status = ShopApplicationStatus.Pending,
            CreatedBy = User.Identity?.Name ?? "system",
            ModifiedBy = User.Identity?.Name ?? "system"
        };

        var result = await _shopAppService.SubmitApplicationAsync(application);
        if (result)
        {
            _logger.LogInformation("Shop application submitted by account {AccountId}", accountId);
            TempData["Success"] = "Đã gửi đơn đăng ký shop thành công! Vui lòng chờ admin duyệt.";
        }
        else
        {
            TempData["Error"] = "Không thể gửi đơn đăng ký. Vui lòng thử lại.";
        }

        return RedirectToPage();
    }
}
