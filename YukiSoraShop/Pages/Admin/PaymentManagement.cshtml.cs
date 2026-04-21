using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator,Moderator")]
    public class PaymentManagementModel : PageModel
    {
        private readonly IPaymentHistoryService _paymentHistoryService;
        private readonly ILogger<PaymentManagementModel> _logger;

        public PaymentManagementModel(IPaymentHistoryService paymentHistoryService, ILogger<PaymentManagementModel> logger)
        {
            _paymentHistoryService = paymentHistoryService;
            _logger = logger;
        }

        public List<PaymentHistoryDto> Payments { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 50;

        public int TotalPayments => Payments.Count;
        public int PaidCount => Payments.Count(p => p.Status == Domain.Enums.PaymentStatus.Paid);
        public int PendingCount => Payments.Count(p => p.Status == Domain.Enums.PaymentStatus.Pending);
        public int CanceledCount => Payments.Count(p => p.Status == Domain.Enums.PaymentStatus.Canceled);
        public decimal TotalRevenue => Payments.Where(p => p.Status == Domain.Enums.PaymentStatus.Paid).Sum(p => p.Amount);

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading payment history for admin - Page: {Page}, Size: {Size}", PageNumber, PageSize);
                
                Payments = await _paymentHistoryService.GetAllPaymentHistoryAsync(PageNumber, PageSize);
                
                _logger.LogInformation("Loaded {Count} payments for admin view", Payments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment history for admin");
                TempData["ErrorMessage"] = "Không th? t?i danh sách thanh toán. Vui lòng th? l?i.";
                Payments = new List<PaymentHistoryDto>();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDetailAsync(int id)
        {
            try
            {
                _logger.LogInformation("Loading payment detail for ID {PaymentId}", id);
                
                var payment = await _paymentHistoryService.GetPaymentByIdAsync(id);
                
                if (payment == null)
                {
                    _logger.LogWarning("Payment {PaymentId} not found", id);
                    return NotFound(new { error = "Không tìm th?y giao d?ch" });
                }

                return new JsonResult(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment detail for ID {PaymentId}", id);
                return BadRequest(new { error = "Không th? t?i chi ti?t thanh toán" });
            }
        }
    }
}
