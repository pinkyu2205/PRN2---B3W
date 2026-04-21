using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace YukiSoraShop.Pages.Staff.Products
{
    [Authorize(Roles = "Moderator,Staff")]
    public class StaffProductCreateModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<StaffProductCreateModel> _logger;

        public StaffProductCreateModel(IProductService productService, ILogger<StaffProductCreateModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [BindProperty]
        [ValidateNever]
        public Product Product { get; set; } = new();

        [BindProperty]
        public List<ProductDetail> ProductDetails { get; set; } = new();

        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCategoryOptions();
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var category = await _productService.GetCategoryByIdAsync(Product.CategoryId);
                if (category == null)
                {
                    ModelState.AddModelError("Product.CategoryId", "Danh mục không hợp lệ.");
                    await LoadCategoryOptions();
                    return Page();
                }

                Product.CategoryName = (category.CategoryName ?? string.Empty).Trim();

                // Validate Product using correct prefix
                ModelState.Clear();
                if (!TryValidateModel(Product, nameof(Product)))
                {
                    var errors = ModelState
                        .Where(kvp => kvp.Value?.Errors?.Count > 0)
                        .SelectMany(kvp => kvp.Value!.Errors.Select(err => new { Field = kvp.Key, Error = err.ErrorMessage }))
                        .ToList();
                    foreach (var e in errors)
                    {
                        _logger.LogWarning("Product create validation error: {Field} - {Error}", e.Field, e.Error);
                    }
                    TempData["Error"] = "Vui lòng kiểm tra các lỗi ở biểu mẫu và thử lại.";
                    await LoadCategoryOptions();
                    return Page();
                }

                var username = HttpContext.User?.Identity?.Name ?? "system";
                Product.CreatedAt = DateTime.UtcNow;
                Product.CreatedBy = username;
                Product.ModifiedAt = DateTime.UtcNow;
                Product.ModifiedBy = username;
                Product.IsDeleted = false;

                // Validate ProductDetails: require at least one and ensure each has some value
                if (ProductDetails == null || !ProductDetails.Any())
                {
                    TempData["Error"] = "Vui lòng nhập thông tin chi tiết sản phẩm.";
                    await LoadCategoryOptions();
                    return Page();
                }

                for (var i = 0; i < ProductDetails.Count; i++)
                {
                    var detail = ProductDetails[i];
                    bool allEmpty =
                        string.IsNullOrWhiteSpace(detail.Color) &&
                        string.IsNullOrWhiteSpace(detail.Size) &&
                        string.IsNullOrWhiteSpace(detail.Material) &&
                        string.IsNullOrWhiteSpace(detail.Origin) &&
                        string.IsNullOrWhiteSpace(detail.ImageUrl) &&
                        string.IsNullOrWhiteSpace(detail.Description) &&
                        !detail.AdditionalPrice.HasValue;

                    if (allEmpty)
                    {
                        ModelState.AddModelError(string.Empty, "Mỗi biến thể sản phẩm phải có ít nhất một thông tin được nhập.");
                        TempData["Error"] = "Vui lòng nhập đầy đủ thông tin cho từng biến thể sản phẩm.";
                        await LoadCategoryOptions();
                        return Page();
                    }

                    var prefix = $"{nameof(ProductDetails)}[{i}]";
                    ModelState.Remove($"{prefix}.Product");
                    if (!TryValidateModel(detail, prefix))
                    {
                        var fieldPrefix = prefix;
                        var detailErrors = ModelState
                            .Where(kvp => kvp.Key.StartsWith(fieldPrefix, StringComparison.OrdinalIgnoreCase))
                            .SelectMany(kvp => kvp.Value?.Errors ?? Enumerable.Empty<ModelError>())
                            .Select(e => e.ErrorMessage)
                            .ToList();
                        foreach (var error in detailErrors)
                        {
                            _logger.LogWarning("Product detail validation error (index {Index}): {Message}", i, error);
                        }
                        TempData["Error"] = "Vui lòng kiểm tra lại thông tin chi tiết sản phẩm.";
                        await LoadCategoryOptions();
                        return Page();
                    }

                    detail.CreatedAt = DateTime.UtcNow;
                    detail.CreatedBy = username;
                    detail.ModifiedAt = DateTime.UtcNow;
                    detail.ModifiedBy = username;
                }

                Product.ProductDetails = ProductDetails;

                var success = await _productService.CreateProductAsync(Product);
                if (success)
                {
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToPage("/Staff/Products/List");
                }

                TempData["Error"] = "Có lỗi xảy ra khi thêm sản phẩm. Vui lòng thử lại.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product {ProductName}", Product?.ProductName);
                TempData["Error"] = "Đã xảy ra lỗi. Vui lòng thử lại.";
            }

            return Page();
        }

        private async Task LoadCategoryOptions()
        {
            var categories = await _productService.GetAllCategoriesAsync();
            CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CategoryName
            }).ToList();
        }
    }
}
