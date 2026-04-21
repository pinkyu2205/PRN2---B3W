using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Customer.MyShop;

[Authorize(Roles = "Customer")]
public class DeleteProductModel : PageModel
{
    private readonly IProductService _productService;
    private readonly IShopService _shopService;
    private readonly ILogger<DeleteProductModel> _logger;

    public DeleteProductModel(IProductService productService, IShopService shopService, ILogger<DeleteProductModel> logger)
    {
        _productService = productService;
        _shopService = shopService;
        _logger = logger;
    }

    public Product? Product { get; set; }

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

        Product = await _productService.GetProductEntityByIdAsync(id);
        if (Product == null || Product.ShopId != shop.Id)
        {
            TempData["Error"] = "Sản phẩm không tồn tại hoặc không thuộc shop của bạn.";
            return RedirectToPage("/Customer/MyShop");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
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

        var result = await _productService.DeleteProductAsync(id);
        if (result)
        {
            _logger.LogInformation("Product {ProductId} deleted by shop {ShopId}", id, shop.Id);
            TempData["Success"] = "Đã xóa sản phẩm thành công.";
        }
        else
        {
            TempData["Error"] = "Không thể xóa sản phẩm.";
        }

        return RedirectToPage("/Customer/MyShop");
    }
}
