using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Auth
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(IAuthService authService, ILogger<ChangePasswordModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public ChangePasswordInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                Response.Redirect("/Auth/Login");
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Use claims for current user email
                var userEmail = User.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
                if (string.IsNullOrEmpty(userEmail)) return RedirectToPage("/Auth/Login");

                // Validate model
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Verify current password
                var currentUser = await _authService.LoginAsync(userEmail, Input.CurrentPassword);
                if (currentUser == null)
                {
                    ErrorMessage = "Mật khẩu hiện tại không đúng.";
                    return Page();
                }

                // Validate new password
                if (Input.NewPassword != Input.ConfirmPassword)
                {
                    ErrorMessage = "Mật khẩu mới và xác nhận không khớp.";
                    return Page();
                }

                if (Input.NewPassword.Length < 6)
                {
                    ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                    return Page();
                }

                // Update password in database
                var success = await _authService.ChangePasswordAsync(userEmail, Input.NewPassword);
                if (success)
                {
                    SuccessMessage = "🎉 Đổi mật khẩu thành công! Tài khoản của bạn đã an toàn hơn.";
                    TempData["SuccessMessage"] = "🎉 Đổi mật khẩu thành công! Tài khoản của bạn đã an toàn hơn.";
                    return RedirectToPage("/Customer/Profile");
                }
                else
                {
                    ErrorMessage = "❌ Đổi mật khẩu thất bại. Vui lòng thử lại.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Đã xảy ra lỗi khi đổi mật khẩu. Vui lòng thử lại.";
                _logger.LogError(ex, "Error changing password for current user");
                return Page();
            }
        }
    }

    public class ChangePasswordInputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
