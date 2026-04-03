using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Settings
{
    public class PublicSettingsViewModel
    {
        [DisplayName("Contact Email")]
        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? ContactEmail { get; set; }

        [DisplayName("Contact Phone Number")]
        [StringLength(50, ErrorMessage = "Max 50 characters are allowed")]
        public string? ContactPhone { get; set; }

        [DisplayName("Facebook URL")]
        [StringLength(200, ErrorMessage = "Max 200 characters are allowed")]
        public string? FacebookUrl { get; set; }

        [DisplayName("Insta URL")]
        [StringLength(200, ErrorMessage = "Max 200 characters are allowed")]
        public string? InstagramUrl { get; set; }

        [DisplayName("Whatsapp Contact")]
        [StringLength(500, ErrorMessage = "Max 500 characters are allowed")]
        public string? ContactWhatsapp { get; set; }

        [DisplayName("Parallax Image")]
        [StringLength(100, ErrorMessage = "Max 100 characters are allowed")]
        public string? ParallaxImage { get; set; }

        [DisplayName("Parallax Image File")]
        public IFormFile? ParallaxImageFile { get; set; }
    }
}
