using Application.IRepository;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public class ShopApplicationRepository : GenericRepository<ShopApplication>, IShopApplicationRepository
{
    public ShopApplicationRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<ShopApplication>> GetByStatusAsync(ShopApplicationStatus status)
    {
        return await _dbSet
            .Include(a => a.Applicant)
            .Where(a => !a.IsDeleted && a.Status == status)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ShopApplication>> GetByApplicantIdAsync(int applicantId)
    {
        return await _dbSet
            .Where(a => !a.IsDeleted && a.ApplicantId == applicantId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
