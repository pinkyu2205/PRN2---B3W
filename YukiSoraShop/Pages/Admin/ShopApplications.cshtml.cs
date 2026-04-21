using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using YukiSoraShop.Hubs;

namespace YukiSoraShop.Pages.Admin;

[Authorize(Roles = "Administrator")]
public class ShopApplicationsModel : PageModel
{
    private readonly IShopApplicationService _shopAppService;
    private readonly IHubContext<AdminDashboardHub> _hubContext;
    private readonly ILogger<ShopApplicationsModel> _logger;

    public ShopApplicationsModel(
        IShopApplicationService shopAppService,
        IHubContext<AdminDashboardHub> hubContext,
        ILogger<ShopApplicationsModel> logger)
    {
        _shopAppService = shopAppService;
        _hubContext = hubContext;
        _logger = logger;
    }

    public List<ShopApplication> Applications { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty]
    public string? RejectReason { get; set; }

    private int GetAccountId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : 0;
    }

    public async Task OnGetAsync()
    {
        var allApps = await _shopAppService.GetAllApplicationsAsync();

        if (!string.IsNullOrWhiteSpace(StatusFilter) && Enum.TryParse<ShopApplicationStatus>(StatusFilter, out var status))
        {
            Applications = allApps.Where(a => a.Status == status).ToList();
        }
        else
        {
            Applications = allApps;
        }

        // Eager load Applicant - fetch individually if needed
        foreach (var app in Applications)
        {
            if (app.Applicant == null)
            {
                var loaded = await _shopAppService.GetApplicationByIdAsync(app.Id);
                if (loaded?.Applicant != null)
                {
                    app.Applicant = loaded.Applicant;
                }
            }
        }
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var reviewerId = GetAccountId();
        var result = await _shopAppService.ApproveApplicationAsync(id, reviewerId);

        if (result)
        {
            _logger.LogInformation("Shop application {AppId} approved by admin {AdminId}", id, reviewerId);
            TempData["Success"] = "Đã duyệt đơn đăng ký shop thành công!";

            // Notify via SignalR
            await _hubContext.Clients.Group(AdminDashboardHub.AdminGroupName)
                .SendAsync("ShopApplicationUpdated", id, "Approved");
        }
        else
        {
            TempData["Error"] = "Không thể duyệt đơn. Đơn có thể đã được xử lý.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var reviewerId = GetAccountId();
        var reason = RejectReason ?? "Không đáp ứng yêu cầu.";
        var result = await _shopAppService.RejectApplicationAsync(id, reviewerId, reason);

        if (result)
        {
            _logger.LogInformation("Shop application {AppId} rejected by admin {AdminId}", id, reviewerId);
            TempData["Success"] = "Đã từ chối đơn đăng ký.";

            await _hubContext.Clients.Group(AdminDashboardHub.AdminGroupName)
                .SendAsync("ShopApplicationUpdated", id, "Rejected");
        }
        else
        {
            TempData["Error"] = "Không thể từ chối đơn.";
        }

        return RedirectToPage();
    }
}
