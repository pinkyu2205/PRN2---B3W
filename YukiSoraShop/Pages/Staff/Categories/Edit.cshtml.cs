using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Staff.Categories
{
    [Authorize(Roles = "Moderator,Staff")]
    public class StaffCategoryEditModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public StaffCategoryEditModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Category Category { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var cat = await _categoryService.GetByIdAsync(id);
            if (cat == null) return RedirectToPage("/Staff/Categories/List");
            Category = cat;
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            Category.ModifiedAt = DateTime.UtcNow;
            Category.ModifiedBy = User.Identity?.Name ?? "system";
            var ok = await _categoryService.UpdateAsync(Category);
            if (ok)
            {
                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return RedirectToPage("/Staff/Categories/List");
            }
            ModelState.AddModelError(string.Empty, "Không thể cập nhật danh mục.");
            return Page();
        }
    }
}
