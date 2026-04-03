using KidNest.Core.Shared;
using KidNest.Services.DTOs.Contents;
using Microsoft.AspNetCore.Http;

namespace KidNest.Services.Interfaces
{
    public interface IContentsService
    {
        Task<IEnumerable<ContentDTO>> GetAllContentsAsync();
        Task<ContentDTO?> GetContentByIdAsync(int contentId);
        Task<OperationResult> CreateContentAsync(ContentCreateDTO contentCreateDTO);
        Task<OperationResult> UpdateContentAsync(ContentDTO contentDTO);
        Task<OperationResult> DeleteContentAsync(int contentId);
    }
}
