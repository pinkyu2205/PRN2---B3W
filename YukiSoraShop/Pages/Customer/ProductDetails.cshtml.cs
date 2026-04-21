using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading;

namespace YukiSoraShop.Pages.Customer
{
    public class ProductDetailsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ILogger<ProductDetailsModel> _logger;

        public ProductDTO Product { get; set; } = new();
        public List<ProductDetailDTO> ProductDetails { get; set; } = new();

        public ProductDetailsModel(IProductService productService, ICartService cartService, ILogger<ProductDetailsModel> logger)
        {
            _productService = productService;
            _cartService = cartService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _productService.GetProductDtoByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToPage("/Customer/Catalog");
            }

            Product = product;
            ProductDetails = product.ProductDetails ?? new List<ProductDetailDTO>();
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAddToCartAsync(int id, int productDetailId, CancellationToken ct = default)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Sản phẩm không hợp lệ.";
                return RedirectToPage("/Customer/ProductDetails", new { id });
            }

            var accountIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdStr, out var accountId) || accountId <= 0)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Auth/Login");
            }

            // Check role restrictions
            if (User.IsInRole("Administrator"))
            {
                TempData["Error"] = "Tài khoản quản trị không thể thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Customer/ProductDetails", new { id });
            }

            if (User.IsInRole("Moderator"))
            {
                TempData["Error"] = "Nhân viên không thể thêm sản phẩm vào giỏ hàng.";
                return RedirectToPage("/Customer/ProductDetails", new { id });
            }

            try
            {
                var product = await _productService.GetProductDtoByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại.";
                    return RedirectToPage("/Customer/Catalog");
                }

                // Validate availability
                if (!product.IsAvailable || product.Stock <= 0)
                {
                    TempData["Error"] = "Sản phẩm tạm thời không có sẵn.";
                    return RedirectToPage("/Customer/ProductDetails", new { id });
                }

                // Calculate variant info for display
                ProductDetailDTO? selectedDetail = null;
                
                if (productDetailId > 0 && product.ProductDetails != null && product.ProductDetails.Any())
                {
                    selectedDetail = product.ProductDetails.FirstOrDefault(d => d.Id == productDetailId);
                    if (selectedDetail == null)
                    {
                        TempData["Error"] = "Biến thể sản phẩm không tồn tại.";
                        return RedirectToPage("/Customer/ProductDetails", new { id });
                    }
                }

                // If product has variants but none selected, require selection
                if (product.ProductDetails != null && product.ProductDetails.Any() && productDetailId <= 0)
                {
                    TempData["Error"] = "Vui lòng chọn biến thể sản phẩm trước khi thêm vào giỏ hàng.";
                    return RedirectToPage("/Customer/ProductDetails", new { id });
                }

                // Add to cart using product ID only (price is fetched from product entity)
                await _cartService.AddItemAsync(accountId, product.Id, 1, ct);

                var variantInfo = selectedDetail != null 
                    ? $" ({selectedDetail.Color}" + (!string.IsNullOrWhiteSpace(selectedDetail.Size) ? $" - {selectedDetail.Size}" : "") + ")"
                    : "";

                TempData["Success"] = $"Đã thêm {product.Name}{variantInfo} vào giỏ hàng!";
                
                // Update cart count for UI
                var items = await _cartService.GetItemsAsync(accountId, ct);
                TempData["CartCount"] = items?.Sum(i => i.Quantity) ?? 0;

                return RedirectToPage("/Customer/Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductId} (detail {DetailId}) to cart for account {AccountId}", 
                    id, productDetailId, accountId);
                TempData["Error"] = "Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng. Vui lòng thử lại.";
                return RedirectToPage("/Customer/ProductDetails", new { id });
            }
        }
    }
}
