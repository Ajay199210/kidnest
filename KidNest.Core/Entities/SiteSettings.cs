using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Core.Entities
{
    public class SiteSettings : BaseEntity
    {
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? FacebookUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? ContactWhatsapp { get; set; }
        public string? ParallaxImage { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
