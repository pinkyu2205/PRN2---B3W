using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer;

[Authorize(Roles = "Customer")]
public class MyShopModel : PageModel
{
    private readonly IShopService _shopService;
    private readonly IProductService _productService;
    private readonly ILogger<MyShopModel> _logger;

    public MyShopModel(IShopService shopService, IProductService productService, ILogger<MyShopModel> logger)
    {
        _shopService = shopService;
        _productService = productService;
        _logger = logger;
    }

    public Shop? Shop { get; set; }
    public List<Product> ShopProducts { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    private int GetAccountId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : 0;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var accountId = GetAccountId();
        if (accountId <= 0) return RedirectToPage("/Auth/Login");

        Shop = await _shopService.GetShopByOwnerIdAsync(accountId);
        if (Shop == null)
        {
            TempData["Error"] = "Bạn chưa có shop. Vui lòng đăng ký shop trước.";
            return RedirectToPage("/Customer/RegisterShop");
        }

        // Get products belonging to this shop
        var allProducts = await _productService.GetAllProductsAsync();
        ShopProducts = allProducts
            .Where(p => p.ShopId == Shop.Id)
            .ToList();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            ShopProducts = ShopProducts
                .Where(p => p.ProductName.Contains(Search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var cats = await _productService.GetAllCategoriesAsync();
        CategoryOptions = cats.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CategoryName }).ToList();

        return Page();
    }
}
