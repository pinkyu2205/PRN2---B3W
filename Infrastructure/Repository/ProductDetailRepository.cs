using Application.IRepository;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repository;

namespace Infrastructure.Repository
{
    public class ProductDetailRepository : GenericRepository<ProductDetail>, IProductDetailRepository
    {
        public ProductDetailRepository(AppDbContext context) : base(context)
        {
        }
    }
}
