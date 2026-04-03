using KidNest.Core.Entities;

namespace KidNest.Core.Interfaces
{
    public interface IContentsRepository
    {
        Task<IEnumerable<Content>> GetAllAsync();
        Task<Content?> GetByIdAsync(int id);
        Task<int> AddAsync(Content content);
        Task<bool> UpdateAsync(Content content);
        Task<bool> DeleteAsync(int id);
    }
}
