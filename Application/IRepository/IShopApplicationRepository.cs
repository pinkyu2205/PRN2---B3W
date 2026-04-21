using Domain.Entities;
using Domain.Enums;

namespace Application.IRepository;

public interface IShopApplicationRepository : IGenericRepository<ShopApplication>
{
    Task<IEnumerable<ShopApplication>> GetByStatusAsync(ShopApplicationStatus status);
    Task<IEnumerable<ShopApplication>> GetByApplicantIdAsync(int applicantId);
}
