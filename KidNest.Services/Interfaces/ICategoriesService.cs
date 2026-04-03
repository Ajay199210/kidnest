using KidNest.Core.Shared;
using KidNest.Services.DTOs.Categories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KidNest.Services.Interfaces
{
    public interface ICategoriesService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO?> GetCategoryByIdAsync(int categoryId);
        Task<OperationResult> CreateCategoryAsync(CategoryCreateDTO category);
        Task<OperationResult> UpdateCategoryAsync(CategoryDTO category);
        Task<OperationResult> DeleteCategoryAsync(int categoryId);

        Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync(int? selectedCategoryId = null);
    }
}
