using KidNest.Core.Shared;
using KidNest.Services.DTOs.Users;
using KidNest.Services.Interfaces;
using KidNest.Services.Services;
using KidNest.Web.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(AuthenticationSchemes = "AdminScheme")]
    //[Authorize(AuthenticationSchemes = "AdminScheme"), Roles = "Admin]
    [Authorize(Policy = "AdminOnly")] // can replace the above
    public class UsersController : Controller
    {
        private readonly IUsersService _usersService;

        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersData([FromBody] DataTableRequest request)
        {
            try
            {
                var result = await _usersService.GetPaginatedUsersAsync(request);

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = result.TotalCount,
                    recordsFiltered = result.TotalCount, // Same if no separate filtering
                    data = result.Items
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Server error processing your request", details = ex.Message });
            }
        }

        // GET: UsersController
        //public async Task<IActionResult> Index()
        public IActionResult Index()
        {
            //var users = await _usersService.GetAppUsersAsync();

            //var usersIndexVM = users.Select(u => new UserIndexViewModel()
            //{
            //    Id = u.Id,
            //    FullName = u.FullName,
            //    PhoneNumber = u.PhoneNumber ?? "N/A", // Handle NULL
            //    Email = u.Email ?? "N/A",
            //    DOB = u.DOB,
            //    Code = u.Code ?? "N/A",
            //    Password = u.Password,
            //    LastLoginDate = u.LastLoginDate,
            //    LastLogInPCName = u.LastLogInPCName ?? "N/A",
            //    IsActive = u.IsActive,
            //    RowVersion = u.RowVersion,
            //    UserUpdatedBy = u.UserUpdatedBy ?? "N/A",
            //    LastUpdated = u.LastUpdated, 
            //    UserCreatedBy = u.UserCreatedBy ?? "N/A",
            //    CreatedDate = u.CreatedDate,
            //});

            //return View(usersIndexVM);

            return View();
        }

        // GET: UsersController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            AppUserDTO? userDTO = await _usersService.GetAppUserByIdAsync(id);

            if (userDTO == null)
            {
                return NotFound();
            }

            UserDetailsViewModel userDetailsVM = new()
            {
                Id = userDTO.Id,
                FullName = userDTO.FullName,
                PhoneNumber = userDTO.PhoneNumber ?? "N/A", // Handle NULL
                Email = userDTO.Email ?? "N/A",
                Address = userDTO.Address,
                DOB = userDTO.DOB,
                Code = userDTO.Code ?? "N/A",
                Password = userDTO.Password,
                LastLoginDate = userDTO.LastLoginDate,
                LastLogInPCName = userDTO.LastLogInPCName ?? "N/A",
                IsActive = userDTO.IsActive,
                RowVersion = userDTO.RowVersion,
                UserUpdatedBy = userDTO.UserUpdatedBy ?? "N/A",
                LastUpdated = userDTO.LastUpdated,
                UserCreatedBy = userDTO.UserCreatedBy ?? "N/A",
                CreatedDate = userDTO.CreatedDate,
            };

            return View(userDetailsVM);
        }

        // GET: UsersController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UsersController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsersController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            AppUserDTO? userDTO = await _usersService.GetAppUserByIdAsync(id);

            if (userDTO == null)
            {
                return NotFound();
            }

            // Map from general product view model to the product edit view model
            UserEditViewModel userEditVM = new()
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

            return View(userEditVM);
        }

        // POST: UsersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UsersController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userDTO = await _usersService.GetAppUserByIdAsync(id);

                if (userDTO == null)
                    return NotFound();

                UserDeleteViewModel userDeleteVM = new()
                {
                    Id = userDTO.Id,
                    FullName = userDTO.FullName,
                    PhoneNumber = userDTO.PhoneNumber ?? "N/A", // Handle NULL
                    Email = userDTO.Email ?? "N/A", // Handle NULL
                    Address = userDTO.Address,
                    DOB = userDTO.DOB,
                    Code = userDTO.Code ?? "N/A", // Handle NULL
                    Password = userDTO.Password,
                    LastLoginDate = userDTO.LastLoginDate,
                    LastLogInPCName = userDTO.LastLogInPCName,
                    IsActive = userDTO.IsActive,
                    RowVersion = userDTO.RowVersion,
                    UserUpdatedBy = userDTO.UserUpdatedBy ?? "N/A", // Handle NULL
                    LastUpdated = userDTO.LastUpdated,
                    UserCreatedBy = userDTO.UserCreatedBy ?? "N/A", // Handle NULL 
                    CreatedDate = userDTO.CreatedDate,
                };

                return View(userDeleteVM);
            }
            catch (Exception ex)
            {
                // Log error (ex, "Error fetching product")
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: UsersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, UserDeleteViewModel userDeleteVM)
        {
            if (id != userDeleteVM.Id)
            {
                return NotFound();
            }

            // Necessary fields
            AppUserDTO userDTO = new() 
            {
                FullName = userDeleteVM.FullName,
                PhoneNumber = userDeleteVM.PhoneNumber,
                Email = userDeleteVM.Email,
                Address= userDeleteVM.Address,
                Password = userDeleteVM.Password,
            };

            try
            {
                OperationResult deleteResult = await _usersService.DeleteAppUserAsync(id, userDTO);

                if (deleteResult.Success)
                    return RedirectToAction(nameof(Index));
                else
                {
                    ModelState.AddModelError("", deleteResult.Message!);
                    return View(userDeleteVM);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
