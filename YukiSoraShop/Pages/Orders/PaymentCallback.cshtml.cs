using Application.Payments.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Orders
{
    [Authorize]
    public class OrdersPaymentCallbackModel : PageModel
    {
        private readonly IPaymentOrchestrator _payment;
        private readonly ILogger<OrdersPaymentCallbackModel> _logger;

        public OrdersPaymentCallbackModel(IPaymentOrchestrator payment, ILogger<OrdersPaymentCallbackModel> logger)
        {
            _payment = payment;
            _logger = logger;
        }

        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? TransactionRef { get; set; }
        public string? BankCode { get; set; }
        public string? PayDate { get; set; }
        public decimal Amount { get; set; }
        public int OrderId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("PaymentCallback OnGet: Processing VNPay callback with query: {Query}", 
                Request.QueryString.ToString());

            try
            {
                // VNPay redirects to vnp_ReturnUrl with query parameters
                var result = await _payment.HandleCallbackAsync(Request.Query);

                IsSuccess = result.IsSuccess;
                Message = result.Message ?? (result.IsSuccess ? "Thanh toán thành công" : "Thanh toán th?t b?i");
                TransactionRef = result.TransactionRef;
                BankCode = result.BankCode;
                PayDate = result.PayDate;
                Amount = result.Amount;
                OrderId = result.OrderId;

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Payment successful for order {OrderId}, transaction {TransactionRef}", 
                        OrderId, TransactionRef);
                    TempData["Success"] = "Thanh toán thành công! ??n hàng c?a b?n ?ang ???c x? lý.";
                }
                else
                {
                    _logger.LogWarning("Payment failed for order {OrderId}: {Message}", OrderId, Message);
                    TempData["Error"] = $"Thanh toán th?t b?i: {Message}";
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback");
                IsSuccess = false;
                Message = "Có l?i x?y ra khi x? lý thanh toán. Vui lòng liên h? h? tr?.";
                return Page();
            }
        }
    }
}
