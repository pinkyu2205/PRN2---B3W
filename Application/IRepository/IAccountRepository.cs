using Domain.Entities;

namespace Application.IRepository
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Task<Account?> GetByEmailAsync(string email);
    }
}
