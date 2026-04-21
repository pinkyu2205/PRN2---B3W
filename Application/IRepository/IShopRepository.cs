using Domain.Entities;

namespace Application.IRepository;

public interface IShopRepository : IGenericRepository<Shop>
{
    Task<Shop?> GetByOwnerIdAsync(int ownerId);
}
