using KidNest.Core.Entities;
using KidNest.Core.Enums;
using KidNest.Core.Interfaces;
using KidNest.Core.Shared;
using KidNest.Services.DTOs.Contents;
using KidNest.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace KidNest.Services.Services
{
    public class ContentsService : IContentsService
    {
        private readonly IContentsRepository _contentsRepo;

        public ContentsService(IContentsRepository contentsRepo)
        {
            _contentsRepo = contentsRepo;
        }

        public async Task<OperationResult> CreateContentAsync(ContentCreateDTO contentCreateDTO)
        {
            try
            {
                //bool isNameExists = await _contentsRepo.ExistsByNameAsync(contentCreateDTO.Name!);
                //if (isNameExists)
                //    return OperationResult.Fail("Category name already exists!");

                Content contentToAdd = new()
                {
                    Name = contentCreateDTO.Name,
                    Type = contentCreateDTO.Type,
                    Path = contentCreateDTO.Path,
                    IsActive = contentCreateDTO.IsActive
                };
                
                var rowsAffected = await _contentsRepo.AddAsync(contentToAdd);

                if (rowsAffected == 0)
                    return OperationResult.Fail("Failed to create the content. No rows affected.");

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                // Here you could log the exception
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while creating the content : {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteContentAsync(int contentId)
        {
            try
            {
                // Add business logic (e.g., check if content has products)
                // Example: Check if the content is linked to products before deletion.
                //bool hasProducts = await _contentsRepo.HasProductsAsync(contentId);
                //if (hasProducts)
                //{
                //    return OperationResult.Fail("Cannot delete content: it contains associated products.");
                //}

                bool isDeleted = await _contentsRepo.DeleteAsync(contentId);
                if (isDeleted)
                {
                    return OperationResult.Ok();
                }

                return OperationResult.Fail("Category deletion failed. The content may not exist.");
            }
            catch (Exception ex)
            {
                // Log the exception here (Serilog, NLog, etc.)
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while deleting the content: {ex.Message}");
            }
        }

        public async Task<IEnumerable<ContentDTO>> GetAllContentsAsync()
        {
            var contents = await _contentsRepo.GetAllAsync();

            return contents.Select(c => new ContentDTO
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                Path = c.Path ?? "N/A", // Handle NULL
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<ContentDTO?> GetContentByIdAsync(int contentId)
        {
            // Fetch domain model from repository
            var content = await _contentsRepo.GetByIdAsync(contentId);

            if (content == null)
                return null;

            // Map domain model to ViewModel
            return new ContentDTO
            {
                Id = content.Id,
                Name = content.Name,
                Type = content.Type,
                Path = content.Path,
                IsActive = content.IsActive
            };
        }

        public async Task<OperationResult> UpdateContentAsync(ContentDTO contentDTO)
        {
            try
            {
                //bool isNameExists = await _contentsRepo.ExistsByNameAsync(categoryDTO.Name!, categoryDTO.Id);
                //if (isNameExists)
                //{
                //    return OperationResult.Fail("Category name already exists.");
                //}
               
                var contentToEdit = new Content
                {
                    Id = contentDTO.Id,
                    Name = contentDTO.Name,
                    Type = contentDTO.Type,
                    Path = contentDTO.Path,
                    IsActive = contentDTO.IsActive
                };

                bool isContentUpdated = await _contentsRepo.UpdateAsync(contentToEdit);

                if (isContentUpdated)
                {
                    return OperationResult.Ok();
                }

                return OperationResult.Fail("Contents update failed. " +
                    "Contents might not exist or no changes were detected.");
            }
            catch (Exception)
            {
                // Log the exception here (Serilog, NLog, built-in logger, etc.)
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while updating contents");
            }
        }
    }
}
