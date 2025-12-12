using ETicaret.Business.Abstract;
using ETicaret.Data.Abstract;
using ETicaret.Entity.Concrete;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.Business.Concrete
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _unitOfWork.Categories.GetAllAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _unitOfWork.Categories.FindAsync(c => c.IsActive);
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _unitOfWork.Categories.GetByIdAsync(id);
        }

        public async Task<Category?> GetCategoryWithProductsAsync(int id)
        {
            return await _unitOfWork.Categories.GetQueryable()
                .AsNoTracking()
                .Include(c => c.Products.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category.CreatedDate = DateTime.Now;
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            category.UpdatedDate = DateTime.Now;
            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return false;

            // Soft delete
            category.IsActive = false;
            category.UpdatedDate = DateTime.Now;
            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
