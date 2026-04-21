using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly Application.Services.Interfaces.IAuthService _auth;

        public LogoutModel(Application.Services.Interfaces.IAuthService auth)
        {
            _auth = auth;
        }
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            // Domain-level hook (if any)
            await _auth.LogoutAsync();

            // Sign out khỏi authentication
            await HttpContext.SignOutAsync("CookieAuth");
            
            // Xóa session
            HttpContext.Session.Clear();
            
            TempData["LoggedOut"] = "Bạn đã đăng xuất.";
            return RedirectToPage("/Home");
        }
    }
}
