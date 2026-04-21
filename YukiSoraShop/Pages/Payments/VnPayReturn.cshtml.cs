using Application.Payments.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Payments
{
    public class PaymentsVnPayReturnModel : PageModel
    {
        private readonly IPaymentOrchestrator _payment;

        public PaymentsVnPayReturnModel(IPaymentOrchestrator payment)
        {
            _payment = payment;
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
            // VNPay redirect về vnp_ReturnUrl -> Page này lấy Query để xử lý
            var result = await _payment.HandleCallbackAsync(Request.Query);

            IsSuccess = result.IsSuccess;
            Message = result.Message;
            TransactionRef = result.TransactionRef;
            BankCode = result.BankCode;
            PayDate = result.PayDate;
            Amount = result.Amount;
            OrderId = result.OrderId;

            return Page();
        }
    }
}
