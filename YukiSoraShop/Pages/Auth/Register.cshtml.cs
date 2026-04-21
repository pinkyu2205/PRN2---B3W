using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;

        public RegisterModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public Application.DTOs.AccountRegistrationDTO Input { get; set; } = new();

        public void OnGet()
        {
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Kiểm tra điều khoản sử dụng
            if (!Input.AgreeToTerms)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng đồng ý với điều khoản sử dụng.");
                return Page();
            }

            // Thực hiện đăng ký
            var result = await _authService.RegisterAsync(Input);
            
            if (result)
            {
                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToPage("./Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email này đã được sử dụng. Vui lòng chọn email khác.");
                return Page();
            }
        }
    }
}
