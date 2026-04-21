using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace YukiSoraShop.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(IAuthService authService, ILogger<ForgotPasswordModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public Application.DTOs.ForgotPasswordDTO Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
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

                // Check if email exists in database
                var account = await _authService.GetAccountByEmailAsync(Input.Email);
                if (account == null)
                {
                    ErrorMessage = "❌ Email không tồn tại trong hệ thống. Vui lòng kiểm tra lại.";
                    return Page();
                }

                // Generate a simple reset token (in real app, this would be more secure)
                var resetToken = Guid.NewGuid().ToString("N")[..8].ToUpper();
                
                // Store reset token in session (in real app, store in database with expiry)
                HttpContext.Session.SetString("ResetToken", resetToken);
                HttpContext.Session.SetString("ResetEmail", Input.Email);
                HttpContext.Session.SetString("ResetExpiry", DateTime.UtcNow.AddMinutes(15).ToString());

                // Simulate sending email (in real app, send actual email)
                _logger.LogInformation("Reset token generated for {Email}: {Token}", Input.Email, resetToken);
                
                // Show the reset code directly for demo
                SuccessMessage = $"✅ Mã khôi phục đã được tạo!<br/><br/>" +
                                $"<div class='alert alert-info mt-3'>" +
                                $"<strong>Mã reset của bạn:</strong><br/>" +
                                $"<code class='fs-4 fw-bold text-primary'>{resetToken}</code><br/>" +
                                $"<small class='text-muted'>Mã này có hiệu lực trong 15 phút</small>" +
                                $"</div>" +
                                $"<p class='mt-3'>Vui lòng copy mã này và click vào nút bên dưới để đặt lại mật khẩu.</p>";
                
                // Don't redirect immediately, show the code first
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "❌ Có lỗi xảy ra. Vui lòng thử lại sau.";
                _logger.LogError(ex, "Error in ForgotPassword for {Email}", Input?.Email);
                return Page();
            }
        }
    }
}
