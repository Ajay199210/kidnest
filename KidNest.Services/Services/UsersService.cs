using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using KidNest.Core.Shared;
using KidNest.Services.DTOs.Products;
using KidNest.Services.DTOs.Users;
using KidNest.Services.Interfaces;
using KidNest.Web.Models.PasswordReset;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;

namespace KidNest.Services.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRespository _usersRepo;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IMemoryCache _cache;

        public UsersService(IUsersRespository usersRespo, IPasswordHasher<AppUser> passwordHasher, 
            IOptions<IdentityOptions> identityOptions, IMemoryCache cache)
        {
            _usersRepo = usersRespo;
            _passwordHasher = passwordHasher;
            _identityOptions = identityOptions;
            _cache = cache;
        }

        public async Task<OperationResult> CreateAppUserAsync(AppUserCreateDTO userCreateDTO)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userCreateDTO.Email) && string.IsNullOrWhiteSpace(userCreateDTO.PhoneNumber))
                {
                    return OperationResult.Fail("Email or Phone must be provided.");
                }

                string identifier = userCreateDTO.Email ?? userCreateDTO.PhoneNumber!;

                if (await _usersRepo.IsEmailOrPhoneExistsAsync(identifier))
                {
                    return OperationResult.Fail("This email or phone number is already registered.");
                }

                // Validate password
                var passwordErrors = ValidatePassword(userCreateDTO.Password!);
                if(passwordErrors.Count > 0)
                {
                    return OperationResult.Fail(passwordErrors);
                }

                AppUser user = new()
                {
                    FullName = userCreateDTO.FullName,
                    Email = userCreateDTO.Email,
                    PhoneNumber = userCreateDTO.PhoneNumber,
                    Address = userCreateDTO.Address,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    UserCreatedBy = userCreateDTO.FullName,
                };

                user.Password = _passwordHasher.HashPassword(user, userCreateDTO.Password!);

                await _usersRepo.AddAsync(user);

                return OperationResult.Ok();
            }
            catch (Exception ex)
            {
                // Log the exception here if you have a logger (recommended!)
                return OperationResult.Fail($"An error occurred while creating the user: {ex.Message}");
            }
        }

        public async Task<ClaimsIdentity?> AuthenticateUserAsync(AppUserLoginDTO userLoginDTO)
        {
            var user = await _usersRepo.GetByEmailOrPhoneAsync(userLoginDTO.EmailOrPhone!);

            if (user != null)
            {
                var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.Password!, userLoginDTO.Password);

                if (passwordResult == PasswordVerificationResult.Success)
                {
                    // Get user role
                    string userRole = await _usersRepo.GetUserRole(user.Id);

                    if (userRole == "User" || userRole is null)
                    {
                        var claims = new List<Claim>
                        {
                            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new(ClaimTypes.Name, user.FullName!),
                            new(ClaimTypes.Email, user.Email ?? string.Empty),
                            new("PhoneNumber", user.PhoneNumber ?? string.Empty),
                            new(ClaimTypes.Role, userRole ?? "User"),
                        };

                        //return new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var identity = new ClaimsIdentity(claims, "KidNestCookieAuth");

                        // Update info after a successful login
                        user.LastLoginDate = DateTime.UtcNow;
                        user.LastLogInPCName = Dns.GetHostName();

                        await _usersRepo.UpdateAsync(user);

                        return identity;
                    }
                }
            }

            return null;
        }

        public async Task<ClaimsIdentity?> AuthenticateAdminAsync(AdminLoginDTO adminLoginDTO)
        {
            // Get user from the database
            var user = await _usersRepo.GetByEmailOrPhoneAsync(adminLoginDTO.EmailOrPhone);
            if (user != null)
            {
                var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.Password!, adminLoginDTO.Password);

                if (passwordResult == PasswordVerificationResult.Success)
                {
                    // Check if the user is an Admin
                    string userRole = await _usersRepo.GetUserRole(user.Id);

                    if (userRole == "Admin")
                    {
                        var claims = new List<Claim>
                        {
                        new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new (ClaimTypes.Name, user.FullName!),
                        new (ClaimTypes.Email, user.Email!),
                        new (ClaimTypes.Role, "Admin") // Add role claim
                        };

                        //var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var identity = new ClaimsIdentity(claims, "AdminScheme");

                        // Update info after a successful login
                        user.LastLoginDate = DateTime.UtcNow;
                        user.LastLogInPCName = Dns.GetHostName();

                        await _usersRepo.UpdateAsync(user);

                        return identity;
                    }
                }
            }

            return null; // Return null if authentication fails or role is not admin
        }

        public async Task<OperationResult> UpdateAppUserAsync(AppUserDTO userDTO)
        {
            try
            {
                var userToUpdate = await _usersRepo.GetByIdAsync(userDTO.Id);

                if (userToUpdate == null)
                    return OperationResult.Fail("User not found.");

                var isOldPasswordCorrect = _passwordHasher.VerifyHashedPassword(
                    userToUpdate,
                    userToUpdate.Password!,
                    userDTO.Password! // Must verify against the old password
                );

                if (isOldPasswordCorrect != PasswordVerificationResult.Success)
                    return OperationResult.Fail("Password is incorrect.");

                userToUpdate.FullName = userDTO.FullName;
                userToUpdate.Email = userDTO.Email;
                userToUpdate.PhoneNumber = userDTO.PhoneNumber;
                userToUpdate.Address = userDTO.Address;
                userToUpdate.IsActive = userDTO.IsActive;
                userToUpdate.LastUpdated = DateTime.Now;
                userToUpdate.UserUpdatedBy = userDTO.UserUpdatedBy;

                if (!string.IsNullOrWhiteSpace(userDTO.NewPassword))
                {
                    userToUpdate.Password = _passwordHasher.HashPassword(userToUpdate, userDTO.NewPassword);

                    // Validate password
                    var passwordErrors = ValidatePassword(userDTO.Password!);
                    if (passwordErrors.Count > 0)
                    {
                        return OperationResult.Fail(passwordErrors);
                    }
                }

                userToUpdate.LastUpdated = DateTime.UtcNow;
                userToUpdate.RowVersion++;

                bool isUserUpdated = await _usersRepo.UpdateAsync(userToUpdate);

                return isUserUpdated ?
                    OperationResult.Ok() :
                    OperationResult.Fail("User update failed. " +
                    "The user might not exist or no changes were detected.");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"An unexpected error occurred while updating the user: {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteAppUserAsync(int userId, AppUserDTO userDTO)
        {
            var userToDelete = await _usersRepo.GetByIdAsync(userId);
            if (userToDelete == null)
            {
                // or throw custom NotFoundException
                return OperationResult.Fail("User account does not exist.");
            }

            var isOldPasswordCorrect = _passwordHasher.VerifyHashedPassword(
                    userToDelete,
                    userToDelete.Password!,
                    userDTO.Password! // Must verify against the old password
                );

            if (isOldPasswordCorrect != PasswordVerificationResult.Success)
                return OperationResult.Fail("Password is incorrect.");

            try
            {
                bool isDeleted = await _usersRepo.DeleteAsync(userId);

                if (isDeleted)
                {
                    return OperationResult.Ok();
                }

                return OperationResult.Fail("User account deletion failed!");

            }
            catch (Exception ex)
            {
                // Log the exception here (Serilog, NLog, etc.)
                return OperationResult.Fail($"An unexpected error occurred " +
                    $"while closing user account: {ex.Message}");
            }
        }

        public async Task<AppUserDTO?> GetAppUserByIdAsync(int userId)
        {
            // Fetch domain model from repository
            var user = await _usersRepo.GetByIdAsync(userId);

            if (user == null)
                return null;  // Or throw NotFoundException

            // Map domain model to ViewModel
            return new AppUserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Address = user.Address,
                DOB = user.DOB,
                Code = user.Code,
                Password = user.Password,
                LastLoginDate = user.LastLoginDate,
                LastLogInPCName = user.LastLogInPCName,
                IsActive = user.IsActive,
                RowVersion = user.RowVersion,
                UserUpdatedBy = user.UserUpdatedBy,
                LastUpdated = user.LastUpdated,
                UserCreatedBy = user.UserCreatedBy,
                CreatedDate = user.CreatedDate,
            };
        }

        public async Task<AppUserDTO?> GetUserByEmailOrPhoneAsync(string emailOrPhone)
        {
            var user = await _usersRepo.GetByEmailOrPhoneAsync(emailOrPhone);

            if (user == null)
                return null;  // Or throw NotFoundException

            return new AppUserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Address = user.Address,
                DOB = user.DOB,
                Code = user.Code,
                Password = user.Password,
                LastLoginDate = user.LastLoginDate,
                LastLogInPCName = user.LastLogInPCName,
                IsActive = user.IsActive,
                RowVersion = user.RowVersion,
                UserUpdatedBy = user.UserUpdatedBy,
                LastUpdated = user.LastUpdated,
                UserCreatedBy = user.UserCreatedBy,
                CreatedDate = user.CreatedDate,
            };
        }

        public async Task<IEnumerable<AppUserDTO>> GetAppUsersAsync()
        {
            var users = await _usersRepo.GetAllAsync();

            return users.Select(u => new AppUserDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                DOB = u.DOB,
                Address = u.Address,
                Code = u.Code,
                Password = u.Password,
                LastLoginDate = u.LastLoginDate,
                LastLogInPCName = u.LastLogInPCName,
                IsActive = u.IsActive,
                RowVersion = u.RowVersion,
                UserUpdatedBy = u.UserUpdatedBy,
                LastUpdated = u.LastUpdated,
                UserCreatedBy = u.UserCreatedBy,
                CreatedDate = u.CreatedDate,
            }).ToList();
        }

        public string MaskContactInfo(string contact)
        {
            if (string.IsNullOrEmpty(contact)) return string.Empty;

            if (contact.Contains("@"))
            {
                var parts = contact.Split('@');

                return $"{parts[0][0]}***@{parts[1]}";
            }
            else
            {
                return contact.Length > 4
                    ? $"{contact[..2]}******{contact[^2..]}"
                    : "******";
            }
        }

        public GenerateOtpResponseDTO GenerateOtpCode(string emailOrPhone)
        {
            if (IsUserLockedOut(emailOrPhone))
            {
                return new GenerateOtpResponseDTO()
                {
                    IsSuccess = false,
                    IsLockedOut = true,
                    LockoutTimeRemaining = GetLockoutRemainingTime(emailOrPhone),
                    Message = "You are locked out. Please try again later."
                };
            }

            // Clear any previous attempts when generating new OTP
            var attemptsKey = $"otp_attempts:{emailOrPhone}";
            _cache.Remove(attemptsKey);

            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            Console.WriteLine(otp); // Remove in production

            _cache.Set($"otp:{emailOrPhone}", otp, TimeSpan.FromSeconds(60));

            return new GenerateOtpResponseDTO()
            {
                IsSuccess = true,
                IsLockedOut = false,
                Message = "OTP Code sent!",
                OtpCode = otp,
            };
        }

        public VerifyOtpResponseDTO VerifyOtpCode(string emailOrPhone, string otpCode)
        {
            // 1. Check lockout status first
            if (IsUserLockedOut(emailOrPhone))
            {
                return new VerifyOtpResponseDTO
                {
                    IsSuccess = false,
                    IsLockedOut = true,
                    LockoutTimeRemaining = GetLockoutRemainingTime(emailOrPhone),
                    Message = $"You are locked out. Please try again in {GetLockoutRemainingTime(emailOrPhone)} minutes."
                };
            }

            // 2. Get or initialize attempts
            var attemptsKey = $"otp_attempts:{emailOrPhone}";
            var attempts = _cache.GetOrCreate(attemptsKey, entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(5);
                return 0;
            });

            // 3. Check OTP existence
            var cacheKey = $"otp:{emailOrPhone}";
            if (!_cache.TryGetValue(cacheKey, out string? storedOtp))
            {
                return new VerifyOtpResponseDTO
                {
                    IsSuccess = false,
                    RemainingAttempts = 3 - attempts,
                    Message = "OTP expired. Please request a new one."
                };
            }

            // 4. Validate OTP
            if (storedOtp != otpCode)
            {
                attempts++;
                _cache.Set(attemptsKey, attempts, DateTimeOffset.UtcNow.AddMinutes(5));

                int remainingAttempts = 3 - attempts;

                if (remainingAttempts <= 0)
                {
                    SetLockout(emailOrPhone, 3); // Lock for 3 minutes
                    return new VerifyOtpResponseDTO
                    {
                        IsSuccess = false,
                        IsLockedOut = true,
                        LockoutTimeRemaining = 3,
                        Message = "Too many attempts. Please try again in 3 minutes."
                    };
                }

                return new VerifyOtpResponseDTO
                {
                    IsSuccess = false,
                    IsLockedOut = false,
                    RemainingAttempts = remainingAttempts,
                    Message = $"Incorrect OTP. Remaining attempt(s): {remainingAttempts}"
                };
            }

            // 5. Successful verification - cleanup
            _cache.Remove(cacheKey);
            _cache.Remove(attemptsKey);
            _cache.Remove($"reset_lockout:{emailOrPhone}");

            return new VerifyOtpResponseDTO
            {
                IsSuccess = true,
                IsLockedOut = false,
                Message = "OTP verified successfully!"
            };
        }

        public bool IsUserLockedOut(string emailOrPhone)
        {
            var lockoutKey = $"reset_lockout:{emailOrPhone}";

            return _cache.TryGetValue(lockoutKey, out DateTimeOffset _);
        }

        public int GetLockoutRemainingTime(string emailOrPhone)
        {
            var lockoutKey = $"reset_lockout:{emailOrPhone}";
            if (_cache.TryGetValue(lockoutKey, out DateTimeOffset lockoutTime))
            {
                return (int)Math.Ceiling((lockoutTime - DateTimeOffset.UtcNow).TotalMinutes);
            }

            return 0;
        }

        public async Task<OperationResult> ResetPasswordAsync(string emailOrPhone, string newPassword)
        {
            try
            {
                // 1. Verify OTP status first (critical security check)
                //var otpVerification = VerifyOtpCode(request.EmailOrPhone, request.Otp);
                //if (!otpVerification.IsSuccess)
                //{
                //    return OperationResult.Fail(otpVerification.Message);
                //}

                // 2. Get user by email/phone
                var user = await _usersRepo.GetByEmailOrPhoneAsync(emailOrPhone);
                if (user == null)
                {
                    return OperationResult.Fail("User not found.");
                }

                // 3. Validate new password strength
                var passwordErrors = ValidatePassword(newPassword);
                if (passwordErrors.Count > 0)
                {
                    return OperationResult.Fail(passwordErrors);
                }

                // 4. Check if new password is different from current
                var isSamePassword = _passwordHasher.VerifyHashedPassword(
                    user,
                    user.Password!,
                    newPassword
                );

                if (isSamePassword == PasswordVerificationResult.Success)
                {
                    return OperationResult.Fail("New password cannot be the same as current password.");
                }

                // 5. Update password
                user.Password = _passwordHasher.HashPassword(user, newPassword!);
                user.LastUpdated = DateTime.UtcNow;
                user.RowVersion++;

                // 6. Save changes
                bool isUpdated = await _usersRepo.UpdateAsync(user);

                if (isUpdated)
                {
                    // Cleanup: Invalidate OTP and any reset tokens
                    _cache.Remove($"otp:{emailOrPhone}");
                    //_cache.Remove($"reset_token:{request.EmailOrPhone}");

                    return OperationResult.Ok();
                }

                return OperationResult.Fail("Password reset failed. Please try again.");
            }
            catch (Exception ex)
            {
                // Log the full exception here
                return OperationResult.Fail($"An error occurred during password reset: {ex.Message}");
            }
        }

        private void SetLockout(string emailOrPhone, int minutes)
        {
            var lockoutKey = $"reset_lockout:{emailOrPhone}";
            var expiry = DateTimeOffset.UtcNow.AddMinutes(minutes);
            _cache.Set(lockoutKey, expiry, expiry);
        }

        private List<string> ValidatePassword(string password)
        {
            var options = _identityOptions!.Value.Password;

            var criteria = new List<string>()
            {
                $"Password must be at least {options.RequiredLength} characters",
                $"Password must contain at least one uppercase letter (A-Z)",
                $"Password must contain at least one lowercase letter (a-z)",
                $"Password must contain at least one digit (0-9)",
                $"Password must contain at least {options.RequiredUniqueChars} special characters"
            };

            if(string.IsNullOrEmpty(password) || password.Length < options.RequiredLength)
            {
                return criteria;
            }

            if(options.RequireUppercase && !password.Any(char.IsUpper))
            {
                return criteria;
            }

            if (options.RequireLowercase && !password.Any(char.IsDigit))
            {
                return criteria;
            }

            if (options.RequiredUniqueChars >= 1 && password.Distinct().Count() < options.RequiredUniqueChars)
            {
                return criteria;
            }

            return [];
        }

        // Filtering, Sorting & Pagination
        public async Task<DataTableResponse<AppUserDTO>> GetPaginatedUsersAsync(DataTableRequest request)
        {
            // Get sorting info from first column
            var sortColumn = request.Order.FirstOrDefault();
            var columnName = sortColumn != null ? request.Columns[sortColumn.Column].Data : "Id";

            var (users, totalCount) = await _usersRepo.GetFilteredUsersAsync(
                request.Start,
                request.Length,
                request.Search.Value,
                columnName,
                sortColumn?.Dir ?? "asc");

            var usersDTOs = users.Select(u => new AppUserDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                DOB = u.DOB,
                Code = u.Code,
                Password = u.Password,
                LastLoginDate = u.LastLoginDate,
                LastLogInPCName = u.LastLogInPCName,
                IsActive = u.IsActive,
                RowVersion = u.RowVersion,
                UserUpdatedBy = u.UserUpdatedBy,
                LastUpdated = u.LastUpdated,
                UserCreatedBy = u.UserCreatedBy,
                CreatedDate = u.CreatedDate,
            }).ToList();

            return new DataTableResponse<AppUserDTO>
            {
                Items = usersDTOs,
                TotalCount = totalCount
            };
        }
    }
}
