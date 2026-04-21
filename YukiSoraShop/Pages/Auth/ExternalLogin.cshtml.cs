using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace YukiSoraShop.Pages.Auth
{
    public class ExternalLoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(IAuthService authService, ILogger<ExternalLoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public string? ReturnUrl { get; set; }

        /// <summary>
        /// GET: Trigger external authentication
        /// </summary>
        public IActionResult OnGet(string provider, string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            // Request a redirect to the external login provider
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = new AuthenticationProperties 
            { 
                RedirectUri = redirectUrl,
                Items = { { "scheme", provider } }
            };

            return Challenge(properties, provider);
        }

        /// <summary>
        /// GET: Callback handler after external authentication
        /// </summary>
        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                _logger.LogWarning("External login error: {Error}", remoteError);
                TempData["ErrorMessage"] = $"L?i t? nhà cung c?p: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Get external login info
            var info = await HttpContext.AuthenticateAsync("CookieAuth");
            if (info?.Principal == null)
            {
                _logger.LogWarning("Unable to load external login info");
                TempData["ErrorMessage"] = "Không th? t?i thông tin ??ng nh?p t? Google.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            try
            {
                // Extract user information from external provider
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                var nameIdentifier = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var provider = info.Principal.Identity?.AuthenticationType ?? "Google";

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email claim not found in external login");
                    TempData["ErrorMessage"] = "Không th? l?y email t? tài kho?n Google.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                // Process external login (create or update account)
                var account = await _authService.ExternalLoginAsync(
                    email: email,
                    fullName: name,
                    provider: provider,
                    externalId: nameIdentifier ?? email
                );

                if (account == null)
                {
                    _logger.LogError("Failed to process external login for {Email}", email);
                    TempData["ErrorMessage"] = "Không th? t?o ho?c c?p nh?t tài kho?n.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                // Create claims for the authenticated user
                var roleName = account.Role?.RoleName ?? (account.RoleId switch
                {
                    2 => "Administrator",
                    3 => "Moderator",
                    _ => "Customer"
                });

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                    new Claim(ClaimTypes.Name, account.FullName ?? account.UserName),
                    new Claim(ClaimTypes.Email, account.Email),
                    new Claim(ClaimTypes.Role, roleName),
                    new Claim("Provider", provider)
                };

                // Add picture claim if available
                var pictureClaim = info.Principal.FindFirst("urn:google:picture");
                if (pictureClaim != null)
                {
                    claims.Add(new Claim("picture", pictureClaim.Value));
                }

                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in the user
                await HttpContext.SignInAsync("CookieAuth", claimsPrincipal, new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                });

                _logger.LogInformation("User {Email} logged in with {Provider}", email, provider);
                TempData["SuccessMessage"] = $"??ng nh?p thành công v?i {provider}!";

                return LocalRedirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing external login callback");
                TempData["ErrorMessage"] = "?ã x?y ra l?i trong quá trình ??ng nh?p. Vui lòng th? l?i.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
        }
    }
}
