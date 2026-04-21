using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Application;
using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace YukiSoraShop.Pages.Customer
{
    [Authorize]
    public class EditProfileModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<EditProfileModel> _logger;

        public EditProfileModel(IUserService userService, ILogger<EditProfileModel> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [BindProperty]
        public EditProfileInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var id = GetCurrentUserId();
            if (id <= 0) return RedirectToPage("/Auth/Login");

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return RedirectToPage("/Auth/Login");

            Input.FullName = user.FullName ?? string.Empty;
            Input.PhoneNumber = user.PhoneNumber ?? string.Empty;
            Input.Address = user.Address ?? string.Empty;
            Input.DateOfBirth = user.DateOfBirth == default ? (DateTime?)null : user.DateOfBirth;
            Input.Gender = user.Gender ?? string.Empty;
            Input.AvatarUrl = user.AvatarUrl ?? string.Empty;
            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var id = GetCurrentUserId();
            if (id <= 0) return RedirectToPage("/Auth/Login");

            var cmd = new UpdateProfileCommand
            {
                AccountId = id,
                FullName = Input.FullName,
                PhoneNumber = Input.PhoneNumber,
                Address = Input.Address ?? string.Empty,
                DateOfBirth = Input.DateOfBirth,
                Gender = Input.Gender ?? string.Empty,
                AvatarUrl = Input.AvatarUrl,
                ModifiedBy = User.Identity?.Name ?? "user"
            };

            var ok = await _userService.UpdateProfileAsync(cmd);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật hồ sơ. Vui lòng thử lại.");
                return Page();
            }

            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToPage("/Customer/Profile");
        }

        private int GetCurrentUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            return int.TryParse(idStr, out var id) ? id : 0;
        }
    }

    public class EditProfileInput
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        [Url]
        [StringLength(500)]
        public string? AvatarUrl { get; set; }
    }
}
