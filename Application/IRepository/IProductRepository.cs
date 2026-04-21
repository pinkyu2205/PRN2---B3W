using Domain.Entities;

namespace Application.IRepository
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<List<Product>> GetAllProductsAsync();
    }
}
