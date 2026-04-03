using KidNest.Services.DTOs.Products;
using KidNest.Services.DTOs.Settings;
using KidNest.Services.Extensions;
using KidNest.Services.Interfaces;
using KidNest.Services.Services;
using KidNest.Web.ViewModels.Products;
using KidNest.Web.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using NToastNotify;
using System.Security.Principal;

namespace KidNest.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(AuthenticationSchemes = "AdminScheme")]
    //[Authorize(AuthenticationSchemes = "AdminScheme"), Roles = "Admin]
    [Authorize(Policy = "AdminOnly")] // can replace the above
    public class SettingsController : Controller
    {
        private readonly ISettingsService _settingsService;
        private readonly IToastNotification _toastNotification;

        public SettingsController(ISettingsService settingsService, IToastNotification toastNotification)
        {
            _settingsService = settingsService;
            _toastNotification = toastNotification;
        }

        // GET: SettingsController
        public async Task<IActionResult> Index()
        {
            SettingsDTO? settingsDTO = await _settingsService.GetSettingsAsync();

            if (settingsDTO == null)
            {
                return NotFound();
            }

            // Map from general product view model to the product edit view model
            PublicSettingsViewModel settingsEditVM = new()
            {
                ContactEmail = settingsDTO.ContactEmail,
                ContactPhone = settingsDTO.ContactPhone,
                FacebookUrl = settingsDTO.FacebookUrl,
                InstagramUrl = settingsDTO.InstagramUrl,
                ContactWhatsapp = settingsDTO.ContactWhatsapp,
                ParallaxImage = settingsDTO.ParallaxImage
            };

            return View(settingsEditVM);
        }

        // POST: SettingsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PublicSettingsViewModel settingsEditVM)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Index), settingsEditVM);
            }

            try
            {
                SettingsDTO settingsDTO = new()
                {
                    ContactEmail = settingsEditVM.ContactEmail,
                    ContactPhone = settingsEditVM.ContactPhone,
                    FacebookUrl = settingsEditVM.FacebookUrl,
                    InstagramUrl = settingsEditVM.InstagramUrl,
                    ContactWhatsapp = settingsEditVM.ContactWhatsapp,
                    ParallaxImage = settingsEditVM.ParallaxImage,
                    ParallaxImageFile = settingsEditVM.ParallaxImageFile,
                };

                var result = await _settingsService.UpdateSettingsAsync(settingsDTO);

                if (!result.Success)
                {
                    ModelState.AddErrors(result);

                    return View(nameof(Index), settingsEditVM);
                }

                return RedirectToAction(nameof(Index), "Home");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
