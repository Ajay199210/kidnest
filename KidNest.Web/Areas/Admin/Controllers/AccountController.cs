using KidNest.Core.Shared;
using KidNest.Services.DTOs.Users;
using KidNest.Services.Extensions;
using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Security.Claims;

namespace KidNest.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(AuthenticationSchemes = "AdminScheme")]
    //[Authorize(AuthenticationSchemes = "AdminScheme"), Roles = "Admin]
    [Authorize(Policy = "AdminOnly")] // can replace the above
    public class AccountController : Controller
    {
        private readonly IUsersService _usersService;
        private readonly IToastNotification _toastNotification;

        public AccountController(IUsersService usersService, IToastNotification toastNotification)
        {
            _usersService = usersService;
            _toastNotification = toastNotification;
        }

        // GET: AccountController
        [Route("/Admin/Account")]
        public async Task<IActionResult> Edit()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!int.TryParse(userId, out int id)) 
            {
                return Unauthorized();
            }

            AppUserDTO? userDTO = await _usersService.GetAppUserByIdAsync(id);

            if (userDTO == null)
            {
                return NotFound();
            }

            // Map from general product view model to the product edit view model
            AdminEditViewModel adminVM = new()
            {
                FullName = userDTO.FullName,
                PhoneNumber = userDTO.PhoneNumber,
                Email = userDTO.Email
            };

            return View(adminVM);
        }

        // POST: AccountController/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Admin/Account")]
        public async Task<IActionResult> Edit(AdminEditViewModel adminEditVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(adminEditVM);
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
                    FullName = $"{adminEditVM.FullName?.Trim()}",
                    Email = adminEditVM.Email!.Contains("@") ? adminEditVM.Email : null,
                    PhoneNumber = !adminEditVM.PhoneNumber!.Contains("@") ? adminEditVM.PhoneNumber : null,
                    Password = adminEditVM.Password
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

                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, $"{adminEditVM.FullName?.Trim()}"));

                    await HttpContext.SignOutAsync("AdminScheme");

                    await HttpContext.SignInAsync(
                        "AdminScheme",
                        new ClaimsPrincipal(claimsIdentity)
                    );

                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                }

                if (result.Errors.Count > 0)
                {
                    ModelState.AddErrors(result);
                    //_toastNotification.AddErrorToastMessage(error);
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("An unknown error occurred during the update.");
                }

                return View(adminEditVM);
            }
            catch (Exception)
            {
                // Log the error (you could use a logger here)
                //_toastNotification.AddErrorToastMessage("An unexpected error occurred.");
                // Optionally: log ex.Message or ex.ToString() to a logging service
                return View(adminEditVM);
            }
        }

        // GET: AccountController/Details
        public async Task<IActionResult> Details()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!int.TryParse(userId, out int id))
            {
                return Unauthorized();
            }

            AppUserDTO? userDTO = await _usersService.GetAppUserByIdAsync(id);

            if (userDTO == null)
            {
                return NotFound();
            }

            // Map from general product view model to the product edit view model
            AdminViewModel adminVM = new()
            {
                Id = userDTO.Id,
                FullName = userDTO.FullName,
                PhoneNumber = userDTO.PhoneNumber,
                Email = userDTO.Email,
                Address = userDTO.Address,
                DOB = userDTO.DOB,
                Code = userDTO.Code,
                Password = userDTO.Password,
                LastLoginDate = userDTO.LastLoginDate,
                LastLogInPCName = userDTO.LastLogInPCName,
                IsActive = userDTO.IsActive,
                RowVersion = userDTO.RowVersion,
                UserUpdatedBy = userDTO.UserUpdatedBy,
                LastUpdated = userDTO.LastUpdated,
                UserCreatedBy = userDTO.UserCreatedBy,
                CreatedDate = userDTO.CreatedDate
            };

            return View(adminVM);
        }
    }
}
