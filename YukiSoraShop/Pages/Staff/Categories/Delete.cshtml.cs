using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Staff.Categories
{
    [Authorize(Roles = "Moderator,Staff")]
    public class StaffCategoryDeleteModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public StaffCategoryDeleteModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public Category? Category { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Category = await _categoryService.GetByIdAsync(id);
            if (Category == null)
            {
                TempData["Error"] = "Danh mục không tồn tại.";
                return RedirectToPage("/Staff/Categories/List");
            }
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (Category == null) return RedirectToPage("/Staff/Categories/List");

            var ok = await _categoryService.DeleteAsync(Category.Id);
            if (ok)
            {
                TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            }
            else
            {
                TempData["Error"] = "Không thể xóa danh mục.";
            }
            return RedirectToPage("/Staff/Categories/List");
        }
    }
}
