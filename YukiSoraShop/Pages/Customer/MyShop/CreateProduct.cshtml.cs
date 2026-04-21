using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer.MyShop;

[Authorize(Roles = "Customer")]
public class CreateProductModel : PageModel
{
    private readonly IProductService _productService;
    private readonly IShopService _shopService;
    private readonly ILogger<CreateProductModel> _logger;

    public CreateProductModel(IProductService productService, IShopService shopService, ILogger<CreateProductModel> logger)
    {
        _productService = productService;
        _shopService = shopService;
        _logger = logger;
    }

    [BindProperty]
    public Product Product { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; set; } = new();

    private int GetAccountId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : 0;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var accountId = GetAccountId();
        var shop = await _shopService.GetShopByOwnerIdAsync(accountId);
        if (shop == null) return RedirectToPage("/Customer/RegisterShop");

        await LoadCategories();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var accountId = GetAccountId();
        var shop = await _shopService.GetShopByOwnerIdAsync(accountId);
        if (shop == null) return RedirectToPage("/Customer/RegisterShop");

        // Set CategoryName from CategoryId
        var category = await _productService.GetCategoryByIdAsync(Product.CategoryId);
        if (category != null)
        {
            Product.CategoryName = category.CategoryName;
        }

        Product.ShopId = shop.Id;
        Product.CreatedBy = User.Identity?.Name ?? "system";
        Product.ModifiedBy = User.Identity?.Name ?? "system";

        var result = await _productService.CreateProductAsync(Product);
        if (result)
        {
            _logger.LogInformation("Product created by shop {ShopId}", shop.Id);
            TempData["Success"] = "Thêm sản phẩm thành công!";
            return RedirectToPage("/Customer/MyShop");
        }

        TempData["Error"] = "Không thể thêm sản phẩm. Vui lòng thử lại.";
        await LoadCategories();
        return Page();
    }

    private async Task LoadCategories()
    {
        var cats = await _productService.GetAllCategoriesAsync();
        CategoryOptions = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CategoryName }).ToList();
    }
}
