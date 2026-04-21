using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Admin.Interfaces;
using Application.Payments.Interfaces;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using YukiSoraShop.Hubs;

namespace YukiSoraShop.Pages.Orders
{
    [Authorize]
    public class OrdersPaymentMethodModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentOrchestrator _paymentOrchestrator;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IAdminDashboardService _dashboardService;
        private readonly IHubContext<AdminDashboardHub> _hubContext;
        private readonly ILogger<OrdersPaymentMethodModel> _logger;

        public OrdersPaymentMethodModel(
            IOrderService orderService,
            IPaymentOrchestrator paymentOrchestrator,
            IPaymentMethodService paymentMethodService,
            IAdminDashboardService dashboardService,
            IHubContext<AdminDashboardHub> hubContext,
            ILogger<OrdersPaymentMethodModel> logger)
        {
            _orderService = orderService;
            _paymentOrchestrator = paymentOrchestrator;
            _paymentMethodService = paymentMethodService;
            _dashboardService = dashboardService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int OrderId { get; set; }

        [BindProperty]
        public string SelectedMethod { get; set; } = string.Empty;

        public decimal GrandTotal { get; set; }
        public bool IsAwaitingCash { get; set; }
        public List<PaymentMethodOption> PaymentMethods { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("=== PaymentMethod OnGet START ===");
            _logger.LogInformation("OrderId received: {OrderId}", OrderId);

            if (OrderId <= 0)
            {
                _logger.LogWarning("Invalid OrderId: {OrderId}", OrderId);
                TempData["Error"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToPage("/Customer/MyOrders");
            }

            var order = await LoadOrderAsync();
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found or access denied", OrderId);
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập.";
                return RedirectToPage("/Customer/MyOrders");
            }

            _logger.LogInformation("Order {OrderId} loaded, Status: {Status}", OrderId, order.Status);

            if (EqualsName(order.Status, "Paid"))
            {
                _logger.LogInformation("Order {OrderId} is already paid", OrderId);
                TempData["Info"] = "Đơn hàng đã được thanh toán.";
                return RedirectToPage("/Customer/MyOrders");
            }

            GrandTotal = order.GrandTotal ?? (order.Subtotal + order.ShippingFee);
            IsAwaitingCash = EqualsName(order.Status, "AwaitingCash");

            _logger.LogInformation("Loading payment methods...");
            if (!await LoadPaymentMethodsAsync())
            {
                _logger.LogError("No active payment methods found!");
                TempData["Error"] = "⚠️ Hiện chưa có phương thức thanh toán nào khả dụng. Vui lòng liên hệ quản trị viên để kích hoạt phương thức thanh toán.";
                // KHÔNG redirect, hiển thị trang với message
                PaymentMethods = new List<PaymentMethodOption>();
                return Page();
            }

            _logger.LogInformation("Found {Count} active payment methods", PaymentMethods.Count);

            if (IsAwaitingCash && HasMethod("Cash"))
            {
                SelectedMethod = "Cash";
                _logger.LogInformation("Auto-selected Cash (order is AwaitingCash)");
            }
            else if (string.IsNullOrWhiteSpace(SelectedMethod) || !HasMethod(SelectedMethod))
            {
                SelectedMethod = PaymentMethods.First().Name;
                _logger.LogInformation("Auto-selected first method: {Method}", SelectedMethod);
            }

            _logger.LogInformation("=== PaymentMethod OnGet END - Displaying page ===");
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("=== PaymentMethod OnPost START ===");
            _logger.LogInformation("OrderId: {OrderId}, SelectedMethod: {Method}", OrderId, SelectedMethod);

            var order = await LoadOrderAsync();
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found on POST", OrderId);
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (EqualsName(order.Status, "Paid"))
            {
                _logger.LogInformation("Order {OrderId} is already paid on POST", OrderId);
                TempData["Info"] = "Đơn hàng đã được thanh toán.";
                return RedirectToPage("/Customer/MyOrders");
            }

            GrandTotal = order.GrandTotal ?? (order.Subtotal + order.ShippingFee);
            IsAwaitingCash = EqualsName(order.Status, "AwaitingCash");

            if (!await LoadPaymentMethodsAsync())
            {
                _logger.LogError("No active payment methods found on POST!");
                TempData["Error"] = "Hiện chưa có phương thức thanh toán nào khả dụng.";
                return Page();
            }

            if (!HasMethod(SelectedMethod))
            {
                _logger.LogWarning("Selected method {Method} is not available", SelectedMethod);
                ModelState.AddModelError(nameof(SelectedMethod), "Phương thức thanh toán không khả dụng.");
                SelectedMethod = PaymentMethods.First().Name;
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid");
                return Page();
            }

            if (EqualsName(SelectedMethod, "Cash"))
            {
                _logger.LogInformation("Processing Cash payment for Order {OrderId}", OrderId);
                var createdBy = User.Identity?.Name ?? "customer";
                var result = await _paymentOrchestrator.CreateCashPaymentAsync(OrderId, createdBy);
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("Cash payment created successfully for Order {OrderId}", OrderId);
                    try
                    {
                        var orderSnapshot = await _orderService.GetOrderWithDetailsAsync(OrderId);
                        if (orderSnapshot != null)
                        {
                            await _hubContext.Clients.Group(AdminDashboardHub.AdminGroupName)
                                .SendAsync("CashOrderAdded", new
                                {
                                    orderId = orderSnapshot.Id,
                                    customerName = orderSnapshot.Account?.FullName ?? orderSnapshot.Account?.UserName ?? $"User #{orderSnapshot.AccountId}",
                                    customerEmail = orderSnapshot.Account?.Email ?? string.Empty,
                                    createdAt = orderSnapshot.CreatedAt,
                                    grandTotal = orderSnapshot.GrandTotal ?? (orderSnapshot.Subtotal + orderSnapshot.ShippingFee),
                                    paymentStatus = "Pending"
                                });
                        }

                        var summary = await _dashboardService.RefreshSummaryAsync();
                        await _hubContext.Clients.Group(AdminDashboardHub.AdminGroupName)
                            .SendAsync("DashboardSummaryUpdated", summary);
                    }
                    catch (Exception broadcastEx)
                    {
                        _logger.LogWarning(broadcastEx, "Failed to broadcast cash order creation for OrderId {OrderId}", OrderId);
                    }
                    
                    TempData["SuccessMessage"] = result.Message;
                    _logger.LogInformation("Redirecting to CashConfirmation for Order {OrderId}", OrderId);
                    return RedirectToPage("/Orders/CashConfirmation", new { OrderId });
                }
                else
                {
                    _logger.LogError("Failed to create cash payment: {Message}", result.Message);
                    TempData["Error"] = result.Message ?? "Không thể tạo thanh toán tiền mặt.";
                    return Page();
                }
            }

            if (!EqualsName(SelectedMethod, "VNPay"))
            {
                _logger.LogWarning("Unsupported payment method: {Method}", SelectedMethod);
                TempData["Error"] = "Phương thức thanh toán chưa được hỗ trợ.";
                return Page();
            }

            _logger.LogInformation("Redirecting to Checkout (VNPay) for Order {OrderId}", OrderId);
            return RedirectToPage("/Orders/Checkout", new { OrderId });
        }

        private async Task<bool> LoadPaymentMethodsAsync()
        {
            try
            {
                var methods = await _paymentMethodService.GetActiveAsync();
                PaymentMethods = methods
                    .Select(pm => new PaymentMethodOption
                    {
                        Id = pm.Id,
                        Name = pm.Name,
                        Description = pm.Description ?? string.Empty
                    })
                    .ToList();
                
                _logger.LogInformation("Loaded {Count} payment methods: {Methods}", 
                    PaymentMethods.Count, 
                    string.Join(", ", PaymentMethods.Select(m => m.Name)));
                
                return PaymentMethods.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment methods");
                PaymentMethods = new List<PaymentMethodOption>();
                return false;
            }
        }

        private async Task<Domain.Entities.Order?> LoadOrderAsync()
        {
            var order = await _orderService.GetOrderWithDetailsAsync(OrderId);
            if (order == null)
            {
                return null;
            }

            var userId = GetCurrentUserId();
            if (order.AccountId != userId)
            {
                _logger.LogWarning("Order {OrderId} belongs to user {OwnerId}, but current user is {CurrentUserId}", 
                    OrderId, order.AccountId, userId);
                return null;
            }

            return order;
        }

        private bool HasMethod(string? name) =>
            !string.IsNullOrWhiteSpace(name) && PaymentMethods.Any(pm => EqualsName(pm.Name, name));

        private static bool EqualsName(string? left, string? right) =>
            string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return int.TryParse(idStr, out var id) ? id : 0;
        }

        public class PaymentMethodOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}
