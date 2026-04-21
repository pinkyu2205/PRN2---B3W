using System.Linq.Expressions;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Application.IRepository
{
    public interface IGenericRepository<TModel> where TModel : BaseFullEntity
    {
        Task AddAsync(TModel model);
        void Update(TModel model);
        void HardDelete(TModel model);
        void SoftDelete(TModel model);
        Task<IEnumerable<TModel>> GetAllAsync();
        Task<TModel> GetByIdAsync(int id);
        IQueryable<TModel> GetAllQueryable(string includeProperties = "");
        Task<TModel> FindOneAsync(Expression<Func<TModel, bool>> predicate, string includeProperties = "");
        Task<int> GetCountAsync();
        Task SaveChangesAsync();
    }
}
