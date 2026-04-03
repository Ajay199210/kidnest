using KidNest.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NToastNotify;
using KidNest.Services.DTOs.Users;
using KidNest.Web.ViewModels.Users;

namespace KidNest.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(AuthenticationSchemes = "AdminScheme")]
    //[Authorize(AuthenticationSchemes = "AdminScheme"), Roles = "Admin]
    [Authorize(Policy = "AdminOnly")] // can replace the above
    public class HomeController : Controller
    {
        private readonly IUsersService _usersService;
        private readonly IToastNotification _toastNotification;

        public HomeController(IUsersService usersService, IToastNotification toastNotification)
        {
            _usersService = usersService;
            _toastNotification = toastNotification;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // Explicitly check the admin scheme
            var result = await HttpContext.AuthenticateAsync("AdminScheme");

            if(result != null && result.Succeeded)
            {
                // Verify it's actually an admin claim
                if (result.Principal.IsInRole("Admin"))
                {
                    return RedirectToAction("Index");
                }
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        // POST: Admin/Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(AdminLoginViewModel adminLoginVM, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                // If the model is not valid, return the login form with validation errors
                return View(adminLoginVM);
            }

            var adminLoginDTO = new AdminLoginDTO
            {
                EmailOrPhone = adminLoginVM.EmailOrPhone!,
                Password = adminLoginVM.Password!,
            };

            // Authenticate the admin user (this will handle the logic of verifying email/phone and password)
            var identity = await _usersService.AuthenticateAdminAsync(adminLoginDTO);

            if (identity != null)
            {
                // Determine if the "Remember Me" checkbox is checked
                var authenticationProperties = new AuthenticationProperties
                {
                    IsPersistent = adminLoginVM.RememberMe,
                };

                // Sign in the admin with the AdminScheme cookie
                await HttpContext.SignInAsync(
                    "AdminScheme",
                    new ClaimsPrincipal(identity),
                    authenticationProperties
                );

                // Redirect safely
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Redirect to the Admin Dashboard (Index of Admin area)
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            // If authentication fails, show an error message and return the login page again
            ModelState.AddModelError(string.Empty, "Invalid credentials.");

            return View(adminLoginVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AdminScheme");

            //Response.Cookies.Delete("AdminAuthCookie", new CookieOptions
            //{
            //    Path = "/Admin"
            //});

            return RedirectToAction("Login");
        }
    }
}
