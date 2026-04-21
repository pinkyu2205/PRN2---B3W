using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace YukiSoraShop.Pages.Staff.Products
{
    [Authorize(Roles = "Moderator,Staff")]
    public class StaffProductsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<StaffProductsModel> _logger;

        public StaffProductsModel(IProductService productService, ILogger<StaffProductsModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public List<Product> Products { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int Size { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Category { get; set; }

        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public List<SelectListItem> CategoryOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Normalize page/size
                Size = Size <= 0 ? 10 : Size;
                Page = Page <= 0 ? 1 : Page;

                // Load products with filters
                var paged = await _productService.GetProductsPagedEntitiesAsync(Page, Size, Search, Category);
                Products = paged.Items.ToList();
                TotalPages = paged.TotalPages;
                TotalItems = paged.TotalItems;

                if (TotalPages > 0 && Page > TotalPages) Page = TotalPages;

                // Load categories for filter
                var categories = await _productService.GetAllCategoriesAsync();
                CategoryOptions = categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryName,
                    Text = c.CategoryName,
                    Selected = c.CategoryName == Category
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products list");
                TempData["Error"] = "Không thể tải danh sách sản phẩm. Vui lòng thử lại sau.";
                Products = new List<Product>();
                TotalPages = 0;
                TotalItems = 0;
                CategoryOptions = new List<SelectListItem>();
            }

            return Page();
        }
    }
}
