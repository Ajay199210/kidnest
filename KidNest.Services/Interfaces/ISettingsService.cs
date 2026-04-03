using KidNest.Core.Shared;
using KidNest.Services.DTOs.Settings;

namespace KidNest.Services.Interfaces
{
    public interface ISettingsService
    {
        Task<SettingsDTO?> GetSettingsAsync();
        Task<OperationResult> UpdateSettingsAsync(SettingsDTO productDTO);
    }
}
