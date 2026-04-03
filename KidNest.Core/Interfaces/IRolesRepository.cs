using KidNest.Core.Entities;

namespace KidNest.Core.Interfaces
{
    public interface IRolesRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int roleId);
    }
}
