using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using KidNest.Core.Shared;
using KidNest.Infrastructure.Repositories;
using KidNest.Services.DTOs.Settings;
using KidNest.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Services.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepo;
        private readonly IFileStorageService _fileStorageService;

        public SettingsService(ISettingsRepository settingsRepo, IFileStorageService fileStorageService) 
        {
            _settingsRepo = settingsRepo;
            _fileStorageService = fileStorageService;
        }

        public async Task<SettingsDTO?> GetSettingsAsync()
        {
            var settings = await _settingsRepo.GetAsync();

            if (settings == null)
                return null; // Or throw NotFoundException

            return new SettingsDTO
            {
                ContactEmail = settings.ContactEmail,
                ContactPhone = settings.ContactPhone,
                FacebookUrl = settings.FacebookUrl,
                InstagramUrl = settings.InstagramUrl,
                ContactWhatsapp = settings.ContactWhatsapp,
                ParallaxImage = settings.ParallaxImage,
                LastUpdated = settings.LastUpdated
            };
        }

        public async Task<OperationResult> UpdateSettingsAsync(SettingsDTO settingsDTO)
        {
            try
            {
                // Handle new image upload
                if (settingsDTO.ParallaxImageFile != null && settingsDTO.ParallaxImageFile.Length > 0)
                {
                    const string parallaxFolder = "uploads/img/parallax";

                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(settingsDTO.ParallaxImage))
                    {
                        _fileStorageService.DeleteFile(Path.Combine(parallaxFolder, settingsDTO.ParallaxImage));
                    }

                    // Save new image (file extension is validated inside SaveFileAsync)
                    string? savedPath = await _fileStorageService.SaveFileAsync(settingsDTO.ParallaxImageFile, parallaxFolder);

                    if (savedPath == null)
                        return OperationResult.Fail("Failed to save the new image. " +
                            "Ensure it's a valid image format.");

                    // Store only the file name in the database
                    settingsDTO.ParallaxImage = Path.GetFileName(savedPath);
                }

                var settingsToUpdate = new SiteSettings
                {
                    ContactEmail = settingsDTO.ContactEmail,
                    ContactPhone = settingsDTO.ContactPhone,
                    FacebookUrl = settingsDTO.FacebookUrl,
                    InstagramUrl = settingsDTO.InstagramUrl,
                    ContactWhatsapp = settingsDTO.ContactWhatsapp,
                    ParallaxImage = settingsDTO.ParallaxImage,
                    LastUpdated = DateTime.UtcNow
                };

                bool isUpdated = await _settingsRepo.UpdateAsync(settingsToUpdate);

                return isUpdated
                    ? OperationResult.Ok()
                    : OperationResult.Fail("Update failed. The settings may not exist or no changes were made.");
            }
            catch (Exception ex)
            {
                // Log the exception
                return OperationResult.Fail($"An error occurred while updating site settings: {ex.Message}");
            }
        }
    }
}
