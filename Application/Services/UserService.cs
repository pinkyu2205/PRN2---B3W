using Application.DTOs;
using Application.IRepository;
using Application.Services.Interfaces;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        private readonly AutoMapper.IMapper _mapper;
        public UserService(IUnitOfWork uow, AutoMapper.IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<AccountDTO?> GetUserByIdAsync(int id)
        {
            var account = await _uow.AccountRepository.GetByIdAsync(id);
            if (account == null) return null;
            return _mapper.Map<AccountDTO>(account);
        }

        public async Task<List<AccountDTO>> GetAllUsersAsync()
        {
            var accounts = await _uow.AccountRepository.GetAllAsync();
            return accounts.Select(a => _mapper.Map<AccountDTO>(a)).ToList();
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileCommand command)
        {
            try
            {
                var acc = await _uow.AccountRepository.GetByIdAsync(command.AccountId);
                if (acc == null) return false;

                acc.FullName = command.FullName?.Trim() ?? string.Empty;
                acc.PhoneNumber = command.PhoneNumber?.Trim();
                acc.Address = command.Address?.Trim() ?? string.Empty;
                if (command.DateOfBirth.HasValue)
                {
                    acc.DateOfBirth = command.DateOfBirth.Value;
                }
                acc.Gender = command.Gender?.Trim() ?? string.Empty;
                acc.AvatarUrl = string.IsNullOrWhiteSpace(command.AvatarUrl) ? string.Empty : command.AvatarUrl.Trim();
                acc.ModifiedAt = DateTime.UtcNow;
                acc.ModifiedBy = string.IsNullOrWhiteSpace(command.ModifiedBy) ? "user" : command.ModifiedBy;

                _uow.AccountRepository.Update(acc);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetTotalUsersAsync()
        {
            try
            {
                return await _uow.AccountRepository.GetCountAsync();
            }
            catch
            {
                return 0;
            }
        }
    }
}
