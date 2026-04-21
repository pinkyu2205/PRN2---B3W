using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class PaymentMethodsModel : PageModel
    {
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ILogger<PaymentMethodsModel> _logger;

        public PaymentMethodsModel(IPaymentMethodService paymentMethodService, ILogger<PaymentMethodsModel> logger)
        {
            _paymentMethodService = paymentMethodService;
            _logger = logger;
        }

        public List<PaymentMethodVm> Methods { get; private set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            _logger.LogInformation("=== Admin PaymentMethods Page Load ===");
            await LoadAsync();
        }

        public async Task<IActionResult> OnPostSetStatusAsync(int id, bool isActive)
        {
            try
            {
                _logger.LogInformation("=== Admin Payment Method Status Update ===");
                _logger.LogInformation("Received POST: id={Id}, isActive={IsActive}", id, isActive);
                _logger.LogInformation("User: {User}", User?.Identity?.Name ?? "unknown");
                
                // Validate input
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid id: {Id}", id);
                    ErrorMessage = "ID phương thức thanh toán không hợp lệ.";
                    return RedirectToPage();
                }
                
                var modifiedBy = User?.Identity?.Name ?? "administrator";
                
                _logger.LogInformation("Calling SetStatusAsync with id={Id}, isActive={IsActive}, modifiedBy={ModifiedBy}", 
                    id, isActive, modifiedBy);
                
                var success = await _paymentMethodService.SetStatusAsync(id, isActive, modifiedBy);
                
                if (success)
                {
                    _logger.LogInformation("✅ Successfully updated payment method {Id} to IsActive={IsActive}", id, isActive);
                    StatusMessage = isActive
                        ? "✅ Đã kích hoạt lại phương thức thanh toán."
                        : "✅ Đã tắt phương thức thanh toán.";
                }
                else
                {
                    _logger.LogError("❌ SetStatusAsync returned false for id={Id}", id);
                    ErrorMessage = "Không tìm thấy phương thức thanh toán hoặc không thể cập nhật.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception updating status for payment method {Id} to {IsActive}", id, isActive);
                ErrorMessage = $"Lỗi: {ex.Message}. Vui lòng thử lại.";
            }

            return RedirectToPage();
        }

        private async Task LoadAsync()
        {
            try
            {
                var items = await _paymentMethodService.GetAllAsync();
                
                _logger.LogInformation("Loaded {Count} payment methods from service", items.Count);
                
                Methods = items.Select(pm => new PaymentMethodVm
                {
                    Id = pm.Id,
                    Name = pm.Name,
                    Description = pm.Description ?? string.Empty,
                    IsActive = pm.IsActive,
                    ModifiedAt = pm.ModifiedAt,
                    ModifiedBy = pm.ModifiedBy ?? string.Empty
                }).ToList();
                
                _logger.LogInformation("Converted to {Count} ViewModels", Methods.Count);
                
                foreach (var method in Methods)
                {
                    _logger.LogInformation("  📋 Method #{Id}: {Name}, IsActive={IsActive}, ModifiedBy={ModifiedBy}", 
                        method.Id, method.Name, method.IsActive, method.ModifiedBy);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error loading payment methods");
                Methods = new List<PaymentMethodVm>();
                ErrorMessage = "Không thể tải danh sách phương thức thanh toán.";
            }
        }

        public class PaymentMethodVm
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public DateTime? ModifiedAt { get; set; }
            public string ModifiedBy { get; set; } = string.Empty;
        }
    }
}
