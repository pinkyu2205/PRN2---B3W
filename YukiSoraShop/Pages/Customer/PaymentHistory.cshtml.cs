using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer
{
    [Authorize(Roles = "Customer,Administrator,Moderator")]
    public class PaymentHistoryModel : PageModel
    {
        private readonly IPaymentHistoryService _paymentHistoryService;
        private readonly ILogger<PaymentHistoryModel> _logger;

        public PaymentHistoryModel(IPaymentHistoryService paymentHistoryService, ILogger<PaymentHistoryModel> logger)
        {
            _paymentHistoryService = paymentHistoryService;
            _logger = logger;
        }

        public List<PaymentHistoryDto> Payments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountIdClaim) || !int.TryParse(accountIdClaim, out var accountId))
            {
                _logger.LogWarning("Invalid account ID claim");
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                Payments = await _paymentHistoryService.GetUserPaymentHistoryAsync(accountId);
                _logger.LogInformation("Loaded {Count} payments for account {AccountId}", Payments.Count, accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment history for account {AccountId}", accountId);
                TempData["ErrorMessage"] = "Không th? t?i l?ch s? thanh toán. Vui lòng th? l?i sau.";
            }

            return Page();
        }
    }
}
