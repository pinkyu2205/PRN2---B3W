using Application.IRepository;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class ShopRepository : GenericRepository<Shop>, IShopRepository
{
    public ShopRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Shop?> GetByOwnerIdAsync(int ownerId)
    {
        return await _dbSet
            .Where(s => !s.IsDeleted && s.OwnerId == ownerId)
            .FirstOrDefaultAsync();
    }
}
