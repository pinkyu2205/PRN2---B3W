using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace YukiSoraShop.Pages.Staff.Categories
{
    [Authorize(Roles = "Moderator,Staff")]
    public class StaffCategoriesModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ILogger<StaffCategoriesModel> _logger;

        public StaffCategoriesModel(IProductService productService, ILogger<StaffCategoriesModel> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public List<Category> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra quyền Staff
            

            try
            {
                // Lấy danh sách danh mục
                Categories = await _productService.GetAllCategoriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories list");
                Categories = new List<Category>();
            }

            return Page();
        }
    }
}
