using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NToastNotify;
using KidNest.Services.Interfaces;
using KidNest.Core.Shared;
using KidNest.Web.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using KidNest.Services.DTOs.Users;
using Twilio.Http;
using KidNest.Services.Services;
using KidNest.Web.Models.PasswordReset;
using KidNest.Services.DTOs.PasswordReset;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.Controllers
{
    //[Authorize(AuthenticationSchemes = "UserScheme")]
    //[Authorize(AuthenticationSchemes = "UserScheme", Roles = "User")]
    [Authorize(Policy = "UserOnly")] // can replace the above
    public class AccountController : Controller
    {
        private readonly IUsersService _usersService;
        private readonly IToastNotification _toastNotification;

        public AccountController(IUsersService usersService, IToastNotification toastNotification)
        {
            _usersService = usersService;
            _toastNotification = toastNotification;
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterViewModel userRegisterVM)
        {
            if (userRegisterVM.Password != userRegisterVM.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords must match");
            }

            if (!ModelState.IsValid)
            {
                return View(userRegisterVM);
            }

            var userCreateDTO = new AppUserCreateDTO
            {
                FullName = $"{userRegisterVM.FullName}".Trim(),
                Email = userRegisterVM.EmailOrPhone!.Contains("@") ? userRegisterVM.EmailOrPhone : null,
                PhoneNumber = !userRegisterVM.EmailOrPhone!.Contains("@") ? userRegisterVM.EmailOrPhone : null,
                Address = userRegisterVM.Address,
                Password = userRegisterVM.Password // raw password, to be hashed in Service
            };

            OperationResult result = await _usersService.CreateAppUserAsync(userCreateDTO);

            if (!result.Success)
            {
                if (result.Errors.Count > 0)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                        //_toastNotification.AddErrorToastMessage(error);
                    }

                    return View(userRegisterVM);
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("An unknown error occurred during registration.");
                }

                return View(userRegisterVM);
            }

            TempData["Success"] = "Your account has been created successfully! <a href='/Account/Login'>Login to your account</a>";

            return RedirectToAction(nameof(Index), "Home");
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginViewModel userLoginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(userLoginVM);
            }

            var userLoginDTO = new AppUserLoginDTO
            {
                EmailOrPhone = userLoginVM.EmailOrPhone!,
                Password = userLoginVM.Password!
            };

            ClaimsIdentity? identity = await _usersService.AuthenticateUserAsync(userLoginDTO);

            if (identity != null)
            {
                // Determine if the "Remember Me" checkbox is checked
                var authenticationProperties = new AuthenticationProperties
                {
                    IsPersistent = userLoginVM.RememberMe
                };

                await HttpContext.SignInAsync(
                    "UserScheme",  // Use User cookie scheme here
                    new ClaimsPrincipal(identity),
                    authenticationProperties
                );

                return RedirectToAction(nameof(Index), "Home");
            }

            //_toastNotification.AddErrorToastMessage($"Invalid login attempt.");
            ModelState.AddModelError("", "Invalid login attempt.");

            return View(userLoginVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserScheme");

            return RedirectToAction(nameof(Index), "Home");
        }

        public async Task<IActionResult> Update()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return Forbid(); // Or redirect to login if appropriate
            }

            AppUserDTO? userDTO = await _usersService.GetAppUserByIdAsync(id);

            if (userDTO == null)
            {
                return NotFound(); // No user found
            }

            var userEditVM = new UserEditViewModel
            {
                Id = id,
                FullName = userDTO.FullName?.ToString() ?? string.Empty,
                EmailOrPhone = userDTO.Email != null ? userDTO.Email.ToString() : userDTO.PhoneNumber?.ToString(),
                Address = userDTO.Address
            };

            return View(userEditVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UserEditViewModel userEditVM)
        {
            // Validate password confirmation if a new password was entered
            if (!string.IsNullOrEmpty(userEditVM.NewPassword))
            {
                if (userEditVM.NewPassword != userEditVM.ConfirmNewPassword)
                {
                    ModelState.AddModelError("ConfirmNewPassword", "The confirmation password does not match.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(userEditVM);
            }

            // Get current user ID from claims
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return Forbid(); // Or redirect to login if not authenticated
            }

            // Map ViewModel to DTO
            var userDTO = new AppUserDTO
            {
                Id = id,
                FullName = $"{userEditVM.FullName?.Trim()}",
                Email = userEditVM.EmailOrPhone!.Contains("@") ? userEditVM.EmailOrPhone : null,
                PhoneNumber = !userEditVM.EmailOrPhone!.Contains("@") ? userEditVM.EmailOrPhone : null,
                Address = userEditVM.Address,
                Password = userEditVM.Password,
                NewPassword = userEditVM.NewPassword,
            };

            // Call service
            OperationResult result = await _usersService.UpdateAppUserAsync(userDTO);

            if (result.Success)
            {
                // Update ClaimsPrincipal after a successful update
                var claimsIdentity = (ClaimsIdentity)User.Identity!;
                var nameClaim = claimsIdentity.FindFirst(ClaimTypes.Name);
                if (nameClaim != null)
                {
                    claimsIdentity.RemoveClaim(nameClaim);
                }

                claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, $"{userEditVM.FullName?.Trim()}"));

                await HttpContext.SignOutAsync("UserScheme");

                await HttpContext.SignInAsync(
                    //"KidNestCookieAuth",
                    "UserScheme",
                    new ClaimsPrincipal(claimsIdentity)
                );

                TempData["Success"] = "Your account has been updated successfully!";

                return RedirectToAction(nameof(Index), "Home");
            }

            if (result.Errors.Count > 0)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                    _toastNotification.AddErrorToastMessage(error);
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("An unknown error occurred during the update.");
            }

            return View(userEditVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(UserDeleteViewModel userDeleteVM)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return Unauthorized(new { success = false, message = "You are not authenticated." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, errors });
            }

            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out int userId))
            {
                return BadRequest(new { success = false, message = "Invalid user ID." });
            }

            var userDTO = new AppUserDTO
            {
                Password = userDeleteVM.Password
            };

            OperationResult result = await _usersService.DeleteAppUserAsync(userId, userDTO);

            if (result.Success)
            {
                await HttpContext.SignOutAsync("UserScheme");

                return NoContent(); // 204 No Content

                // OR if you need the redirect URL:
                //return Ok(new { success = true, redirectUrl = Url.Action(nameof(Index), "Home") });
            }

            return BadRequest(new
            {
                success = false,
                errors = result.Errors,
                message = result.Message
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyUser([FromBody] VerifyUserRequestDTO verifyUserRequestDTO)
        {
            try
            {
                var user = await _usersService.GetUserByEmailOrPhoneAsync(verifyUserRequestDTO.EmailOrPhone!);

                if (user == null)
                {
                    return Ok(new { exists = false });
                }

                //var isLockedOut = 
              
                return Ok(new
                {
                    exists = true,
                    isLocked = _usersService.IsUserLockedOut(verifyUserRequestDTO.EmailOrPhone!),
                    remainingLockoutTime = _usersService.GetLockoutRemainingTime(verifyUserRequestDTO.EmailOrPhone!),
                    maskedContact = _usersService.MaskContactInfo(verifyUserRequestDTO.EmailOrPhone!)
                });
            }
            catch (Exception)
            {
                // Optionally log the exception: _logger.LogError(ex, "Error verifying user");

                return StatusCode(500, new
                {
                    message = "An error occurred while verifying the user. Please try again."
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult SendOtp([FromBody] SendOtpRequestDTO sendOtpRequestDTO)
        {
            try
            {
                GenerateOtpResponseDTO generateOtpResponse = _usersService.GenerateOtpCode(sendOtpRequestDTO.EmailOrPhone!);

                if(generateOtpResponse.IsLockedOut)
                {
                    // HTTP 423 Locked
                    return StatusCode(423, new
                    {
                        success = generateOtpResponse.IsSuccess,
                        isLockedOut = generateOtpResponse.IsLockedOut,
                        lockoutTimeRemaining = generateOtpResponse.LockoutTimeRemaining,
                        message = generateOtpResponse.Message
                    });
                }

                //_smsService.Send(request.PhoneNumber, $"Your OTP is: {otp}");

                return Ok(new
                {
                    success = generateOtpResponse.IsSuccess,
                    isLockedOut = generateOtpResponse.IsLockedOut,
                    message = generateOtpResponse.Message,
                    otp = generateOtpResponse.OtpCode
                });
            }
            catch (Exception)
            {
                // Optional: _logger.LogError(ex, "Failed to send OTP");

                return StatusCode(500, new
                {
                    message = "An error occurred while sending the OTP. Please try again later."
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequestDTO verifyOtpRequestDTO)
        {
            try
            {
                var verifyOtpResponseDTO = _usersService.VerifyOtpCode(
                    verifyOtpRequestDTO.EmailOrPhone!,
                    verifyOtpRequestDTO.Otp!
                );

                if(verifyOtpResponseDTO.IsSuccess)
                {
                    return Ok(new
                    {
                        valid = verifyOtpResponseDTO.IsSuccess,
                        message = verifyOtpResponseDTO.Message
                    });
                }
              
                if (verifyOtpResponseDTO.IsLockedOut)
                {
                    // HTTP 423 Locked
                    return StatusCode(423, new
                    {
                        valid = verifyOtpResponseDTO.IsSuccess,
                        isLockedOut = verifyOtpResponseDTO.IsLockedOut,
                        lockoutTimeRemaining = verifyOtpResponseDTO.LockoutTimeRemaining,
                        message = verifyOtpResponseDTO.Message
                    });
                }

                return BadRequest(new
                {
                    message = verifyOtpResponseDTO.Message,
                    isLockedOut = verifyOtpResponseDTO.IsLockedOut,
                    attemptsLeft = verifyOtpResponseDTO.RemainingAttempts
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred during OTP verification. Please try again.",
                    isLockedOut = false
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            // Basic model validation
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            // Password confirmation match
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Password and confirmation do not match"
                });
            }

            try
            {
                // Call the service layer
                var result = await _usersService.ResetPasswordAsync(request.EmailOrPhone!, request.NewPassword!);

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message ?? "Password reset successfully"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result.Message,
                    errors = result.Errors
                });
            }
            catch (Exception ex)
            {
                // Log the full exception here
                //_logger.LogError(ex, "Password reset failed for {EmailOrPhone}", request.EmailOrPhone);

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while resetting your password"
                });
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
