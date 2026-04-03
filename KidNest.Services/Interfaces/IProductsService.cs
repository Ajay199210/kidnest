using KidNest.Core.Shared;
using KidNest.Services.DTOs.Products;
using Microsoft.AspNetCore.Http;

namespace KidNest.Services.Interfaces
{
    public interface IProductsService
    {
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<IEnumerable<ProductVariantDTO>> GetAllProductVariantsAsync(int productId);
        Task<IEnumerable<ProductDTO>> GetAllProductsNewReleasesAsync();
        Task<IEnumerable<ProductDTO>> GetAllProductsHotDealsAsync();
        Task<ProductDTO?> GetProductByIdAsync(int productId);
        Task<ProductVariantDTO?> GetProductVariantByKeyAsync(string key);
        Task<OperationResult> CreateProductAsync(ProductCreateDTO productCreateDTO);
        Task<OperationResult> UpdateProductAsync(ProductDTO productDTO);
        Task<OperationResult> DeleteProductAsync(int productId);
        Task<IEnumerable<ProductDTO>> GetProductsByCategoryIdAsync(int categoryId, int? count = null);
        Task<IEnumerable<ProductDTO>> SearchProductsByNameOrDescAsync(string searchQuery);
        Task<IEnumerable<ProductDTO>> SearchProductsByCategoryAndQueryAsync(int categoryId, string query);

        // Filtering, Sorting & Pagination
        Task<DataTableResponse<ProductDTO>> GetPaginatedProductsAsync(DataTableRequest request);
    }
}
