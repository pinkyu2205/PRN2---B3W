using System;
using System.Linq;
using Application.Payments.Interfaces;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Application.Admin.Interfaces;
using YukiSoraShop.Hubs;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class CashPaymentsModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentOrchestrator _paymentOrchestrator;
        private readonly IAdminDashboardService _dashboardService;
        private readonly IHubContext<AdminDashboardHub> _hubContext;
        private readonly ILogger<CashPaymentsModel> _logger;

        public CashPaymentsModel(
            IOrderService orderService,
            IPaymentOrchestrator paymentOrchestrator,
            IAdminDashboardService dashboardService,
            IHubContext<AdminDashboardHub> hubContext,
            ILogger<CashPaymentsModel> logger)
        {
            _orderService = orderService;
            _paymentOrchestrator = paymentOrchestrator;
            _dashboardService = dashboardService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public List<CashOrderVm> Orders { get; private set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostConfirmAsync(int orderId)
        {
            if (orderId <= 0)
            {
                ErrorMessage = "Đơn hàng không hợp lệ.";
                return RedirectToPage();
            }

            var confirmedBy = User?.Identity?.Name ?? "administrator";
            try
            {
                var result = await _paymentOrchestrator.ConfirmCashPaymentAsync(orderId, confirmedBy);
                if (result.IsSuccess)
                {
                    StatusMessage = $"✅ Đã xác nhận thanh toán tiền mặt cho đơn hàng #{orderId}.";
                    try
                    {
                        var summary = await _dashboardService.RefreshSummaryAsync();
                        await BroadcastSummaryAsync(summary);
                        await _hubContext.Clients.Group(AdminDashboardHub.AdminGroupName)
                            .SendAsync("CashOrderRemoved", new { orderId });
                    }
                    catch (Exception broadcastEx)
                    {
                        _logger.LogWarning(broadcastEx, "Failed to broadcast SignalR update for order {OrderId}", orderId);
                    }
                }
                else
                {
                    ErrorMessage = result.Message ?? "Không thể xác nhận thanh toán tiền mặt.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming cash payment for order {OrderId}", orderId);
                ErrorMessage = "Không thể xác nhận thanh toán. Vui lòng thử lại.";
            }

            return RedirectToPage();
        }

        private async Task LoadAsync()
        {
            var orders = await _orderService.GetOrdersAwaitingCashAsync();
            Orders = orders.Select(o =>
            {
                var cashPayment = o.Payments?
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault(p => string.Equals(p.PaymentMethod?.Name, "Cash", StringComparison.OrdinalIgnoreCase));

                return new CashOrderVm
                {
                    OrderId = o.Id,
                    CustomerName = o.Account?.FullName ?? o.Account?.UserName ?? $"User #{o.AccountId}",
                    CustomerEmail = o.Account?.Email ?? string.Empty,
                    CreatedAt = o.CreatedAt,
                    GrandTotal = o.GrandTotal ?? (o.Subtotal + o.ShippingFee),
                    PaymentMethod = cashPayment?.PaymentMethod?.Name ?? "Cash",
                    PaymentStatus = cashPayment?.PaymentStatus.ToString() ?? "Pending"
                };
            }).ToList();
        }

        private Task BroadcastSummaryAsync(Application.Admin.DTOs.AdminDashboardSummary summary)
        {
            return _hubContext.Clients.Group(AdminDashboardHub.AdminGroupName)
                .SendAsync("DashboardSummaryUpdated", summary);
        }

        public class CashOrderVm
        {
            public int OrderId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public decimal GrandTotal { get; set; }
            public string PaymentMethod { get; set; } = string.Empty;
            public string PaymentStatus { get; set; } = string.Empty;
        }
    }
}
