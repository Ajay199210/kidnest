using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Services.DTOs.Settings
{
    public class SettingsDTO
    {
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? ContactWhatsapp { get; set; }
        public string? ParallaxImage { get; set; }
        public IFormFile? ParallaxImageFile { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
