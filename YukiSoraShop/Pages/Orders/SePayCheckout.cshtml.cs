using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Orders
{
    [Authorize]
    public class SePayCheckoutModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _config;

        public SePayCheckoutModel(IOrderService orderService, IConfiguration config)
        {
            _orderService = orderService;
            _config = config;
        }

        [BindProperty(SupportsGet = true)]
        public int OrderId { get; set; }

        public decimal GrandTotal { get; set; }
        public string BankAccount { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var order = await _orderService.GetOrderByIdAsync(OrderId);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToPage("/Customer/MyOrders");
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId) || order.AccountId != userId)
            {
                TempData["Error"] = "Bạn không có quyền truy cập đơn hàng này.";
                return RedirectToPage("/Customer/MyOrders");
            }

            if (order.Status == "PAID" || order.Status == "Completed")
            {
                TempData["Info"] = "Đơn hàng đã được thanh toán thành công.";
                return RedirectToPage("/Customer/MyOrders");
            }

            GrandTotal = order.GrandTotal ?? 0;
            BankAccount = _config["SePay:BankAccount"] ?? "";
            BankName = _config["SePay:BankName"] ?? "";

            return Page();
        }
    }
}
