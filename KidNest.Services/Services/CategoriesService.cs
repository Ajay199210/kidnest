using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using KidNest.Core.Shared;
using KidNest.Services.DTOs.Categories;
using KidNest.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KidNest.Services.Services
{
    public class CategoriesService : ICategoriesService
    {
        private readonly ICategoriesRepository _categoriesRepo;

        public CategoriesService(ICategoriesRepository categoriesRepo)
        {
            _categoriesRepo = categoriesRepo;
        }

        public async Task<OperationResult> CreateCategoryAsync(CategoryCreateDTO categoryCreateDTO)
        {
            try
            {
                bool isNameExists = await _categoriesRepo.ExistsByNameAsync(categoryCreateDTO.Name!);
                if (isNameExists)
                    return OperationResult.Fail("Category name already exists!");

                var categoryToAdd = new Category
                {
                    Name = categoryCreateDTO.Name,
                    Description = categoryCreateDTO.Description,
                };

                var rowsAffected = await _categoriesRepo.AddAsync(categoryToAdd);

                if (rowsAffected == 0)
                    return OperationResult.Fail("Failed to create the category. No rows affected.");

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                // Here you could log the exception
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while creating the category : {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteCategoryAsync(int categoryId)
        {
            try
            {
                // Check if the category is linked to products before deletion.
                bool hasProducts = await _categoriesRepo.HasProductsAsync(categoryId);
                if (hasProducts)
                {
                    return OperationResult.Fail("Cannot delete category: it contains associated products.");
                }

                bool isDeleted = await _categoriesRepo.DeleteAsync(categoryId);
                if (!isDeleted)
                {
                    return OperationResult.Fail("Category deletion failed. The category may not exist.");
                }

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                // Log the exception here (Serilog, NLog, etc.)
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while deleting the category: {ex.Message}");
            }
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _categoriesRepo.GetAllAsync();

            return categories.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();
        }

        public async Task<CategoryDTO?> GetCategoryByIdAsync(int categoryId)
        {
            // Fetch domain model from repository
            var category = await _categoriesRepo.GetByIdAsync(categoryId);

            if (category == null)
                return null;  // Or throw NotFoundException

            // Map domain model to ViewModel
            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
            };
        }

        public async Task<OperationResult> UpdateCategoryAsync(CategoryDTO categoryDTO)
        {
            try
            {
                bool isNameExists = await _categoriesRepo.ExistsByNameAsync(categoryDTO.Name!, categoryDTO.Id);
                if (isNameExists)
                {
                    return OperationResult.Fail("Category name already exists.");
                }

                var categoryToEdit = new Category
                {
                    Id = categoryDTO.Id,
                    Name = categoryDTO.Name,
                    Description = categoryDTO.Description
                };

                bool isCategoryUpdated = await _categoriesRepo.UpdateAsync(categoryToEdit);

                if (!isCategoryUpdated)
                {
                    return OperationResult.Fail("Category update failed. " +
                        "The category might not exist or no changes were detected.");
                }

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                // Log the exception here (Serilog, NLog, built-in logger, etc.)
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while updating the category: {ex.Message}");
            }
        }

        public async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync(int? selectedCategoryId)
        {
            var categories = await _categoriesRepo.GetAllAsync();

            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = selectedCategoryId.HasValue && c.Id == selectedCategoryId.Value // Set selected
            });
        }
    }
}
