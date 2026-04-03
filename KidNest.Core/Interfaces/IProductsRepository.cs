using KidNest.Core.Entities;
using KidNest.Core.Shared;

namespace KidNest.Core.Interfaces
{
    public interface IProductsRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<ProductVariant>> GetAllVariantsAsync(int productId);
        Task<IEnumerable<Product>> GetAllNewReleasesAsync();
        Task<IEnumerable<Product>> GetAllHotDealsAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<ProductVariant?> GetVariantByKeyAsync(string key);
        Task<ProductVariant?> GetVariantByIdAsync(int id);
        Task<int> AddAsync(Product product);
        Task<bool> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsByNameAsync(string name, int? excludedId = null);
        Task<bool> IsBarcodeDuplicateAsync(string barcode, int? excludeId = null);
        
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId, int? count = null);
        Task<IEnumerable<Product>> SearchByNameOrDescAsync(string searchQuery);
        Task<IEnumerable<Product>> SearchByNameDescAndCategoryAsync(string searchQuery, int categoryId);

        Task<(IEnumerable<Product> products, int totalCount)> GetFilteredProductsAsync(int start,
            int length,
            string searchValue,
            string sortColumn,
            string sortDirection);
    }
}
