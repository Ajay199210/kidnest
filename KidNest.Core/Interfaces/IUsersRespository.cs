using KidNest.Core.Entities;

namespace KidNest.Core.Interfaces
{
    public interface IUsersRespository
    {
        Task<IEnumerable<AppUser>> GetAllAsync();
        Task<AppUser?> GetByIdAsync(int userId);
        Task<AppUser?> GetByEmailOrPhoneAsync(string emailOrPhone);
        Task<int> AddAsync(AppUser user);
        Task<bool> UpdateAsync(AppUser appUser);
        Task<bool> DeleteAsync(int userId);
        Task<string> GetUserRole(int userId);

        Task<bool> IsEmailOrPhoneExistsAsync(string emailOrPhone, int? excludedId = null);
        Task<(IEnumerable<AppUser> users, int totalCount)> GetFilteredUsersAsync(
           int start,
           int length,
           string searchValue,
           string sortColumn,
           string sortDirection);
    }
}
