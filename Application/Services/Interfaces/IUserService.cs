using Application.DTOs;
using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<AccountDTO?> GetUserByIdAsync(int id);
        Task<List<AccountDTO>> GetAllUsersAsync();
        Task<bool> UpdateProfileAsync(UpdateProfileCommand command);
        Task<int> GetTotalUsersAsync();
    }
}
