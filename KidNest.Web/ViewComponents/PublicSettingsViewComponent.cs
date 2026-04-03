using KidNest.Services.DTOs.Settings;
using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.Settings;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.ViewComponents
{
    public class PublicSettingsViewComponent : ViewComponent
    {
        private readonly ISettingsService _settingsService;

        public PublicSettingsViewComponent(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string section)
        {
            SettingsDTO? settingsDTO = await _settingsService.GetSettingsAsync();

            if (settingsDTO != null)
            {
                PublicSettingsViewModel settings = new()
                {
                    ContactEmail = settingsDTO.ContactEmail,
                    ContactPhone = settingsDTO.ContactPhone,
                    FacebookUrl = settingsDTO.FacebookUrl,
                    InstagramUrl = settingsDTO.InstagramUrl,
                    ContactWhatsapp = settingsDTO.ContactWhatsapp,
                    ParallaxImage = settingsDTO.ParallaxImage,
                };

                return section switch
                {
                    "ContactInfo" => View("_ContactInfoPartial", settings),
                    "SocialIcons" => View("_SocialIconsPartial", settings),
                    "Parallax" => View("_ParallaxSectionPartial", settings),
                    _ => View(settings) // fallback
                };
            }

            return View();
        }
    }
}
