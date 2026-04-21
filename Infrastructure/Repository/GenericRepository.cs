using Application.IRepository;
using Domain.Common;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repository
{
    public class GenericRepository<TModel> : IGenericRepository<TModel> where TModel : BaseFullEntity
    {
        protected DbSet<TModel> _dbSet;
        protected AppDbContext _dbContext;

        public GenericRepository(AppDbContext dbContext)
        {
            _dbSet = dbContext.Set<TModel>();
            _dbContext = dbContext;
        }

        public async Task AddAsync(TModel model)
        {
            await _dbSet.AddAsync(model);
        }

        public void HardDelete(TModel model)
        {
            _dbSet.Remove(model);
        }

        public async Task<IEnumerable<TModel>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().Where(e => !e.IsDeleted).ToListAsync();
        }

        public async Task<TModel> GetByIdAsync(int id)
        {
            TModel? model = await _dbSet.FindAsync(id);
            if (model == null || model.IsDeleted == true)
            {
                return null;
            }
            return model;
        }

        public void SoftDelete(TModel model)
        {
            model.IsDeleted = true;
            model.ModifiedAt = DateTime.UtcNow;
        }

        public void Update(TModel model)
        {
            if (model == null || model.IsDeleted == true)
            {
                //throw new Exceptions.InfrastructureException(HttpStatusCode.BadRequest, "Data is not exist");
            }
            model.ModifiedAt = DateTime.UtcNow;
            _dbSet.Update(model);
        }

        public virtual IQueryable<TModel> GetAllQueryable(string includeProperties = "")
        {
            IQueryable<TModel> query = _dbSet;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return query.Where(x => !x.IsDeleted);
        }

        public async Task<TModel> FindOneAsync(Expression<Func<TModel, bool>> predicate, string includeProperties = "")
        {
            IQueryable<TModel> query = _dbSet;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return await query.Where(x => !x.IsDeleted).FirstOrDefaultAsync(predicate);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _dbSet.CountAsync(e => !e.IsDeleted);
        }
    }
}
