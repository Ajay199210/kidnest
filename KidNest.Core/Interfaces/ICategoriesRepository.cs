using KidNest.Core.Entities;

namespace KidNest.Core.Interfaces
{
    public interface ICategoriesRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<int> AddAsync(Category category);
        Task<bool> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsByNameAsync(string name, int? excludedId = null);
        Task<bool> HasProductsAsync(int id);
    }
}
