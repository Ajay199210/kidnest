using KidNest.Core.Shared;
using KidNest.Services.DTOs.Users;
using KidNest.Web.Models.PasswordReset;
using System.Security.Claims;

namespace KidNest.Services.Interfaces
{
    public interface IUsersService
    {
        // CRUD
        Task<OperationResult> CreateAppUserAsync(AppUserCreateDTO userCreateDTO);
        Task<OperationResult> UpdateAppUserAsync(AppUserDTO userDTO);
        Task<OperationResult> DeleteAppUserAsync(int userId, AppUserDTO userDTO);
        
        Task<IEnumerable<AppUserDTO>> GetAppUsersAsync();
        Task<AppUserDTO?> GetAppUserByIdAsync(int userId);
        Task<AppUserDTO?> GetUserByEmailOrPhoneAsync(string emailOrPhone);
        
        // Filtering, Sorting & Pagination
        Task<DataTableResponse<AppUserDTO>> GetPaginatedUsersAsync(DataTableRequest request);

        // Auth
        Task<ClaimsIdentity?> AuthenticateUserAsync(AppUserLoginDTO userLoginDTO);
        Task<ClaimsIdentity?> AuthenticateAdminAsync(AdminLoginDTO adminLoginDTO);

        // Password Reset
        string MaskContactInfo(string contact);
        GenerateOtpResponseDTO GenerateOtpCode(string emailOrPhone);
        VerifyOtpResponseDTO VerifyOtpCode(string emailOrPhone, string otpCode);
        bool IsUserLockedOut(string emailOrPhone);
        int GetLockoutRemainingTime(string emailOrPhone);
        Task<OperationResult> ResetPasswordAsync(string emailOrPhone, string newPassword);
    }
}
