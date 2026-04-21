using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Orders
{
    [Authorize]
    public class OrdersCheckoutModel : PageModel
    {
        private readonly IPaymentOrchestrator _payment;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<OrdersCheckoutModel> _logger;

        public OrdersCheckoutModel(IPaymentOrchestrator payment, IUnitOfWork uow, ILogger<OrdersCheckoutModel> logger)
        {
            _payment = payment;
            _uow = uow;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int OrderId { get; set; }

        [BindProperty]
        public string? BankCode { get; set; }

        [BindProperty]
        public string OrderTypeCode { get; set; } = "other";

        public decimal? GrandTotal { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Checkout OnGet: OrderId={OrderId}", OrderId);

            if (OrderId <= 0)
            {
                _logger.LogWarning("Checkout OnGet: Missing or invalid OrderId");
                TempData["Error"] = "Thiếu mã đơn hàng.";
                return RedirectToPage("/Customer/Catalog");
            }

            var order = await _uow.OrderRepository.GetByIdAsync(OrderId);
            if (order == null || order.AccountId != GetCurrentUserId())
            {
                _logger.LogWarning("Checkout OnGet: Order {OrderId} not found or access denied for user {UserId}", 
                    OrderId, GetCurrentUserId());
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (string.Equals(order.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Info"] = "Đơn hàng đã được thanh toán.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (!await IsVnPayActiveAsync())
            {
                TempData["Error"] = "Phương thức VNPay hiện không khả dụng. Vui lòng chọn phương thức khác.";
                return RedirectToPage("/Orders/PaymentMethod", new { OrderId });
            }

            GrandTotal = order.GrandTotal ?? (order.Subtotal + order.ShippingFee);
            _logger.LogInformation("Checkout OnGet: Order {OrderId} loaded, GrandTotal={GrandTotal}", 
                OrderId, GrandTotal);

            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Checkout OnPost: OrderId={OrderId}, BankCode={BankCode}, OrderType={OrderType}", 
                OrderId, BankCode ?? "none", OrderTypeCode);

            if (OrderId <= 0)
            {
                _logger.LogWarning("Checkout OnPost: Missing or invalid OrderId");
                TempData["Error"] = "Thiếu mã đơn hàng.";
                return RedirectToPage("/Customer/Catalog");
            }

            var order = await _uow.OrderRepository.GetByIdAsync(OrderId);
            if (order == null || order.AccountId != GetCurrentUserId())
            {
                _logger.LogWarning("Checkout OnPost: Order {OrderId} not found or access denied for user {UserId}", 
                    OrderId, GetCurrentUserId());
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (string.Equals(order.Status, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Info"] = "Đơn hàng đã được thanh toán.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (!await IsVnPayActiveAsync())
            {
                TempData["Error"] = "Phương thức VNPay hiện không khả dụng. Vui lòng chọn phương thức khác.";
                return RedirectToPage("/Orders/PaymentMethod", new { OrderId });
            }

            try
            {
                var cmd = new CreatePaymentCommand
                {
                    OrderId = OrderId,
                    BankCode = string.IsNullOrWhiteSpace(BankCode) ? null : BankCode,
                    ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    OrderDescription = $"Thanh toan don hang #{OrderId}",
                    OrderTypeCode = OrderTypeCode
                };

                var dto = await _payment.CreateCheckoutAsync(cmd);
                _logger.LogInformation("Checkout OnPost: Created VNPay checkout URL for Order {OrderId}", OrderId);
                
                return Redirect(dto.CheckoutUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout OnPost: Error creating VNPay checkout for Order {OrderId}", OrderId);
                TempData["Error"] = "Không thể tạo liên kết thanh toán. Vui lòng thử lại.";
                GrandTotal = order.GrandTotal ?? (order.Subtotal + order.ShippingFee);
                return Page();
            }
        }

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return int.TryParse(idStr, out var id) ? id : 0;
        }

        private async Task<bool> IsVnPayActiveAsync()
        {
            var method = await _uow.PaymentMethodRepository.FindOneAsync(pm => pm.Name == "VNPay");
            return method?.IsActive ?? false;
        }
    }
}
