using Application.Services.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _uow;

        public CategoryService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            var items = await _uow.CategoryRepository.GetAllAsync();
            return items.ToList();
        }

        public Task<Category?> GetByIdAsync(int id)
        {
            return _uow.CategoryRepository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(Category category)
        {
            try
            {
                await _uow.CategoryRepository.AddAsync(category);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Category category)
        {
            try
            {
                _uow.CategoryRepository.Update(category);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var cat = await _uow.CategoryRepository.GetByIdAsync(id);
                if (cat == null) return false;
                _uow.CategoryRepository.SoftDelete(cat);
                await _uow.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

