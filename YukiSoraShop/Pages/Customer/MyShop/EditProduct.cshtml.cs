using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer.MyShop;

[Authorize(Roles = "Customer")]
public class EditProductModel : PageModel
{
    private readonly IProductService _productService;
    private readonly IShopService _shopService;
    private readonly ILogger<EditProductModel> _logger;

    public EditProductModel(IProductService productService, IShopService shopService, ILogger<EditProductModel> logger)
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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var accountId = GetAccountId();
        var shop = await _shopService.GetShopByOwnerIdAsync(accountId);
        if (shop == null) return RedirectToPage("/Customer/RegisterShop");

        var product = await _productService.GetProductEntityByIdAsync(id);
        if (product == null || product.ShopId != shop.Id)
        {
            TempData["Error"] = "Sản phẩm không tồn tại hoặc không thuộc shop của bạn.";
            return RedirectToPage("/Customer/MyShop");
        }

        Product = product;
        await LoadCategories();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var accountId = GetAccountId();
        var shop = await _shopService.GetShopByOwnerIdAsync(accountId);
        if (shop == null) return RedirectToPage("/Customer/RegisterShop");

        var existing = await _productService.GetProductEntityByIdAsync(Product.Id);
        if (existing == null || existing.ShopId != shop.Id)
        {
            TempData["Error"] = "Sản phẩm không tồn tại hoặc không thuộc shop của bạn.";
            return RedirectToPage("/Customer/MyShop");
        }

        // Update fields
        existing.ProductName = Product.ProductName;
        existing.Description = Product.Description;
        existing.Price = Product.Price;
        existing.StockQuantity = Product.StockQuantity;
        existing.CategoryId = Product.CategoryId;
        existing.ModifiedBy = User.Identity?.Name ?? "system";

        var category = await _productService.GetCategoryByIdAsync(Product.CategoryId);
        if (category != null)
        {
            existing.CategoryName = category.CategoryName;
        }

        var result = await _productService.UpdateProductAsync(existing);
        if (result)
        {
            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToPage("/Customer/MyShop");
        }

        TempData["Error"] = "Không thể cập nhật sản phẩm.";
        await LoadCategories();
        return Page();
    }

    private async Task LoadCategories()
    {
        var cats = await _productService.GetAllCategoriesAsync();
        CategoryOptions = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CategoryName }).ToList();
    }
}
