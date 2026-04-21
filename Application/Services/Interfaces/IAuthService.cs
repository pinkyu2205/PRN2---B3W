using Application.DTOs;
using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(AccountRegistrationDTO model);
        Task<Account?> LoginAsync(string email, string password);
        Task<Account?> ExternalLoginAsync(string email, string? fullName, string provider, string externalId);
        Task<bool> ChangePasswordAsync(string email, string newPassword);
        Task<Account?> GetAccountByEmailAsync(string email);
        Task LogoutAsync();
    }
}

