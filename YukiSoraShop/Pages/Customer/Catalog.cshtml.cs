using Application.DTOs;
using Application.DTOs.Pagination;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer
{
    public class CustomerCatalogModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly ILogger<CustomerCatalogModel> _logger;

        public CustomerCatalogModel(IProductService productService, ICartService cartService, ILogger<CustomerCatalogModel> logger)
        {
            _productService = productService;
            _cartService = cartService;
            _logger = logger;
        }

        public List<ProductDTO> Products { get; set; } = new();
        public List<ProductDTO> HotProducts { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int Size { get; set; } = 12;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Sort { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                var size = Size <= 0 ? 12 : Math.Min(Size, PaginationDefaults.MaxPageSize);
                var page = Page <= 0 ? PaginationDefaults.DefaultPageNumber : Page;

                HotProducts = await _productService.GetHotProductsAsync(8);

                var paged = await _productService.GetProductsPagedAsync(
                    page, size, Search, Category, MinPrice, MaxPrice, Sort);

                Products = paged.Items.ToList();
                TotalPages = paged.TotalPages;
                TotalItems = paged.TotalItems;

                if (TotalPages > 0 && Page > TotalPages) Page = TotalPages;
                if (Page <= 0) Page = 1;
                Size = size;

                var cats = await _productService.GetAllCategoriesAsync();
                CategoryOptions = cats
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryName,
                        Text = c.CategoryName,
                        Selected = c.CategoryName == Category
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load catalog: page={Page}, size={Size}, search={Search}, category={Category}",
                    Page, Size, Search, Category);

                TempData["Error"] = "Không thể tải danh sách sản phẩm. Vui lòng thử lại sau.";
                Products = new List<ProductDTO>();
                HotProducts = new List<ProductDTO>();
                TotalPages = 0;
                TotalItems = 0;
                CategoryOptions = new List<SelectListItem>();
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAddToCart(int id, CancellationToken ct = default)
        {
            return await AddProductToCartAsync(id, productDetailId: 0, redirectToCart: false, ct);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostBuyNowAsync(int id, int productDetailId = 0, CancellationToken ct = default)
        {
            return await AddProductToCartAsync(id, productDetailId, redirectToCart: true, ct);
        }

        private async Task<IActionResult> AddProductToCartAsync(int id, int productDetailId, bool redirectToCart, CancellationToken ct)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Sản phẩm không hợp lệ.";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort, MinPrice, MaxPrice });
            }

            var accountId = GetAccountIdFromUser();
            if (accountId <= 0)
            {
                return RedirectToPage("/Auth/Login");
            }

            if (User.IsInRole("Administrator"))
            {
                _logger.LogWarning("Administrator account {AccountId} attempted to add product to cart", accountId);
                TempData["Error"] = "Tài khoản quản trị không thể mua hàng.";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort, MinPrice, MaxPrice });
            }

            if (User.IsInRole("Moderator"))
            {
                _logger.LogWarning("Moderator account {AccountId} attempted to add product to cart", accountId);
                TempData["Error"] = "Tài khoản nhân viên không thể mua hàng.";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort, MinPrice, MaxPrice });
            }

            try
            {
                var product = await _productService.GetProductDtoByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại.";
                    return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort, MinPrice, MaxPrice });
                }

                if (!product.IsAvailable || product.Stock <= 0)
                {
                    TempData["Error"] = "Sản phẩm tạm thời không có sẵn.";
                    return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort, MinPrice, MaxPrice });
                }

                var hasVariants = product.ProductDetails != null && product.ProductDetails.Any();
                ProductDetailDTO? selectedDetail = null;

                if (hasVariants)
                {
                    if (productDetailId <= 0)
                    {
                        TempData["Info"] = "Sản phẩm có nhiều biến thể. Vui lòng chọn biến thể trước khi mua.";
                        return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort, MinPrice, MaxPrice });
                    }

                    selectedDetail = product.ProductDetails.FirstOrDefault(d => d.Id == productDetailId);
                    if (selectedDetail == null)
                    {
                        TempData["Error"] = "Biến thể sản phẩm không hợp lệ.";
                        return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort, MinPrice, MaxPrice });
                    }
                }

                await _cartService.AddItemAsync(accountId, id, 1, ct);

                var items = await _cartService.GetItemsAsync(accountId, ct);
                TempData["CartCount"] = items?.Sum(i => i.Quantity) ?? 0;

                var variantInfo = selectedDetail != null
                    ? $" ({selectedDetail.Color}" + (!string.IsNullOrWhiteSpace(selectedDetail.Size) ? $" - {selectedDetail.Size}" : string.Empty) + ")"
                    : string.Empty;

                _logger.LogInformation("Product {ProductId} added to cart for account {AccountId}", id, accountId);
                TempData["Success"] = $"Đã thêm \"{product.Name}{variantInfo}\" vào giỏ hàng.";

                if (redirectToCart)
                {
                    return RedirectToPage("/Customer/Cart");
                }

                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort, MinPrice, MaxPrice });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add product {ProductId} to cart for account {AccountId}", id, accountId);
                TempData["Error"] = "Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại sau.";
                return RedirectToPage("/Customer/Catalog", new { Page, Size, Search, Category, Sort, MinPrice, MaxPrice });
            }
        }

        private int GetAccountIdFromUser()
        {
            var accountIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(accountIdStr, out var accountId) ? accountId : 0;
        }
    }
}
