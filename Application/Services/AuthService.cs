using Application.Services.Interfaces;
using Application.DTOs;
using Domain.Entities;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;

        public AuthService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<bool> RegisterAsync(AccountRegistrationDTO model)
        {
            try
            {
                var existing = await _uow.AccountRepository.GetByEmailAsync(model.Email);
                if (existing != null) return false;

                var account = new Account
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    RoleId = 1,
                    Status = "Active",
                    IsExternal = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    ModifiedAt = DateTime.UtcNow,
                    ModifiedBy = "System",
                    IsDeleted = false,
                    Password = model.Password // plain text for now; replace with hashing
                };

                await _uow.AccountRepository.AddAsync(account);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Account?> LoginAsync(string email, string password)
        {
            try
            {
                var account = await _uow.AccountRepository.GetByEmailAsync(email);
                if (account == null) return null;
                return password == account.Password ? account : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// X? lý ??ng nh?p external (Google, Facebook, etc.)
        /// T? ??ng t?o tài kho?n n?u ch?a t?n t?i
        /// </summary>
        public async Task<Account?> ExternalLoginAsync(string email, string? fullName, string provider, string externalId)
        {
            try
            {
                var account = await _uow.AccountRepository.GetByEmailAsync(email);
                
                if (account == null)
                {
                    // T?o tài kho?n m?i cho external user
                    account = new Account
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName ?? email.Split('@')[0],
                        RoleId = 1, // Customer role
                        Status = "Active",
                        IsExternal = true,
                        ExternalProvider = provider,
                        Password = null, // External users không có password
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "External",
                        ModifiedAt = DateTime.UtcNow,
                        ModifiedBy = "External",
                        IsDeleted = false,
                        Address = string.Empty,
                        Gender = string.Empty,
                        AvatarUrl = string.Empty,
                        DateOfBirth = DateTime.UtcNow
                    };

                    await _uow.AccountRepository.AddAsync(account);
                    await _uow.SaveChangesAsync();
                }
                else
                {
                    // C?p nh?t thông tin n?u tài kho?n ?ã t?n t?i
                    if (!account.IsExternal)
                    {
                        account.IsExternal = true;
                        account.ExternalProvider = provider;
                    }
                    
                    account.ModifiedAt = DateTime.UtcNow;
                    account.ModifiedBy = "External";
                    
                    _uow.AccountRepository.Update(account);
                    await _uow.SaveChangesAsync();
                }

                return account;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ChangePasswordAsync(string email, string newPassword)
        {
            try
            {
                var account = await _uow.AccountRepository.GetByEmailAsync(email);
                if (account == null) return false;
                account.Password = newPassword;
                account.ModifiedAt = DateTime.UtcNow;
                account.ModifiedBy = "User";
                _uow.AccountRepository.Update(account);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Task<Account?> GetAccountByEmailAsync(string email)
        {
            return _uow.AccountRepository.GetByEmailAsync(email);
        }

        public Task LogoutAsync()
        {
            // Domain-level logout hook (no-op). Web layer handles cookie sign-out.
            return Task.CompletedTask;
        }
    }
}

