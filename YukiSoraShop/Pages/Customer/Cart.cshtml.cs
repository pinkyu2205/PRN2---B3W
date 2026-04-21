using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer
{
    [Authorize]
    public class CustomerCartModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly ILogger<CustomerCartModel> _logger;

        public CustomerCartModel(IOrderService orderService, ICartService cartService, ILogger<CustomerCartModel> logger)
        {
            _orderService = orderService;
            _cartService = cartService;
            _logger = logger;
        }

        public List<CartItemDTO> CartItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public bool CanCheckout { get; set; } = true;

        public async Task OnGetAsync()
        {
            // Check if user is Admin or Staff
            if (User.IsInRole("Administrator") || User.IsInRole("Moderator"))
            {
                CanCheckout = false;
            }
            
            await LoadFromPersistentCartAsync();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostUpdateQuantityAsync(int productId, string action)
        {
            var accountId = GetCurrentUserId();
            if (accountId <= 0) return RedirectToPage("/Auth/Login");

            var items = await _cartService.GetItemsAsync(accountId);
            var item = items.FirstOrDefault(i => i.ProductId == productId);
            var currentQty = item?.Quantity ?? 0;
            var newQty = action == "increase" ? currentQty + 1 : Math.Max(1, currentQty - 1);
            if (currentQty == 0 && action == "decrease") newQty = 0;

            await _cartService.UpdateQuantityAsync(accountId, productId, newQty);
            await LoadFromPersistentCartAsync();
            return RedirectToPage();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostRemoveItemAsync(int productId)
        {
            var accountId = GetCurrentUserId();
            if (accountId <= 0) return RedirectToPage("/Auth/Login");
            await _cartService.RemoveItemAsync(accountId, productId);
            await LoadFromPersistentCartAsync();
            return RedirectToPage();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostCheckout()
        {
            var accountId = GetCurrentUserId();
            if (accountId <= 0) return RedirectToPage("/Auth/Login");

            // Block Admin and Moderator from checking out
            if (User.IsInRole("Administrator"))
            {
                _logger.LogWarning("Administrator account {AccountId} attempted to checkout", accountId);
                TempData["Error"] = "Tài khoản quản trị không thể đặt hàng. Vui lòng sử dụng tài khoản khách hàng.";
                return RedirectToPage();
            }

            if (User.IsInRole("Moderator"))
            {
                _logger.LogWarning("Moderator account {AccountId} attempted to checkout", accountId);
                TempData["Error"] = "Tài khoản nhân viên không thể đặt hàng. Vui lòng sử dụng tài khoản khách hàng.";
                return RedirectToPage();
            }

            var createdBy = User.Identity?.Name ?? "customer";
            var orderItems = await _cartService.ToOrderItemsAsync(accountId);
            if (orderItems == null || orderItems.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToPage();
            }

            try
            {
                var order = await _orderService.CreateOrderFromCartAsync(accountId, orderItems, createdBy);
                await _cartService.ClearAsync(accountId);
                await LoadFromPersistentCartAsync();
                
                _logger.LogInformation("Order {OrderId} created successfully for account {AccountId}", order.Id, accountId);
                TempData["Success"] = $"✅ Đơn hàng #{order.Id} đã được tạo thành công!";
                return RedirectToPage("/Orders/PaymentMethod", new { OrderId = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for account {AccountId}", accountId);
                TempData["Error"] = "Không thể tạo đơn hàng. Vui lòng thử lại sau.";
                return RedirectToPage();
            }
        }

        private async Task LoadFromPersistentCartAsync()
        {
            var accountId = GetCurrentUserId();
            if (accountId <= 0)
            {
                CartItems = new List<CartItemDTO>();
                TotalAmount = 0;
                TotalItems = 0;
                return;
            }
            var items = await _cartService.GetItemsAsync(accountId);
            CartItems = items.Select(i => new CartItemDTO
            {
                Quantity = i.Quantity,
                Product = new ProductDTO
                {
                    Id = i.ProductId,
                    Name = i.Product?.ProductName ?? $"Product #{i.ProductId}",
                    Description = i.Product?.Description ?? string.Empty,
                    Price = i.Product?.Price ?? 0m,
                    ImageUrl = i.Product?.ProductDetails.FirstOrDefault()?.ImageUrl ?? string.Empty,
                    Category = i.Product?.CategoryName ?? string.Empty,
                    Stock = i.Product?.StockQuantity ?? 0,
                    IsAvailable = !(i.Product?.IsDeleted ?? false)
                }
            }).ToList();
            TotalAmount = CartItems.Sum(ci => ci.TotalPrice);
            TotalItems = CartItems.Sum(ci => ci.Quantity);
        }

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return int.TryParse(idStr, out var id) ? id : 0;
        }

    }
}
