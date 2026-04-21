using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Staff.Categories
{
    [Authorize(Roles = "Moderator,Staff")]
    public class StaffCategoryCreateModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public StaffCategoryCreateModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Category Category { get; set; } = new();

        public void OnGet() { }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            Category.CreatedAt = DateTime.UtcNow;
            Category.ModifiedAt = DateTime.UtcNow;
            Category.CreatedBy = User.Identity?.Name ?? "system";
            Category.ModifiedBy = User.Identity?.Name ?? "system";
            Category.IsDeleted = false;

            var ok = await _categoryService.CreateAsync(Category);
            if (ok)
            {
                TempData["SuccessMessage"] = "Thêm danh mục thành công!";
                return RedirectToPage("/Staff/Categories/List");
            }

            ModelState.AddModelError(string.Empty, "Không thể thêm danh mục. Tên có thể đã tồn tại.");
            return Page();
        }
    }
}
