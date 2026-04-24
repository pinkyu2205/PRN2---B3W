using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class NotificationsModel : PageModel
    {
        private readonly INotificationService _notificationService;
        private readonly Application.Services.Interfaces.IUserService _userService;
        private readonly ILogger<NotificationsModel> _logger;

        public NotificationsModel(INotificationService notificationService, Application.Services.Interfaces.IUserService userService, ILogger<NotificationsModel> logger)
        {
            _notificationService = notificationService;
            _userService = userService;
            _logger = logger;
        }

        [BindProperty]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        public string Message { get; set; } = string.Empty;

        [BindProperty]
        public string Type { get; set; } = "info";

        [BindProperty]
        public int? TargetAccountId { get; set; }

        public List<SelectListItem> AccountOptions { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadAccountsAsync();
        }

        public async Task<IActionResult> OnPostPushAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAccountsAsync();
                return Page();
            }

            try
            {
                var success = await _notificationService.CreateNotificationAsync(TargetAccountId, Title, Message, Type);
                if (success)
                {
                    TempData["Success"] = "Đã gửi thông báo thành công.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi gửi thông báo.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to push notification");
                TempData["Error"] = "Lỗi hệ thống khi gửi thông báo.";
            }

            return RedirectToPage();
        }

        private async Task LoadAccountsAsync()
        {
            var accounts = await _userService.GetAllUsersAsync();
            AccountOptions = accounts.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = $"{a.FullName ?? a.Username} ({a.Email})"
            }).ToList();
        }
    }
}
