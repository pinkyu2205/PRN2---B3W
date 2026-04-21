using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Orders
{
    [Authorize]
    public class OrdersCashConfirmationModel : PageModel
    {
        private readonly IOrderService _orderService;

        public OrdersCashConfirmationModel(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [BindProperty(SupportsGet = true)]
        public int OrderId { get; set; }

        public OrderSummaryVm? Order { get; private set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var order = await _orderService.GetOrderWithDetailsAsync(OrderId);
            if (order == null || order.AccountId != GetCurrentUserId())
            {
                TempData["Error"] = "Không tìm thấy đơn hàng phù hợp.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (!string.Equals(order.Status, "AwaitingCash", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Info"] = "Đơn hàng không còn ở trạng thái chờ thanh toán tiền mặt.";
                return RedirectToPage("/Customer/MyOrders");
            }

            var latestPayment = order.Payments
                .OrderByDescending(p => p.Id)
                .FirstOrDefault();

            var paymentStatusDisplay = "Chờ thanh toán";
            var paymentStatusClass = "badge bg-warning text-dark";
            if (latestPayment != null)
            {
                switch (latestPayment.PaymentStatus)
                {
                    case Domain.Enums.PaymentStatus.Paid:
                        paymentStatusDisplay = "Đã thanh toán";
                        paymentStatusClass = "badge bg-success";
                        break;
                    case Domain.Enums.PaymentStatus.Canceled:
                        paymentStatusDisplay = "Thanh toán thất bại";
                        paymentStatusClass = "badge bg-danger";
                        break;
                    default:
                        paymentStatusDisplay = string.Equals(latestPayment.PaymentMethod?.Name, "Cash", StringComparison.OrdinalIgnoreCase)
                            ? "Chờ thanh toán tiền mặt"
                            : "Đang chờ xử lý";
                        paymentStatusClass = string.Equals(latestPayment.PaymentMethod?.Name, "Cash", StringComparison.OrdinalIgnoreCase)
                            ? "badge bg-warning text-dark"
                            : "badge bg-secondary";
                        break;
                }
            }

            Order = new OrderSummaryVm
            {
                OrderId = order.Id,
                CreatedAt = order.CreatedAt,
                StatusDisplay = "Chờ thanh toán tiền mặt",
                StatusBadgeClass = "badge bg-warning text-dark",
                Subtotal = order.Subtotal,
                ShippingFee = order.ShippingFee,
                GrandTotal = order.GrandTotal ?? order.Subtotal + order.ShippingFee,
                PaymentStatusDisplay = paymentStatusDisplay,
                PaymentStatusBadgeClass = paymentStatusClass,
                PaymentMethodName = latestPayment?.PaymentMethod?.Name ?? "Cash",
                Items = order.OrderDetails
                    .OrderBy(od => od.Id)
                    .Select(od => new OrderItemVm
                    {
                        ProductName = od.Product?.ProductName ?? $"Sản phẩm #{od.ProductId}",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        LineTotal = od.LineTotal ?? (od.UnitPrice * od.Quantity)
                    })
                    .ToList()
            };

            return Page();
        }

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return int.TryParse(idStr, out var id) ? id : 0;
        }

        public class OrderSummaryVm
        {
            public int OrderId { get; set; }
            public DateTime CreatedAt { get; set; }
            public string StatusDisplay { get; set; } = string.Empty;
            public string StatusBadgeClass { get; set; } = string.Empty;
            public decimal Subtotal { get; set; }
            public decimal ShippingFee { get; set; }
            public decimal GrandTotal { get; set; }
            public string PaymentStatusDisplay { get; set; } = string.Empty;
            public string PaymentStatusBadgeClass { get; set; } = string.Empty;
            public string PaymentMethodName { get; set; } = string.Empty;
            public List<OrderItemVm> Items { get; set; } = new();
        }

        public class OrderItemVm
        {
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal LineTotal { get; set; }
        }
    }
}
