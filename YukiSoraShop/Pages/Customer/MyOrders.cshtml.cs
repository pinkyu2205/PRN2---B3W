using Application;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer
{
    [Authorize]
    public class CustomerOrdersModel : PageModel
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<CustomerOrdersModel> _logger;

        public CustomerOrdersModel(IUnitOfWork uow, ILogger<CustomerOrdersModel> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public List<OrderItemVm> Orders { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var idStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (!int.TryParse(idStr, out var userId) || userId <= 0) return RedirectToPage("/Auth/Login");

            try
            {
                var query = _uow.OrderRepository
                    .GetAllQueryable("Payments.PaymentMethod,Invoices")
                    .AsNoTracking()
                    .Where(o => o.AccountId == userId)
                    .OrderByDescending(o => o.CreatedAt);

                var orders = await query.ToListAsync();

                Orders = orders.Select(o => new OrderItemVm
                {
                    OrderId = o.Id,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    ShippingFee = o.ShippingFee,
                    GrandTotal = o.GrandTotal ?? o.Subtotal + o.ShippingFee,
                    PaymentStatus = o.Payments.OrderByDescending(p => p.Id).FirstOrDefault()?.PaymentStatus.ToString() ?? "N/A",
                    InvoiceCount = o.Invoices.Count,
                    PaymentMethodName = o.Payments.OrderByDescending(p => p.Id).FirstOrDefault()?.PaymentMethod?.Name ?? string.Empty
                }).ToList();

                foreach (var order in Orders)
                {
                    (order.StatusDisplay, order.StatusBadgeClass, order.ShowPayButton) = order.Status switch
                    {
                        var s when string.Equals(s, "Paid", StringComparison.OrdinalIgnoreCase)
                            => ("Đã thanh toán", "badge bg-success", false),
                        var s when string.Equals(s, "AwaitingCash", StringComparison.OrdinalIgnoreCase)
                            => ("Chờ thanh toán tiền mặt", "badge bg-warning text-dark", false),
                        _ => ("Chờ thanh toán", "badge bg-secondary", true)
                    };

                    order.PaymentStatusDisplay = order.PaymentStatus switch
                    {
                        nameof(PaymentStatus.Paid) => "Đã thanh toán",
                        nameof(PaymentStatus.Pending) when string.Equals(order.PaymentMethodName, "Cash", StringComparison.OrdinalIgnoreCase)
                            => "Chờ thanh toán tiền mặt",
                        nameof(PaymentStatus.Pending) => "Đang chờ xử lý",
                        nameof(PaymentStatus.Canceled) => "Thanh toán thất bại",
                        _ => "Chưa thanh toán"
                    };

                    order.PaymentStatusClass = order.PaymentStatus switch
                    {
                        nameof(PaymentStatus.Paid) => "text-success",
                        nameof(PaymentStatus.Pending) when string.Equals(order.PaymentMethodName, "Cash", StringComparison.OrdinalIgnoreCase)
                            => "text-warning",
                        nameof(PaymentStatus.Pending) => "text-warning",
                        nameof(PaymentStatus.Canceled) => "text-danger",
                        _ => "text-muted"
                    };

                    order.PaymentStatusIcon = order.PaymentStatus switch
                    {
                        nameof(PaymentStatus.Paid) => "fas fa-check-circle",
                        nameof(PaymentStatus.Pending) when string.Equals(order.PaymentMethodName, "Cash", StringComparison.OrdinalIgnoreCase)
                            => "fas fa-money-bill-wave",
                        nameof(PaymentStatus.Pending) => "fas fa-hourglass-half",
                        nameof(PaymentStatus.Canceled) => "fas fa-times-circle",
                        _ => "fas fa-receipt"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders for user {UserId}", userId);
                Orders = new List<OrderItemVm>();
            }

            return Page();
        }

        public class OrderItemVm
        {
            public int OrderId { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Status { get; set; } = string.Empty;
            public decimal Subtotal { get; set; }
            public decimal ShippingFee { get; set; }
            public decimal GrandTotal { get; set; }
            public string PaymentStatus { get; set; } = string.Empty;
            public int InvoiceCount { get; set; }
            public string PaymentMethodName { get; set; } = string.Empty;
            public string StatusDisplay { get; set; } = string.Empty;
            public string StatusBadgeClass { get; set; } = "badge bg-secondary";
            public bool ShowPayButton { get; set; }
            public string PaymentStatusDisplay { get; set; } = string.Empty;
            public string PaymentStatusClass { get; set; } = "text-muted";
            public string PaymentStatusIcon { get; set; } = "fas fa-receipt";
        }
    }
}

