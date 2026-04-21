using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Staff.Products
{
    [Authorize(Roles = "Moderator,Staff")]
    public class StaffProductDeleteModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<StaffProductDeleteModel> _logger;

        public StaffProductDeleteModel(IProductService productService, ILogger<StaffProductDeleteModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public Product Product { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            Product = product;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var deleted = await _productService.DeleteProductAsync(Id);
                if (deleted)
                {
                    TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                    return RedirectToPage("/Staff/Products/List");
                }

                TempData["Error"] = "Không thể xóa sản phẩm.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", Id);
                TempData["Error"] = "Đã xảy ra lỗi khi xóa sản phẩm.";
                return Page();
            }
        }
    }
}
