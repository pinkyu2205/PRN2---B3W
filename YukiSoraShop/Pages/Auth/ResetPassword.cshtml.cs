using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace YukiSoraShop.Pages.Auth
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IAuthService _authService;

        public ResetPasswordModel(IAuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public ResetPasswordInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet(string email)
        {
            // Pre-fill email if provided
            if (!string.IsNullOrEmpty(email))
            {
                Input.Email = email;
            }
            else
            {
                // Try to get email from session
                var sessionEmail = HttpContext.Session.GetString("ResetEmail");
                if (!string.IsNullOrEmpty(sessionEmail))
                {
                    Input.Email = sessionEmail;
                }
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Get reset token from session
                var sessionToken = HttpContext.Session.GetString("ResetToken");
                var sessionEmail = HttpContext.Session.GetString("ResetEmail");
                var sessionExpiry = HttpContext.Session.GetString("ResetExpiry");

                // Check if reset token exists and is valid
                if (string.IsNullOrEmpty(sessionToken) || string.IsNullOrEmpty(sessionEmail))
                {
                    ErrorMessage = "❌ Reset token không hợp lệ hoặc đã hết hạn. Vui lòng thử lại.";
                    return Page();
                }

                // Check if token matches (case insensitive)
                if (string.IsNullOrEmpty(Input.ResetToken) || 
                    !string.Equals(Input.ResetToken.Trim(), sessionToken, StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = $"❌ Mã reset không đúng. Mã bạn nhập: '{Input.ResetToken}', Mã đúng: '{sessionToken}'";
                    return Page();
                }

                // Check if email matches
                if (Input.Email != sessionEmail)
                {
                    ErrorMessage = "❌ Email không khớp với yêu cầu reset.";
                    return Page();
                }

                // Check expiry (simple check)
                if (DateTime.TryParse(sessionExpiry, out var expiry) && DateTime.UtcNow > expiry)
                {
                    ErrorMessage = "❌ Mã reset đã hết hạn. Vui lòng yêu cầu mã mới.";
                    return Page();
                }

                // Validate new password
                if (Input.NewPassword != Input.ConfirmPassword)
                {
                    ErrorMessage = "❌ Mật khẩu mới và xác nhận không khớp.";
                    return Page();
                }

                if (Input.NewPassword.Length < 6)
                {
                    ErrorMessage = "❌ Mật khẩu mới phải có ít nhất 6 ký tự.";
                    return Page();
                }

                // Update password in database
                var success = await _authService.ChangePasswordAsync(Input.Email, Input.NewPassword);
                if (success)
                {
                    // Clear session data
                    HttpContext.Session.Remove("ResetToken");
                    HttpContext.Session.Remove("ResetEmail");
                    HttpContext.Session.Remove("ResetExpiry");

                    SuccessMessage = "🎉 Mật khẩu đã được đặt lại thành công! Bạn có thể đăng nhập với mật khẩu mới.";
                    TempData["SuccessMessage"] = "🎉 Mật khẩu đã được đặt lại thành công! Bạn có thể đăng nhập với mật khẩu mới.";
                    TempData["ShowSuccess"] = "true";
                    return RedirectToPage("./Login");
                }
                else
                {
                    ErrorMessage = "❌ Không thể đặt lại mật khẩu. Vui lòng thử lại.";
                    return Page();
                }
            }
            catch (Exception)
            {
                ErrorMessage = "❌ Có lỗi xảy ra. Vui lòng thử lại sau.";
                
                return Page();
            }
        }
    }

    public class ResetPasswordInputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reset code is required")]
        [Display(Name = "Reset Code")]
        public string ResetToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Display(Name = "Confirm New Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}



