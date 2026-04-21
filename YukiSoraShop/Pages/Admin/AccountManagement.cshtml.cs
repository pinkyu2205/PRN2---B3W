using Application.DTOs;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class AccountManagementModel : PageModel
    {
        private readonly IUserService _userService;

        public AccountManagementModel(IUserService userService)
        {
            _userService = userService;
        }

        public List<AccountDTO> Users { get; set; } = new List<AccountDTO>();

        public async Task OnGetAsync()
        {
            try
            {
                Users = await _userService.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}");
            }
        }
    }
}
